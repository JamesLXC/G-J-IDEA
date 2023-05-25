using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedkitPickup : Interactable
{
    [Header("Medkit Container Settings")] 
    [SerializeField] private int maxItems = 3;
    [SerializeField] private int minItems = 1;
    private int currentItemsLeft;
#pragma warning disable
    private bool emptyBox = false;
#pragma warning restore


    void Awake()
    {
     currentItemsLeft = (UnityEngine.Random.Range(minItems, maxItems));
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
