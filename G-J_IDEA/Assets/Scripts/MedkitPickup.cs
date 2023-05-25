using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedkitPickup : Interactable
{
    [SerializeField] private bool emptyBox = false;
    [SerializeField] private int maxItemsLeft = 3;
    private int currentItemsLeft;

    void Awake()
    {
     currentItemsLeft = (UnityEngine.Random.Range(0, maxItemsLeft));
    }
    public override void OnFocus()
    {

    }

    public override void Oninteract()
    {
        if(currentItemsLeft > 0)
        {
            currentItemsLeft -= 1;
            FirstPersonController.pickUpMedkit(1);
            print("Grabbed Medkit");
            print(currentItemsLeft + " Medkits Remaining");
        }
        else
        {
            emptyBox = true;
            print("Box is Empty");
        }
   
    }

    public override void OnLoseFocus()
    {
    
    }


}
