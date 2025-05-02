using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwapManager : Singleton<SwapManager>
{
    private Candy[,] candyGrid;
    private Candy firstSelectedCandy;
    private bool isProcessing;

    public void Initialize()
    {
        candyGrid = GridManager.Ins.candyGrid;
        Debug.Log("SwapManager initialized");
    }

    void Update()
    {
        if (isProcessing || Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Candy candy = hit.collider.GetComponent<Candy>();
                if (candy != null)
                {
                    SelectCandy(candy);
                }
            }
        }
    }

    public void SelectCandy(Candy candy)
    {
        if (isProcessing || candy.isLocked) return;
        SoundManager.Ins.ClickCandySound();
        if (firstSelectedCandy == null)
        {
            firstSelectedCandy = candy;
            candy.transform.localScale = Vector3.one * 0.7f;
        }
        else
        {
            if (IsAdjacent(firstSelectedCandy, candy) && !candy.isLocked)
            {
                StartCoroutine(ProcessSwap(firstSelectedCandy, candy));
            }
            else
            {
                firstSelectedCandy.transform.localScale = Vector3.one * 0.5f;
                firstSelectedCandy = candy;
                candy.transform.localScale = Vector3.one * 0.7f;
            }
        }
    }

    public bool IsAdjacent(Candy candy1, Candy candy2)
    {
        return Mathf.Abs(candy1.row - candy2.row) + Mathf.Abs(candy1.column - candy2.column) == 1;
    }

    private IEnumerator ProcessSwap(Candy candy1, Candy candy2)
    {
        isProcessing = true;
        yield return StartCoroutine(AnimateSwap(candy1, candy2));
        SwapCandies(candy1, candy2);

        bool hasSpecialEffect = candy1.isSpecial || candy2.isSpecial;
        List<Candy> matches = MatchManager.Ins.CheckMatches();

        if (hasSpecialEffect)
        {
            LevelManager.Ins.OnSwap();
            UIManager.Ins.UpdateUI();
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
            yield return StartCoroutine(SpecialCandyManager.Ins.ProcessSpecialCandy(candy1, candy2));
            yield return StartCoroutine(GameStateManager.Ins.FillEmptySpaces());
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
        }
        else if (matches.Count > 0)
        {
            LevelManager.Ins.OnSwap();
            UIManager.Ins.UpdateUI();
            Debug.Log($"ProcessSwap: matches={matches.Count}");
            yield return StartCoroutine(GameStateManager.Ins.ProcessMatches());
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

    private IEnumerator AnimateSwap(Candy candy1, Candy candy2)
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

    private void SwapCandies(Candy candy1, Candy candy2)
    {
        candyGrid[candy1.row, candy1.column] = candy2;
        candyGrid[candy2.row, candy2.column] = candy1;

        int tempRow = candy1.row;
        int tempCol = candy1.column;
        candy1.row = candy2.row;
        candy1.column = candy2.column;
        candy2.row = tempRow;
        candy2.column = tempCol;
        SoundManager.Ins.SwapCandySound();
    }
}