using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable

    
{
    public Animator MedBoxAnim;
    public bool isShut;

    public void Start()
    {
        isShut = true;
    }
    public override void OnFocus()
    {
    }

    public override void Oninteract()
    {
       if(isShut == true)
        {
            MedBoxAnim.SetBool("DoorOpen", true);
            isShut = false;
        }

        if (isShut == false)
        {
            MedBoxAnim.SetBool("DoorOpen", false);
            isShut = true;

            
        }
    }

    public override void OnLoseFocus()
    {
    print("Stopped Kissing " + gameObject.name);
    }


}
