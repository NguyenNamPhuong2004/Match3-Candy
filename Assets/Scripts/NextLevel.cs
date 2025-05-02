using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevel : ButtonAbstract
{
    public override void OnClick()
    {
        UIManager.Ins.NextLevel();
    }
}
