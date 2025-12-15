using UnityEngine;

public class MovementManager : MonoBehaviour

{

    public Transform cameraTransform;

    public CharacterController controller;

    public float moveSpeed = 6f;

    public float sprintSpeed = 10f;

    public float jumpHeight = 1.5f;

    public float gravity = 9.81f;

    public float maxSprint = 5f;

    public float regenTime = 5f;

    public float currentSprint;

    public float groundCheckDistance = 0.2f;

    public LayerMask groundMask = -1;

    private Vector3 velocity;

    private bool isGrounded;

    private bool isSprinting = false;

    private SprintBar sprintBar;

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

    }

    void Update()

    {

        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + controller.height / 2f, groundMask);

        if (isGrounded && velocity.y < 0)

            velocity.y = -2f;

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

        if (Input.GetButtonDown("Jump") && isGrounded)

            velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);

        velocity.y -= gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (sprintBar != null)

            sprintBar.UpdateSprintBar(maxSprint, currentSprint);

    }

}
 