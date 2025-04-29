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
        LevelManager.Ins.Initialize(levels);
        UIManager.Ins.Initialize(levelText, swapsLeftText, goalText);
        GridManager.Ins.Initialize(LevelManager.Ins.currentLevel, candyPrefabs, specialCandyPrefabs, dirtTilePrefab, lockedTilePrefab, emptyTilePrefab);
        MatchManager.Ins.Initialize();
        SwapManager.Ins.Initialize();
        SpecialCandyManager.Ins.Initialize();
        TileManager.Ins.Initialize();
        GameStateManager.Ins.Initialize();
    }

    public IEnumerator InitializeLevel()
    {
        isProcessing = true;
        yield return StartCoroutine(GridManager.Ins.InitializeBoard());
        yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        UIManager.Ins.UpdateUI();
        isProcessing = false;
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            if (LevelManager.Ins.CheckWin())
            {
                Debug.Log("Level Complete!");
                LevelManager.Ins.NextLevel();
                if (LevelManager.Ins.currentLevel == null)
                {
                    Debug.Log("Game Completed All Levels!");
                    yield break;
                }
                yield return StartCoroutine(InitializeLevel());
            }
            else if (LevelManager.Ins.CheckLose())
            {
                Debug.Log("Game Over");
                yield break;
            }
            yield return null;
        }
    }
}