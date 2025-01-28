using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public class GameManager : Singleton<GameManager>
{
    public ResourceManager ResourceManager { get; private set; }
    public ProgressionSystem ProgressionSystem { get; private set; }
    public BuildSystem BuildSystem { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        ResourceManager = FindObjectOfType<ResourceManager>();
        ProgressionSystem = FindObjectOfType<ProgressionSystem>();
        BuildSystem = FindObjectOfType<BuildSystem>();

        if (ResourceManager == null || ProgressionSystem == null || BuildSystem == null)
        {
            Debug.LogError("Some systems are not assigned in GameManager!");
        }
    }

    private void Start()
    {
        if (File.Exists(Application.persistentDataPath + "/save.json"))
        {
            LoadGame();
        }
       
        ResourceManager.AddResources(ResourceType.Commercial,10);
        ResourceManager.AddResources(ResourceType.Municipal,10);
        ResourceManager.AddResources(ResourceType.House,10);
        ResourceManager.AddResources(ResourceType.Industrial,10);
    }

    public void ExitGame()
    {
        SaveGame();
        Application.Quit();
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            resources = ResourceManager.Save()
                .Select(r => new ResourceData { resourceType = r.Key, amount = r.Value })
                .ToList(), 
            progression = ProgressionSystem.level,
            xp = ProgressionSystem.xp,
            buildings = GameManager.Instance.BuildSystem.buildings
                .Select(b => new BuildingSaveData
                {
                    buildingName = b.buildingName,
                    prefabName = b.buildingData.prefab.name, 
                    level = b.level,
                    resourceType = b.buildingData.resourceType,
                    position = b.transform.position 
                })
                .ToList()
        };

        SaveManager.Save(saveData);
    }



    public void LoadGame()
    {
        SaveData loadedData = SaveManager.Load<SaveData>(); 
        loadedData.ConvertResourcesToDictionary(); 

        ResourceManager.Load(loadedData.resourcesDictionary);
        ProgressionSystem.level = loadedData.progression;
        ProgressionSystem.xp = loadedData.xp;

        BuildSystem buildSystem = GameManager.Instance.BuildSystem;
        foreach (var buildingData in loadedData.buildings)
        {
            BuildingData prefabData = buildSystem.buildingPrefabs
                .FirstOrDefault(p => p.prefab.name == buildingData.prefabName);

            if (prefabData != null)
            {
                buildSystem.BuildLoad(prefabData, buildingData.position, buildingData.resourceType);
            }
            else
            {
                Debug.LogWarning($"Не найден префаб {buildingData.prefabName}, возможно, он был удален.");
            }
        }
    }


}

[System.Serializable]
public class SaveData
{
    public List<ResourceData> resources;
    public int progression;
    public int xp;
    public List<BuildingSaveData> buildings; 

    public void ConvertResourcesToDictionary()
    {
        resourcesDictionary = resources.ToDictionary(r => r.resourceType, r => r.amount);
    }

    public Dictionary<ResourceType, int> resourcesDictionary;
}
[System.Serializable]
public class BuildingSaveData
{
    public string buildingName;
    public string prefabName; 
    public int level;
    public ResourceType resourceType;
    public Vector3 position; 
}

[System.Serializable]
public class ResourceData
{
    public ResourceType resourceType;
    public int amount;
}
