using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : MonoBehaviour
{

    [Header("Rifle things")]
    public Camera cam;
    public float giveDamageOf = 10f;
    public float shootingRange = 100f;
    public float fireCharge = 15f;
    public float nextTimeToShoot = 0f;
    public PlayerController playerController;
    public Animator animator;

    [Header("Rifle Ammunition and shooting")]
    public int maxAmmo = 30;
    public int mag = 10;
    private int presentAmmo;
    public float reloadingTime = 1.3f;
    private bool setReloading;

    [Header("Rifle Effects")]
    public ParticleSystem muzzleSpark;

    // Update is called once per frame
    void Update()
    {

        if (setReloading)
            return;

        if(presentAmmo <= 0 || Input.GetKeyDown(KeyCode.R))
        {
            
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToShoot)
        {
            animator.SetBool("Fire", true);
            animator.SetBool("Aim", false);
            animator.SetBool("Idle", false);
            nextTimeToShoot = Time.time + 1f/fireCharge;
            Shoot();
        }else if(Input.GetButton("Fire2") && Input.GetButton("Fire1"))
        {
            animator.SetBool("Fire", true);
            animator.SetBool("Aim", true);
            animator.SetBool("Idle", false);
        }
        else if (Input.GetButton("Fire2"))
        {
            animator.SetBool("Fire", false);
            animator.SetBool("Aim", true);
            animator.SetBool("Idle", false);
        }
        else
        {
            animator.SetBool("Fire", false);
            animator.SetBool("Aim", false);
            animator.SetBool("Idle", true);
        }
    }

    private void Shoot()
    {

        //check for mag
        if (mag == 0)
        {
            //show ammo out text
            return;
        }

        presentAmmo--;
        if (presentAmmo == 0) 
        {
            mag--;
        }

        //updating the UI

        muzzleSpark.Play();
        RaycastHit hitInfo;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, shootingRange))
        {
            ObjectToHit hit = hitInfo.transform.GetComponent<ObjectToHit>();

            Debug.Log(hitInfo.transform.name);

            if (hit != null)
            {
                hit.ObjectHitDamage(giveDamageOf);
            }

            
        }
    }

    IEnumerator Reload()
    {
        playerController.playerSpeed = 1f;
        setReloading = true;
        animator.SetBool("Reload", true);
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadingTime);
        animator.SetBool("Reload", false);
        presentAmmo = maxAmmo;
        playerController.playerSpeed = 4f;
        setReloading = false;
    }
}
