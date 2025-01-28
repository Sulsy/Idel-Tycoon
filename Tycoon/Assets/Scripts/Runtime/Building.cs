using UnityEngine;
using System;
using System.Collections;

public class Building : MonoBehaviour
{
    public string buildingName;
    public int level = 1;
    public BuildingData buildingData;

    public event Action<ResourceType, int> OnResourceGenerated; 

    private void Start()
    {
        StartCoroutine(GenerateResourcesRoutine()); 
    }

    public void Initialize(BuildingData data)
    {
        buildingData = data;
        buildingName = data.name;
    }
    

    private IEnumerator GenerateResourcesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); 

            int resourceAmount = buildingData.baseProduction * level; 
            OnResourceGenerated?.Invoke(buildingData.resourceType, resourceAmount); 
            Debug.Log($"{buildingName} generated {resourceAmount} {buildingData.resourceType}");
        }
    }
}