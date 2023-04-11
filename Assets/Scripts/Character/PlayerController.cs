using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
public class PlayerController : MonoBehaviour
{
    public TextMeshProUGUI Speed;
    public TextMeshProUGUI CurrentS;

    Vector3 _moveDirection;
    //Movement // Jump // Check // Reference // Inputs // StateHandler //Slope
    public enum CurrentState
    {
        walking,
        wallRunning,
        sliding,
        dashing,
        air
    }
    public CurrentState State;
    [Header("Movement")]
    [SerializeField] float _walkingSpeed = 15f;
    private float _moveSpeed = 7f;
    private float _moveSpeedMultip = 10f;
    private float _multiplierInAir = 0.4f;

    [Space]
    [Header("Jump")]
    [SerializeField] float _jumpForce = 12f;
    [SerializeField] float _jumpCooldown = 0.2f;
    [SerializeField] bool _readyToJump;
    [SerializeField] bool _doubleJump = true;

    [Space]
    [Header("Check")]
    [SerializeField] float _groundCheckDistance = 0.4f;
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] bool _isGround;
    [SerializeField] float _playerHeight = 2f;

    [Space]
    [Header("Slope")]
    [SerializeField] float _maxAngleSlope = 40f;
    bool _isExitSlope;
    float _slopeDistance = 0.4f;
    RaycastHit _slopeHit;

    [Space]
    [Header("Sliding")]
    public bool IsSlide;

    [Space]
    [Header("WallRun")]
    public bool isWallRunning;
    [SerializeField] float _wallRunSpeed = 17f; 

    [Space]
    [Header("Inputs")]
    float _horizontalMove;
    float _verticalMove;
    KeyCode _jumpKey = KeyCode.Space;
    KeyCode _sprintKey = KeyCode.LeftShift;

    [Space]
    [Header("References")]
    [SerializeField] Rigidbody _rb;
    [SerializeField] Transform _orientaion;
    [SerializeField] CameraController _camScript;

    [Space]
    [Header("Dashing")]
    public float DashSpeed;
    public bool Dashing;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void Update()
    {
        MyInputs();
        CheckGround();
        ControlSpeed();
        OnSlope();
        ControlDrag();
        StateHandler();
    }
    void StateHandler()
    {

        if (Dashing)
        {
            State = CurrentState.dashing;
            _moveSpeed = DashSpeed;
        }
        else if (isWallRunning)
        {
            State = CurrentState.wallRunning;
            _moveSpeed = _wallRunSpeed;
        }
        else if (IsSlide)
        {
            State = CurrentState.sliding;
            _moveSpeed = 20f;
        }
        else if (Input.GetKey(_sprintKey) && _isGround)
        {
            State = CurrentState.walking;
            _moveSpeed = _walkingSpeed;
        }
        else if (_isGround)
        {
            State = CurrentState.walking;
            _moveSpeed = _walkingSpeed;
        }
        else
        {
            State = CurrentState.air;
        }
        CurrentS.text = State.ToString();
    }
    void MyInputs()
    {
        _horizontalMove = Input.GetAxisRaw("Horizontal");
        _verticalMove = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(_jumpKey) && _isGround && _readyToJump)
        {
            _readyToJump = false;
            _isExitSlope = true;
            Jump();
            Invoke("ResetJump", _jumpCooldown);
        }
        else if (Input.GetKeyDown(_jumpKey) && _doubleJump && !_isGround && _readyToJump)
        {
            _readyToJump = false;
            _isExitSlope = true;
            _doubleJump = false;
            Jump();
            Invoke("ResetJump", _jumpCooldown);
        }

        if (_isGround)
        {
            _doubleJump = true;
        }
    }
    void Jump()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

        //force
        _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
    }
    void MovePlayer()
    {
        _moveDirection = _orientaion.right * _horizontalMove + _orientaion.forward * _verticalMove;

        if (OnSlope() && !_isExitSlope)
        {
            _rb.AddForce(GetSlopeDirection(_moveDirection) * _moveSpeed * _moveSpeedMultip, ForceMode.Force);
            if (_rb.velocity.y > 0)
                _rb.AddForce(Vector3.down * 30, ForceMode.Force);

        }
        else if (_isGround)
            _rb.AddForce(_moveDirection.normalized * _moveSpeed * _moveSpeedMultip, ForceMode.Force);
        else if (!_isGround)
            _rb.AddForce(_moveDirection.normalized * _moveSpeed * _moveSpeedMultip * _multiplierInAir, ForceMode.Force);


        _rb.useGravity = !OnSlope();
        Speed.text = ((int)_rb.velocity.magnitude).ToString();

        _camScript.DoFov(60f);
        //bu elave
        if (_horizontalMove > 0 && !isWallRunning && !IsSlide && !Dashing)
        {
            _camScript.DoTilt(-2.5f);
        }
        else if (_horizontalMove < 0 && !isWallRunning && !IsSlide && !Dashing)
        {
            _camScript.DoTilt(2.5f);
        }
        else if (_horizontalMove == 0 && !isWallRunning && !IsSlide && !Dashing)
        {
            _camScript.DoFov(60f);
            _camScript.DoTilt(0f);
        }


        if (_verticalMove > 0 && !isWallRunning && !IsSlide && !Dashing)
        {
            _camScript.DoTiltX(2.5f);
        }
        else if (_verticalMove < 0 && !isWallRunning && !IsSlide && !Dashing)
        {
            _camScript.DoTiltX(-2.5f);
        }
        else if (_horizontalMove == 0 && !isWallRunning && !IsSlide && !Dashing)
        {
            _camScript.DoTiltX(0f);
        }
    }
    void CheckGround()
    {
        _isGround = Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + _groundCheckDistance, _groundLayer);
    }
    void ResetJump()
    {
        _isExitSlope = false;
        _readyToJump = true;
    }
    void ControlDrag()
    {
        if (_isGround)
            _rb.drag = 4f;
        else
            _rb.drag = 0f;
    }
    void ControlSpeed()
    {
        if (OnSlope() && !_isExitSlope)
        {
            if (_rb.velocity.magnitude > _moveSpeed)
            {
                _rb.velocity = _rb.velocity.normalized * _moveSpeed;
            }
        }
        else
        {

            Vector3 flatVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

            if (flatVel.magnitude > _moveSpeed)
            {
                Vector3 limitedVel = _rb.velocity.normalized * _moveSpeed;
                _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
            }
        }

    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, _playerHeight * 0.5f + _slopeDistance))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < _maxAngleSlope && angle != 0;
        }
        return false;
    }
    public Vector3 GetSlopeDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;
    }
}
