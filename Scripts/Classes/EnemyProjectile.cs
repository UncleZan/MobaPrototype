using UnityEngine;
using System.Collections;

public class EnemyProjectile : MonoBehaviour
{
    int damage;

    float speed = 15;
    Vector2 direction;
    Vector2 origin;

    NPC enemy;

    void Awake()
    {
        origin = transform.position;
    }

    void Update()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        //if (screenPos.y >= Screen.height || screenPos.y <= 0 || screenPos.x >= Screen.width || screenPos.x <= 0)
        if (Vector3.Distance(origin, transform.position) > enemy.AttackRange)
            Destroy(gameObject);

        GetComponent<Rigidbody2D>().velocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Player" && coll is BoxCollider2D)
        {
            Game.enemyPrjPool.DisableObject(gameObject);

        }
    }

    public void Initialize(NPC pEnemy, Vector2 dir)
    {
        direction = dir;
        enemy = pEnemy;
        damage = enemy.Attack;
    }

    void OnEnable()
    {
        origin = transform.position;
    }
}