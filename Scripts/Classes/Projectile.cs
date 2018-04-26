using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    float mSpeed;
    Vector3 mOrigin;

    Vector2 mDirection;

    PlayerCharacter mPlayer;

    // Use this for initialization
    void Awake()
    {
        mDirection = PlayerController.lastDirection;
        mSpeed = 15;
        mOrigin = transform.position;
        mPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().pc;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        //if (screenPos.y >= Screen.height || screenPos.y <= 0 || screenPos.x >= Screen.width || screenPos.x <= 0)
        if (Vector3.Distance(mOrigin, transform.position) > mPlayer.AttackRange)
            PlayerController.prjPool.DisableObject(gameObject);

        //transform.Translate(direction * Time.deltaTime * speed);
        GetComponent<Rigidbody2D>().velocity = mDirection * mSpeed;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Enemy")
        {
            coll.GetComponent<EnemyController>().InflictDamage(mPlayer.Attack);
            PlayerController.prjPool.DisableObject(gameObject);
        }
    }

    void OnEnable()
    {
        mDirection = PlayerController.lastDirection;
        mOrigin = transform.position;
    }
}