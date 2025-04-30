using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleRandomMovement2D : MonoBehaviour
{
    [Header("Tilemap of walkable (green) tiles")]
    public Tilemap walkableTilemap;
    [Header("Obstacle Detection")]
    public LayerMask obstacleLayer;       // Layer for obstacles
    public float obstacleCheckRadius = 0.2f;

    [Header("Movement Settings")]
    public float moveInterval = 0.5f;
    public float moveSpeed = 3f;

    private Rigidbody2D rb;
    private Vector3Int currentCell;
    public GameObject LooseScreen;
    public Image LooseScreenImage;
    public Sprite loossprite;

    

    // 4 directions on grid
    private static readonly Vector3Int[] dirs = new Vector3Int[]
    {
        new Vector3Int(1,0,0),
        new Vector3Int(-1,0,0),
        new Vector3Int(0,1,0),
        new Vector3Int(0,-1,0)
    };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // snap to nearest cell center
        currentCell = walkableTilemap.WorldToCell(transform.position);
        Vector3 center = walkableTilemap.GetCellCenterWorld(currentCell);
        transform.position = center;
        StartCoroutine(RandomWalk());
        LooseScreen = GameManager.Instance.LooseScreen;
        
        LooseScreenImage = GameManager.Instance.LooseScreenImage;

    }

    private IEnumerator RandomWalk()
    {
        while (true)
        {
            // gather valid neighbor cells
            List<Vector3Int> valid = new List<Vector3Int>();
            foreach (var d in dirs)
            {
                Vector3Int n = currentCell + d;
                if (!walkableTilemap.HasTile(n))
                    continue;
                // also ensure no obstacle collider in that cell
                Vector3 world = walkableTilemap.GetCellCenterWorld(n);
                Collider2D hit = Physics2D.OverlapCircle(world, obstacleCheckRadius, obstacleLayer);
                if (hit == null)
                    valid.Add(n);
            }

            if (valid.Count > 0)
            {
                // pick random neighbor
                Vector3Int nextCell = valid[Random.Range(0, valid.Count)];
                Vector3 targetWorld = walkableTilemap.GetCellCenterWorld(nextCell);

                // move smoothly towards it
                while ((Vector2)rb.position != (Vector2)targetWorld)
                {
                    Vector2 newPos = Vector2.MoveTowards(rb.position, (Vector2)targetWorld, moveSpeed * Time.deltaTime);
                    rb.MovePosition(newPos);
                    yield return new WaitForFixedUpdate();
                }

                currentCell = nextCell;
            }

            // wait before next step
            yield return new WaitForSeconds(moveInterval);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // نتأكد إن اللي لمس الرهينة هو اللاعب
        if (!collision.CompareTag("Player"))
            return;

        LooseScreenImage.sprite = loossprite;
        GameManager.Instance.loss = true;
        LooseScreen.SetActive(true);
    }
}