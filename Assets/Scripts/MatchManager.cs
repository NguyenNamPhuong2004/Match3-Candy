using UnityEngine;
using System.Collections.Generic;

public class MatchManager : Singleton<MatchManager>
{
    private Candy[,] candyGrid;
    private int GRID_WIDTH;
    private int GRID_HEIGHT;
    public List<Candy> lastMatches;
    private List<(Vector2Int pos, SpecialCandyType type)> specialCandies = new List<(Vector2Int, SpecialCandyType)>();

    protected override void ResetValue()
    {
        base.ResetValue();
        candyGrid = GridManager.Ins.candyGrid;
        GRID_WIDTH = GridManager.Ins.GRID_WIDTH;
        GRID_HEIGHT = GridManager.Ins.GRID_HEIGHT;
        lastMatches = new List<Candy>();
    }

    public List<Candy> CheckMatches(List<Vector2Int> swapPositions = null)
    {
        Debug.Log($"MatchManager: CheckMatches called with swapPositions={(swapPositions != null ? string.Join(", ", swapPositions) : "null")}");
        lastMatches.Clear();
        HashSet<Candy> processedCandies = new HashSet<Candy>();
        specialCandies.Clear();
        int candyCount = 0;
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (candyGrid[row, col] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)))
                {
                    candyCount++;
                }
            }
        }
        Debug.Log($"MatchManager: candyGrid initialized with {candyCount} candies");
        for (int row = 0; row < GRID_HEIGHT - 2; row++)
        {
            for (int col = 0; col < GRID_WIDTH - 2; col++)
            {
                CheckLShape(row, col, processedCandies);
                CheckTShape(row, col, processedCandies);
            }
        }
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
                    List<Vector2Int> matchPositions = new List<Vector2Int>();
                    for (int i = 0; i < matchLength; i++)
                    {
                        if (!GridManager.Ins.IsEmptyTile(new Vector2Int(row, col + i)))
                        {
                            Candy candy = candyGrid[row, col + i];
                            if (candy != null && !processedCandies.Contains(candy))
                            {
                                processedCandies.Add(candy);
                                lastMatches.Add(candy);
                                matchPositions.Add(new Vector2Int(row, col + i));
                            }
                        }
                    }
                    if (matchLength >= 5)
                    {
                        LevelManager.Ins.OnCandyMatched(candyGrid[row, col + 2]);
                        specialCandies.Add((new Vector2Int(row, col + 2), SpecialCandyType.ColorBomb));
                    }
                    else if (matchLength == 4)
                    {
                        Vector2Int specialPos = new Vector2Int(row, col + 1);
                        if (swapPositions != null)
                        {
                            foreach (var pos in swapPositions)
                            {
                                if (matchPositions.Contains(pos))
                                {
                                    specialPos = pos;
                                    break;
                                }
                            }
                        }
                        LevelManager.Ins.OnCandyMatched(candyGrid[specialPos.x, specialPos.y]);
                        specialCandies.Add((specialPos, SpecialCandyType.StripedVertical));
                        Debug.Log($"MatchManager: Added StripedVertical at {specialPos} for horizontal match");
                    }
                    col += matchLength;
                }
                else
                {
                    col++;
                }
            }
        }
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
                    List<Vector2Int> matchPositions = new List<Vector2Int>();
                    for (int i = 0; i < matchLength; i++)
                    {
                        if (!GridManager.Ins.IsEmptyTile(new Vector2Int(row + i, col)))
                        {
                            Candy candy = candyGrid[row + i, col];
                            if (candy != null && !processedCandies.Contains(candy))
                            {
                                processedCandies.Add(candy);
                                lastMatches.Add(candy);
                                matchPositions.Add(new Vector2Int(row + i, col));
                            }
                        }
                    }
                    if (matchLength >= 5)
                    {
                        LevelManager.Ins.OnCandyMatched(candyGrid[row + 2, col]);
                        specialCandies.Add((new Vector2Int(row + 2, col), SpecialCandyType.ColorBomb));
                    }
                    else if (matchLength == 4)
                    {
                        Vector2Int specialPos = new Vector2Int(row + 1, col);
                        if (swapPositions != null)
                        {
                            foreach (var pos in swapPositions)
                            {
                                if (matchPositions.Contains(pos))
                                {
                                    specialPos = pos;
                                    break;
                                }
                            }
                        }
                        LevelManager.Ins.OnCandyMatched(candyGrid[specialPos.x, specialPos.y]);
                        specialCandies.Add((specialPos, SpecialCandyType.StripedHorizontal));
                        Debug.Log($"MatchManager: Added StripedHorizontal at {specialPos} for vertical match");
                    }
                    row += matchLength;
                }
                else
                {
                    row++;
                }
            }
        }
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
        Debug.Log($"MatchManager: CheckMatches completed, matches={lastMatches.Count}, specialCandies={specialCandies.Count}");
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
                if (r < 0 || r >= GRID_HEIGHT || c < 0 || c >= GRID_WIDTH || candyGrid[r, c] == null || candyGrid[r, c].type != reference.type || GridManager.Ins.IsEmptyTile(new Vector2Int(r, c)) || candyGrid[r, c].type == CandyType.Special)
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
                if (r < 0 || r >= GRID_HEIGHT || c < 0 || c >= GRID_WIDTH || candyGrid[r, c] == null || candyGrid[r, c].type != reference.type || GridManager.Ins.IsEmptyTile(new Vector2Int(r, c)) || candyGrid[r, c].type == CandyType.Special)
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
               !GridManager.Ins.IsEmptyTile(new Vector2Int(row + 2 * rowOffset, col + 2 * colOffset)) &&
               candyGrid[row, col].type != CandyType.Special;
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