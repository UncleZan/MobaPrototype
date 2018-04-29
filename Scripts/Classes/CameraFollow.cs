using UnityEngine;
using UnityEngine.Networking;

public class CameraFollow : MonoBehaviour
{

    public Transform target;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    private float rightBound;
    private float leftBound;
    private float topBound;
    private float bottomBound;

    private Vector3 pos;
    public SpriteRenderer background;

    void Start()
    {
        if (background == null)
            background = GameObject.Find("Background").GetComponent<SpriteRenderer>();
        float vertExtent = GetComponent<Camera>().orthographicSize;
        float horzExtent = GetComponent<Camera>().aspect * vertExtent;
        leftBound = (float)(background.bounds.min.x + horzExtent);
        rightBound = (float)(background.bounds.max.x - horzExtent);
        bottomBound = (float)(background.bounds.min.y + vertExtent);
        topBound = (float)(background.bounds.max.y - vertExtent);
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 pos = new Vector3(target.position.x, target.position.y, transform.position.z) + offset;
            pos.x = Mathf.Clamp(pos.x, leftBound, rightBound);
            pos.y = Mathf.Clamp(pos.y, bottomBound, topBound);
            transform.position = pos;
        }
    }

    public void SetTarget(Transform pTarget)
    {
        target = pTarget;
    }

}