using UnityEngine;

public class ChickenArea : MonoBehaviour
{
    [SerializeField] private BoxCollider2D areaCollider;

    private void Awake()
    {
        if (areaCollider == null)
        {
            areaCollider = GetComponent<BoxCollider2D>();
        }
    }

    public Vector2 GetRandomPoint()
    {
        Bounds bounds = areaCollider.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector2(x, y);
    }
}