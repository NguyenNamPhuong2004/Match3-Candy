using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

//public class GameManager : MonoBehaviour
//{
//    public static GameManager instance;
//    public GameObject[] candyPrefabs;
//    public GameObject[] specialCandyPrefabs;
//    //public ParticleSystem matchEffect;
//    //public AudioClip matchSound;
//    //public AudioClip specialMatchSound;
//    //public AudioClip specialEffectSound;
//    //private AudioSource audioSource;
//    public Text scoreText;
//    public int gridWidth = 8;
//    public int gridHeight = 8;
//    private Candy[,] candyGrid;
//    private bool isProcessing = false;
//    private int score = 0;
//    private Candy firstSelectedCandy;
//    private List<(Vector2Int pos, Candy.SpecialType type)> specialCandies = new List<(Vector2Int, Candy.SpecialType)>();

//    void Awake()
//    {
//        if (instance == null) instance = this;
//        else Destroy(gameObject);
//        //audioSource = GetComponent<AudioSource>();
//        //if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
//    }

//    void Start()
//    {
//        candyGrid = new Candy[gridWidth, gridHeight];
//        UpdateScoreUI();
//        StartCoroutine(InitializeBoard());
//    }

//    void UpdateScoreUI()
//    {
//        if (scoreText != null)
//            scoreText.text = "Score: " + score;
//    }

//    IEnumerator InitializeBoard()
//    {
//        yield return StartCoroutine(GenerateBoard());
//        while (CheckMatches().Count > 0)
//        {
//            yield return StartCoroutine(ProcessMatches());
//        }
//    }

//    IEnumerator GenerateBoard()
//    {
//        for (int row = 0; row < gridHeight; row++)
//        {
//            for (int col = 0; col < gridWidth; col++)
//            {
//                yield return StartCoroutine(SpawnCandy(row, col, true));
//            }
//        }
//    }

//    IEnumerator SpawnCandy(int row, int col, bool withDrop = false, Candy.SpecialType specialType = Candy.SpecialType.None)
//    {
//        Debug.Log(specialType +" Spawn");
//        GameObject prefab;
//        if (specialType != Candy.SpecialType.None)
//        {
//            prefab = specialCandyPrefabs[specialType == Candy.SpecialType.StripedHorizontal ? 0 :
//                                       specialType == Candy.SpecialType.StripedVertical ? 1 : 2];
//        }
//        else
//        {
//            prefab = candyPrefabs[Random.Range(0, candyPrefabs.Length)];
//        }

//        GameObject candyObj = Instantiate(prefab, new Vector3(col, gridHeight), Quaternion.identity);
//        Candy candy = candyObj.GetComponent<Candy>();
//        candy.row = row;
//        candy.column = col;
//        candy.isSpecial = specialType != Candy.SpecialType.None;
//        candy.specialType = specialType;
//        candyGrid[row, col] = candy;

//        if (withDrop)
//        {
//            Vector3 targetPos = new Vector3(col, row, 0);
//            float speed = 10f;
//            while (Vector3.Distance(candyObj.transform.position, targetPos) > 0.1f)
//            {
//                candyObj.transform.position = Vector3.MoveTowards(
//                    candyObj.transform.position, targetPos, Time.deltaTime * speed);
//                yield return null;
//            }
//        }
//        candyObj.transform.position = new Vector3(col, row, 0);
//    }

//    public void SelectCandy(Candy candy)
//    {
//        if (isProcessing) return;

//        if (firstSelectedCandy == null)
//        {
//            firstSelectedCandy = candy;
//            candy.transform.localScale = Vector3.one * 0.7f;
//        }
//        else
//        {
//            if (IsAdjacent(firstSelectedCandy, candy))
//            {
//                StartCoroutine(ProcessSwap(firstSelectedCandy, candy));
//            }
//            else
//            {
//                firstSelectedCandy.transform.localScale = Vector3.one * 0.5f;
//                firstSelectedCandy = null;
//            }
//        }
//    }

//    bool IsAdjacent(Candy candy1, Candy candy2)
//    {
//        return Mathf.Abs(candy1.row - candy2.row) +
//               Mathf.Abs(candy1.column - candy2.column) == 1;
//    }

