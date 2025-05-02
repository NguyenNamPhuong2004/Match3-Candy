using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToMenu : ButtonAbstract
{
    public override void OnClick()
    {
        UIManager.Ins.ReturnToMenu();
    }
}
