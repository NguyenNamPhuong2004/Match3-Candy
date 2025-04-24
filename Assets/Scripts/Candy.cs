using UnityEngine;
using System.Collections;

public class Candy : MonoBehaviour
{
    public enum SpecialType { None, StripedHorizontal, StripedVertical, Wrapped, ColorBomb }
    public CandyType type;
    public SpecialType specialType = SpecialType.None;
    public int row;
    public int column;
    public bool isSpecial = false;
    public bool isMatched = false;
    private bool isTriggered = false;

    private void OnMouseDown()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SelectCandy(this);
        }
    }

    public void TriggerSpecialEffect(Candy otherCandy = null)
    {
        if (!isSpecial || isTriggered) return; 
        isTriggered = true; 

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
            case SpecialType.ColorBomb:
                if (otherCandy != null)
                {
                    GameManager.instance.ClearColorType(otherCandy.type);
                }
                else
                {
                    CandyType[] types = (CandyType[])System.Enum.GetValues(typeof(CandyType));
                    GameManager.instance.ClearColorType(types[Random.Range(0, types.Length)]);
                }
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
    public void ResetTrigger()
    {
        isTriggered = false;
    }
}