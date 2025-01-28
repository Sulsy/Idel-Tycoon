using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public event Action<ResourceType, int> OnResourceChanged; 
    public event Action<Dictionary<ResourceType, int>> OnResourcesChanged; 

    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    public void AddResources(ResourceType resourceType, int amount)
    {
        if (resources.ContainsKey(resourceType))
        {
            resources[resourceType] += amount;
        }
        else
        {
            resources[resourceType] = amount;
        }

        OnResourceChanged?.Invoke(resourceType, resources[resourceType]);
        OnResourcesChanged?.Invoke(resources);
    }

    public bool CanSpendResources(ResourceType resourceType, int amount)
    {
        return resources.ContainsKey(resourceType) && resources[resourceType] >= amount;
    }

    public void SpendResources(ResourceType resourceType, int amount)
    {
        if (CanSpendResources(resourceType, amount))
        {
            resources[resourceType] -= amount;
            OnResourceChanged?.Invoke(resourceType, resources[resourceType]);
            OnResourcesChanged?.Invoke(resources);
        }
        else
        {
            Debug.LogError($"Not enough {resourceType}!");
        }
    }

    public Dictionary<ResourceType, int> Save()
    {
        return new Dictionary<ResourceType, int>(resources);
    }

    public void Load(Dictionary<ResourceType, int> loadedResources)
    {
        resources = new Dictionary<ResourceType, int>(loadedResources);
        OnResourcesChanged?.Invoke(resources); 
    }
}