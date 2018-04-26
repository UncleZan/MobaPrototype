using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemyController : MonoBehaviour
{
    public NPC enemy;
    public static Transform target;
    Animator animator;
    bool isInFront;
    bool isFacingRight;
    Vector3 distanceFromTarget;
    float attackTime = 0;


    SpriteRenderer[] renderers;

    PoolOfObjects prjPools;

    void Start()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        enemy = new NPC((NPCType)(Random.Range(0, 3) < 2 ? 1 : 1), Game.stageLevel);
        prjPools = Game.enemyPrjPool;
    }

    void Update()
    {
        distanceFromTarget = target.position - transform.position;
        if (distanceFromTarget.magnitude > enemy.AttackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, enemy.Speed * Time.deltaTime * 2);
        }
        else
        {
            if (enemy.AttackSpeed == 0)
                BasicAttack();
            else if (attackTime < Time.time)
                BasicAttack();
        }
        Flip();

    }

    void Flip()
    {
    }

    void AdjustZOrder()
    {
        foreach (SpriteRenderer sr in renderers)
        {
            if (isInFront)
            {
                sr.sortingLayerName = "Player";
            }
            else
            {
                sr.sortingLayerName = "Default";
            }
        }
    }

    void BasicAttack()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        GameObject newProjectile = prjPools.GetObject();

        newProjectile.transform.position = transform.position;
        newProjectile.GetComponent<EnemyProjectile>().Initialize(enemy, direction);
        attackTime = Time.time + (1 / enemy.AttackSpeed);
    }

    public void InflictDamage(int mDamage)
    {
        enemy.InflictDamage(mDamage);
        if (!enemy.IsAlive)
            Game.KillEnemy(gameObject);
    }
}