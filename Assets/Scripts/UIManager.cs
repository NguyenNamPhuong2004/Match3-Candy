using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    private Text levelText, swapsLeftText, goalText;

    public void Initialize(Text levelText, Text swapsLeftText, Text goalText)
    {
        this.levelText = levelText;
        this.swapsLeftText = swapsLeftText;
        this.goalText = goalText;
        Debug.Log("UIManager initialized");
    }

    public void UpdateUI()
    {
        var levelManager = LevelManager.Ins;
        if (levelManager == null || levelManager.currentLevel == null) return;

        levelText.text = "Level: " + levelManager.currentLevel.level;
        swapsLeftText.text = "Swaps Left: " + levelManager.remainingSwaps;
        string goalStr = "Goals:\n";
        if (levelManager.currentLevel.clearAllDirtTiles)
        {
            goalStr += $"Dirt Tiles: {levelManager.clearedDirtTiles.Count}/{levelManager.currentLevel.dirtTiles.Count}\n";
        }
        if (levelManager.currentLevel.clearAllLockedTiles)
        {
            goalStr += $"Locked Tiles: {levelManager.clearedLockedTiles.Count}/{levelManager.currentLevel.lockedTiles.Count}\n";
        }
        foreach (var goal in levelManager.currentLevel.goals)
        {
            int matched = levelManager.matchedCandies.ContainsKey(goal.type) ? levelManager.matchedCandies[goal.type] : 0;
            goalStr += $"{goal.type}: {matched}/{goal.count}\n";
        }
        goalText.text = goalStr;
        Debug.Log($"UpdateUI: swapsLeft={levelManager.remainingSwaps}, goals={goalStr}");
    }
}