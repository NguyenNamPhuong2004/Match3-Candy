using UnityEngine;
using System.Collections;

public class Candy : MonoBehaviour
{
    public CandyType type;
    public SpecialCandyType specialType = SpecialCandyType.None;
    public int row;
    public int column;
    public bool isSpecial = false;
    public bool isMatched = false;
    public bool isLocked = false;
    private bool isTriggered = false;

    private void OnMouseDown()
    {
        if (!isLocked && SwapManager.Ins != null)
        {
            SwapManager.Ins.SelectCandy(this);
        }
    }

    public void MoveTo(Vector3 target)
    {
        StartCoroutine(MoveCoroutine(target));
    }

    private IEnumerator MoveCoroutine(Vector3 target)
    {
        float t = 0;
        Vector3 start = transform.position;
        while (t < 1)
        {
            t += Time.deltaTime / 0.2f;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.position = target;
    }

    public void ResetTrigger()
    {
        isTriggered = false;
    }
}