//    IEnumerator ProcessSwap(Candy candy1, Candy candy2)
//    {
//        isProcessing = true;
//        yield return StartCoroutine(AnimateSwap(candy1, candy2));
//        SwapCandies(candy1, candy2);

//        List<Candy> matches = CheckMatches();
//        bool hasSpecialEffect = candy1.isSpecial || candy2.isSpecial;

//        if (hasSpecialEffect)
//        {
//            //audioSource.PlayOneShot(specialEffectSound);
//            candy1.TriggerSpecialEffect();
//            candy2.TriggerSpecialEffect();
//            yield return StartCoroutine(ProcessMatches());
//        }
//        else if (matches.Count > 0)
//        {
//            yield return StartCoroutine(ProcessMatches());
//        }
//        else
//        {
//            yield return StartCoroutine(AnimateSwap(candy1, candy2));
//            SwapCandies(candy1, candy2);
//            firstSelectedCandy.transform.localScale = Vector3.one * 0.5f;
//            firstSelectedCandy = null;
//        }       
//        isProcessing = false;
//    }

//    IEnumerator AnimateSwap(Candy candy1, Candy candy2)
//    {
//        Vector3 pos1 = candy1.transform.position;
//        Vector3 pos2 = candy2.transform.position;
//        float swapTime = 0.3f;
//        float elapsed = 0f;

//        while (elapsed < swapTime)
//        {
//            elapsed += Time.deltaTime;
//            float t = elapsed / swapTime;
//            candy1.transform.position = Vector3.Lerp(pos1, pos2, t);
//            candy2.transform.position = Vector3.Lerp(pos2, pos1, t);
//            yield return null;
//        }
//    }

//    void SwapCandies(Candy candy1, Candy candy2)
//    {
//        candyGrid[candy1.row, candy1.column] = candy2;
//        candyGrid[candy2.row, candy2.column] = candy1;

//        int tempRow = candy1.row;
//        int tempCol = candy1.column;
//        candy1.row = candy2.row;
//        candy1.column = candy2.column;
//        candy2.row = tempRow;
//        candy2.column = tempCol;
//    }

//    List<Candy> CheckMatches()
//    {
//        List<Candy> matches = new List<Candy>();
//        HashSet<Candy> uniqueMatches = new HashSet<Candy>();


//        for (int row = 0; row < gridHeight; row++)
//        {
//            for (int col = 0; col < gridWidth - 2; col++)
//            {
//                if (IsValidMatch3(row, col, 0, 1))
//                {
//                    int matchLength = 3;
//                    while (col + matchLength < gridWidth && IsValidMatchMore3(row, col, 0, matchLength))
//                    {
//                        matchLength++;
//                    }
//                    Debug.Log(matchLength);
//                    for (int i = 0; i < matchLength; i++)
//                    {
//                        uniqueMatches.Add(candyGrid[row, col + i]);
//                        candyGrid[row, col + i].isMatched = true;
//                    }
//                    if (matchLength == 4)
//                    {
//                        Debug.Log("M4");
//                        specialCandies.Add((new Vector2Int(row, col + 1), Candy.SpecialType.StripedHorizontal));
//                    }
//                    else if (matchLength >= 5)
//                    {
//                        specialCandies.Add((new Vector2Int(row, col + 2), Candy.SpecialType.Wrapped));
//                    }
//                    AddScore(matchLength * 100);
//                }
//            }
//        }

//        for (int col = 0; col < gridWidth; col++)
//        {
//            for (int row = 0; row < gridHeight - 2; row++)
//            {
//                if (IsValidMatch3(row, col, 1, 0))
//                {
//                    int matchLength = 3;
//                    while (row + matchLength < gridHeight && IsValidMatchMore3(row, col, matchLength, 0))
//                    {
//                        matchLength++;
//                    }
//                    Debug.Log(matchLength);
//                    for (int i = 0; i < matchLength; i++)
//                    {
//                        uniqueMatches.Add(candyGrid[row + i, col]);
//                        candyGrid[row + i, col].isMatched = true;
//                    }
//                    if (matchLength == 4)
//                    {
//                        Debug.Log("M4");
//                        specialCandies.Add((new Vector2Int(row + 1, col), Candy.SpecialType.StripedVertical));
//                    }
//                    else if (matchLength >= 5)
//                    {
//                        specialCandies.Add((new Vector2Int(row + 2, col), Candy.SpecialType.Wrapped));
//                    }
//                    AddScore(matchLength * 100);
//                }
//            }
//        }

