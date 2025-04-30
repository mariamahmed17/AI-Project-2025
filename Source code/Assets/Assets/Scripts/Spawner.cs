using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Spawner : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap groundTilemap;

    [Header("Prefabs")]
    public GameObject hostagePrefab;
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public int numberOfHostages = 5;
    public int numberOfEnemies = 3;
    public Transform playerStartPoint; 

    private List<Vector3> validPositions = new List<Vector3>();

    private void Start()
    {
        GetAllWalkableTiles();
        FilterReachablePositions(); 
        SpawnHostages();
        SpawnEnemies();
    }

    private void GetAllWalkableTiles()
    {
        validPositions.Clear();
        BoundsInt bounds = groundTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase tile = groundTilemap.GetTile(cellPos);
                if (tile != null)
                {
                    Vector3 worldPos = groundTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0f);
                    validPositions.Add(worldPos);
                }
            }
        }
    }

    private void FilterReachablePositions()
    {
        List<Vector3> reachable = new List<Vector3>();
        foreach (var pos in validPositions)
        {
            var path = AStarPathfinder2D.Instance.FindPath(playerStartPoint.position, pos);
            if (path != null && path.Count > 0)
            {
                reachable.Add(pos);
            }
        }
        validPositions = reachable;
    }

    private void SpawnHostages()
    {
        int spawnCount = Mathf.Min(numberOfHostages, validPositions.Count);
        int spawned = 0;

        while (spawned < spawnCount && validPositions.Count > 0)
        {
            int index = Random.Range(0, validPositions.Count);
            Vector3 spawnPos = validPositions[index];
            validPositions.RemoveAt(index);

            if (IsInsideSafeZone(spawnPos)) continue;

            var go = Instantiate(hostagePrefab, spawnPos, Quaternion.identity);
            go.tag = "Hostage";
            spawned++;
        }
    }

    private void SpawnEnemies()
    {
        int spawnCount = Mathf.Min(numberOfEnemies, validPositions.Count);
        int spawned = 0;

        while (spawned < spawnCount && validPositions.Count > 0)
        {
            int index = Random.Range(0, validPositions.Count);
            Vector3 spawnPos = validPositions[index];
            validPositions.RemoveAt(index);

            if (IsInsideSafeZone(spawnPos)) continue;

            var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.tag = "Enemy";

            var em = enemy.GetComponent<SimpleRandomMovement2D>();
            if (em != null)
            {
                em.walkableTilemap = groundTilemap;
            }

            spawned++;
        }
    }
    private bool IsInsideSafeZone(Vector3 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, 0.1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("SafeZone"))
                return true;
        }
        return false;
    }

}
