using UnityEngine;
using System.Collections;

public class SpecialCandyManager : Singleton<SpecialCandyManager>
{
    private Candy[,] candyGrid;
    private const int GRID_WIDTH = 8;
    private const int GRID_HEIGHT = 8;

    public void Initialize()
    {
        candyGrid = GridManager.Ins.candyGrid;
        Debug.Log("SpecialCandyManager initialized");
    }

    public IEnumerator ProcessSpecialCandy(Candy candy1, Candy candy2)
    {
        if (candy1.isSpecial && candy2.isSpecial)
        {
            yield return StartCoroutine(ClearAll());
        }
        else if (candy1.isSpecial)
        {
            yield return StartCoroutine(ExecuteSpecialCandy(candy1, candy2));
        }
        else if (candy2.isSpecial)
        {
            yield return StartCoroutine(ExecuteSpecialCandy(candy2, candy1));
        }
    }

    private IEnumerator ExecuteSpecialCandy(Candy special, Candy normal)
    {
        switch (special.specialType)
        {
            case SpecialCandyType.StripedHorizontal:
                yield return StartCoroutine(ClearRow(special.row));
                break;
            case SpecialCandyType.StripedVertical:
                yield return StartCoroutine(ClearColumn(special.column));
                break;
            case SpecialCandyType.Wrapped:
                yield return StartCoroutine(ClearArea(special.row, special.column));
                break;
            case SpecialCandyType.ColorBomb:
                yield return StartCoroutine(ClearColorType(normal.type));
                break;
        }
    }

    public IEnumerator ClearRow(int row)
    {
        for (int col = 0; col < GRID_WIDTH; col++)
        {
            if (candyGrid[row, col] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)))
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[row, col]));
                candyGrid[row, col] = null;
            }
        }
        yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
    }

    public IEnumerator ClearColumn(int col)
    {
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            if (candyGrid[row, col] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)))
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[row, col]));
                candyGrid[row, col] = null;
            }
        }
        yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
    }

    public IEnumerator ClearArea(int row, int col)
    {
        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                if (r >= 0 && r < GRID_HEIGHT && c >= 0 && c < GRID_WIDTH && candyGrid[r, c] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(r, c)))
                {
                    yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[r, c]));
                    candyGrid[r, c] = null;
                }
            }
        }
        yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
    }

    public IEnumerator ClearLargeArea(int row, int col)
    {
        for (int r = row - 2; r <= row + 2; r++)
        {
            for (int c = col - 2; c <= col + 2; c++)
            {
                if (r >= 0 && r < GRID_HEIGHT && c >= 0 && c < GRID_WIDTH && candyGrid[r, c] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(r, c)))
                {
                    yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[r, c]));
                    candyGrid[r, c] = null;
                }
            }
        }
        yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
    }

    public IEnumerator ClearMultipleRows(int centerRow)
    {
        for (int row = centerRow - 1; row <= centerRow + 1; row++)
        {
            if (row >= 0 && row < GRID_HEIGHT)
            {
                yield return StartCoroutine(ClearRow(row));
            }
        }
    }

    public IEnumerator ClearMultipleColumns(int centerCol)
    {
        for (int col = centerCol - 1; col <= centerCol + 1; col++)
        {
            if (col >= 0 && col < GRID_WIDTH)
            {
                yield return StartCoroutine(ClearColumn(col));
            }
        }
    }

    public IEnumerator ClearColorType(CandyType type)
    {
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (candyGrid[row, col] != null && candyGrid[row, col].type == type && !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)))
                {
                    yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[row, col]));
                    candyGrid[row, col] = null;
                }
            }
        }
        yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
    }

    public IEnumerator ClearAll()
    {
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (candyGrid[row, col] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)))
                {
                    yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[row, col]));
                    candyGrid[row, col] = null;
                }
            }
        }
        yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
    }
}