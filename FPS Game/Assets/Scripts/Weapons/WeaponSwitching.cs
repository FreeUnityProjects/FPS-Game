using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwitching : MonoBehaviour
{
    public int selectedWeapon = 0;
    private AK47WeaponsScript weaponScript;

    public Image WeaponIconBackground1;
    public Image WeaponIconBackground2;
    public Image WeaponIconBackground3;
    public Image WeaponIconBackground4;

    void Start()
    {
        SelectedWeapon();
    }

    void Update()
    {
        weaponScript = FindObjectOfType<AK47WeaponsScript>();

        int previousSelectedWeapon = selectedWeapon;

        if(Input.GetKeyDown(KeyCode.Alpha1) && transform.childCount >= 1)
        {
            if(weaponScript.isReloading == false)
            {
                selectedWeapon = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
        {
            if (weaponScript.isReloading == false)
            {
                selectedWeapon = 1;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3)
        {
            if (weaponScript.isReloading == false)
            {
                selectedWeapon = 2;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount >= 4)
        {
            if (weaponScript.isReloading == false)
            {
                selectedWeapon = 3;
            }
        }

        if (previousSelectedWeapon != selectedWeapon)
        {
            SelectedWeapon();
        }

        if(selectedWeapon == 0)
        {
            WeaponIconBackground1.enabled = true;
            WeaponIconBackground2.enabled = false;
            WeaponIconBackground3.enabled = false;
            WeaponIconBackground4.enabled = false;
        }
        if (selectedWeapon == 1)
        {
            WeaponIconBackground1.enabled = false;
            WeaponIconBackground2.enabled = true;
            WeaponIconBackground3.enabled = false;
            WeaponIconBackground4.enabled = false;
        }
        if (selectedWeapon == 2)
        {
            WeaponIconBackground1.enabled = false;
            WeaponIconBackground2.enabled = false;
            WeaponIconBackground3.enabled = true;
            WeaponIconBackground4.enabled = false;
        }
        if (selectedWeapon == 3)
        {
            WeaponIconBackground1.enabled = false;
            WeaponIconBackground2.enabled = false;
            WeaponIconBackground3.enabled = false;
            WeaponIconBackground4.enabled = true;
        }
    }

    void SelectedWeapon()
    {
        int i = 0;
        foreach(Transform weapon in transform)
        {
            if (i == selectedWeapon)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
            i++;
        }
    }
}