//        for (int row = 0; row < gridHeight - 2; row++)
//        {
//            for (int col = 0; col < gridWidth - 2; col++)
//            {
//                Candy center = candyGrid[row + 1, col + 1];
//                if (center == null) continue;

//                if (CheckLShape(row, col, uniqueMatches, center.type, specialCandies))
//                {
//                    AddScore(500);
//                }
//                if (CheckTShape(row, col, uniqueMatches, center.type, specialCandies))
//                {
//                    AddScore(500);
//                }
//            }
//        }

//        matches.AddRange(uniqueMatches);

//        foreach (var special in specialCandies)
//        {
//            Vector2Int pos = special.pos;
//            if (matches.Contains(candyGrid[pos.x, pos.y]))
//            {
//                matches.Remove(candyGrid[pos.x, pos.y]);
//                candyGrid[pos.x, pos.y].isMatched = false;
//            }
//            //Vector2Int pos = special.pos;
//            //if (pos.x >= 0 && pos.x < gridHeight && pos.y >= 0 && pos.y < gridWidth && candyGrid[pos.x, pos.y] != null)
//            //{
//            //    if (matches.Contains(candyGrid[pos.x, pos.y]))
//            //    {
//            //        matches.Remove(candyGrid[pos.x, pos.y]);
//            //        candyGrid[pos.x, pos.y].isMatched = false;
//            //    }
//            //}
//        }

//        return matches;
//    }

//    IEnumerator ReplaceSpecialCandy(int row, int col, Candy.SpecialType specialType)
//    {
//        if (candyGrid[row, col] != null)
//        {
//            Destroy(candyGrid[row, col].gameObject);
//            candyGrid[row, col] = null;
//        }
//        Debug.Log(specialType + " Prepare Spawn");
//        yield return StartCoroutine(SpawnCandy(row, col, false, specialType));
//    }

//    bool IsValidMatch3(int row, int col, int rowOffset, int colOffset)
//    {
//        if (row + 2 * rowOffset >= gridHeight || col + 2 * colOffset >= gridWidth || row < 0 || col < 0)
//            return false;

//        Candy candy1 = candyGrid[row, col];
//        Candy candy2 = candyGrid[row + rowOffset, col + colOffset];
//        Candy candy3 = candyGrid[row + 2 * rowOffset, col + 2 * colOffset];

//        return candy1 != null && candy2 != null && candy3 != null &&
//               candy1.type == candy2.type && candy1.type == candy3.type;
//    }

//    bool IsValidMatchMore3(int row, int col, int rowOffset, int colOffset)
//    {
//        if (row + rowOffset >= gridHeight || col + colOffset >= gridWidth || row < 0 || col < 0)
//            return false;

//        Candy candy1 = candyGrid[row, col];
//        Candy candy2 = candyGrid[row + rowOffset, col + colOffset];

//        return candy1 != null && candy2 != null &&
//               candy1.type == candy2.type;
//    }

//    bool CheckLShape(int row, int col, HashSet<Candy> matches, CandyType centerType, List<(Vector2Int pos, Candy.SpecialType type)> specialCandies)
//    {
//        if (row + 2 >= gridHeight || col + 2 >= gridWidth) return false;

//        bool TryMatchPattern(int[] rows, int[] cols, Vector2Int wrappedPos)
//        {
//            for (int i = 0; i < 5; i++)
//            {
//                int r = row + rows[i];
//                int c = col + cols[i];
//                if (r < 0 || r >= gridHeight || c < 0 || c >= gridWidth || candyGrid[r, c] == null || candyGrid[r, c].type != centerType)
//                    return false;
//            }
//            for (int i = 0; i < 5; i++)
//            {
//                int r = row + rows[i];
//                int c = col + cols[i];
//                matches.Add(candyGrid[r, c]);
//                candyGrid[r, c].isMatched = true;
//            }
//            specialCandies.Add((new Vector2Int(row + wrappedPos.x, col + wrappedPos.y), Candy.SpecialType.Wrapped));
//            return true;
//        }

