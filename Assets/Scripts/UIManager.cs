using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Text levelText, swapsLeftText;
    [SerializeField] private GameObject goalContainer;
    [SerializeField] private GameObject goalItemPrefab;
    [SerializeField] private Sprite[] candySprites; 
    [SerializeField] private Sprite dirtTileSprite; 
    [SerializeField] private Sprite lockedTileSprite; 
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
        LoadCandySprites();
        LoadDirtTileSprite();
        LoadLockedTileSprite();
        LoadLevelText();
        LoadSwapsLeftText();
        LoadGoalContainer();
        LoadGoalItemPrefab();
        LoadSetting();
        LoadWinPanel();
        LoadLosePanel();
    }
    protected virtual void LoadCandySprites()
    {
        if (this.candySprites != null && this.candySprites.Length > 0) return;
        this.candySprites = Resources.LoadAll<Sprite>("Sprites/Candy");       
    }

    protected virtual void LoadDirtTileSprite()
    {
        if (this.dirtTileSprite != null) return;
        this.dirtTileSprite = Resources.Load<Sprite>("Sprites/Object/DirtTile");     
    }

    protected virtual void LoadLockedTileSprite()
    {
        if (this.lockedTileSprite != null) return;
        this.lockedTileSprite = Resources.Load<Sprite>("Sprites/Object/LockTile");
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

    protected virtual void LoadGoalContainer()
    {
        if (this.goalContainer != null) return;
        this.goalContainer = GameObject.Find("UI").transform.Find("GoalContainer").gameObject;
    }

    protected virtual void LoadGoalItemPrefab()
    {
        if (this.goalItemPrefab != null) return;
        this.goalItemPrefab = Resources.Load<GameObject>("Prefabs/Object/Goal");
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

        // Xóa các GoalItem cũ trong goalContainer
        foreach (Transform child in goalContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Thêm mục tiêu cho Dirt Tiles
        if (levelManager.currentLevel.clearAllDirtTiles)
        {
            GameObject goalItem = Instantiate(goalItemPrefab, goalContainer.transform);
            Text countText = goalItem.GetComponentInChildren<Text>();
            countText.text = $"{levelManager.clearedDirtTiles.Count}/{levelManager.currentLevel.dirtTiles.Count}";
            Image candyImage = goalItem.GetComponentInChildren<Image>();
            candyImage.sprite = dirtTileSprite; // Có thể dùng Sprite riêng cho Dirt Tiles
        }

        // Thêm mục tiêu cho Locked Tiles
        if (levelManager.currentLevel.clearAllLockedTiles)
        {
            GameObject goalItem = Instantiate(goalItemPrefab, goalContainer.transform);
            Text countText = goalItem.GetComponentInChildren<Text>();
            countText.text = $"{levelManager.clearedLockedTiles.Count}/{levelManager.currentLevel.lockedTiles.Count}";
            Image candyImage = goalItem.GetComponentInChildren<Image>();
            candyImage.sprite = lockedTileSprite;// Có thể dùng Sprite riêng cho Locked Tiles
            candyImage.color = new Color(1, 1, 1, 0.4f);                                    
        }

        // Thêm mục tiêu cho kẹo
        foreach (var goal in levelManager.currentLevel.goals)
        {
            GameObject goalItem = Instantiate(goalItemPrefab, goalContainer.transform);
            Text countText = goalItem.GetComponentInChildren<Text>();
            int matched = levelManager.matchedCandies.ContainsKey(goal.type) ? levelManager.matchedCandies[goal.type] : 0;
            countText.text = $"{matched}/{goal.count}";
            Image candyImage = goalItem.GetComponentInChildren<Image>();
            int spriteIndex = (int)goal.type; // Giả định CandyType là enum với thứ tự khớp với candySprites
            if (spriteIndex >= 0 && spriteIndex < candySprites.Length)
            {
                candyImage.sprite = candySprites[spriteIndex];
            }
            else
            {
                candyImage.gameObject.SetActive(false); // Ẩn nếu không có Sprite
            }
        }

        Debug.Log($"UpdateUI: swapsLeft={levelManager.remainingSwaps}, goals updated in GoalContainer");
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