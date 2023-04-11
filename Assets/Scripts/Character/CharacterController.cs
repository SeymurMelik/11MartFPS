using UnityEditor.SceneManagement;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    //Character Move Speed
    [Header("Movement")]
    [SerializeField] Transform _orientation;
    [SerializeField] float _moveSpeed = 10f;
    [SerializeField] float _moveSpeedMultiplier = 10f;
    [SerializeField] float _moveSpeedMultiplierAir = .4f;
    [SerializeField] Rigidbody _rbPlayer;
    float _horizontalMove;
    float _verticalMove;
    Vector3 _moveDirection;




    [Space]
    [Header("Jump Variables")]
    [SerializeField] float _jumpForce;
    [SerializeField] float _playerHeight = 2f;
    [SerializeField] KeyCode _jumpKey = KeyCode.Space;

    [Space]
    [Header("Boolean Variables")]
    bool _isGround;

    [Space]
    [Header("Drag Variables ")]
    [SerializeField] float _rigidBodyDragOnGround = 6f;
    [SerializeField] float _rigidBodyDragOnAir = 2f;


    private void Start()
    {
        _rbPlayer = GetComponent<Rigidbody>();
        //Character Dont Fall
        _rbPlayer.freezeRotation = true;
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void Update()
    {
        GroundCheck();
        CharacterJumpInput();
        ControlDrag();
        CharacterMovementInput();
    }

    void GroundCheck()
    {
        _isGround = Physics.Raycast(transform.position, Vector3.down, _playerHeight / 2 + .1f);
    }
    void ControlDrag()
    {
        if (_isGround)
        {
            _rbPlayer.drag = _rigidBodyDragOnGround;
        }
        else if (!_isGround)
        {
            _rbPlayer.drag = _rigidBodyDragOnAir;
        }

    }
    void CharacterJumpInput()
    {
        if (Input.GetKeyDown(_jumpKey) && _isGround)
        {
            Jump();
        }
    }

    void CharacterMovementInput()
    {
        _horizontalMove = Input.GetAxisRaw("Horizontal");
        _verticalMove = Input.GetAxisRaw("Vertical");


        _moveDirection = _verticalMove * _orientation.forward + _horizontalMove * _orientation.right;
    }
    void MovePlayer()
    {
        if (_isGround)
            _rbPlayer.AddForce(_moveDirection * _moveSpeed * _moveSpeedMultiplier, ForceMode.Force);
        else if (!_isGround)
            _rbPlayer.AddForce(_moveDirection * _moveSpeed * _moveSpeedMultiplier * _moveSpeedMultiplierAir, ForceMode.Force);

    }
    void Jump()
    {
        _rbPlayer.velocity = new Vector3(_rbPlayer.velocity.x, 0, _rbPlayer.velocity.z);
        _rbPlayer.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
    }



}