//        int[] l1Rows = { 0, 1, 2, 2, 2 };
//        int[] l1Cols = { 2, 2, 0, 1, 2 };
//        if (TryMatchPattern(l1Rows, l1Cols, new Vector2Int(2, 2))) return true;

//        int[] l2Rows = { 0, 0, 0, 1, 2 };
//        int[] l2Cols = { 0, 1, 2, 2, 2 };
//        if (TryMatchPattern(l2Rows, l2Cols, new Vector2Int(0, 2))) return true;

//        int[] l3Rows = { 0, 0, 0, 1, 2 };
//        int[] l3Cols = { 0, 1, 2, 0, 0 };
//        if (TryMatchPattern(l3Rows, l3Cols, new Vector2Int(0, 0))) return true;

//        int[] l4Rows = { 0, 1, 2, 2, 2 };
//        int[] l4Cols = { 0, 0, 0, 1, 2 };
//        if (TryMatchPattern(l4Rows, l4Cols, new Vector2Int(2, 0))) return true;

//        return false;
//    }

//    bool CheckTShape(int row, int col, HashSet<Candy> matches, CandyType centerType, List<(Vector2Int pos, Candy.SpecialType type)> specialCandies)
//    {
//        if (row + 2 >= gridHeight || col + 2 >= gridWidth) return false;

//        bool TryMatchPattern(int[] rows, int[] cols, Vector2Int wrappedPos)
//        {
//            for (int i = 0; i < 5; i++)
//            {
//                int r = row + rows[i];
//                int c = col + cols[i];
//                if (r < 0 || r >= gridHeight || c < 0 || c >= gridWidth || candyGrid[r, c] == null || candyGrid[r, c].type != centerType)
//                    return false;
//            }
//            for (int i = 0; i < 5; i++)
//            {
//                int r = row + rows[i];
//                int c = col + cols[i];
//                matches.Add(candyGrid[r, c]);
//                candyGrid[r, c].isMatched = true;
//            }
//            specialCandies.Add((new Vector2Int(row + wrappedPos.x, col + wrappedPos.y), Candy.SpecialType.Wrapped));
//            return true;
//        }

//        int[] t1Rows = { 0, 1, 2, 2, 2 };
//        int[] t1Cols = { 1, 1, 0, 1, 2 };
//        if (TryMatchPattern(t1Rows, t1Cols, new Vector2Int(2, 1))) return true;

//        int[] t2Rows = { 0, 0, 0, 1, 2 };
//        int[] t2Cols = { 0, 1, 2, 1, 1 };
//        if (TryMatchPattern(t2Rows, t2Cols, new Vector2Int(0, 1))) return true;

//        int[] t3Rows = { 0, 1, 1, 1, 2 };
//        int[] t3Cols = { 0, 0, 1, 2, 0 };
//        if (TryMatchPattern(t3Rows, t3Cols, new Vector2Int(1, 0))) return true;

//        int[] t4Rows = { 0, 1, 1, 1, 2 };
//        int[] t4Cols = { 2, 0, 1, 2, 2 };
//        if (TryMatchPattern(t4Rows, t4Cols, new Vector2Int(1, 2))) return true;

//        return false;
//    }

//    public void ClearRow(int row)
//    {
//        for (int col = 0; col < gridWidth; col++)
//        {
//            if (candyGrid[row, col] != null)
//            {
//                StartCoroutine(ShrinkCandy(candyGrid[row, col]));
//                candyGrid[row, col] = null;
//            }
//        }
//        AddScore(200);
//    }

//    public void ClearColumn(int col)
//    {
//        for (int row = 0; row < gridHeight; row++)
//        {
//            if (candyGrid[row, col] != null)
//            {
//                StartCoroutine(ShrinkCandy(candyGrid[row, col]));
//                candyGrid[row, col] = null;
//            }
//        }
//        AddScore(200);
//    }

//    public void ClearArea(int row, int col)
//    {
//        for (int i = -1; i <= 1; i++)
//        {
//            for (int j = -1; j <= 1; j++)
//            {
//                int r = row + i;
//                int c = col + j;
//                if (r >= 0 && r < gridHeight && c >= 0 && c >= gridWidth && candyGrid[r, c] != null)
//                {
//                    StartCoroutine(ShrinkCandy(candyGrid[r, c]));
//                    candyGrid[r, c] = null;
//                }
//            }
//        }
//        AddScore(300);
//    }

