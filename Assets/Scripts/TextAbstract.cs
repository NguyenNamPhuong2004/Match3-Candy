using UnityEngine;
using UnityEngine.UI;

public abstract class TextAbstract : LoadData
{
    [SerializeField] protected Text text;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadText();
    }

    protected virtual void LoadText()
    {
        if (this.text != null) return;
        this.text = GetComponent<Text>();
        Debug.Log(transform.name + ": LoadText", gameObject);
    }
}
