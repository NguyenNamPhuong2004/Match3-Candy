using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        Debug.Log($"ProcessSpecialCandy: candy1={candy1.specialType}, candy2={candy2.specialType}");
        if (candy1.isSpecial && candy2.isSpecial)
        {
            if (candy1.specialType == SpecialCandyType.ColorBomb && candy2.specialType == SpecialCandyType.ColorBomb)
            {
                yield return StartCoroutine(ClearAll());
            }
            else if (candy1.specialType == SpecialCandyType.ColorBomb)
            {
                yield return StartCoroutine(ExecuteSpecialCombo(candy2));
            }
            else if (candy2.specialType == SpecialCandyType.ColorBomb)
            {
                yield return StartCoroutine(ExecuteSpecialCombo(candy1));
            }
            else
            {
                yield return StartCoroutine(ExecuteSpecialPair(candy1, candy2));
            }
            if (candyGrid[candy1.row, candy1.column] != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[candy1.row, candy1.column]));
                candyGrid[candy1.row, candy1.column] = null;
            }
            if (candyGrid[candy2.row, candy2.column] != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[candy2.row, candy2.column]));
                candyGrid[candy2.row, candy2.column] = null;
            }
        }
        else if (candy1.isSpecial)
        {
            yield return StartCoroutine(ExecuteSpecialCandy(candy1, candy2));
            if (candyGrid[candy1.row, candy1.column] != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[candy1.row, candy1.column]));
                candyGrid[candy1.row, candy1.column] = null;
            }
        }
        else if (candy2.isSpecial)
        {
            yield return StartCoroutine(ExecuteSpecialCandy(candy2, candy1));
            if (candyGrid[candy2.row, candy2.column] != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[candy2.row, candy2.column]));
                candyGrid[candy2.row, candy2.column] = null;
            }
        }
        yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
        yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
    }

    private IEnumerator ExecuteSpecialCandy(Candy special, Candy normal)
    {
        switch (special.specialType)
        {
            case SpecialCandyType.StripedHorizontal:
                yield return StartCoroutine(ClearRow(special.row, true));
                break;
            case SpecialCandyType.StripedVertical:
                yield return StartCoroutine(ClearColumn(special.column, true));
                break;
            case SpecialCandyType.Wrapped:
                yield return StartCoroutine(ClearArea(special.row, special.column, true));
                break;
            case SpecialCandyType.ColorBomb:
                yield return StartCoroutine(ClearColorType(normal.type, true));
                break;
        }
    }

    private IEnumerator ExecuteSpecialCombo(Candy other)
    {
        if (other.isSpecial)
        {
            switch (other.specialType)
            {
                case SpecialCandyType.StripedHorizontal:
                case SpecialCandyType.StripedVertical:
                    yield return StartCoroutine(ActivateStripedCandies(other.type));
                    break;
                case SpecialCandyType.Wrapped:
                    yield return StartCoroutine(ActivateWrappedCandies(other.type));
                    break;
                default:
                    yield break;
            }
        }
        else
        {
            yield return StartCoroutine(ClearColorType(other.type, true));
        }
    }

    private IEnumerator ExecuteSpecialPair(Candy candy1, Candy candy2)
    {
        if ((candy1.specialType == SpecialCandyType.StripedHorizontal || candy1.specialType == SpecialCandyType.StripedVertical) &&
            (candy2.specialType == SpecialCandyType.StripedHorizontal || candy2.specialType == SpecialCandyType.StripedVertical))
        {
            yield return StartCoroutine(ClearRow(candy1.row, false));
            yield return StartCoroutine(ClearColumn(candy2.column, false));
            yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        }
        else if (candy1.specialType == SpecialCandyType.Wrapped &&
                 (candy2.specialType == SpecialCandyType.StripedHorizontal || candy2.specialType == SpecialCandyType.StripedVertical))
        {
            yield return StartCoroutine(ClearLargeArea(candy1.row, candy1.column, false));
            yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        }
        else if (candy2.specialType == SpecialCandyType.Wrapped &&
                 (candy1.specialType == SpecialCandyType.StripedHorizontal || candy1.specialType == SpecialCandyType.StripedVertical))
        {
            yield return StartCoroutine(ClearLargeArea(candy2.row, candy2.column, false));
            yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        }
        else if (candy1.specialType == SpecialCandyType.Wrapped && candy2.specialType == SpecialCandyType.Wrapped)
        {
            yield return StartCoroutine(ClearLargeArea(candy1.row, candy1.column, false));
            yield return StartCoroutine(ClearLargeArea(candy2.row, candy2.column, false));
            yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        }
    }

    public IEnumerator ClearRow(int row, bool postProcess = true)
    {
        SoundManager.Ins.StrippedSound();
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int col = 0; col < GRID_WIDTH; col++)
        {
            if (candyGrid[row, col] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)))
            {
                Candy candy = candyGrid[row, col];
                if (candy.isSpecial)
                {
                    specialCandiesToTrigger.Add(candy);
                }
                else
                {
                    candiesToShrink.Add(candy);
                }
                candyGrid[row, col] = null;
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(TriggerSpecialCandyEffect(candy));
            }
        }

        if (postProcess)
        {
            yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        }
    }

    public IEnumerator ClearColumn(int col, bool postProcess = true)
    {
        SoundManager.Ins.StrippedSound();
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            if (candyGrid[row, col] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)))
            {
                Candy candy = candyGrid[row, col];
                if (candy.isSpecial)
                {
                    specialCandiesToTrigger.Add(candy);
                }
                else
                {
                    candiesToShrink.Add(candy);
                }
                candyGrid[row, col] = null;
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(TriggerSpecialCandyEffect(candy));
            }
        }

        if (postProcess)
        {
            yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        }
    }

    public IEnumerator ClearArea(int row, int col, bool postProcess = true)
    {
        SoundManager.Ins.WrappedSound();
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                if (r >= 0 && r < GRID_HEIGHT && c >= 0 && c < GRID_WIDTH &&
                    candyGrid[r, c] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(r, c)))
                {
                    Candy candy = candyGrid[r, c];
                    if (candy.isSpecial)
                    {
                        specialCandiesToTrigger.Add(candy);
                    }
                    else
                    {
                        candiesToShrink.Add(candy);
                    }
                    candyGrid[r, c] = null;
                }
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(TriggerSpecialCandyEffect(candy));
            }
        }

        if (postProcess)
        {
            yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        }
    }

    public IEnumerator ClearLargeArea(int row, int col, bool postProcess = true)
    {
        SoundManager.Ins.WrappedSound();
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int r = row - 2; r <= row + 2; r++)
        {
            for (int c = col - 2; c <= col + 2; c++)
            {
                if (r >= 0 && r < GRID_HEIGHT && c >= 0 && c < GRID_WIDTH &&
                    candyGrid[r, c] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(r, c)))
                {
                    Candy candy = candyGrid[r, c];
                    if (candy.isSpecial)
                    {
                        specialCandiesToTrigger.Add(candy);
                    }
                    else
                    {
                        candiesToShrink.Add(candy);
                    }
                    candyGrid[r, c] = null;
                }
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(TriggerSpecialCandyEffect(candy));
            }
        }

        if (postProcess)
        {
            yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        }
    }

    public IEnumerator ClearMultipleRows(int centerRow)
    {
        for (int row = centerRow - 1; row <= centerRow + 1; row++)
        {
            if (row >= 0 && row < GRID_HEIGHT)
            {
                yield return StartCoroutine(ClearRow(row, false));
            }
        }
        yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
        yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
    }

    public IEnumerator ClearMultipleColumns(int centerCol)
    {
        for (int col = centerCol - 1; col <= centerCol + 1; col++)
        {
            if (col >= 0 && col < GRID_WIDTH)
            {
                yield return StartCoroutine(ClearColumn(col, false));
            }
        }
        yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
        yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
    }

    public IEnumerator ClearColorType(CandyType type, bool postProcess = true)
    {
        SoundManager.Ins.ColorBombSound();
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (candyGrid[row, col] != null && candyGrid[row, col].type == type &&
                    !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)))
                {
                    Candy candy = candyGrid[row, col];
                    if (candy.isSpecial)
                    {
                        specialCandiesToTrigger.Add(candy);
                    }
                    else
                    {
                        candiesToShrink.Add(candy);
                    }
                    candyGrid[row, col] = null;
                }
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(TriggerSpecialCandyEffect(candy));
            }
        }

        if (postProcess)
        {
            yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        }
    }

    public IEnumerator ClearAll(bool postProcess = true)
    {
        SoundManager.Ins.ColorBombSound();
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (candyGrid[row, col] != null && !GridManager.Ins.IsEmptyTile(new Vector2Int(row, col)))
                {
                    Candy candy = candyGrid[row, col];
                    if (candy.isSpecial)
                    {
                        specialCandiesToTrigger.Add(candy);
                    }
                    else
                    {
                        candiesToShrink.Add(candy);
                    }
                    candyGrid[row, col] = null;
                }
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(TriggerSpecialCandyEffect(candy));
            }
        }

        if (postProcess)
        {
            yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        }
    }

    private IEnumerator ActivateStripedCandies(CandyType type)
    {
        List<(int row, int col)> positions = new List<(int, int)>();
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (candyGrid[row, col] != null && candyGrid[row, col].type == type)
                {
                    positions.Add((row, col));
                }
            }
        }

        foreach (var pos in positions)
        {
            if (candyGrid[pos.row, pos.col] != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[pos.row, pos.col]));
                candyGrid[pos.row, pos.col] = null;
                SpecialCandyType stripedType = Random.Range(0, 2) == 0 ?
                    SpecialCandyType.StripedHorizontal : SpecialCandyType.StripedVertical;
                yield return StartCoroutine(GridManager.Ins.SpawnCandy(pos.row, pos.col, false, stripedType));
                if (candyGrid[pos.row, pos.col] != null)
                {
                    yield return StartCoroutine(TriggerSpecialCandyEffect(candyGrid[pos.row, pos.col]));
                }
            }
        }
    }

    private IEnumerator ActivateWrappedCandies(CandyType type)
    {
        List<(int row, int col)> positions = new List<(int, int)>();
        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (candyGrid[row, col] != null && candyGrid[row, col].type == type)
                {
                    positions.Add((row, col));
                }
            }
        }

        foreach (var pos in positions)
        {
            if (candyGrid[pos.row, pos.col] != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[pos.row, pos.col]));
                candyGrid[pos.row, pos.col] = null;
                yield return StartCoroutine(GridManager.Ins.SpawnCandy(pos.row, pos.col, false, SpecialCandyType.Wrapped));
                if (candyGrid[pos.row, pos.col] != null)
                {
                    yield return StartCoroutine(TriggerSpecialCandyEffect(candyGrid[pos.row, pos.col]));
                }
            }
        }
    }

    private IEnumerator TriggerSpecialCandyEffect(Candy candy)
    {
        Debug.Log($"TriggerSpecialCandyEffect: type={candy.specialType}, pos=({candy.row},{candy.column})");
        switch (candy.specialType)
        {
            case SpecialCandyType.StripedHorizontal:
                yield return StartCoroutine(ClearRow(candy.row, false));
                break;
            case SpecialCandyType.StripedVertical:
                yield return StartCoroutine(ClearColumn(candy.column, false));
                break;
            case SpecialCandyType.Wrapped:
                yield return StartCoroutine(ClearArea(candy.row, candy.column, false));
                break;
            case SpecialCandyType.ColorBomb:
                CandyType[] types = (CandyType[])System.Enum.GetValues(typeof(CandyType));
                yield return StartCoroutine(ClearColorType(types[Random.Range(0, types.Length)], false));
                break;
        }
        // Phá kẹo đặc biệt sau khi kích hoạt hiệu ứng
        if (candyGrid[candy.row, candy.column] != null)
        {
            yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyGrid[candy.row, candy.column]));
            candyGrid[candy.row, candy.column] = null;
        }
    }
}