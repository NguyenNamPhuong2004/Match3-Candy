using UnityEngine;
using System.Collections.Generic;

public class MatchManager : Singleton<MatchManager>
{
    private Candy[,] candyGrid;
    private const int GRID_WIDTH = 8;
    private const int GRID_HEIGHT = 8;
    public List<Candy> lastMatches;
    private List<(Vector2Int pos, SpecialCandyType type)> specialCandies = new List<(Vector2Int, SpecialCandyType)>();

    public void Initialize()
    {
        candyGrid = GridManager.Ins.candyGrid;
        lastMatches = new List<Candy>();
        Debug.Log("MatchManager initialized");
    }

    public List<Candy> CheckMatches()
    {
        lastMatches.Clear();
        HashSet<Candy> processedCandies = new HashSet<Candy>();
        specialCandies.Clear();

        // Check ngang
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            int col = 0;
            while (col < GRID_WIDTH - 2)
            {
                if (IsValidMatch3(row, col, 0, 1))
                {
                    int matchLength = 3;
                    while (col + matchLength < GRID_WIDTH && IsValidMatchMore3(row, col, 0, matchLength))
                    {
                        matchLength++;
                    }
                    for (int i = 0; i < matchLength; i++)
                    {
                        if (!GridManager.Ins.IsEmptyTile(new Vector2Int(row, col + i)))
                        {
                            Candy candy = candyGrid[row, col + i];
                            if (candy != null && !processedCandies.Contains(candy))
                            {
                                processedCandies.Add(candy);
                                lastMatches.Add(candy);
                            }
                        }
                    }
                    if (matchLength >= 5)
                    {
                        specialCandies.Add((new Vector2Int(row, col + 2), SpecialCandyType.ColorBomb));
                    }
                    else if (matchLength == 4)
                    {
                        specialCandies.Add((new Vector2Int(row, col + 1), SpecialCandyType.StripedVertical));
                    }
                    col += matchLength;
                }
                else
                {
                    col++;
                }
            }
        }

        // Check dọc
        for (int col = 0; col < GRID_WIDTH; col++)
        {
            int row = 0;
            while (row < GRID_HEIGHT - 2)
            {
                if (IsValidMatch3(row, col, 1, 0))
                {
                    int matchLength = 3;
                    while (row + matchLength < GRID_HEIGHT && IsValidMatchMore3(row, col, matchLength, 0))
                    {
                        matchLength++;
                    }
                    for (int i = 0; i < matchLength; i++)
                    {
                        if (!GridManager.Ins.IsEmptyTile(new Vector2Int(row + i, col)))
                        {
                            Candy candy = candyGrid[row + i, col];
                            if (candy != null && !processedCandies.Contains(candy))
                            {
                                processedCandies.Add(candy);
                                lastMatches.Add(candy);
                            }
                        }
                    }
                    if (matchLength >= 5)
                    {
                        specialCandies.Add((new Vector2Int(row + 2, col), SpecialCandyType.ColorBomb));
                    }
                    else if (matchLength == 4)
                    {
                        specialCandies.Add((new Vector2Int(row + 1, col), SpecialCandyType.StripedHorizontal));
                    }
                    row += matchLength;
                }
                else
                {
                    row++;
                }
            }
        }

        // Check L và T
        for (int row = 0; row < GRID_HEIGHT - 2; row++)
        {
            for (int col = 0; col < GRID_WIDTH - 2; col++)
            {
                CheckLShape(row, col, processedCandies);
                CheckTShape(row, col, processedCandies);
            }
        }

        // Cập nhật UI
        UIManager.Ins.UpdateUI();

        foreach (var special in specialCandies)
        {
            Vector2Int pos = special.pos;
            if (pos.x >= 0 && pos.x < GRID_HEIGHT && pos.y >= 0 && pos.y < GRID_WIDTH && candyGrid[pos.x, pos.y] != null)
            {
                Candy candy = candyGrid[pos.x, pos.y];
                if (lastMatches.Contains(candy) && candy != null && candy.gameObject != null)
                {
                    lastMatches.Remove(candy);
                    candy.isMatched = false;
                }
            }
        }