//    IEnumerator ProcessMatches()
//    {
//        List<Candy> matches = CheckMatches();
//        if (matches.Count == 0) yield break;

//       // audioSource.PlayOneShot(matches.Count >= 4 ? specialMatchSound : matchSound);
//        foreach (Candy candy in matches)
//        {
//            if (candy != null && !candy.isSpecial)
//            {
//               // ParticleSystem effect = Instantiate(matchEffect, candy.transform.position, Quaternion.identity);
//               // Destroy(effect.gameObject, effect.main.duration);
//                StartCoroutine(ShrinkCandy(candy));
//            }
//        }

//        yield return new WaitForSeconds(0.3f);

//        ClearMatches(matches);
//        Debug.Log(specialCandies.Count);
//        foreach (var special in specialCandies)
//        {
//            Vector2Int pos = special.pos;
//            Debug.Log(special.type);
//            ReplaceSpecialCandy(pos.x, pos.y, special.type);
//        }
//        specialCandies.Clear();
//        yield return StartCoroutine(FillEmptySpaces());

//        if (CheckMatches().Count > 0)
//        {
//            yield return StartCoroutine(ProcessMatches());
//        }
//    }

//    IEnumerator ShrinkCandy(Candy candy)
//    {
//        float shrinkTime = 0.3f;
//        float elapsed = 0f;
//        Vector3 originalScale = candy.transform.localScale;

//        while (elapsed < shrinkTime)
//        {
//            elapsed += Time.deltaTime;
//            float t = elapsed / shrinkTime;
//            candy.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
//            yield return null;
//        }
//        Destroy(candy.gameObject);
//    }

//    void ClearMatches(List<Candy> matches)
//    {
//        foreach (Candy candy in matches)
//        {
//            if (candy != null && !candy.isSpecial)
//            {
//                candyGrid[candy.row, candy.column] = null;
//            }
//        }
//    }

//    IEnumerator FillEmptySpaces()
//    {
//        bool moved = false;
//        for (int col = 0; col < gridWidth; col++)
//        {
//            int emptyCount = 0;
//            for (int row = 0; row < gridHeight; row++)
//            {
//                if (candyGrid[row, col] == null)
//                {
//                    emptyCount++;
//                }
//                else if (emptyCount > 0)
//                {
//                    Candy candy = candyGrid[row, col];
//                    candyGrid[row - emptyCount, col] = candy;
//                    candyGrid[row, col] = null;
//                    candy.row = row - emptyCount;
//                    StartCoroutine(MoveCandy(candy, new Vector3(col, row - emptyCount, 0)));
//                    moved = true;
//                }
//            }

//            for (int i = 0; i < emptyCount; i++)
//            {
//                int row = gridHeight - emptyCount + i;
//                yield return StartCoroutine(SpawnCandy(row, col, true));
//            }
//        }

