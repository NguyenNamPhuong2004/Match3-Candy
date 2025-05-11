using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStateManager : Singleton<GameStateManager>
{
    private Candy[,] candyGrid;
    private int GRID_WIDTH;
    private int GRID_HEIGHT;

    protected override void ResetValue()
    {
        base.ResetValue();
        candyGrid = GridManager.Ins.candyGrid;
        GRID_WIDTH = GridManager.Ins.GRID_WIDTH;
        GRID_HEIGHT = GridManager.Ins.GRID_HEIGHT;
    }

    private void Start()
    {
        StartCoroutine(InitializeAndProcessMatches());
    }

    private IEnumerator InitializeAndProcessMatches()
    {
        while (GridManager.Ins == null || GridManager.Ins.candyGrid == null)
        {
            Debug.LogWarning("Waiting for GridManager to initialize");
            yield return null;
        }

        int candyCount = 0;
        while (candyCount < (GRID_WIDTH * GRID_HEIGHT))
        {
            candyCount = 0;
            for (int row = 0; row < GRID_HEIGHT; row++)
            {
                for (int col = 0; col < GRID_WIDTH; col++)
                {
                    if (GridManager.Ins.candyGrid[row, col] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)))
                    {
                        candyCount++;
                    }
                }
            }
            if (candyCount < (GRID_WIDTH * GRID_HEIGHT))
            {
                Debug.LogWarning($"candyGrid has only {candyCount} candies, waiting for GridManager to fill");
                yield return null;
            }
        }
        Debug.Log($"GameStateManager: candyGrid initialized with {candyCount} candies");

        yield return StartCoroutine(ProcessMatches());
    }

    public IEnumerator ProcessMatches(List<Vector2Int> swapPositions = null)
    {
        Debug.Log($"GameStateManager: ProcessMatches called with swapPositions={(swapPositions != null ? string.Join(", ", swapPositions) : "null")}");
        while (true)
        {
            List<Candy> matches = MatchManager.Ins.CheckMatches(swapPositions);
            Debug.Log($"GameStateManager: matches={matches.Count}");
            if (matches.Count == 0) break;

            foreach (Candy candy in matches)
            {
                if (candy != null && candy.gameObject != null)
                {
                    StartCoroutine(ShrinkCandy(candy));
                }
            }
            SoundManager.Ins.MatchCandySound();
            yield return new WaitForSeconds(0.3f);

            ClearMatches(matches);
            TileManager.Ins.ProcessMatchEffects(matches);
            foreach (var special in MatchManager.Ins.GetSpecialCandies())
            {
                Vector2Int pos = special.pos;
                if (pos.x >= 0 && pos.x < GRID_HEIGHT && pos.y >= 0 && pos.y < GRID_WIDTH && !GridManager.Ins.IsEmptyTile(pos))
                {
                    GridManager.Ins.ReplaceWithSpecialCandy(pos.x, pos.y, special.type);
                }
            }
            yield return StartCoroutine(FillEmptySpaces());
            swapPositions = null; // Reset swapPositions sau khi lấp đầy lưới
        }

        if (!HasPossibleMoves() && !HasSpecialCandies())
        {
            Debug.Log("No valid moves or special candies remain. Clearing all candies.");
            yield return StartCoroutine(ClearAllCandies());
        }

        if (LevelManager.Ins.CheckWin())
        {
            Debug.Log("Level Complete!");
            UIManager.Ins.OpenWinPanel();
            SoundManager.Ins.WinSound();
            if (DataPlayer.GetLevelGame() == DataPlayer.GetUnlockLevelGame()) DataPlayer.SetUnlockLevelGame();
        }
        else if (LevelManager.Ins.CheckLose())
        {
            Debug.Log("Game Over");
            UIManager.Ins.OpenLosePanel();
            SoundManager.Ins.LoseSound();
        }
    }

    public IEnumerator ShrinkCandy(Candy candy)
    {
        if (candy == null || candy.gameObject == null) yield break;

        LevelManager.Ins.OnCandyMatched(candy);

        float shrinkTime = 0.3f;
        float elapsed = 0f;
        Transform candyTransform = candy.transform;
        Vector3 originalScale = candyTransform.localScale;

        while (elapsed < shrinkTime)
        {
            if (candy == null || candyTransform == null || candy.gameObject == null)
            {
                yield break;
            }
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkTime;
            candyTransform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }

        if (candy != null && candy.gameObject != null)
        {
            Debug.Log($"GameStateManager: Candy matched and removed: type={candy.type}, id={candy.GetInstanceID()}");
            Destroy(candy.gameObject);
        }
    }

    private void ClearMatches(List<Candy> matches)
    {
        foreach (Candy candy in matches)
        {
            if (candy != null)
            {
                candyGrid[candy.row, candy.column] = null;
            }
        }
    }

    public IEnumerator FillEmptySpaces()
    {
        yield return StartCoroutine(GridManager.Ins.FillEmptySpaces());
    }

    public bool HasPossibleMoves()
    {
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (col < GRID_WIDTH - 1)
                {
                    Candy candy1 = candyGrid[row, col];
                    Candy candy2 = candyGrid[row, col + 1];
                    if (candy1 != null && candy2 != null && !candy1.isLocked && !candy2.isLocked)
                    {
                        SwapCandies(row, col, row, col + 1);
                        if (MatchManager.Ins.CheckMatches().Count > 0)
                        {
                            SwapCandies(row, col, row, col + 1);
                            return true;
                        }
                        SwapCandies(row, col, row, col + 1);
                    }
                }

                if (row < GRID_HEIGHT - 1)
                {
                    Candy candy1 = candyGrid[row, col];
                    Candy candy2 = candyGrid[row + 1, col];
                    if (candy1 != null && candy2 != null && !candy1.isLocked && !candy2.isLocked)
                    {
                        SwapCandies(row, col, row + 1, col);
                        if (MatchManager.Ins.CheckMatches().Count > 0)
                        {
                            SwapCandies(row, col, row + 1, col);
                            return true;
                        }
                        SwapCandies(row, col, row + 1, col);
                    }
                }
            }
        }
        return false;
    }

    public bool HasSpecialCandies()
    {
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (candyGrid[row, col] != null && candyGrid[row, col].isSpecial)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void SwapCandies(int row1, int col1, int row2, int col2)
    {
        Candy temp = candyGrid[row1, col1];
        candyGrid[row1, col1] = candyGrid[row2, col2];
        candyGrid[row2, col2] = temp;

        if (candyGrid[row1, col1] != null)
        {
            candyGrid[row1, col1].row = row1;
            candyGrid[row1, col1].column = col1;
        }
        if (candyGrid[row2, col2] != null)
        {
            candyGrid[row2, col2].row = row2;
            candyGrid[row2, col2].column = col2;
        }
    }

    private IEnumerator ClearAllCandies()
    {
        List<Coroutine> shrinkCoroutines = new List<Coroutine>();
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (candyGrid[row, col] != null)
                {
                    shrinkCoroutines.Add(StartCoroutine(ShrinkCandy(candyGrid[row, col])));
                    candyGrid[row, col] = null;
                }
            }
        }
        foreach (var coroutine in shrinkCoroutines)
        {
            yield return coroutine;
        }      
        yield return StartCoroutine(FillEmptySpaces());     
        yield return StartCoroutine(ProcessMatches());    
    }
}