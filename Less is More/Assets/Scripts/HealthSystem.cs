using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem
{
    private int maxHealth;
    private int currentHealth;

    public HealthSystem(int health = 0)
    {
        maxHealth = health;
        currentHealth = health;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    public void Damage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;
    }

    public void Heal(int heal)
    {
        currentHealth += heal;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public void MaxHeal()
    {
        currentHealth = maxHealth;
    }

    public void SetMaxHealth(int maxHealth, bool updateCurrentHealth)
    {
        this.maxHealth = maxHealth;

        if (updateCurrentHealth)
            currentHealth = maxHealth;
    }
}
