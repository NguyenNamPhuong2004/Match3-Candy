using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLevelPanel : ButtonAbstract
{
    public override void OnClick()
    {
        MenuManager.Ins.OpenLevelPanel();
    }
}
