using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGame : ButtonAbstract
{
    public override void OnClick()
    {
        MenuManager.Ins.QuitGame();
    }
}
