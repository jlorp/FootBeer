using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabPusher : MonoBehaviour
{
    public bool set = false;
    public HandMover _hand;

    public void SetTab()
    {
        set = _hand.SetTab();
    }
}