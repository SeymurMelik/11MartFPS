using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Gun variables")]
    public int Damage, MagazineSize, BulletsPerTap, BulletsLeft, BulletsShot;
    public float TimeBetweenShooting, spread, Range, ReloadTime, TimeBetweenShots;

    [Header("Bools")]
    public bool AllowButtonHold;
    bool _realoading;
    bool _readyToShot;
    bool _isShooting;

    [Header("References")]
    public Camera FpsCam;
    public Transform AttachPoint;
    RaycastHit whatIHit;
    public LayerMask WhatIsEnemy;

    [Header("Particle")]
    public ParticleSystem MuzzleFlash;

    private void Awake()
    {
        BulletsLeft = MagazineSize;
        _readyToShot = true;
    }

    private void Update()
    {
        MyInputs();
    }

    void MyInputs()
    {
        if (AllowButtonHold) _isShooting =  Input.GetKey(KeyCode.Mouse0);
        else _isShooting =  Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && !_realoading && BulletsLeft < MagazineSize) Reload();
        if (_readyToShot && _isShooting && !_realoading && BulletsLeft > 0)
        {
            BulletsShot = BulletsPerTap;
            Shoot();
        }
    }
    void Shoot()
    {
        MuzzleFlash.Play();
        _readyToShot = false;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 direction = FpsCam.transform.position + new Vector3(x, y, 0);
        if (Physics.Raycast(FpsCam.transform.position,FpsCam.transform.forward, out whatIHit, Range,WhatIsEnemy))
        {
            IDamageable damageable = whatIHit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(Damage);
            }
        }

        Instantiate(MuzzleFlash, whatIHit.point, Quaternion.Euler(0,180,0));

        BulletsLeft--;
        BulletsShot--;

        Invoke(nameof(ResetShoot), TimeBetweenShooting);

        if (BulletsShot > 0 && BulletsLeft > 0)
        {
            Invoke(nameof(Shoot), TimeBetweenShots);
        }
    }

    void ResetShoot()
    {
        _readyToShot = true;
    }

    void Reload()
    {
        Debug.Log("reloading...");
        _realoading = true;
        Invoke(nameof(ReloadFinished), ReloadTime);
    }

    void ReloadFinished()
    {
        Debug.Log("Reload Finishing");
        _realoading = false;
        BulletsLeft = MagazineSize;
    }
}
