using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Text levelText, swapsLeftText, goalText;
    [SerializeField] private GameObject setting;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    private void Start()
    {
        UpdateUI();
    }
    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadLevelText();
        LoadSwapsLeftText();
        LoadGoalText();
        LoadSetting();
        LoadWinPanel();
        LoadLosePanel();
    }
    protected virtual void LoadLevelText()
    {
        if (this.levelText != null) return;
        this.levelText = GameObject.Find("LevelText").GetComponent<Text>();
    } 
    protected virtual void LoadSwapsLeftText()
    {
        if (this.swapsLeftText != null) return;
        this.swapsLeftText = GameObject.Find("SwapLeftText").GetComponent<Text>();
    } 
    protected virtual void LoadGoalText()
    {
        if (this.goalText != null) return;
        this.goalText = GameObject.Find("GoalText").GetComponent<Text>();
    }
    protected virtual void LoadSetting()
    {
        if (this.setting != null) return;
        this.setting = GameObject.Find("UI").transform.Find("Setting").gameObject;
    }
    protected virtual void LoadWinPanel()
    {
        if (this.winPanel != null) return;
        this.winPanel = GameObject.Find("UI").transform.Find("Win").gameObject;
    }
    protected virtual void LoadLosePanel()
    {
        if (this.losePanel != null) return;
        this.losePanel = GameObject.Find("UI").transform.Find("Lose").gameObject;
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