using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : Interactable
{
    public override void OnFocus()
    {
        print("Kissed " + gameObject.name);
    }

    public override void Oninteract()
    {
        print("Smooched " + gameObject.name);
    }

    public override void OnLoseFocus()
    {
        print("Stopped Kissing " + gameObject.name);
    }

}
