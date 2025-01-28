using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject BuildPanel;

    [Header("Resource UI")]
    public Text resource1Text;
    public Text resource2Text;
    public Text resource3Text;
    public Text resource4Text;

    [Header("Building UI")]
    public Transform buildingListContainer;
    public GameObject buildingButtonPrefab;
    public GameObject notEnoughResourcesMessage;

    private BuildingData selectedBuilding;
    private GameObject previewBuilding;
    private bool isPlacingBuilding = false;
    private bool canPlaceBuilding = false;
    private Collider towerZone;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameManager.Instance.ResourceManager.OnResourcesChanged += UpdateResourceUI;
        notEnoughResourcesMessage.SetActive(false);
        mainMenuPanel.SetActive(false);
    }


    private void UpdateResourceUI(Dictionary<ResourceType, int> resources)
    {
        foreach (var resource in resources)
        {
            switch (resource.Key)
            {
                case ResourceType.Commercial:
                    resource1Text.text = $"{resource.Value}";
                    break;
                case ResourceType.House:
                    resource2Text.text = $"{resource.Value}";
                    break;
                case ResourceType.Municipal:
                    resource3Text.text = $"{resource.Value}";
                    break;
                case ResourceType.Industrial:
                    resource4Text.text = $"{resource.Value}";
                    break;
            }
        }
    }

    public void MainMenu()
    {
        bool isActive = mainMenuPanel.activeSelf;

        if (isActive)
        {
            mainMenuPanel.SetActive(false);  
            BuildPanel.SetActive(true);
            TimeManager.Instance.ResetTimeScale();  
        }
        else
        {
            mainMenuPanel.SetActive(true); 
            BuildPanel.SetActive(false);
            TimeManager.Instance.TimeScale = 0f; 
        }
    }

    public void LoadBuildingsByCategory(List<BuildingData> buildings)
    {
        foreach (Transform child in buildingListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var building in buildings)
        {
            GameObject button = Instantiate(buildingButtonPrefab, buildingListContainer);
            button.GetComponentInChildren<Text>().text = building.name;
            button.GetComponent<Button>().onClick.AddListener(() => SelectBuilding(building));
        }
    }

    private void SelectBuilding(BuildingData buildingData)
    {
        selectedBuilding = buildingData;
        isPlacingBuilding = true;
    }

    private void Update()
    {
        if (isPlacingBuilding && selectedBuilding != null)
        {
            if (previewBuilding == null)
            {
                previewBuilding = Instantiate(selectedBuilding.prefab);
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                previewBuilding.transform.position = hit.point;
                CheckPlacementValidity();
            }

            SetPreviewMaterial(previewBuilding);

            if (Input.GetMouseButtonDown(0) && canPlaceBuilding)
            {
                PlacePreviewBuilding();
            }
        }
    }

    private void CheckPlacementValidity()
    {
        Collider[] colliders = Physics.OverlapBox(previewBuilding.transform.position, previewBuilding.transform.localScale / 2);
        canPlaceBuilding = false;

        foreach (var col in colliders)
        {
            if (col.CompareTag("TowerZone"))
            {
                canPlaceBuilding = true;
                towerZone = col;
                SnapToZone(towerZone);
                break;
            }
        }
    }
    public void LoadCategory(string category)
    {
        if (Enum.TryParse(category, out ResourceType parsedCategory))
        {
            var filteredBuildingPrefabs = GameManager.Instance.BuildSystem.buildingPrefabs
                .Where(building => building.resourceType == parsedCategory) 
                .OrderBy(building => building.resourceType)  
                .ToList();

            LoadBuildingsByCategory(filteredBuildingPrefabs);
        }
        else
        {
            Debug.LogError($"Invalid category: {category}. Could not convert to ResourceType.");
        }
    }


    private void SnapToZone(Collider zone)
    {
        previewBuilding.transform.position = zone.transform.position;
    }

    private void SetPreviewMaterial(GameObject building)
    {
        Color color = canPlaceBuilding ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        Renderer[] renderers = building.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material.color = color;
        }
    }

    private void PlacePreviewBuilding()
    {
        if (previewBuilding != null && canPlaceBuilding)
        {
            Collider[] colliders = Physics.OverlapBox(previewBuilding.transform.position, previewBuilding.transform.localScale / 2);
            foreach (var col in colliders)
            {
                Building existingBuilding = col.GetComponent<Building>();
                if (existingBuilding != null && existingBuilding.gameObject!=previewBuilding )
                {
                    Destroy(existingBuilding.gameObject);
                    break;
                }
            }
            GameManager.Instance.BuildSystem.Build(selectedBuilding, previewBuilding.transform.position,selectedBuilding.upgradeType, OnBuildFailed);
            Destroy(previewBuilding);
        }
        isPlacingBuilding = false;
    }

    private void OnBuildFailed(string message)
    {
        notEnoughResourcesMessage.SetActive(true);
        notEnoughResourcesMessage.GetComponent<Text>().text = message;
        Invoke(nameof(HideMessage), 2f);
    }

    private void HideMessage()
    {
        notEnoughResourcesMessage.SetActive(false);
    }
}
