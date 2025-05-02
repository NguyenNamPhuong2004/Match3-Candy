using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenGameSetting : ButtonAbstract
{
    public override void OnClick()
    {
        UIManager.Ins.OpenSetting();
    }
}
