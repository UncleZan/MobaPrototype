using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerCharacter pc;
    public float playerSpeed = 15;
    public float attackTime = 0;
    private float lastDirX = 0, lastDirY = -1;
    private int facingDirection;
    bool isMoving = false;

    private SpriteRenderer pcSpriteRenderer;
    private SpriteRenderer wpSpriteRenderer;
    private Animator pcAnimator;
    private Animator wpAnimator;

    public static Rigidbody2D rb2d;
    public Transform weapon;
    public static PoolOfObjects prjPool;
    public GameObject projectilePrefab;
    public Transform prjContainer;
    private Vector3 wpPos;
    public static Vector2 lastDirection = new Vector2(0, -1);
    private List<GameObject> foesInRange;

    // Use this for initialization
    void Awake()
    {
        wpPos = weapon.localPosition;
        pcSpriteRenderer = GetComponent<SpriteRenderer>();
        wpSpriteRenderer = weapon.GetComponent<SpriteRenderer>();
        pcAnimator = GetComponent<Animator>();
        wpAnimator = weapon.GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        foesInRange = new List<GameObject>();
        if (prjPool == null)
            prjPool = new PoolOfObjects(prjContainer, projectilePrefab);

        pc = new PlayerCharacter();
    }

    void Update()
    {
        ComputeVelocity();
        //if (rb2d.velocity.normalized != Vector2.zero)
        //    lastDirection = rb2d.velocity.normalized;
        if (pc.AttackSpeed == 0)
        {
            if (Input.GetButton("Jump"))
            {
                GameObject newProjectile = prjPool.GetObject();
                newProjectile.transform.position = weapon.position;
            }
        }
        else if (Input.GetButton("Jump") && Time.time > attackTime)
        {
            attackTime = Time.time + 1 / pc.AttackSpeed;
            GameObject newProjectile = prjPool.GetObject();
            newProjectile.transform.position = weapon.position;
        }
    }

    protected void ComputeVelocity()
    {
        Vector3 rightMovement = Vector3.right * Input.GetAxis("Horizontal");
        Vector3 upMovement = Vector3.up * Input.GetAxis("Vertical");

        Vector3 heading = Vector3.Normalize((rightMovement + upMovement) * playerSpeed * Time.deltaTime);

        bool flipSpriteWpH = (wpSpriteRenderer.flipX ? (rightMovement.x > 0) : (rightMovement.x < 0));
        bool flipSpriteWpV = (facingDirection < 2 || facingDirection > 5) ? (wpSpriteRenderer.sortingOrder == 1) : (wpSpriteRenderer.sortingOrder == -1);

        if (flipSpriteWpH)
        {
            wpSpriteRenderer.flipX = !wpSpriteRenderer.flipX;
        }
        if (flipSpriteWpV)
        {
            SwapWeaponLayer();
        }

        rb2d.velocity = heading * playerSpeed;
        UpdateAnimation(heading);
    }

    void UpdateAnimation(Vector3 dir)
    {
        if (dir.x == 0f && dir.y == 0f)
        {
            pcAnimator.SetFloat("LastDirX", lastDirX);
            pcAnimator.SetFloat("LastDirY", lastDirY);
            wpAnimator.SetFloat("LastDirX", lastDirX);
            wpAnimator.SetFloat("LastDirY", lastDirY);
            pcAnimator.SetBool("IsMoving", false);
            wpAnimator.SetBool("IsMoving", false);
            isMoving = false;
        }
        else
        {
            lastDirX = dir.x;
            lastDirY = dir.y;
            pcAnimator.SetBool("IsMoving", true);
            wpAnimator.SetBool("IsMoving", true);
            isMoving = true;
        }

        if (foesInRange.Count > 0)
        {
           Vector3 distanceFromTarget = foesInRange[0].transform.position - transform.position;
            for (int i = 1; i < foesInRange.Count; i++)
            {
                if (distanceFromTarget.magnitude > (foesInRange[i].transform.position - transform.position).magnitude)
                {
                    distanceFromTarget = foesInRange[i].transform.position - transform.position;
                }
            }
            dir = distanceFromTarget.normalized;
            lastDirX = dir.x;
            lastDirY = dir.y;
        }
        if (dir.normalized != Vector3.zero)
            lastDirection = dir.normalized;
        pcAnimator.SetFloat("DirX", dir.x);
        pcAnimator.SetFloat("DirY", dir.y);
        wpAnimator.SetFloat("DirX", dir.x);
        wpAnimator.SetFloat("DirY", dir.y);
    }

    public void SwapWeaponLayer()
    {
        if (wpSpriteRenderer.sortingOrder == 1)
            wpSpriteRenderer.sortingOrder = -1;
        else
            wpSpriteRenderer.sortingOrder = 1;
    }
    
    public void CharacterFacingDirection(int direction)
    {
        facingDirection = direction;
        wpPos.x = (facingDirection > 3) ? -0.055f : 0.055f;
        wpPos.y = (facingDirection < 2 || facingDirection == 7) ? 0.05f : -0.05f;
        weapon.localPosition = wpPos;
    }

    public void InflictDamage(int pDamage)
    {
        pc.InflictDamage(pDamage);
        if (!pc.IsAlive)
        {
            Debug.Log("You lose");
            Time.timeScale = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll is CircleCollider2D)
        {
            if (coll.gameObject.tag == "Enemy")
            {
                foesInRange.Add(coll.gameObject);
            }
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll is CircleCollider2D)
        {
            if (coll.gameObject.tag == "Enemy")
            {
                foesInRange.Remove(coll.gameObject);
            }
        }
    }

}