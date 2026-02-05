using UnityEngine;

public class Check
{
    private static readonly LayerMask defaultLayer = 1 << 0;

    public static RaycastHit2D CheckGround(Vector2 origin) => CheckGround(origin, defaultLayer);

    public static Collider2D CheckWall(Vector2 origin, float leftSide, float topSide, float rightSide, float bottomSide) => CheckWall(origin, leftSide, topSide, rightSide, bottomSide, defaultLayer);

    public static RaycastHit2D CheckGround(Vector2 origin, LayerMask layerMask, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity)
    {
        return Physics2D.Raycast(origin, Vector2.down, Mathf.Infinity, layerMask, minDepth, maxDepth);
    }

    public static Collider2D CheckWall(Vector2 origin, float leftSide, float topSide, float rightSide, float bottomSide, LayerMask layerMask, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity)
    {        
        return Physics2D.OverlapArea(
            new Vector2(origin.x - leftSide, origin.y + topSide), 
            new Vector2(origin.x + rightSide, origin.y - bottomSide), 
            layerMask,
            minDepth,
            maxDepth
        );
    }
}
