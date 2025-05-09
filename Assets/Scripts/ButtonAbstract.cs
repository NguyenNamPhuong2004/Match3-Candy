using UnityEngine;
using UnityEngine.UI;

public abstract class ButtonAbstract : LoadData
{
    [SerializeField] protected Button button;

    protected override void Awake()
    {
        base.Awake();
        this.AddOnClickEvent();
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadButton();
    }

    protected virtual void LoadButton()
    {
        if (this.button != null) return;
        this.button = GetComponent<Button>();
        Debug.Log(transform.name + ": LoadButton", gameObject);
    }

    protected virtual void AddOnClickEvent()
    {
        this.button.onClick.AddListener(this.OnClick);
    }

    public abstract void OnClick();
}
