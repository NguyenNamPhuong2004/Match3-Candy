using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStateManager : Singleton<GameStateManager>
{
    private Candy[,] candyGrid;
    private const int GRID_WIDTH = 8;
    private const int GRID_HEIGHT = 8;

    public void Initialize()
    {
        candyGrid = GridManager.Ins.candyGrid;
        Debug.Log("GameStateManager initialized");
    }

    public IEnumerator ProcessMatches()
    {
        while (true)
        {
            List<Candy> matches = MatchManager.Ins.CheckMatches();
            Debug.Log($"ProcessMatches: matches={matches.Count}");
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
        }

        // Check if there are no possible moves and no special candies
        if (!HasPossibleMoves() && !HasSpecialCandies())
        {
            Debug.Log("No valid moves or special candies remain. Clearing all candies.");
            yield return StartCoroutine(ClearAllCandies());
        }

        // Check win/lose conditions after processing all matches
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

        // Count candy into matchedCandiesSet and matchedCandies
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
            Debug.Log($"Candy matched and removed: type={candy.type}, id={candy.GetInstanceID()}");
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

    // Check if there are any possible moves that can create a match
    public bool HasPossibleMoves()
    {
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                // Check swap with right neighbor
                if (col < GRID_WIDTH - 1)
                {
                    Candy candy1 = candyGrid[row, col];
                    Candy candy2 = candyGrid[row, col + 1];
                    if (candy1 != null && candy2 != null && !candy1.isLocked && !candy2.isLocked)
                    {
                        SwapCandies(row, col, row, col + 1);
                        if (MatchManager.Ins.CheckMatches().Count > 0)
                        {
                            SwapCandies(row, col, row, col + 1); // Swap back
                            return true;
                        }
                        SwapCandies(row, col, row, col + 1); // Swap back
                    }
                }
                // Check swap with bottom neighbor
                if (row < GRID_HEIGHT - 1)
                {
                    Candy candy1 = candyGrid[row, col];
                    Candy candy2 = candyGrid[row + 1, col];
                    if (candy1 != null && candy2 != null && !candy1.isLocked && !candy2.isLocked)
                    {
                        SwapCandies(row, col, row + 1, col);
                        if (MatchManager.Ins.CheckMatches().Count > 0)
                        {
                            SwapCandies(row, col, row + 1, col); // Swap back
                            return true;
                        }
                        SwapCandies(row, col, row + 1, col); // Swap back
                    }
                }
            }
        }
        return false;
    }

    // Check if there are any special candies on the grid
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

    // Swap two candies in the grid
    private void SwapCandies(int row1, int col1, int row2, int col2)
    {
        Candy temp = candyGrid[row1, col1];
        candyGrid[row1, col1] = candyGrid[row2, col2];
        candyGrid[row2, col2] = temp;

        // Update candy positions
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

    // Clear all candies from the grid
    private IEnumerator ClearAllCandies()
    {
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (candyGrid[row, col] != null)
                {
                    yield return StartCoroutine(ShrinkCandy(candyGrid[row, col]));
                    Destroy(candyGrid[row, col].gameObject);
                    candyGrid[row, col] = null;
                }
            }
        }
        yield return StartCoroutine(FillEmptySpaces());
    }
}