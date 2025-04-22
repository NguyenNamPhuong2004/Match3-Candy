using UnityEngine;
using System.Collections;

public class Candy : MonoBehaviour
{
    public enum SpecialType { None, StripedHorizontal, StripedVertical, Wrapped }
    public CandyType type;
    public SpecialType specialType = SpecialType.None;
    public int row { get; set; }
    public int column { get; set; }
    public bool isSpecial = false;
    public bool isMatched = false;

    private void OnMouseDown()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SelectCandy(this);
        }
    }

    public void TriggerSpecialEffect()
    {
        if (!isSpecial) return;

        switch (specialType)
        {
            case SpecialType.StripedHorizontal:
                GameManager.instance.ClearRow(row);
                break;
            case SpecialType.StripedVertical:
                GameManager.instance.ClearColumn(column);
                break;
            case SpecialType.Wrapped:
                GameManager.instance.ClearArea(row, column);
                break;
        }
    }

    public void MoveTo(Vector3 target)
    {
        StartCoroutine(MoveCoroutine(target));
    }

    IEnumerator MoveCoroutine(Vector3 target)
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
}