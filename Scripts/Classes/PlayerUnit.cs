using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// A PlayerUnit is a unit controlled by a player
// This could be a character in an FPS, a zergling in a RTS
// Or a scout in a TBS

public class PlayerUnit : NetworkBehaviour {


    bool initialized = false;
    public PlayerCharacter pc;

    float attackTime = 0;
    private float lastDirX = 0, lastDirY = -1;
    bool isMoving = false;
    private Animator pcAnimator;
    private Animator wpAnimator;
    public Vector3 direction;
    public static Vector3 lastDirection;
    public Transform weapon;
    private SpriteRenderer wpSpriteRenderer;
    private Vector3 wpPos;
    private int facingDirection;
    private List<GameObject> enmsInRange;

    public static PoolOfObjects prjPool;
    public GameObject projectilePrefab;
    public Transform prjContainer;

    [SyncVar(hook = "OnWpSortingOrderChanged")]
    int wpSortingOrder;

    Vector3 velocity;
    Vector3 lastFrameVel = Vector3.zero;

    // The position we think is most correct for this player.
    //   NOTE: if we are the authority, then this will be
    //   exactly the same as transform.position
    Vector3 bestGuessPosition;

    // This is a constantly updated value about our latency to the server
    // i.e. how many second it takes for us to receive a one-way message
    // TODO: This should probably be something we get from the PlayerConnectionObject
    float ourLatency = 0.1f;   

    // This higher this value, the faster our local position will match the best guess position
    float latencySmoothingFactor = 10;

    void Start()
    {
        enmsInRange = new List<GameObject>();
        pc = new PlayerCharacter();
        pcAnimator = GetComponent<Animator>();
        wpAnimator = weapon.GetComponent<Animator>();
        wpSpriteRenderer = weapon.GetComponent<SpriteRenderer>();
        prjContainer = GameObject.Find("ProjectilesContainer").transform;
        if (prjPool == null)
            prjPool = new PoolOfObjects(prjContainer, projectilePrefab);
    }
    // Update is called once per frame
    void Update () {
        // This function runs on ALL PlayerUnits -- not just the ones that I own.

        // Code running right here is running for ALL version of this object, even
        // if it's not the authoratitive copy.
        // But even if we're NOT the owner, we are trying to PREDICT where the object
        // should be right now, based on the last velocity update.

        

        // How do I verify that I am allowed to mess around with this object?
        if (!hasAuthority)
        {
            // We aren't the authority for this object, but we still need to update
            // our local position for this object based on our best guess of where
            // it probably is on the owning player's screen.
            bestGuessPosition = bestGuessPosition + ( velocity * Time.deltaTime );
            UpdateAnimation(direction);
            // Instead of TELEPORTING our position to the best guess's position, we
            // can smoothly lerp to it.

            transform.position = Vector3.Lerp( transform.position, bestGuessPosition, Time.deltaTime * latencySmoothingFactor);

            return;
        }

        if (!initialized)
        {
            lastDirection = new Vector3(0, -1);
            Camera.main.GetComponent<CameraFollow>().SetTarget(transform);
            //name = "PlayerUnit " + netId;
            //tag = "Player";
            initialized = true;
        }

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

        // If we get to here, we are the authoritative owner of this object
        transform.Translate(velocity);

        ComputeVelocity();
        SwapWeaponLayer();
        UpdateAnimation(direction);
        CmdUpdateVelocity(velocity, transform.position);

    }


    void ComputeVelocity()
    {
        Vector3 rightMovement = Vector3.right * Input.GetAxisRaw("Horizontal");
        Vector3 upMovement = Vector3.up * Input.GetAxisRaw("Vertical");
        direction = Vector3.Normalize((rightMovement + upMovement) * Time.deltaTime);

        velocity = Vector3.Normalize((rightMovement + upMovement) * Time.deltaTime) / 10;
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

        if (enmsInRange.Count > 0)
        {
            Vector3 distanceFromTarget = enmsInRange[0].transform.position - transform.position;
            for (int i = 1; i < enmsInRange.Count; i++)
            {
                if (distanceFromTarget.magnitude > (enmsInRange[i].transform.position - transform.position).magnitude)
                {
                    distanceFromTarget = enmsInRange[i].transform.position - transform.position;
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
        bool isToSwap = (facingDirection < 2 || facingDirection > 5) ? (wpSpriteRenderer.sortingOrder == 1) : (wpSpriteRenderer.sortingOrder == -1);
        if (isToSwap)
        {

            if (wpSpriteRenderer.sortingOrder == 1)
                CmdChangeWpSo(-1);
            else
                CmdChangeWpSo(1);
        }
    }

    void OnWpSortingOrderChanged(int so)
    {
        wpSortingOrder = so;
        wpSpriteRenderer.sortingOrder = wpSortingOrder;
    }

    public void CharacterFacingDirection(int direction)
    {
        facingDirection = direction;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll is CircleCollider2D)
        {
            if (coll.gameObject.tag == "Enemy")
            {
                enmsInRange.Add(coll.gameObject);
            }
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll is CircleCollider2D)
        {
            if (coll.gameObject.tag == "Enemy")
            {
                enmsInRange.Remove(coll.gameObject);
            }
        }
    }

    [Command]
    void CmdChangeWpSo(int so)
    {
        wpSortingOrder = so;
    }

    [Command]
    void CmdFire(Vector3 wp)
    {
        RpcFire(wp);
    }

    [ClientRpc]
    void RpcFire(Vector3 wp)
    {
        GameObject newProjectile = prjPool.GetObject();
        newProjectile.transform.position = wp;
        newProjectile.GetComponent<Projectile>().Initialize(lastDirection, wp);
    }

    [Command]
    void CmdUpdateVelocity( Vector3 v, Vector3 p)
    {
        // I am on a server
        transform.position = p;
        velocity = v;
        direction = velocity * 10;
        // If we know what our current latency is, we could do something like this:
        //  transform.position = p + (v * (thisPlayersLatencyToServer))


        // Now let the clients know the correct position of this object.
        RpcUpdateVelocity( velocity, transform.position);
    }

    [ClientRpc]
    void RpcUpdateVelocity( Vector3 v, Vector3 p )
    {
        // I am on a client

        if( hasAuthority )
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
        direction = velocity * 10;
        bestGuessPosition = p + (velocity * (ourLatency));

        // Now position of player one is as close as possible on all player's screens

        // IN FACT, we don't want to directly update transform.position, because then 
        // players will keep teleporting/blinking as the updates come in. It looks dumb.


    }

    

}
