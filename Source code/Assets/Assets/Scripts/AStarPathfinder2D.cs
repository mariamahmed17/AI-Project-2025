using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarPathfinder2D : MonoBehaviour
{
    public static AStarPathfinder2D Instance;

    [Header("Grid Settings")]
    public Tilemap groundTilemap;
    public float nodeSize = 1f;
    public LayerMask obstacleLayer;
    public float enemyAvoidanceCost = 10f; // New cost for tiles near enemies

    private Node[,] grid;
    private Vector3 gridOrigin;
    private int width, height;

    private void Awake()
    {
        Instance = this;
        GenerateGrid();
    }
    private void Start()
    {
        InvokeRepeating(nameof(GenerateGrid), 0f, 0.01f); 
    }

    /// <summary>
    /// ////////////////
    /// </summary>
    public void GenerateGrid()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("Ground Tilemap is missing!");
            return;
        }

        BoundsInt bounds = groundTilemap.cellBounds;
        width = bounds.size.x;
        height = bounds.size.y;
        gridOrigin = groundTilemap.CellToWorld(bounds.min);

        grid = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = gridOrigin + new Vector3(x * nodeSize + nodeSize * 0.5f, y * nodeSize + nodeSize * 0.5f, 0f);

                bool walkable = !Physics2D.OverlapCircle(worldPos, nodeSize * 0.4f, obstacleLayer);

                if (walkable)
                {
                    Collider2D hit = Physics2D.OverlapCircle(worldPos, nodeSize * 0.4f);
                    /////////////////////
                    if (hit != null && hit.CompareTag("Enemy"))
                    {
                        walkable = false;
                    }

                }

                float cost = 1f;

                // Assign higher cost near enemies
                Collider2D[] nearby = Physics2D.OverlapCircleAll(worldPos, nodeSize * 1.5f);
                foreach (var obj in nearby)
                {
                    if (obj.CompareTag("Enemy"))
                    {
                        cost += enemyAvoidanceCost;
                        break;
                    }
                }

                grid[x, y] = new Node(new Vector2Int(x, y), worldPos, walkable, cost);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (grid != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = grid[x, y].walkable ? Color.green : Color.red;
                    Gizmos.DrawWireCube(grid[x, y].worldPosition, Vector3.one * (nodeSize - 0.05f));
                }
            }
        }
    }
    /// <summary>
    /// ///////////////////
    /// </summary>
    public List<Vector3> FindPath(Vector3 startWorld, Vector3 targetWorld)
    {
        Vector2Int start = WorldToGrid(startWorld);
        Vector2Int end = WorldToGrid(targetWorld);

        Node startNode = grid[start.x, start.y];
        Node endNode = grid[end.x, end.y];

        if (!startNode.walkable || !endNode.walkable) return null;

        List<Node> open = new List<Node> { startNode };
        HashSet<Node> closed = new HashSet<Node>();

        foreach (var n in grid) { n.gCost = float.MaxValue; n.parent = null; }

        startNode.gCost = 0;
        startNode.hCost = Heuristic(startNode, endNode);

        while (open.Count > 0)
        {
            Node current = open[0];
            for (int i = 1; i < open.Count; i++)
                if (open[i].FCost < current.FCost || (open[i].FCost == current.FCost && open[i].hCost < current.hCost))
                    current = open[i];

            open.Remove(current);
            closed.Add(current);

            if (current == endNode) break;

            foreach (var neigh in GetNeighbors(current))
            {
                if (!neigh.walkable || closed.Contains(neigh)) continue;

                float newG = current.gCost + Heuristic(current, neigh) * neigh.cost; 
                if (newG < neigh.gCost)
                {
                    neigh.gCost = newG;
                    neigh.hCost = Heuristic(neigh, endNode);
                    neigh.parent = current;
                    if (!open.Contains(neigh)) open.Add(neigh);
                }
            }
        }

        if (endNode.parent == null) return null;

        List<Vector3> waypoints = new List<Vector3>();
        Node node = endNode;
        while (node != startNode)
        {
            waypoints.Add(node.worldPosition);
            node = node.parent;
        }
        waypoints.Reverse();
        return waypoints;
    }

    private float Heuristic(Node a, Node b)
    {
        return Mathf.Abs(a.gridPos.x - b.gridPos.x) + Mathf.Abs(a.gridPos.y - b.gridPos.y);
    }

    private IEnumerable<Node> GetNeighbors(Node node)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var d in dirs)
        {
            int nx = node.gridPos.x + d.x;
            int ny = node.gridPos.y + d.y;

            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                yield return grid[nx, ny];
        }
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / nodeSize);
        int y = Mathf.FloorToInt((worldPos.y - gridOrigin.y) / nodeSize);
        return new Vector2Int(Mathf.Clamp(x, 0, width - 1), Mathf.Clamp(y, 0, height - 1));
    }


    private class Node
    {
        public Vector2Int gridPos;
        public Vector3 worldPosition;
        public bool walkable;
        public float gCost, hCost;
        public Node parent;
        public float cost = 1f;
        public float FCost => gCost + hCost;

        public Node(Vector2Int p, Vector3 w, bool walkable, float cost = 1f)
        {
            gridPos = p;
            worldPosition = w;
            this.walkable = walkable;
            this.cost = cost;
            gCost = float.MaxValue;
        }
    }
}
