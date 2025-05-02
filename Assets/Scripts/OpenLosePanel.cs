using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLosePanel : ButtonAbstract
{
    public override void OnClick()
    {
        UIManager.Ins.OpenLosePanel();
    }
}
