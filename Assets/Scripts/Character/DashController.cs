using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashController : MonoBehaviour
{
    Vector3 DelayingDash;
    //dash //Settings // inputs 

    [Header("Dash Movem")]
    public float DashForce;
    public float DashUpForward;
    public float DashDuration;
    public float dashFov;

    [Header("references")]
    public Transform Orientation;
    public CameraController Cam;
    private PlayerController _pmMaster;
    private Rigidbody _rb;

    [Header("Cooldown")]
    public float DashCd;
    private float _dashTimer;

    [Header("Inputs")]
    KeyCode _dashKey = KeyCode.E;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _pmMaster = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(_dashKey) && !_pmMaster.IsSlide && !_pmMaster.isWallRunning) DashPlayer();
        if (_dashTimer > 0) _dashTimer -= Time.deltaTime;
    }
    void DashPlayer()
    {
        if (_dashTimer > 0) return;
        else _dashTimer = DashCd;

        _pmMaster.Dashing = true;
        _rb.useGravity = false;


        Cam.DoFov(dashFov);


        Vector3 dir = GetDashDirection(Orientation);
        Vector3 forceToApply = dir * DashForce + Orientation.up * DashUpForward;
        DelayingDash = forceToApply;
        Invoke(nameof(DashingDelay), 0.0025f);
        Invoke(nameof(ResetDash), DashDuration);
    }
    void DashingDelay()
    {
        _rb.AddForce(DelayingDash, ForceMode.Impulse);
    }
    void ResetDash()
    {
        _pmMaster.Dashing = false;

        Cam.DoFov(60f);
        Cam.DoTilt(0f);

        _rb.useGravity = true;
    }

    Vector3 GetDashDirection(Transform forwardT)
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float veritcal = Input.GetAxisRaw("Vertical");

        Vector3 directions = forwardT.forward * veritcal + forwardT.right * horizontal;

        if (horizontal == 0 && veritcal == 0)
        {
            directions = forwardT.forward;
        }
        return directions.normalized;

    }
}
