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

        // Kiểm tra win/lose sau khi xử lý toàn bộ match
        if (LevelManager.Ins.CheckWin())
        {
            Debug.Log("Level Complete!");
            LevelManager.Ins.NextLevel();
            if (LevelManager.Ins.currentLevel == null)
            {
                Debug.Log("Game Completed All Levels!");
                yield break;
            }
            yield return StartCoroutine(GameManager.Ins.InitializeLevel());
        }
        else if (LevelManager.Ins.CheckLose())
        {
            Debug.Log("Game Over");
            yield break;
        }
    }

    public IEnumerator ShrinkCandy(Candy candy)
    {
        if (candy == null || candy.gameObject == null) yield break;

        // Đếm kẹo vào matchedCandiesSet và matchedCandies
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
}