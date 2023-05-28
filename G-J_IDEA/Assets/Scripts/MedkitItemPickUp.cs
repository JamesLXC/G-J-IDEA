using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MedkitItemPickUp : Interactable
{
    [Header("Medkit Item Pick Up Parameters")]
    [SerializeField] private int itemAmount = 3;
    [SerializeField] private int itemSizevalue = 5;

    [Header("Pick up Audio Parameters")]
    [SerializeField] private AudioSource pickUpAudioSource = default;
    [SerializeField] private AudioClip pickUpClip = default;
    

    private void Awake()
    {
        pickUpAudioSource.clip = pickUpClip;
    }

    public override void OnFocus()
    {
        print(FirstPersonController.currentItems);
    }

    public override void Oninteract()
    {
        if (FirstPersonController.itemsMax > FirstPersonController.currentItems &&
           (FirstPersonController.currentItems + itemSizevalue) <= FirstPersonController.itemsMax)
        {
            print("added");
            Destroy(GetComponent<MeshRenderer>());
            Destroy(GetComponent<BoxCollider>());
            print("Grabbed Medkit");
            FirstPersonController.pickUpItemSize(itemSizevalue);
            FirstPersonController.pickUpMedkit(itemAmount);
            pickUpAudioSource?.Play();
            Destroy(gameObject, 2);
        }
        else
        {
            print("No add");
           print("Pockets full");
        }   
        
    }
     
    public override void OnLoseFocus()
    {
    }

}
