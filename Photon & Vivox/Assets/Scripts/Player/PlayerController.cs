using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Transform cameraTransform;
    public float gravity = -9.81f;
    [Header("Movement Variables")]
    public float cameraSensitivity;
    public float moveSpeed;
    public float moveInputDeadZone;
    [Header("Gravity")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    [Header("Sliders")]
    public Slider cameraSensitivitySlider;
    public Slider moveSpeedSlider;
    public Slider moveInputDeadZoneSlider;
    [HideInInspector] public Animator animator;

    private int leftFingerID, rightFingerID;
    private float halfScreenWidth;
    private float cameraPitch = 0f;
    private bool isGrounded;
    private Vector2 lookInput;
    private Vector2 moveTouchStartPosition;
    private Vector2 moveInput;
    private Vector3 velocity;
    private CharacterController characterController;

    private void Awake()
    {
        if (!GetComponent<PhotonView>().IsMine)
        {
            Destroy(this.transform.GetChild(4).gameObject);
            Destroy(GetComponent<PlayerController>());
        }

        characterController = GetComponent<CharacterController>();

        leftFingerID = -1;
        rightFingerID = -1;

        halfScreenWidth = Screen.width / 2;

        moveInputDeadZone = Mathf.Pow(Screen.height / moveInputDeadZone, 2);
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
        }

        GetTouchInput();

        if (rightFingerID != -1)
        {
            LookAround();
        }
        if (leftFingerID != -1)
        {
            Move();
        }
    }

    #region Movement
    private void GetTouchInput()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            switch (t.phase)
            {
                case TouchPhase.Began:
                    if (t.position.x < halfScreenWidth && leftFingerID == -1)
                    {
                        leftFingerID = t.fingerId;
                        moveTouchStartPosition = t.position;
                    }
                    else if (t.position.x > halfScreenWidth && rightFingerID == -1)
                    {
                        rightFingerID = t.fingerId;
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (t.fingerId == leftFingerID)
                    {
                        leftFingerID = -1;
                        if (animator != null && animator.GetBool("Walking") == true)
                        {
                            animator.SetBool("Walking", false);
                        }
                    }
                    else if (t.fingerId == rightFingerID)
                    {
                        rightFingerID = -1;
                    }
                    break;
                case TouchPhase.Moved:
                    if (t.fingerId == rightFingerID)
                    {
                        lookInput = t.deltaPosition * cameraSensitivity * Time.deltaTime;
                    }
                    else if (t.fingerId == leftFingerID)
                    {
                        moveInput = t.position - moveTouchStartPosition;
                    }
                    break;
                case TouchPhase.Stationary:
                    if (t.fingerId == rightFingerID)
                    {
                        lookInput = Vector2.zero;
                    }
                    if (t.fingerId == leftFingerID)
                    {
                        if (animator != null && animator.GetBool("Walking") == true)
                        {
                            animator.SetBool("Walking", false);
                        }
                    }
                    break;
            }
        }
    }

    private void LookAround()
    {
        cameraPitch = Mathf.Clamp(cameraPitch - lookInput.y, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, cameraTransform.localRotation.y, 0f);
        
        transform.Rotate(transform.up, lookInput.x);
    }

    private void Move()
    {
        if (moveInput.sqrMagnitude <= moveInputDeadZone)
            return;

        if (animator != null && animator.GetBool("Walking") == false)
        {
            animator.SetBool("Walking", true);
        }

        Vector3 movementDirection = moveInput.normalized * moveSpeed * Time.deltaTime;
        characterController.Move(transform.right * movementDirection.x + transform.forward * movementDirection.y);


        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    #endregion

    #region Options Menu
    public void UpdateCameraSensitivity()
    {
        cameraSensitivity = cameraSensitivitySlider.value;
    }

    public void UpdateMovementSensitivity()
    {
        float f = 14 - moveInputDeadZoneSlider.value;
        moveInputDeadZone = Mathf.Pow(Screen.height / f, 2);
    }

    public void UpdateMovementSpeed()
    {
        moveSpeed = moveSpeedSlider.value;
    }
    #endregion
}
