using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseMenuSetting : ButtonAbstract
{
    public override void OnClick()
    {
        MenuManager.Ins.CloseSetting();
    }
}
