using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable

{
    public Animator MedBoxAnim;
    [SerializeField] private bool isShut = true;
    public override void OnFocus()
    {
    }
    public override void Oninteract()
    {
        if (isShut)
        {
            print("Opened " + gameObject.name);
            MedBoxAnim.SetBool("DoorOpen", true);
            isShut = false;
        }
        else
        {
            print("Shut " + gameObject.name);
            MedBoxAnim.SetBool("DoorOpen", false);
            isShut = true;
        }

       // if (!isShut)
       // {
         
       // }
        
    }      

    public override void OnLoseFocus()
    {
   
    }


}
