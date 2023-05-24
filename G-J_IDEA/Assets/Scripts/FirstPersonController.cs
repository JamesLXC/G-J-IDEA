using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
 public bool CanMove {get; private set; } = true;

 [Header("Movement Parameters")]
 [SerializeField] private float walkSpeed = 3.0f;
 [SerializeField] private float gravity = 30.0f;

 [Header("look Parameters")]
 [SerializeField, Range (1,10)] private float lookSpeedX = 2.0f;
 [SerializeField, Range (1,10)] private float lookSpeedY = 2.0f;
 [SerializeField, Range (1,180)] private float upperLookLimit = 80.0f;
 [SerializeField, Range (1,80)] private float lowerLookLimit = 80.0f;

 private Camera playerCamera;
 private CharacterController characterController;

 private Vector3 moveDirection;
 private Vector2 currentIput;

 private float roationX = 0;
   
  void Awake()
   {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
   }

    private void Update()
    {
        if (CanMove)
        {
             HandleMovementInput();
             HandleMOuseLook();

             ApplyFinalMovements();

        }
    }
    private void HandleMovementInput()
    {
        currentIput = new Vector2(walkSpeed * Input.GetAxis("Vertical"), walkSpeed * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentIput.x) + (transform.TransformDirection(Vector3.right) * currentIput.y);
        moveDirection.y = moveDirectionY;

    }


    private void HandleMOuseLook()
    {
        roationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        roationX = Mathf.Clamp(roationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(roationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }


    private void ApplyFinalMovements()
    {
        if(!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }
}
