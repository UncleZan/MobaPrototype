﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public PlayerCharacter pc;
    public float playerSpeed = 15;
    public float attackTime = 0;
    private float lastDirX = 0, lastDirY = -1;
    private int facingDirection;
    bool isMoving = false;

    private Animator pcAnimator;
    private Animator wpAnimator;
    private SpriteRenderer wpSpriteRenderer;

    public static Rigidbody2D rb2d;
    public Transform weapon;
    public static PoolOfObjects prjPool;
    public GameObject projectilePrefab;
    public Transform prjContainer;
    private Vector3 wpPos;
    public static Vector2 lastDirection = new Vector2(0, -1);
    Vector3 velocity;
    private List<GameObject> foesInRange;

    // The position we think is most correct for this player.
    //   NOTE: if we are the authority, then this will be
    //   exactly the same as transform.position
    Vector3 bestGuessPosition;

    // This is a constantly updated value about our latency to the server
    // i.e. how many second it takes for us to receive a one-way message
    // TODO: This should probably be something we get from the PlayerConnectionObject
    float ourLatency;

    // This higher this value, the faster our local position will match the best guess position
    float latencySmoothingFactor = 10;
    bool initialized = false;

    // Use this for initialization
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        transform.SetParent(GameObject.Find("Characters").transform);
        wpPos = weapon.localPosition;
        wpSpriteRenderer = weapon.GetComponent<SpriteRenderer>();
        pcAnimator = GetComponent<Animator>();
        wpAnimator = weapon.GetComponent<Animator>();
        foesInRange = new List<GameObject>();
        if (prjContainer == null)
            prjContainer = GameObject.Find("ProjectilesContainer").transform;
        if (prjPool == null)
            prjPool = new PoolOfObjects(prjContainer, projectilePrefab);
        pc = new PlayerCharacter();
    }

    void Update()
    {
        Debug.Log("IsServer? " + isServer + " Velocity " + velocity);
        rb2d.velocity = velocity;
        if (!hasAuthority)
        {
            return;
        }
        if (!initialized)
        {
            Camera.main.GetComponent<CameraFollow>().SetTarget(transform);
            name = "PlayerUnit " + netId;
            initialized = true;
        }
        ComputeVelocity();
        CmdUpdateVelocity(velocity, transform.position);

        if (pc.AttackSpeed == 0)
        {
            if (Input.GetButton("Jump"))
            {
                //GameObject newProjectile = prjPool.GetObject();
                //newProjectile.transform.position = weapon.position;
                CmdFire(weapon.position);
            }
        }
        else if (Input.GetButton("Jump") && Time.time > attackTime)
        {
            attackTime = Time.time + 1 / pc.AttackSpeed;
            //GameObject newProjectile = prjPool.GetObject();
            //newProjectile.transform.position = weapon.position;
            CmdFire(weapon.position);
        }
    }

    protected void ComputeVelocity()
    {
        Vector3 rightMovement = Vector3.right * Input.GetAxisRaw("Horizontal");
        Vector3 upMovement = Vector3.up * Input.GetAxisRaw("Vertical");

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

        velocity = heading * playerSpeed;
        //UpdateAnimation(heading);
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

    [Command]
    void CmdFire(Vector3 wPos)
    {
        GameObject newProjectile = prjPool.GetObject();
        Debug.Log(newProjectile.name);
        newProjectile.transform.position = wPos;

        NetworkServer.SpawnWithClientAuthority(newProjectile, connectionToClient);
    }

    [Command]
    void CmdUpdateVelocity(Vector3 v, Vector3 p)
    {
        // I am on a server
        transform.position = p;
        velocity = v;

        // If we know what our current latency is, we could do something like this:
        //  transform.position = p + (v * (thisPlayersLatencyToServer))


        // Now let the clients know the correct position of this object.
        RpcUpdateVelocity(velocity, transform.position);
    }

    [ClientRpc]
    void RpcUpdateVelocity(Vector3 v, Vector3 p)
    {
        // I am on a client

        if (hasAuthority)
        {
            // Hey, this is my own object. I "should" already have the most accurate
            // position/velocity (possibly more "Accurate") than the server
            // Depending on the game, I MIGHT want to change to patch this info
            // from the server, even though that might look a little wonky to the user.

            // Let's assume for now that we're just going to ignore the message from the server.
            return;
        }

        // I am a non-authoratative client, so I definitely need to listen to the server.

        // If we know what our current latency is, we could do something like this:
        //  transform.position = p + (v * (ourLatency))

        //transform.position = p;

        velocity = v;
        bestGuessPosition = p + (velocity * (ourLatency));


        // Now position of player one is as close as possible on all player's screens

        // IN FACT, we don't want to directly update transform.position, because then 
        // players will keep teleporting/blinking as the updates come in. It looks dumb.


    }
}