        Debug.Log($"CheckMatches: matches={lastMatches.Count}, specialCandies={specialCandies.Count}");
        return lastMatches;
    }

    private bool CheckLShape(int row, int col, HashSet<Candy> matches)
    {
        if (row + 2 >= GRID_HEIGHT || col + 2 >= GRID_WIDTH) return false;

        bool TryMatchPattern(int[] rows, int[] cols, Vector2Int wrappedPos)
        {
            Candy reference = candyGrid[row + rows[0], col + cols[0]];
            if (reference == null || GridManager.Ins.IsEmptyTile(new Vector2Int(row + rows[0], col + cols[0]))) return false;

            for (int i = 1; i < 5; i++)
            {
                int r = row + rows[i];
                int c = col + cols[i];
                if (r < 0 || r >= GRID_HEIGHT || c < 0 || c < GRID_WIDTH || candyGrid[r, c] == null || candyGrid[r, c].type != reference.type || GridManager.Ins.IsEmptyTile(new Vector2Int(r, c)))
                    return false;
            }
            for (int i = 0; i < 5; i++)
            {
                int r = row + rows[i];
                int c = col + cols[i];
                Candy candy = candyGrid[r, c];
                if (candy != null && !matches.Contains(candy))
                {
                    matches.Add(candy);
                    candy.isMatched = true;
                    lastMatches.Add(candy);
                }
            }
            specialCandies.Add((new Vector2Int(row + wrappedPos.x, col + wrappedPos.y), SpecialCandyType.Wrapped));
            return true;
        }

        int[] l1Rows = { 0, 1, 2, 2, 2 };
        int[] l1Cols = { 2, 2, 0, 1, 2 };
        if (TryMatchPattern(l1Rows, l1Cols, new Vector2Int(2, 2))) return true;

        int[] l2Rows = { 0, 0, 0, 1, 2 };
        int[] l2Cols = { 0, 1, 2, 2, 2 };
        if (TryMatchPattern(l2Rows, l2Cols, new Vector2Int(0, 2))) return true;

        int[] l3Rows = { 0, 0, 0, 1, 2 };
        int[] l3Cols = { 0, 1, 2, 0, 0 };
        if (TryMatchPattern(l3Rows, l3Cols, new Vector2Int(0, 0))) return true;

        int[] l4Rows = { 0, 1, 2, 2, 2 };
        int[] l4Cols = { 0, 0, 0, 1, 2 };
        if (TryMatchPattern(l4Rows, l4Cols, new Vector2Int(2, 0))) return true;

        return false;
    }

    private bool CheckTShape(int row, int col, HashSet<Candy> matches)
    {
        if (row + 2 >= GRID_HEIGHT || col + 2 >= GRID_WIDTH) return false;

        bool TryMatchPattern(int[] rows, int[] cols, Vector2Int wrappedPos)
        {
            Candy reference = candyGrid[row + rows[0], col + cols[0]];
            if (reference == null || GridManager.Ins.IsEmptyTile(new Vector2Int(row + rows[0], col + cols[0]))) return false;

            for (int i = 1; i < 5; i++)
            {
                int r = row + rows[i];
                int c = col + cols[i];
                if (r < 0 || r >= GRID_HEIGHT || c < 0 || c < GRID_WIDTH || candyGrid[r, c] == null || candyGrid[r, c].type != reference.type || GridManager.Ins.IsEmptyTile(new Vector2Int(r, c)))
                    return false;
            }
            for (int i = 0; i < 5; i++)
            {
                int r = row + rows[i];
                int c = col + cols[i];
                Candy candy = candyGrid[r, c];
                if (candy != null && !matches.Contains(candy))
                {
                    matches.Add(candy);
                    candy.isMatched = true;
                    lastMatches.Add(candy);
                }
            }
            specialCandies.Add((new Vector2Int(row + wrappedPos.x, col + wrappedPos.y), SpecialCandyType.Wrapped));
            return true;
        }

        int[] t1Rows = { 0, 0, 0, 1, 2 };
        int[] t1Cols = { 0, 1, 2, 1, 1 };
        if (TryMatchPattern(t1Rows, t1Cols, new Vector2Int(0, 1))) return true;

        int[] t2Rows = { 2, 2, 2, 1, 0 };
        int[] t2Cols = { 0, 1, 2, 1, 1 };
        if (TryMatchPattern(t2Rows, t2Cols, new Vector2Int(2, 1))) return true;

        int[] t3Rows = { 0, 1, 1, 1, 2 };
        int[] t3Cols = { 2, 0, 1, 2, 2 };
        if (TryMatchPattern(t3Rows, t3Cols, new Vector2Int(1, 2))) return true;

        int[] t4Rows = { 0, 1, 1, 1, 2 };
        int[] t4Cols = { 0, 0, 1, 2, 0 };
        if (TryMatchPattern(t4Rows, t4Cols, new Vector2Int(1, 0))) return true;

        return false;
    }

    private bool IsValidMatch3(int row, int col, int rowOffset, int colOffset)
    {
        if (row + 2 * rowOffset >= GRID_HEIGHT || col + 2 * colOffset >= GRID_WIDTH || row < 0 || col < 0)
            return false;

        Candy candy1 = candyGrid[row, col];
        Candy candy2 = candyGrid[row + rowOffset, col + colOffset];
        Candy candy3 = candyGrid[row + 2 * rowOffset, col + 2 * colOffset];

        return candy1 != null && candy2 != null && candy3 != null &&
               candy1.type == candy2.type && candy1.type == candy3.type &&
               !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)) &&
               !GridManager.Ins.IsEmptyTile(new Vector2Int(row + rowOffset, col + colOffset)) &&
               !GridManager.Ins.IsEmptyTile(new Vector2Int(row + 2 * rowOffset, col + 2 * colOffset));
    }

    private bool IsValidMatchMore3(int row, int col, int rowOffset, int colOffset)
    {
        if (row + rowOffset >= GRID_HEIGHT || col + colOffset >= GRID_WIDTH || row < 0 || col < 0)
            return false;

        Candy candy1 = candyGrid[row, col];
        Candy candy2 = candyGrid[row + rowOffset, col + colOffset];

        return candy1 != null && candy2 != null &&
               candy1.type == candy2.type &&
               !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)) &&
               !GridManager.Ins.IsEmptyTile(new Vector2Int(row + rowOffset, col + colOffset));
    }

    public List<(Vector2Int pos, SpecialCandyType type)> GetSpecialCandies()
    {
        return specialCandies;
    }
}