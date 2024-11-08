using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxScrolling : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float parallaxFactorX;
    [SerializeField] private float parallaxFactorY;
    [SerializeField] private bool loopX;
    [SerializeField] private bool loopY;

    private Vector2 startPos;
    private Vector2 dimen;

    private void Start()
    {
        startPos = transform.position /*= (Vector2)cam.transform.position*/;
        dimen = GetComponent<SpriteRenderer>().bounds.size;
    }

    private void Update()
    {
        float distanceX = cam.transform.position.x * parallaxFactorX;
        float distanceY = cam.transform.position.y * parallaxFactorY;

        var newPos = new Vector2(startPos.x + distanceX, startPos.y + distanceY);
        transform.position = newPos;

        if (loopX)
        {
            float gapX = cam.transform.position.x * (1 - parallaxFactorX);
            if (gapX > startPos.x + (dimen.x / 2)) startPos.x += dimen.x;
            else if (gapX < startPos.x - (dimen.x / 2)) startPos.x -= dimen.x;
        }

        if (loopY)
        {
            float gapY = cam.transform.position.y * (1 - parallaxFactorY);
            if (gapY > startPos.y + (dimen.y / 2)) startPos.y += dimen.y;
            else if (gapY < startPos.y - (dimen.y / 2)) startPos.y -= dimen.y;
        }

    }
}
