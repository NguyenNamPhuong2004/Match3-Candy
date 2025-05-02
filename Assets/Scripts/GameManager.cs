using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    public GameObject[] candyPrefabs;
    public GameObject[] specialCandyPrefabs;
    public GameObject dirtTilePrefab;
    public GameObject lockedTilePrefab;
    public GameObject emptyTilePrefab;
    public LevelData[] levels;
    public Text levelText, swapsLeftText, goalText;

    private bool isProcessing;

    protected override void Awake()
    {
        base.Awake();
        InitializeComponents();
    }

    void Start()
    {
        StartCoroutine(InitializeLevel());
    }

    private void InitializeComponents()
    {
        if (LevelManager.Ins == null) Debug.LogError("LevelManager is null");
        else LevelManager.Ins.Initialize(levels);

        if (GridManager.Ins == null) Debug.LogError("GridManager is null");
        else GridManager.Ins.Initialize(LevelManager.Ins.currentLevel, candyPrefabs, specialCandyPrefabs, dirtTilePrefab, lockedTilePrefab, emptyTilePrefab);

        if (GameStateManager.Ins == null) Debug.LogError("GameStateManager is null");
        else GameStateManager.Ins.Initialize();

        if (SwapManager.Ins == null) Debug.LogError("SwapManager is null");
        else SwapManager.Ins.Initialize();

        if (MatchManager.Ins == null) Debug.LogError("MatchManager is null");
        else MatchManager.Ins.Initialize();

        if (UIManager.Ins == null) Debug.LogError("UIManager is null");
        else UIManager.Ins.Initialize(levelText, swapsLeftText, goalText);

        if (TileManager.Ins == null) Debug.LogError("TileManager is null");
        else TileManager.Ins.Initialize();

        if (SpecialCandyManager.Ins == null) Debug.LogError("SpecialCandyManager is null");
        else SpecialCandyManager.Ins.Initialize();
    }

    public IEnumerator InitializeLevel()
    {
        isProcessing = true;
        yield return StartCoroutine(GridManager.Ins.InitializeBoard());
        yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        UIManager.Ins.UpdateUI();
        isProcessing = false;
       
    }
}