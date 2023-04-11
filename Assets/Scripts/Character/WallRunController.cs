using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunController : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.Z;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    [SerializeField]bool wallLeft;
    [SerializeField]bool wallRight;

    [Header("References")]
    public Transform orientation;
    private PlayerController pm;
    private Rigidbody rb;
    [SerializeField]CameraController camScript;
    SlideController slideScript;

    [SerializeField] float wallJumpUpForce = 3f;
    [SerializeField] float wallJumpForce = 13f;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerController>();
        slideScript = GetComponent<SlideController>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.isWallRunning && !Input.GetKey(slideScript._slideKey))
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        // Getting Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        // State 1 - Wallrunning
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
        {
            if (!pm.isWallRunning)
                StartWallRun();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                WallJump();
            }
        }

        // State 3 - None
        else
        {
            if (pm.isWallRunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pm.isWallRunning = true;

        camScript.DoFov(85f);
        if (wallLeft)
        {
            camScript.DoTilt(-5f);
        }
        if (wallRight)
        {
            camScript.DoTilt(5f);
        }
    }

    private void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upwards/downwards force
        if (upwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        if (downwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);

        // push to wall force
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    void WallJump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 forceToApply = wallNormal * wallJumpForce + orientation.up * wallJumpUpForce;
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

    private void StopWallRun()
    {
        pm.isWallRunning = false;
        camScript.DoFov(60f);
        camScript.DoTilt(0f);
    }
}
