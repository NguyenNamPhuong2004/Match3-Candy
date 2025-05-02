using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restart : ButtonAbstract
{
    public override void OnClick()
    {
        UIManager.Ins.Restart();
    }
}
