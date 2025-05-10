using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpecialCandyManager : Singleton<SpecialCandyManager>
{
    private Candy[,] candyGrid;
    public GameObject[] specialCandyPrefabs;
    private int GRID_WIDTH;
    private int GRID_HEIGHT;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadSpecialCandyPrefabs();
    }

    protected override void ResetValue()
    {
        base.ResetValue();
        candyGrid = GridManager.Ins.candyGrid;
        GRID_WIDTH = GridManager.Ins.GRID_WIDTH;
        GRID_HEIGHT = GridManager.Ins.GRID_HEIGHT;
    }

    protected virtual void LoadSpecialCandyPrefabs()
    {
        this.specialCandyPrefabs = Resources.LoadAll<GameObject>("Prefabs/Candy/SpecialCandy");
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
            if (candy1 != null && candy1.gameObject != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy1));
                Destroy(candy1.gameObject);
                candyGrid[candy1.row, candy1.column] = null;
            }
            if (candy2 != null && candy2.gameObject != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy2));
                Destroy(candy2.gameObject);
                candyGrid[candy2.row, candy2.column] = null;
            }
        }
        else if (candy1.isSpecial)
        {
            yield return StartCoroutine(ExecuteSpecialCandy(candy1, candy2));
            if (candy1 != null && candy1.gameObject != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy1));
                Destroy(candy1.gameObject);
                candyGrid[candy1.row, candy1.column] = null;
            }
        }
        else if (candy2.isSpecial)
        {
            yield return StartCoroutine(ExecuteSpecialCandy(candy2, candy1));
            if (candy2 != null && candy2.gameObject != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy2));
                Destroy(candy2.gameObject);
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
            CandyType[] types = (CandyType[])System.Enum.GetValues(typeof(CandyType));
            switch (other.specialType)
            {
                case SpecialCandyType.StripedHorizontal:
                case SpecialCandyType.StripedVertical:
                    yield return StartCoroutine(ActivateStripedCandies(types[Random.Range(0, types.Length)]));
                    break;
                case SpecialCandyType.Wrapped:
                    yield return StartCoroutine(ActivateWrappedCandies(types[Random.Range(0, types.Length)]));
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
        List<Vector2Int> clearedPositions = new List<Vector2Int>();

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
                clearedPositions.Add(new Vector2Int(row, col));
                candyGrid[row, col] = null;
                Debug.Log($"Cleared candy at ({row}, {col})");
            }
        }

        foreach (var pos in clearedPositions)
        {
            if (LevelManager.Ins.currentLevel.dirtTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnDirtTileCleared(pos);
            }
            if (LevelManager.Ins.currentLevel.lockedTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnLockedTileCleared(pos);
            }
        }

        List<Coroutine> shrinkCoroutines = new List<Coroutine>();
        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                shrinkCoroutines.Add(StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy)));
                Debug.Log($"Started shrinking normal candy at ({candy.row}, {candy.column})");
            }
        }
        foreach (Coroutine coroutine in shrinkCoroutines)
        {
            yield return coroutine;
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                Destroy(candy.gameObject);
                Debug.Log($"Destroyed normal candy at ({candy.row}, {candy.column})");
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(TriggerSpecialCandyEffect(candy));
                Debug.Log($"Triggered special candy at ({candy.row}, {candy.column})");
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
        List<Vector2Int> clearedPositions = new List<Vector2Int>();

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
                clearedPositions.Add(new Vector2Int(row, col));
                candyGrid[row, col] = null;
            }
        }

        foreach (var pos in clearedPositions)
        {
            if (LevelManager.Ins.currentLevel.dirtTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnDirtTileCleared(pos);
            }
            if (LevelManager.Ins.currentLevel.lockedTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnLockedTileCleared(pos);
            }
        }

        List<Coroutine> shrinkCoroutines = new List<Coroutine>();
        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                shrinkCoroutines.Add(StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy)));
            }
        }
        foreach (Coroutine coroutine in shrinkCoroutines)
        {
            yield return coroutine;
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                Destroy(candy.gameObject);
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
        List<Vector2Int> clearedPositions = new List<Vector2Int>();

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
                    clearedPositions.Add(new Vector2Int(r, c));
                    candyGrid[r, c] = null;
                }
            }
        }

        foreach (var pos in clearedPositions)
        {
            if (LevelManager.Ins.currentLevel.dirtTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnDirtTileCleared(pos);
            }
            if (LevelManager.Ins.currentLevel.lockedTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnLockedTileCleared(pos);
            }
        }

        List<Coroutine> shrinkCoroutines = new List<Coroutine>();
        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                shrinkCoroutines.Add(StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy)));
            }
        }
        foreach (Coroutine coroutine in shrinkCoroutines)
        {
            yield return coroutine;
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                Destroy(candy.gameObject);
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
        List<Vector2Int> clearedPositions = new List<Vector2Int>();

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
                    clearedPositions.Add(new Vector2Int(r, c));
                    candyGrid[r, c] = null;
                }
            }
        }

        foreach (var pos in clearedPositions)
        {
            if (LevelManager.Ins.currentLevel.dirtTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnDirtTileCleared(pos);
            }
            if (LevelManager.Ins.currentLevel.lockedTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnLockedTileCleared(pos);
            }
        }

        List<Coroutine> shrinkCoroutines = new List<Coroutine>();
        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                shrinkCoroutines.Add(StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy)));
            }
        }
        foreach (Coroutine coroutine in shrinkCoroutines)
        {
            yield return coroutine;
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                Destroy(candy.gameObject);
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
        List<Vector2Int> clearedPositions = new List<Vector2Int>();

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
                    clearedPositions.Add(new Vector2Int(row, col));
                    candyGrid[row, col] = null;
                }
            }
        }

        foreach (var pos in clearedPositions)
        {
            if (LevelManager.Ins.currentLevel.dirtTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnDirtTileCleared(pos);
            }
            if (LevelManager.Ins.currentLevel.lockedTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnLockedTileCleared(pos);
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy));
                Destroy(candy.gameObject);
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
        List<Vector2Int> clearedPositions = new List<Vector2Int>();

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
                    clearedPositions.Add(new Vector2Int(row, col));
                    candyGrid[row, col] = null;
                }
            }
        }

        foreach (var pos in clearedPositions)
        {
            if (LevelManager.Ins.currentLevel.dirtTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnDirtTileCleared(pos);
            }
            if (LevelManager.Ins.currentLevel.lockedTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnLockedTileCleared(pos);
            }
        }

        List<Coroutine> shrinkCoroutines = new List<Coroutine>();
        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                shrinkCoroutines.Add(StartCoroutine(GameStateManager.Ins.ShrinkCandy(candy)));
            }
        }
        foreach (Coroutine coroutine in shrinkCoroutines)
        {
            yield return coroutine;
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                Destroy(candy.gameObject);
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
        for (int row = 00; row < GRID_HEIGHT; row++)
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
                LevelManager.Ins.OnCandyMatched(candyGrid[pos.row, pos.col]);
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
                LevelManager.Ins.OnCandyMatched(candyGrid[pos.row, pos.col]);
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
        Candy candyToDestroy = candy;
        switch (candyToDestroy.specialType)
        {
            case SpecialCandyType.StripedHorizontal:
                yield return StartCoroutine(ClearRow(candyToDestroy.row, false));
                break;
            case SpecialCandyType.StripedVertical:
                yield return StartCoroutine(ClearColumn(candyToDestroy.column, false));
                break;
            case SpecialCandyType.Wrapped:
                yield return StartCoroutine(ClearArea(candyToDestroy.row, candyToDestroy.column, false));
                break;
            case SpecialCandyType.ColorBomb:
                CandyType[] types = (CandyType[])System.Enum.GetValues(typeof(CandyType));
                yield return StartCoroutine(ClearColorType(types[Random.Range(0, types.Length)], false));
                break;
        }
        if (candyToDestroy != null && candyToDestroy.gameObject != null)
        {
            yield return StartCoroutine(GameStateManager.Ins.ShrinkCandy(candyToDestroy));
            Destroy(candyToDestroy.gameObject);
            Debug.Log($"Destroyed special candy at ({candyToDestroy.row}, {candyToDestroy.column})");
        }
        if (candyToDestroy.row >= 0 && candyToDestroy.row < GRID_HEIGHT &&
            candyToDestroy.column >= 0 && candyToDestroy.column < GRID_WIDTH)
        {
            candyGrid[candyToDestroy.row, candyToDestroy.column] = null;
            Vector2Int pos = new Vector2Int(candyToDestroy.row, candyToDestroy.column);
            if (LevelManager.Ins.currentLevel.dirtTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnDirtTileCleared(pos);
            }
            if (LevelManager.Ins.currentLevel.lockedTiles.Exists(t => t == pos))
            {
                LevelManager.Ins.OnLockedTileCleared(pos);
            }
        }
    }
}