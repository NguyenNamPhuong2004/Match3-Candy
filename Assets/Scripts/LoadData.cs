using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadData : MonoBehaviour
{
    protected virtual void Reset()
    {
        LoadComponents();
        ResetValue();
    }
    protected virtual void Awake()
    {
        LoadComponents();
        ResetValue();
    }
    protected virtual void ResetValue()
    {
        
    }

    protected virtual void LoadComponents()
    {
        
    }
}
