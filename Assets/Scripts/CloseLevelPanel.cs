using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseLevelPanel : ButtonAbstract
{
    public override void OnClick()
    {
        MenuManager.Ins.CloseLevelPanel();
    }
}