//        if (moved)
//        {
//            yield return new WaitForSeconds(0.3f);
//        }
//    }
//    IEnumerator MoveCandy(Candy candy, Vector3 targetPos)
//    {
//        while (Vector3.Distance(candy.transform.position, targetPos) > 0.1f)
//        {
//            candy.transform.position = Vector3.MoveTowards(
//                candy.transform.position, targetPos, Time.deltaTime * 10f);
//            yield return null;
//        }
//        candy.transform.position = targetPos;
//    }
//    void AddScore(int points)
//    {
//        score += points;
//        UpdateScoreUI();
//    }
//}using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject[] candyPrefabs;
    public GameObject[] specialCandyPrefabs; // 0: StripedH, 1: StripedV, 2: Wrapped
    public Text scoreText; // UI Text cho điểm số
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
                        specialType == Candy.SpecialType.StripedVertical ? 1 : 2;
            prefab = specialCandyPrefabs[index];
        }
        else
        {
            prefab = candyPrefabs[Random.Range(0, candyPrefabs.Length)];
        }

        GameObject candyObj = Instantiate(prefab, new Vector3(col, gridHeight), Quaternion.identity);
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

        List<Candy> matches = CheckMatches();
        bool hasSpecialEffect = candy1.isSpecial || candy2.isSpecial;

        if (hasSpecialEffect)
        {
            candy1.TriggerSpecialEffect();
            candy2.TriggerSpecialEffect();
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

        // Match ngang
        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth - 2; col++)
            {
                if (IsValidMatch3(row, col, 0, 1))
                {
                    Debug.Log("(IsValidMatch(row, col, 0, 1)");
                    int matchLength = 3;
                    while (col + matchLength < gridWidth && IsValidMatchMore3(row, col, 0, matchLength))
                    {
                        matchLength++;
                        Debug.Log(matchLength);
                    }
                    for (int i = 0; i < matchLength; i++)
                    {
                        uniqueMatches.Add(candyGrid[row, col + i]);
                    }
                    if (matchLength == 4)
                    {
                        Debug.Log("H");
                        specialCandies.Add((new Vector2Int(row, col + 1), Candy.SpecialType.StripedHorizontal));
                    }
                    else if (matchLength >= 5)
                    {
                        Debug.Log("H2");
                        specialCandies.Add((new Vector2Int(row, col + 2), Candy.SpecialType.Wrapped));
                    }
                    AddScore(matchLength * 100);
                }
            }
        }

        // Match dọc
        for (int col = 0; col < gridWidth; col++)
        {
            for (int row = 0; row < gridHeight - 2; row++)
            {
                if (IsValidMatch3(row, col, 1, 0))
                {
                    Debug.Log("(IsValidMatch(row, col, 1, 0)");
                    int matchLength = 3;
                    while (row + matchLength < gridHeight && IsValidMatchMore3(row, col, matchLength, 0))
                    {
                        matchLength++;
                        Debug.Log(matchLength);
                    }
                    for (int i = 0; i < matchLength; i++)
                    {
                        uniqueMatches.Add(candyGrid[row + i, col]);
                    }
                    if (matchLength == 4)
                    {
                        Debug.Log("V");
                        specialCandies.Add((new Vector2Int(row + 1, col), Candy.SpecialType.StripedVertical));
                    }
                    else if (matchLength >= 5)
                    {
                        Debug.Log("V2");
                        specialCandies.Add((new Vector2Int(row + 2, col), Candy.SpecialType.Wrapped));
                    }
                    AddScore(matchLength * 100);
                }
            }
        }

        // Match chữ L/T

        for (int row = 0; row < gridHeight - 2; row++)
        {
            for (int col = 0; col < gridWidth - 2; col++)
            {
                Candy center = candyGrid[row + 1, col + 1];
                if (center == null) continue;

                if (CheckLShape(row, col, uniqueMatches, center.type, specialCandies))
                {
                    AddScore(500);
                }
                if (CheckTShape(row, col, uniqueMatches, center.type, specialCandies))
                {
                    AddScore(500);
                }
            }
        }

        matches.AddRange(uniqueMatches);

        // Tạo kẹo đặc biệt
        Debug.Log(specialCandies.Count);
        foreach (var special in specialCandies)
        {
            Vector2Int pos = special.pos;
            if (matches.Contains(candyGrid[pos.x, pos.y]))
            {
                matches.Remove(candyGrid[pos.x, pos.y]);
            }
        }

        return matches;
    }
    bool CheckLShape(int row, int col, HashSet<Candy> matches, CandyType centerType, List<(Vector2Int pos, Candy.SpecialType type)> specialCandies)
    {
        if (row + 2 >= gridHeight || col + 2 >= gridWidth) return false;

        bool TryMatchPattern(int[] rows, int[] cols, Vector2Int wrappedPos)
        {
            for (int i = 0; i < 5; i++)
            {
                int r = row + rows[i];
                int c = col + cols[i];
                if (r < 0 || r >= gridHeight || c < 0 || c >= gridWidth || candyGrid[r, c] == null || candyGrid[r, c].type != centerType)
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

    bool CheckTShape(int row, int col, HashSet<Candy> matches, CandyType centerType, List<(Vector2Int pos, Candy.SpecialType type)> specialCandies)
    {
        if (row + 2 >= gridHeight || col + 2 >= gridWidth) return false;

        bool TryMatchPattern(int[] rows, int[] cols, Vector2Int wrappedPos)
        {
            for (int i = 0; i < 5; i++)
            {
                int r = row + rows[i];
                int c = col + cols[i];
                if (r < 0 || r >= gridHeight || c < 0 || c >= gridWidth || candyGrid[r, c] == null || candyGrid[r, c].type != centerType)
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

    bool CheckLShape(int row, int col)
    {
        if (row + 2 >= gridHeight || col + 2 >= gridWidth) return false;

        Candy center = candyGrid[row + 1, col + 1];
        if (center == null) return false;

        bool[,] patterns = new bool[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Candy candy = candyGrid[row + i, col + j];
                patterns[i, j] = candy != null && candy.type == center.type;
            }
        }
        //L
        if (patterns[0, 0] && patterns[0, 1] && patterns[0, 2] && patterns[1, 0] && patterns[2, 0])
        {
            return true;
        }
        if (patterns[0, 0] && patterns[0, 1] && patterns[0, 2] && patterns[1, 2] && patterns[2, 2])
        {
            return true;
        }
        if (patterns[2, 0] && patterns[2, 1] && patterns[0, 2] && patterns[1, 0] && patterns[2, 0])
        {
            return true;
        }
        if (patterns[2, 0] && patterns[2, 1] && patterns[0, 2] && patterns[1, 2] && patterns[2, 2])
        {
            return true;
        }
        //T
        if (patterns[0, 0] && patterns[0, 1] && patterns[0, 2] && patterns[1, 1] && patterns[2, 1])
        {
            return true;
        }
        if (patterns[1, 0] && patterns[1, 1] && patterns[1, 2] && patterns[0, 2] && patterns[2, 2])
        {
            return true;
        }
        if (patterns[2, 0] && patterns[2, 1] && patterns[0, 2] && patterns[1, 1] && patterns[2, 1])
        {
            return true;
        }
        if (patterns[1, 0] && patterns[1, 1] && patterns[1, 2] && patterns[0, 2] && patterns[2, 2])
        {
            return true;
        }

        return false;
    }

    void AddLShapeMatches(HashSet<Candy> matches, int row, int col)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (candyGrid[row + i, col + j] != null)
                {
                    matches.Add(candyGrid[row + i, col + j]);
                }
            }
        }
    }

    void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
    }

    public void ClearRow(int row)
    {
        for (int col = 0; col < gridWidth; col++)
        {
            if (candyGrid[row, col] != null)
            {
                StartCoroutine(ShrinkCandy(candyGrid[row, col]));
                candyGrid[row, col] = null;
            }
        }
        StartCoroutine(FillEmptySpaces());
        AddScore(200);
        StartCoroutine(ProcessMatches());
    }

    public void ClearColumn(int col)
    {
        for (int row = 0; row < gridHeight; row++)
        {
            if (candyGrid[row, col] != null)
            {
                StartCoroutine(ShrinkCandy(candyGrid[row, col]));
                candyGrid[row, col] = null;
            }
        }
        StartCoroutine(FillEmptySpaces());
        AddScore(200);
        StartCoroutine(ProcessMatches());
    }

    public void ClearArea(int row, int col)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int r = row + i;
                int c = col + j;
                if (r >= 0 && r < gridHeight && c >= 0 && c < gridWidth && candyGrid[r, c] != null)
                {
                    StartCoroutine(ShrinkCandy(candyGrid[r, c]));
                    candyGrid[r, c] = null;
                }
            }
        }
        StartCoroutine(FillEmptySpaces());
        AddScore(300);
        StartCoroutine(ProcessMatches());
    }

    IEnumerator ProcessMatches()
    {
        List<Candy> matches = CheckMatches();
        if (matches.Count == 0) yield break;

        foreach (Candy candy in matches)
        {
            if (candy != null && !candy.isSpecial)
            {
                StartCoroutine(ShrinkCandy(candy));
            }
        }
        Debug.Log(specialCandies.Count);
        yield return new WaitForSeconds(0.3f);

        ClearMatches(matches);
        foreach (var special in specialCandies)
        {
            Vector2Int pos = special.pos;
            ReplaceWithSpecialCandy(pos.x, pos.y, special.type);
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
        float shrinkTime = 0.3f;
        float elapsed = 0f;
        Vector3 originalScale = candy.transform.localScale;

        while (elapsed < shrinkTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkTime;
            candy.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }
        Destroy(candy.gameObject);
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
}