using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovementManager : MonoBehaviour
{
    public Transform cameraTransform;
    public CharacterController controller;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float gravity = 9.81f;

    [Header("Sprint")]
    public float maxSprint = 5f;
    public float regenTime = 5f;
    public float currentSprint;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask = -1;

    [Header("Footsteps")]
    public AudioSource footstepSource;
    public AudioClip footstepClip;
    public float walkStepInterval = 0.55f;
    public float sprintStepInterval = 0.38f;
    public float footstepVolume = 0.65f;
    public Vector2 walkPitchRange = new Vector2(0.95f, 1.05f);
    public Vector2 sprintPitchRange = new Vector2(1.15f, 1.25f);

    private Vector3 velocity;
    private bool isGrounded;
    private bool isSprinting = false;
    private SprintBar sprintBar;
    private float footstepTimer = 0f;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>().transform;

        if (controller == null)
            controller = GetComponent<CharacterController>();

        cameraTransform.position = new Vector3(transform.position.x, transform.position.y + 10f, transform.position.z);
        cameraTransform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        currentSprint = maxSprint;
        sprintBar = FindObjectOfType<SprintBar>();

        if (footstepSource == null)
            footstepSource = GetComponent<AudioSource>();

        if (footstepSource != null)
        {
            footstepSource.loop = false;
            footstepSource.playOnAwake = false;
            footstepSource.clip = footstepClip;
        }
    }

    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + controller.height / 2f, groundMask); //Raycast down to check grounded

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }   
            
        //Inputs
        float moveZ = Input.GetAxis("Vertical");
        float moveX = Input.GetAxis("Horizontal");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);   
        
        if (wantsToSprint && currentSprint > 0f && move.magnitude > 0.1f)
        {
            isSprinting = true;
            currentSprint -= Time.deltaTime;           
        }
        else
        {
            isSprinting = false;
        }
             
        if (!isSprinting && currentSprint < maxSprint)
            currentSprint += Time.deltaTime * (maxSprint / regenTime);

        currentSprint = Mathf.Clamp(currentSprint, 0f, maxSprint);

        float speed = isSprinting ? sprintSpeed : moveSpeed;
        controller.Move(move * speed * Time.deltaTime);



        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (sprintBar != null)
            sprintBar.UpdateSprintBar(maxSprint, currentSprint);

        HandleFootsteps(move);
    }

    void HandleFootsteps(Vector3 move)
    {
        if (!isGrounded || move.magnitude <= 0.1f)
        {
            footstepTimer = 0f;
            return;
        }

        float interval = isSprinting ? sprintStepInterval : walkStepInterval;
        footstepTimer += Time.deltaTime;

        if (footstepTimer >= interval && footstepSource != null && footstepClip != null && !footstepSource.isPlaying)
        {
            footstepSource.clip = footstepClip;
            footstepSource.volume = footstepVolume;
            Vector2 range = isSprinting ? sprintPitchRange : walkPitchRange;
            footstepSource.pitch = Random.Range(range.x, range.y);
            footstepSource.Play();
            footstepTimer = 0f;
        }
    }
}