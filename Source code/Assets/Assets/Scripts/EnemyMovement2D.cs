using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement2D : MonoBehaviour
{
    public enum MovementType { Patrol, RandomWalk }
    public MovementType movementType = MovementType.RandomWalk;
    public Tilemap groundTilemap;

    public float speed = 2f;
    public float repathInterval = 1.5f;
    public float waypointThreshold = 0.1f;
    public float randomTargetRange = 5f;

    public Transform[] patrolPoints;
    private int patrolIndex = 0;

    private Vector3 targetPosition;
    private List<Vector3> path;
    private int pathIndex = 0;

    private Rigidbody2D rb;
    private float repathTimer = 0f;

    public LayerMask obstacleLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetNewTarget();
    }

    void Update()
    {
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            RequestPath();
            repathTimer = repathInterval;
        }

        FollowPath();
    }

    void SetNewTarget()
    {
        if (movementType == MovementType.Patrol && patrolPoints.Length > 0)
        {
            targetPosition = patrolPoints[patrolIndex].position;
        }
        else
        {
            // نجيب كل البلاطات الممكن يمشي عليها
            List<Vector3> walkablePositions = GetWalkablePositionsInRange(transform.position, randomTargetRange);

            if (walkablePositions.Count > 0)
            {
                targetPosition = walkablePositions[Random.Range(0, walkablePositions.Count)];
            }
            else
            {
                targetPosition = transform.position; // fallback
            }
        }
    }
    List<Vector3> GetWalkablePositionsInRange(Vector3 center, float range)
    {
        List<Vector3> valid = new List<Vector3>();

        BoundsInt bounds = groundTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (!groundTilemap.HasTile(cell)) continue;

                Vector3 worldPos = groundTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
                if (Vector3.Distance(center, worldPos) <= range)
                {
                    valid.Add(worldPos);
                }
            }
        }

        return valid;
    }


    void RequestPath()
    {
        path = AStarPathfinder2D.Instance.FindPath(transform.position, targetPosition);
        pathIndex = 0;
    }

    void FollowPath()
    {
        if (path == null || pathIndex >= path.Count) return;

        Vector3 nextPoint = path[pathIndex];
        Vector2 direction = (nextPoint - transform.position).normalized;
        Vector2 move = direction * speed * Time.deltaTime;

        // تفادي الاصطدام
        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, 0.5f, obstacleLayer);
        if (hit.collider == null)
        {
            rb.MovePosition(rb.position + move);
        }
        else
        {
            // لو في حاجز، اطلب مسار جديد لموقع عشوائي أو نقطة جديدة
            SetNewTarget();
            RequestPath();
            return;
        }

        if (Vector2.Distance(transform.position, nextPoint) < waypointThreshold)
        {
            pathIndex++;
            if (pathIndex >= path.Count)
            {
                // وصلنا للهدف
                if (movementType == MovementType.Patrol)
                {
                    patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                }
                SetNewTarget();
                RequestPath();
            }
        }
    }
}
