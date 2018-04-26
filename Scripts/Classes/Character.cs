using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
    [SerializeField]
    Vector3 position;
    [SerializeField]
    protected int health;
    public int Health { get { return health; } }
    [SerializeField]
    protected int currentHealth;
    public int CurrentHealth { get { return currentHealth; } }
    [SerializeField]
    private bool isAlive = true;
    public bool IsAlive { get { return isAlive; } set { isAlive = value; } }
    [SerializeField]
    protected int attack;
    public int Attack { get { return attack; } }

    public virtual void InflictDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            IsAlive = false;
        }
    }

    public void SetPosition(Vector3 newPosition) { }
}