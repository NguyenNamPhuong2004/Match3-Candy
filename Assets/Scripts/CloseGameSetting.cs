using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseGameSetting : ButtonAbstract
{
    public override void OnClick()
    {
        UIManager.Ins.CloseSetting();
    }
}
