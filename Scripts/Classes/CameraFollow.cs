using UnityEngine;

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
    public SpriteRenderer spriteBounds;

    void Start()
    {
        float vertExtent = GetComponent<Camera>().orthographicSize;
        float horzExtent = GetComponent<Camera>().aspect * vertExtent;
        leftBound = (float)(spriteBounds.bounds.min.x + horzExtent);
        rightBound = (float)(spriteBounds.bounds.max.x - horzExtent);
        bottomBound = (float)(spriteBounds.bounds.min.y + vertExtent);
        topBound = (float)(spriteBounds.bounds.max.y - vertExtent);
    }

    void LateUpdate()
    {
        Vector3 pos = new Vector3(target.position.x, target.position.y, transform.position.z) + offset;
        pos.x = Mathf.Clamp(pos.x, leftBound, rightBound);
        pos.y = Mathf.Clamp(pos.y, bottomBound, topBound);
        transform.position = pos;
    }

}