using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject[] candyPrefabs;
    public GameObject[] specialCandyPrefabs; // 0: StripedH, 1: StripedV, 2: Wrapped, 3: ColorBomb
    public Text scoreText;
    public int gridWidth = 8;
    public int gridHeight = 8;
    private Candy[,] candyGrid;
    private bool isProcessing = false;
    private int score = 0;
    private Candy firstSelectedCandy;
    private List<(Vector2Int pos, Candy.SpecialType type)> specialCandies = new List<(Vector2Int, Candy.SpecialType)>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        candyGrid = new Candy[gridWidth, gridHeight];
        UpdateScoreUI();
        StartCoroutine(InitializeBoard());
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    IEnumerator InitializeBoard()
    {
        yield return StartCoroutine(GenerateBoard());
        while (CheckMatches().Count > 0)
        {
            yield return StartCoroutine(ProcessMatches());
        }
    }

    IEnumerator GenerateBoard()
    {
        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                yield return StartCoroutine(SpawnCandy(row, col, true));
            }
        }
    }

    IEnumerator SpawnCandy(int row, int col, bool withDrop, Candy.SpecialType specialType = Candy.SpecialType.None)
    {
        GameObject prefab;
        if (specialType != Candy.SpecialType.None)
        {
            int index = specialType == Candy.SpecialType.StripedHorizontal ? 0 :
                        specialType == Candy.SpecialType.StripedVertical ? 1 :
                        specialType == Candy.SpecialType.Wrapped ? 2 : 3;
            prefab = specialCandyPrefabs[index];
            Debug.Log($"Spawning special candy at ({row}, {col}): {specialType}, prefab: {prefab.name}");
        }
        else
        {
            prefab = candyPrefabs[Random.Range(0, candyPrefabs.Length)];
        }

        GameObject candyObj = Instantiate(prefab, new Vector3(col, gridHeight), Quaternion.identity);
        if (specialType == Candy.SpecialType.StripedHorizontal) candyObj.transform.Rotate(0, 0, 90);
        Candy candy = candyObj.GetComponent<Candy>();
        candy.row = row;
        candy.column = col;
        candy.isSpecial = specialType != Candy.SpecialType.None;
        candy.specialType = specialType;
        candyGrid[row, col] = candy;

        if (withDrop)
        {
            Vector3 targetPos = new Vector3(col, row, 0);
            float speed = 10f;
            while (Vector3.Distance(candyObj.transform.position, targetPos) > 0.1f)
            {
                candyObj.transform.position = Vector3.MoveTowards(
                    candyObj.transform.position, targetPos, Time.deltaTime * speed);
                yield return null;
            }
        }
        candyObj.transform.position = new Vector3(col, row, 0);
    }

    public void SelectCandy(Candy candy)
    {
        if (isProcessing) return;

        if (firstSelectedCandy == null)
        {
            firstSelectedCandy = candy;
            candy.transform.localScale = Vector3.one * 0.7f;
        }
        else
        {
            if (IsAdjacent(firstSelectedCandy, candy))
            {
                StartCoroutine(ProcessSwap(firstSelectedCandy, candy));
            }
            else
            {
                firstSelectedCandy.transform.localScale = Vector3.one * 0.5f;
                firstSelectedCandy = null;
            }
        }
    }

    bool IsAdjacent(Candy candy1, Candy candy2)
    {
        return Mathf.Abs(candy1.row - candy2.row) + Mathf.Abs(candy1.column - candy2.column) == 1;
    }

    IEnumerator ProcessSwap(Candy candy1, Candy candy2)
    {
        isProcessing = true;
        yield return StartCoroutine(AnimateSwap(candy1, candy2));
        SwapCandies(candy1, candy2);

        bool hasSpecialEffect = candy1.isSpecial || candy2.isSpecial;
        List<Candy> matches = CheckMatches();

        if (hasSpecialEffect)
        {
            if (candy1.isSpecial && candy2.isSpecial)
            {
                if (candy1.specialType == Candy.SpecialType.StripedHorizontal || candy1.specialType == Candy.SpecialType.StripedVertical)
                {
                    if (candy2.specialType == Candy.SpecialType.StripedHorizontal || candy2.specialType == Candy.SpecialType.StripedVertical)
                    {
                        ClearRow(candy1.row);
                        ClearColumn(candy1.column);
                        ClearRow(candy2.row);
                        ClearColumn(candy2.column);
                        AddScore(1000);
                    }
                    else if (candy2.specialType == Candy.SpecialType.Wrapped)
                    {
                        ClearMultipleRows(candy1.row);
                        ClearMultipleColumns(candy1.column);
                        AddScore(1500);
                    }
                    else if (candy2.specialType == Candy.SpecialType.ColorBomb)
                    {
                        StartCoroutine(ActivateStripedCandies(candy1.type));
                        AddScore(2000);
                    }
                }
                else if (candy1.specialType == Candy.SpecialType.Wrapped)
                {
                    if (candy2.specialType == Candy.SpecialType.Wrapped)
                    {
                        ClearLargeArea(candy1.row, candy1.column);
                        ClearLargeArea(candy2.row, candy2.column);
                        AddScore(1500);
                    }
                    else if (candy2.specialType == Candy.SpecialType.ColorBomb)
                    {
                        StartCoroutine(ActivateWrappedCandies(candy1.type));
                        AddScore(2000);
                    }
                }
                else if (candy1.specialType == Candy.SpecialType.ColorBomb)
                {
                    if (candy2.specialType == Candy.SpecialType.ColorBomb)
                    {
                        ClearAll();
                        AddScore(2500);
                    }
                    else if (candy2.specialType == Candy.SpecialType.StripedHorizontal || candy2.specialType == Candy.SpecialType.StripedVertical)
                    {
                        StartCoroutine(ActivateStripedCandies(candy2.type));
                        AddScore(2000);
                    }
                    else if (candy2.specialType == Candy.SpecialType.Wrapped)
                    {
                        StartCoroutine(ActivateWrappedCandies(candy2.type));
                        AddScore(2000);
                    }
                }
                candyGrid[candy1.row, candy1.column] = null;
                candyGrid[candy2.row, candy2.column] = null;
                Destroy(candy1.gameObject);
                Destroy(candy2.gameObject);
            }
            else
            {
                if (candy1.specialType == Candy.SpecialType.ColorBomb)
                {
                    candy1.TriggerSpecialEffect(candy2);
                    candyGrid[candy1.row, candy1.column] = null;
                    Destroy(candy1.gameObject);
                }
                else if (candy2.specialType == Candy.SpecialType.ColorBomb)
                {
                    candy2.TriggerSpecialEffect(candy1);
                    candyGrid[candy2.row, candy2.column] = null;
                    Destroy(candy2.gameObject);
                }
                else
                {
                    candy1.TriggerSpecialEffect();
                    candy2.TriggerSpecialEffect();
                }
            }
            yield return StartCoroutine(FillEmptySpaces());
            yield return StartCoroutine(ProcessMatches());
        }
        else if (matches.Count > 0)
        {
            yield return StartCoroutine(ProcessMatches());
        }
        else
        {
            yield return StartCoroutine(AnimateSwap(candy1, candy2));
            SwapCandies(candy1, candy2);
        }

        if (firstSelectedCandy != null)
        {
            firstSelectedCandy.transform.localScale = Vector3.one * 0.5f;
            firstSelectedCandy = null;
        }
        isProcessing = false;
    }

    IEnumerator AnimateSwap(Candy candy1, Candy candy2)
    {
        Vector3 pos1 = candy1.transform.position;
        Vector3 pos2 = candy2.transform.position;
        float swapTime = 0.3f;
        float elapsed = 0f;

        while (elapsed < swapTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swapTime;
            candy1.transform.position = Vector3.Lerp(pos1, pos2, t);
            candy2.transform.position = Vector3.Lerp(pos2, pos1, t);
            yield return null;
        }
    }

    void SwapCandies(Candy candy1, Candy candy2)
    {
        candyGrid[candy1.row, candy1.column] = candy2;
        candyGrid[candy2.row, candy2.column] = candy1;

        int tempRow = candy1.row;
        int tempCol = candy1.column;
        candy1.row = candy2.row;
        candy1.column = candy2.column;
        candy2.row = tempRow;
        candy2.column = tempCol;
    }

    List<Candy> CheckMatches()
    {
        List<Candy> matches = new List<Candy>();
        HashSet<Candy> uniqueMatches = new HashSet<Candy>();
        specialCandies.Clear();

        // Check ngang
        for (int row = 0; row < gridHeight; row++)
        {
            int col = 0;
            while (col < gridWidth - 2)
            {
                if (IsValidMatch3(row, col, 0, 1))
                {
                    Debug.Log($"IsValidMatch(row={row}, col={col}, 0, 1)");
                    int matchLength = 3;
                    while (col + matchLength < gridWidth && IsValidMatchMore3(row, col, 0, matchLength))
                    {
                        matchLength++;
                        Debug.Log($"Match length extended to {matchLength}");
                    }
                    for (int i = 0; i < matchLength; i++)
                    {
                        uniqueMatches.Add(candyGrid[row, col + i]);
                        candyGrid[row, col + i].isMatched = true;
                    }
                    if (matchLength >= 5)
                    {
                        Debug.Log($"H2: Creating ColorBomb at ({row}, {col + 2}) for match length {matchLength}");
                        specialCandies.Add((new Vector2Int(row, col + 2), Candy.SpecialType.ColorBomb));
                    }
                    else if (matchLength == 4)
                    {
                        Debug.Log($"H: Creating StripedVertical at ({row}, {col + 1})");
                        specialCandies.Add((new Vector2Int(row, col + 1), Candy.SpecialType.StripedVertical));
                    }
                    AddScore(matchLength * 100);
                    col += matchLength; // Bỏ qua các cột đã xử lý
                }
                else
                {
                    col++;
                }
            }
        }

        // Check dọc
        for (int col = 0; col < gridWidth; col++)
        {
            int row = 0;
            while (row < gridHeight - 2)
            {
                if (IsValidMatch3(row, col, 1, 0))
                {
                    Debug.Log($"IsValidMatch(row={row}, col={col}, 1, 0)");
                    int matchLength = 3;
                    while (row + matchLength < gridHeight && IsValidMatchMore3(row, col, matchLength, 0))
                    {
                        matchLength++;
                        Debug.Log($"Match length extended to {matchLength}");
                    }
                    for (int i = 0; i < matchLength; i++)
                    {
                        uniqueMatches.Add(candyGrid[row + i, col]);
                        candyGrid[row + i, col].isMatched = true;
                    }
                    if (matchLength >= 5)
                    {
                        Debug.Log($"V2: Creating ColorBomb at ({row + 2}, {col}) for match length {matchLength}");
                        specialCandies.Add((new Vector2Int(row + 2, col), Candy.SpecialType.ColorBomb));
                    }
                    else if (matchLength == 4)
                    {
                        Debug.Log($"V: Creating StripedHorizontal at ({row + 1}, {col})");
                        specialCandies.Add((new Vector2Int(row + 1, col), Candy.SpecialType.StripedHorizontal));
                    }
                    AddScore(matchLength * 100);
                    row += matchLength; // Bỏ qua các hàng đã xử lý
                }
                else
                {
                    row++;
                }
            }
        }

        // Check L và T
        for (int row = 0; row < gridHeight - 2; row++)
        {
            for (int col = 0; col < gridWidth - 2; col++)
            {
                if (CheckLShape(row, col, uniqueMatches))
                {
                    AddScore(500);
                }
                if (CheckTShape(row, col, uniqueMatches))
                {
                    AddScore(500);
                }
            }
        }

        matches.AddRange(uniqueMatches);

        foreach (var special in specialCandies)
        {
            Vector2Int pos = special.pos;
            if (pos.x >= 0 && pos.x < gridHeight && pos.y >= 0 && pos.y < gridWidth && candyGrid[pos.x, pos.y] != null)
            {
                if (matches.Contains(candyGrid[pos.x, pos.y]))
                {
                    matches.Remove(candyGrid[pos.x, pos.y]);
                    candyGrid[pos.x, pos.y].isMatched = false;
                }
            }
        }

        return matches;
    }

    bool CheckLShape(int row, int col, HashSet<Candy> matches)
    {
        if (row + 2 >= gridHeight || col + 2 >= gridWidth) return false;

        bool TryMatchPattern(int[] rows, int[] cols, Vector2Int wrappedPos)
        {
            Candy reference = candyGrid[row + rows[0], col + cols[0]];
            if (reference == null) return false;

            for (int i = 1; i < 5; i++)
            {
                int r = row + rows[i];
                int c = col + cols[i];
                if (r < 0 || r >= gridHeight || c < 0 || c >= gridWidth || candyGrid[r, c] == null || candyGrid[r, c].type != reference.type)
                    return false;
            }
            for (int i = 0; i < 5; i++)
            {
                int r = row + rows[i];
                int c = col + cols[i];
                matches.Add(candyGrid[r, c]);
                candyGrid[r, c].isMatched = true;
            }
            specialCandies.Add((new Vector2Int(row + wrappedPos.x, col + wrappedPos.y), Candy.SpecialType.Wrapped));
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

    bool CheckTShape(int row, int col, HashSet<Candy> matches)
    {
        if (row + 2 >= gridHeight || col + 2 >= gridWidth) return false;

        bool TryMatchPattern(int[] rows, int[] cols, Vector2Int wrappedPos)
        {
            Candy reference = candyGrid[row + rows[0], col + cols[0]];
            if (reference == null) return false;

            for (int i = 1; i < 5; i++)
            {
                int r = row + rows[i];
                int c = col + cols[i];
                if (r < 0 || r >= gridHeight || c < 0 || c < gridWidth || candyGrid[r, c] == null || candyGrid[r, c].type != reference.type)
                    return false;
            }
            for (int i = 0; i < 5; i++)
            {
                int r = row + rows[i];
                int c = col + cols[i];
                matches.Add(candyGrid[r, c]);
                candyGrid[r, c].isMatched = true;
            }
            specialCandies.Add((new Vector2Int(row + wrappedPos.x, col + wrappedPos.y), Candy.SpecialType.Wrapped));
            return true;
        }

        int[] t1Rows = { 0, 1, 2, 2, 2 };
        int[] t1Cols = { 1, 1, 0, 1, 2 };
        if (TryMatchPattern(t1Rows, t1Cols, new Vector2Int(2, 1))) return true;

        int[] t2Rows = { 0, 0, 0, 1, 2 };
        int[] t2Cols = { 0, 1, 2, 1, 1 };
        if (TryMatchPattern(t2Rows, t2Cols, new Vector2Int(0, 1))) return true;

        int[] t3Rows = { 0, 1, 1, 1, 2 };
        int[] t3Cols = { 0, 0, 1, 2, 0 };
        if (TryMatchPattern(t3Rows, t3Cols, new Vector2Int(1, 0))) return true;

        int[] t4Rows = { 0, 1, 1, 1, 2 };
        int[] t4Cols = { 2, 0, 1, 2, 2 };
        if (TryMatchPattern(t4Rows, t4Cols, new Vector2Int(1, 2))) return true;

        return false;
    }

    bool IsValidMatch3(int row, int col, int rowOffset, int colOffset)
    {
        if (row + 2 * rowOffset >= gridHeight || col + 2 * colOffset >= gridWidth || row < 0 || col < 0)
            return false;

        Candy candy1 = candyGrid[row, col];
        Candy candy2 = candyGrid[row + rowOffset, col + colOffset];
        Candy candy3 = candyGrid[row + 2 * rowOffset, col + 2 * colOffset];

        return candy1 != null && candy2 != null && candy3 != null &&
               candy1.type == candy2.type && candy1.type == candy3.type;
    }

    bool IsValidMatchMore3(int row, int col, int rowOffset, int colOffset)
    {
        if (row + rowOffset >= gridHeight || col + colOffset >= gridWidth || row < 0 || col < 0)
            return false;

        Candy candy1 = candyGrid[row, col];
        Candy candy2 = candyGrid[row + rowOffset, col + colOffset];

        return candy1 != null && candy2 != null &&
               candy1.type == candy2.type;
    }

    public void ClearRow(int row)
    {
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int col = 0; col < gridWidth; col++)
        {
            if (candyGrid[row, col] != null)
            {
                Candy candy = candyGrid[row, col];
                if (candy.isSpecial)
                {
                    specialCandiesToTrigger.Add(candy);
                }
                candiesToShrink.Add(candy);
                candyGrid[row, col] = null;
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                StartCoroutine(ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                candy.TriggerSpecialEffect();
            }
        }

        AddScore(200);
    }

    public void ClearColumn(int col)
    {
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int row = 0; row < gridHeight; row++)
        {
            if (candyGrid[row, col] != null)
            {
                Candy candy = candyGrid[row, col];
                if (candy.isSpecial)
                {
                    specialCandiesToTrigger.Add(candy);
                }
                candiesToShrink.Add(candy);
                candyGrid[row, col] = null;
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                StartCoroutine(ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                candy.TriggerSpecialEffect();
            }
        }

        AddScore(200);
    }

    public void ClearArea(int row, int col)
    {
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int r = row + i;
                int c = col + j;
                if (r >= 0 && r < gridHeight && c >= 0 && c < gridWidth && candyGrid[r, c] != null)
                {
                    Candy candy = candyGrid[r, c];
                    if (candy.isSpecial)
                    {
                        specialCandiesToTrigger.Add(candy);
                    }
                    candiesToShrink.Add(candy);
                    candyGrid[r, c] = null;
                }
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                StartCoroutine(ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                candy.TriggerSpecialEffect();
            }
        }

        AddScore(300);
    }

    public void ClearLargeArea(int row, int col)
    {
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                int r = row + i;
                int c = col + j;
                if (r >= 0 && r < gridHeight && c >= 0 && c < gridWidth && candyGrid[r, c] != null)
                {
                    Candy candy = candyGrid[r, c];
                    if (candy.isSpecial)
                    {
                        specialCandiesToTrigger.Add(candy);
                    }
                    candiesToShrink.Add(candy);
                    candyGrid[r, c] = null;
                }
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                StartCoroutine(ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                candy.TriggerSpecialEffect();
            }
        }

        AddScore(500);
    }

    public void ClearMultipleRows(int centerRow)
    {
        for (int row = centerRow - 1; row <= centerRow + 1; row++)
        {
            if (row >= 0 && row < gridHeight)
            {
                ClearRow(row);
            }
        }
    }

    public void ClearMultipleColumns(int centerCol)
    {
        for (int col = centerCol - 1; col <= centerCol + 1; col++)
        {
            if (col >= 0 && col < gridWidth)
            {
                ClearColumn(col);
            }
        }
    }

    public void ClearColorType(CandyType type)
    {
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                if (candyGrid[row, col] != null && candyGrid[row, col].type == type)
                {
                    Candy candy = candyGrid[row, col];
                    if (candy.isSpecial)
                    {
                        specialCandiesToTrigger.Add(candy);
                    }
                    candiesToShrink.Add(candy);
                    candyGrid[row, col] = null;
                }
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                StartCoroutine(ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                candy.TriggerSpecialEffect();
            }
        }

        AddScore(500);
    }

    public void ClearAll()
    {
        List<Candy> specialCandiesToTrigger = new List<Candy>();
        List<Candy> candiesToShrink = new List<Candy>();

        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                if (candyGrid[row, col] != null)
                {
                    Candy candy = candyGrid[row, col];
                    if (candy.isSpecial)
                    {
                        specialCandiesToTrigger.Add(candy);
                    }
                    candiesToShrink.Add(candy);
                    candyGrid[row, col] = null;
                }
            }
        }

        foreach (Candy candy in candiesToShrink)
        {
            if (candy != null && candy.gameObject != null)
            {
                StartCoroutine(ShrinkCandy(candy));
            }
        }

        foreach (Candy candy in specialCandiesToTrigger)
        {
            if (candy != null && candy.gameObject != null)
            {
                candy.TriggerSpecialEffect();
            }
        }

        AddScore(1000);
    }

    IEnumerator ActivateStripedCandies(CandyType type)
    {
        List<(int row, int col)> positions = new List<(int, int)>();
        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
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
                Destroy(candyGrid[pos.row, pos.col].gameObject);
                candyGrid[pos.row, pos.col] = null;
                Candy.SpecialType stripedType = Random.Range(0, 2) == 0 ?
                    Candy.SpecialType.StripedHorizontal : Candy.SpecialType.StripedVertical;
                yield return StartCoroutine(SpawnCandy(pos.row, pos.col, false, stripedType));
                if (candyGrid[pos.row, pos.col] != null)
                {
                    candyGrid[pos.row, pos.col].TriggerSpecialEffect();
                }
            }
        }
    }

    IEnumerator ActivateWrappedCandies(CandyType type)
    {
        List<(int row, int col)> positions = new List<(int, int)>();
        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
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
                Destroy(candyGrid[pos.row, pos.col].gameObject);
                candyGrid[pos.row, pos.col] = null;
                yield return StartCoroutine(SpawnCandy(pos.row, pos.col, false, Candy.SpecialType.Wrapped));
                if (candyGrid[pos.row, pos.col] != null)
                {
                    candyGrid[pos.row, pos.col].TriggerSpecialEffect();
                }
            }
        }
    }

    IEnumerator ProcessMatches()
    {
        List<Candy> matches = CheckMatches();
        if (matches.Count == 0) yield break;

        foreach (Candy candy in matches)
        {
            if (candy != null && !candy.isSpecial && candy.gameObject != null)
            {
                StartCoroutine(ShrinkCandy(candy));
            }
        }
        Debug.Log($"Special candies count: {specialCandies.Count}");
        foreach (var special in specialCandies)
        {
            Debug.Log($"Processing special candy at ({special.pos.x}, {special.pos.y}): {special.type}");
        }
        yield return new WaitForSeconds(0.3f);

        ClearMatches(matches);
        foreach (var special in specialCandies)
        {
            Vector2Int pos = special.pos;
            if (pos.x >= 0 && pos.x < gridHeight && pos.y >= 0 && pos.y < gridWidth)
            {
                ReplaceWithSpecialCandy(pos.x, pos.y, special.type);
            }
        }
        specialCandies.Clear();
        yield return StartCoroutine(FillEmptySpaces());

        if (CheckMatches().Count > 0)
        {
            yield return StartCoroutine(ProcessMatches());
        }
    }

    IEnumerator ShrinkCandy(Candy candy)
    {
        if (candy == null || candy.gameObject == null) yield break;

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
            Destroy(candy.gameObject);
        }
    }

    void ClearMatches(List<Candy> matches)
    {
        foreach (Candy candy in matches)
        {
            if (candy != null && !candy.isSpecial)
            {
                candyGrid[candy.row, candy.column] = null;
            }
        }
    }

    IEnumerator FillEmptySpaces()
    {
        bool moved = false;
        for (int col = 0; col < gridWidth; col++)
        {
            int emptyCount = 0;
            for (int row = 0; row < gridHeight; row++)
            {
                if (candyGrid[row, col] == null)
                {
                    emptyCount++;
                }
                else if (emptyCount > 0)
                {
                    Candy candy = candyGrid[row, col];
                    candyGrid[row - emptyCount, col] = candy;
                    candyGrid[row, col] = null;
                    candy.row = row - emptyCount;
                    StartCoroutine(MoveCandy(candy, new Vector3(col, row - emptyCount, 0)));
                    moved = true;
                }
            }

            for (int i = 0; i < emptyCount; i++)
            {
                int row = gridHeight - emptyCount + i;
                yield return StartCoroutine(SpawnCandy(row, col, true));
            }
        }

        if (moved)
        {
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator MoveCandy(Candy candy, Vector3 targetPos)
    {
        while (Vector3.Distance(candy.transform.position, targetPos) > 0.1f)
        {
            candy.transform.position = Vector3.MoveTowards(
                candy.transform.position, targetPos, Time.deltaTime * 10f);
            yield return null;
        }
        candy.transform.position = targetPos;
    }

    void ReplaceWithSpecialCandy(int row, int col, Candy.SpecialType specialType)
    {
        if (candyGrid[row, col] != null)
        {
            Destroy(candyGrid[row, col].gameObject);
            candyGrid[row, col] = null;
        }
        StartCoroutine(SpawnCandy(row, col, false, specialType));
    }

    void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
    }
}