using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHandler : MonoBehaviour
{
    public Transform W,A,S,D,UP,DOWN,LEFT,RIGHT;

    void Update()
    {
        PressButton(W, Input.GetKey(KeyCode.W));
        PressButton(A, Input.GetKey(KeyCode.A));
        PressButton(S, Input.GetKey(KeyCode.S));
        PressButton(D, Input.GetKey(KeyCode.D));
        PressButton(UP, Input.GetKey(KeyCode.UpArrow));
        PressButton(DOWN, Input.GetKey(KeyCode.DownArrow));
        PressButton(LEFT, Input.GetKey(KeyCode.LeftArrow));
        PressButton(RIGHT, Input.GetKey(KeyCode.RightArrow));
    }

    void PressButton(Transform key, bool pressed)
    {
        if(pressed)
        {
            key.localScale = new Vector3(.9f,.9f, .2f);
        }
        else
        {
            key.localScale = new Vector3(1,1,1);
        }
    }
}
