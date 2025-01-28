using System;
using UnityEngine;

public class ProgressionSystem : MonoBehaviour
{
    public event Action<int> OnLevelChanged; 
    public int level;
    public int xp;
    public int xpToNextLevel;

    public void AddXP(int amount)
    {
        xp += amount;
        if (xp >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        xp -= xpToNextLevel;
        xpToNextLevel += 10; // Example: XP required increases linearly.
        OnLevelChanged.Invoke(level);
        Debug.Log($"Level up! New level: {level}");
    }
}