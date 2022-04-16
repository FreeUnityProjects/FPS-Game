using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    public float health = 50f;
    public float maxHealth = 50f;

    public GameObject explosionEffect;

    public GameObject healthBarUI;
    public Slider slider;

    public float currencyGain = 2f;
    private PlayerMovement player;

    void Start()
    {
        health = maxHealth;
        // slider.value = CalculateHealth();
        player = FindObjectOfType<PlayerMovement>();
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if(health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
        Instantiate(explosionEffect, transform.position, transform.rotation);
        player.currency += currencyGain;
    }

    void Update()
    {
        // slider.value = CalculateHealth();

        if(health < maxHealth)
        {
            healthBarUI.SetActive(true);
        }

        if(health > maxHealth)
        {
            health = maxHealth;
        }
    }

    float CalculateHealth()
    {
        return health / maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "CakeProjectile")
        {
            Die();
        }
    }
}