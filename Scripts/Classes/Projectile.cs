using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour

{
    float mSpeed;
    Vector3 mOrigin;

    Vector2 mDirection;

    PlayerCharacter mPlayer;
    Rigidbody2D rb2d;

    // Use this for initialization
    void Start()
    {
        mSpeed = 15;
        mPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerUnit>().pc;
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        //if (screenPos.y >= Screen.height || screenPos.y <= 0 || screenPos.x >= Screen.width || screenPos.x <= 0)
        if (Vector3.Distance(mOrigin, transform.position) >= mPlayer.AttackRange)
            PlayerUnit.prjPool.DisableObject(gameObject);

        //transform.Translate(direction * Time.deltaTime * speed);
        rb2d.velocity = mDirection * mSpeed;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Enemy")
        {
            coll.GetComponent<EnemyController>().InflictDamage(mPlayer.Attack);
            PlayerUnit.prjPool.DisableObject(gameObject);
        }
    }

    public void Initialize(Vector3 dir, Vector3 pos)
    {
        mDirection = dir;
        mOrigin = pos;
    }

    void OnDisable()
    {
        //Debug.Log("Origin " + mOrigin);
        //Debug.Log("Distance " + Vector3.Distance(mOrigin, transform.position));
    }
}