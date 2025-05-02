using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenMenuSetting : ButtonAbstract
{
    public override void OnClick()
    {
        MenuManager.Ins.OpenSetting();
    }
}
