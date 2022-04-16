using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AK47WeaponsScript : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public float impactForce = 30f;

    public GameObject hitmarker;

    private float nextTimeToFire = 0f;

    public int maxAmmo = 10;
    private int currentAmmo;
    public float reloadTime = 2f;
    public bool isReloading = false;

    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public GameObject impactEnemyEffect;

    public WeaponRecoil recoil;
    private WeaponSwitching weaponSwitching;

    public CameraShake cameraShake;

    public Animator animator;

    public float shakeX = .05f;
    public float shakeY = .2f;

    [SerializeField] private TextMeshProUGUI reloadUILabel;

    void Start()
    {
        currentAmmo = maxAmmo;
        hitmarker.SetActive(false);
        weaponSwitching = FindObjectOfType<WeaponSwitching>();
    }

    void OnEnable()
    {
        isReloading = false;
        animator.SetBool("Reloading", false);
        animator.enabled = false;
    }

    void Update()
    {
        if (isReloading)
            return;

        if(currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if(Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f/fireRate;
            Shoot();
        }

        reloadUILabel.text = currentAmmo.ToString();
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        animator.SetBool("Reloading", true);
        animator.enabled = true;

        yield return new WaitForSeconds(reloadTime - .25f);
        animator.SetBool("Reloading", false);
        yield return new WaitForSeconds(.25f);

        currentAmmo = maxAmmo;
        isReloading = false;
        animator.enabled = false;
    }

    public void Shoot()
    {
        muzzleFlash.Play();
        StartCoroutine(cameraShake.Shake(shakeX, shakeY));
        recoil.Fire();

        currentAmmo--;

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Target target = hit.transform.GetComponent<Target>();
            if(target != null)
            {
                target.TakeDamage(damage);
            }

            if(hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            if(hit.collider.tag == "Enemy")
            {
                HitActive();
                Invoke("HitDisable", 0.2f);

                GameObject impactEnemy = Instantiate(impactEnemyEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactEnemy, 2f);
            }

            if(hit.collider.tag != "Enemy")
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
        } 
    }

    private void HitActive()
    {
        hitmarker.SetActive(true);
    }

    private void HitDisable()
    {
        hitmarker.SetActive(false);
    }
}
