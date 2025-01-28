using System.Collections.Generic;
using UnityEngine;

public class BuildSystem : MonoBehaviour
{
    [Header("Building Prefabs")]
    public List<BuildingData> buildingPrefabs;

    public List<Building> buildings = new List<Building>();

    public void BuildLoad(BuildingData buildingData, Vector3 position, ResourceType type)
    {
        BuildInternal(buildingData, position, type, false, null);
    }

    public void Build(BuildingData buildingData, Vector3 position, ResourceType type, System.Action<string> onBuildFailed)
    {
        if (GameManager.Instance.ResourceManager.CanSpendResources(type, buildingData.cost))
        {
            GameManager.Instance.ResourceManager.SpendResources(type, buildingData.cost);
            BuildInternal(buildingData, position, type, true, onBuildFailed);
        }
        else
        {
            onBuildFailed?.Invoke("Not enough resources to build!");
        }
    }

    private void BuildInternal(BuildingData buildingData, Vector3 position, ResourceType type, bool deductResources, System.Action<string> onBuildFailed)
    {
        GameObject buildingObject = Instantiate(buildingData.prefab, position, Quaternion.identity);

        Building newBuilding = buildingObject.AddComponent<Building>();

        if (newBuilding == null)
        {
            Debug.LogError($"Prefab {buildingData.prefab.name} does not have a Building component!");
            return;
        }

        newBuilding.Initialize(buildingData);
        newBuilding.OnResourceGenerated += GameManager.Instance.ResourceManager.AddResources;
        buildings.Add(newBuilding);

        Debug.Log($"Building {buildingData.name} {(deductResources ? "constructed" : "loaded")} at {position}!");
    }


}

[System.Serializable]
public class BuildingData
{
    public string name;
    public GameObject prefab;
    public int cost;
    public ResourceType upgradeType;
    public ResourceType resourceType; 
    public int baseProduction;
}

