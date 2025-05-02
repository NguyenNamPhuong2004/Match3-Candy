using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    private Text levelText, swapsLeftText, goalText;
    public GameObject setting;
    public GameObject winPanel;
    public GameObject losePanel;

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
    public void OpenSetting()
    {
        SoundManager.Ins.ButtonSound();
        setting.SetActive(true);
    }
    public void CloseSetting()
    {
        SoundManager.Ins.ButtonSound();
        setting.SetActive(false);
    }
    public void OpenWinPanel()
    {
        SoundManager.Ins.ButtonSound();
        winPanel.SetActive(true);
    }
    public void OpenLosePanel()
    {
        SoundManager.Ins.ButtonSound();
        losePanel.SetActive(true);
    }
    public void Restart()
    {
        SoundManager.Ins.ButtonSound();
        SceneManager.LoadScene(1);
    } 
    public void ReturnToMenu()
    {
        SoundManager.Ins.ButtonSound();
        SceneManager.LoadScene(0);
    }
    public void NextLevel()
    {
        SoundManager.Ins.ButtonSound();
        DataPlayer.SetLevelGame(DataPlayer.GetLevelGame() + 1);
        SceneManager.LoadScene(1);
    }
}