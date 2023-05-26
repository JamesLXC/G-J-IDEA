using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 public class MedkitItemPickUp : Interactable
{
    [Header("Medkit Item Pick Up parameters")]
    [SerializeField] private AudioSource pickUpAudioSource = default;
    [SerializeField] private AudioClip pickUpClip = default;

    private void Awake()
    {
        pickUpAudioSource.clip = pickUpClip;
    }

    public override void OnFocus()
    {

    }

    public override void Oninteract()
    {
        Destroy(GetComponent<MeshRenderer>());
        Destroy(GetComponent<BoxCollider>());
        print("Grabbed Medkit");
        FirstPersonController.pickUpMedkit(1);
        pickUpAudioSource?.Play();
        Destroy(gameObject, 2);
    }

    public override void OnLoseFocus()
    {
    }

}
