using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenWinPanel : ButtonAbstract
{
    public override void OnClick()
    {
        UIManager.Ins.OpenWinPanel();
    }
}
