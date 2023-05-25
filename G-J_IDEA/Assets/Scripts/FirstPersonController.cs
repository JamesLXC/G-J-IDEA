using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool ShouldCrouch => Input.GetKeyDown(CrouchKey) && !duringCrouchingAnimation;
    private bool ShouldHeal => Input.GetKeyDown(healKey) && characterController.isGrounded;


    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool willSlideOnSlopes = true;
    [SerializeField] private bool canZoom = true;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool useStam = true;
    [SerializeField] private bool canHeal = true;
    [SerializeField] private bool useFootsteps = true;



    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode CrouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode InteractKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode healKey = KeyCode.F;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float CrouchSpeed = 1.5f;
    [SerializeField] private float SlopeSpeed = 8f;

    [Header("Footstep Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMulti = 1.5f;
    [SerializeField] private float sprintStepMulti = 0.6f;
    [SerializeField] private AudioSource footStepAudioSource = default;
    [SerializeField] private AudioClip[] defaultClips = default;
    [SerializeField] private AudioClip[] woodClips = default;
    [SerializeField] private AudioClip[] gravelClips = default;
    [SerializeField] private AudioClip[] tileClips = default;
    private float footstepTimer = 0;
    private float getCurrentoffset => isCrouching ? baseStepSpeed * crouchStepMulti : IsSprinting ? baseStepSpeed * sprintStepMulti : baseStepSpeed;



    [Header("look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 80)] private float lowerLookLimit = 80.0f;

    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100;
    private float currentHealth;
    public static Action<float> OnTakeDamage;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;

    [Header("Medkit Parameters")]
    [SerializeField] private float medkitMax = 3;
    [SerializeField] private float medkitHealAmount = 35;
    private float currentMedkit;
    public static Action<float> pickUpMedkit;


    [Header("Stamina Parameters")]
    [SerializeField] private float maxStam = 100;
    [SerializeField] private float stamUseMultiplier = 5;
    [SerializeField] private float timeBeforeStamRegenStarts = 5;
    [SerializeField] private float stamValueIncrement = 2;
    [SerializeField] private float stamTimeIncrement = 0.1f;
    private float currentStam;
    private Coroutine regeneratingStam;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float CrouchHeight = 0.5f;
    [SerializeField] private float StandingHeight = 2f;
    [SerializeField] private float TimeToCrouch = 0.25f;
    [SerializeField] private Vector3 CrouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 StandingCenter = new Vector3(0, 0f, 0);
    private bool isCrouching;
    private bool duringCrouchingAnimation;

    [Header("Headbob Paramaeters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float SprintBobSpeed = 18f;
    [SerializeField] private float SprintBobAmount = 0.1f;
    [SerializeField] private float CrouchBobSpeed = 8f;
    [SerializeField] private float CrouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float timer;

    [Header("Zoom Parameters")]
    [SerializeField] private float TimeToZoom = 0.3f;
    [SerializeField] private float ZoomFOV = 30f;
    private float defaultFOV;
    private Coroutine zoomRoutine;

    // Slider Prop

    private Vector3 hitPointNormal;

    private bool IsSliding
    {
        get
        {
            if (characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float InteractionDistance = default;
    [SerializeField] private LayerMask InteractionLayer = default;
    private Interactable currentInteractable;



    private Camera playerCamera;
    private CharacterController characterController;
    private Vector3 moveDirection;
    private Vector2 currentIput;

    private float roationX = 0;

    private void OnEnable()
    {
        OnTakeDamage += ApplyDamage;
        pickUpMedkit += GetMedkit; 
    }

    private void OnDisable()
    {
        OnTakeDamage -= ApplyDamage;
        pickUpMedkit -= GetMedkit;
    }



    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        currentHealth = maxHealth;
        currentStam = maxStam;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMOuseLook();

            if (canJump)
            {
                HandleJump();
            }

            if (canCrouch)
            {
                HandleCrouch();
            }
            if (canUseHeadbob)
            {
                HandleHeadbob();
            }

            if (canInteract)
            {
                HandleInteractCheck();
                HandleInteractInput();
            }

            if (useFootsteps)
                HandleFootsteps();

            if (canZoom)
            {
                HandleZoom();
            }
                
            if (useStam)
                HandleStam();

            if (canHeal)
                HandleHeal();

            ApplyFinalMovements();
        }
    }
    private void HandleMovementInput()
    {
        currentIput = new Vector2((isCrouching ? CrouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? CrouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

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

    private void HandleJump()
    {
        if (ShouldJump)
            moveDirection.y = jumpForce;
    }
    private void HandleCrouch()
    {
        if (ShouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1 || Mathf.Abs(moveDirection.z) > 0.1)
        {
            timer += Time.deltaTime * (isCrouching ? CrouchBobSpeed : IsSprinting ? SprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
              playerCamera.transform.localPosition.x,
              defaultYPos + Mathf.Sin(timer) * (isCrouching ? CrouchBobAmount : IsSprinting ? SprintBobAmount : walkBobAmount),
              playerCamera.transform.localPosition.z);
        }

    }
    private void HandleStam() 
    {
    if(IsSprinting && currentIput != Vector2.zero) 
        {
         // gets here
            if (regeneratingStam != null)
            {
                StopCoroutine(regeneratingStam);
                regeneratingStam = null;
            }
        currentStam -= stamUseMultiplier * Time.deltaTime;

            if (currentStam < 0)
                currentStam = 0;

            if(currentStam <= 0)
                canSprint = false;
        
        }
        if (!IsSprinting && currentStam < maxStam && regeneratingStam == null)
        {

            regeneratingStam = StartCoroutine(regenStam());
        }

    }


    private void HandleZoom()
    {
        if (Input.GetKeyDown(zoomKey))
        {
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;

            }
            zoomRoutine = StartCoroutine(ToggleZoom(true));
        }

        if (Input.GetKeyUp(zoomKey))
        {
            
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;

            }
            zoomRoutine = StartCoroutine(ToggleZoom(false));
        }
    }

    private void HandleHeal()
    {
        if (Input.GetKeyDown(healKey) )
        {

            if (currentMedkit > 0 && currentHealth < maxHealth)
            {
                print("Healed");
                currentMedkit -= 1;
                currentHealth += medkitHealAmount;
            }
            else
                print("No Meds or no health to heal");
            if (currentHealth >= maxHealth)
                currentHealth = maxHealth;
            
        }
        

    }
    private void HandleInteractCheck()
    {
        if(Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit Hit, InteractionDistance))
        {
            if(Hit.collider.gameObject.layer == 7 && (currentInteractable == null || Hit.collider.gameObject.GetInstanceID() != currentInteractable.GetInstanceID() ))
            {
                Hit.collider.TryGetComponent(out currentInteractable);

                if (currentInteractable)
                    currentInteractable.OnFocus();
            }
        }
        else if (currentInteractable)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    private void HandleInteractInput()
    {
        if (Input.GetKeyDown(InteractKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, InteractionDistance, InteractionLayer))
        {
            currentInteractable.Oninteract();
        }
    }


    private void ApplyDamage(float dmg)
    {
        currentHealth -= dmg;

        if(currentHealth <= 0)
           KillPLayer();
    }

    private void GetMedkit(float medkitAmount)
    {
        if (currentMedkit >= medkitMax)
            return;
        currentMedkit += medkitAmount;
    }
    private void KillPLayer()
    {
        currentHealth = 0;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        print("Joey please jesus christ fuck");
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        if (willSlideOnSlopes && IsSliding)
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * SlopeSpeed;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleFootsteps()
    {
        if (!characterController.isGrounded) return;
        if (currentIput == Vector2.zero) return;

        footstepTimer -= Time.deltaTime;

        if(footstepTimer <= 0)
        {
            
            int layerMask = (-1) - (1 << LayerMask.NameToLayer("Player"));
            if (Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3, layerMask))
            {
                switch(hit.collider.tag)
                {
                    case "Footsteps/Wood":
                        footStepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0, woodClips.Length - 1)]);
                        break;
                    case "Footsteps/Tile":
                        footStepAudioSource.PlayOneShot(tileClips[UnityEngine.Random.Range(0, tileClips.Length - 1)]);
                        break;
                    case "Footsteps/Gravel":
                        footStepAudioSource.PlayOneShot(gravelClips[UnityEngine.Random.Range(0, gravelClips.Length - 1)]);
                        break;
                    default:
                        footStepAudioSource.PlayOneShot(defaultClips[UnityEngine.Random.Range(0, defaultClips.Length - 1)]);
                        break;


                }
 
            }

            footstepTimer = getCurrentoffset; 
        }
    }

    private IEnumerator CrouchStand()
    {
        if(isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;

        duringCrouchingAnimation = true;

        float timeElapsed = 0f;
        float targetHeight = isCrouching ? StandingHeight : CrouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? StandingCenter : CrouchingCenter;
        Vector3 currentCenter = characterController.center;

        while (timeElapsed < TimeToCrouch) 
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/TimeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/TimeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;

        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchingAnimation = false;
    }

    private IEnumerator ToggleZoom(bool isEnter)
    {
        
        float targeFOV = isEnter ? ZoomFOV : defaultFOV;
        float startingFOV = playerCamera.fieldOfView;
        float timeElapsed = 0;

        while(timeElapsed < TimeToZoom)
        {
            playerCamera.fieldOfView = Mathf.Lerp(startingFOV, targeFOV, timeElapsed/TimeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.fieldOfView = targeFOV;
        zoomRoutine = null;
    }

    
    private IEnumerator regenStam()
    {
        yield return new WaitForSeconds(timeBeforeStamRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(stamTimeIncrement);

        while (currentStam < maxStam)
        {

            if(currentStam > 0)
                canSprint = true;

            currentStam += stamValueIncrement;

            if(currentStam > maxStam)
                currentStam = maxStam;

            yield return timeToWait;
                
        }
    regeneratingStam = null;
    
    }
}
