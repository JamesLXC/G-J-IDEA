using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual void awake () {
        gameObject.layer = 7;
    }
    public abstract void Oninteract();
    public abstract void OnFocus();
    public abstract void OnLoseFocus();
}
