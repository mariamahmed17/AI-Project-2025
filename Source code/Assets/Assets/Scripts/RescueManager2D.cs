using System.Collections;
using UnityEngine;

public class RescueManager2D : MonoBehaviour
{
    public PlayerAutoMove2D playerMover;
    public Transform exitPoint;
    public GameObject WinScreen;

    private Hostage carriedHostage = null; 

    private void Start()
    {
        StartCoroutine(RescueLoop());
    }

    private IEnumerator RescueLoop()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Hostage").Length > 0);

        while (true)
        {
            // لو مش شايل رهينة، ندور على أقرب رهينة
            if (carriedHostage == null)
            {
                var hostages = GameObject.FindGameObjectsWithTag("Hostage");
                if (hostages.Length == 0)
                {
                    Debug.Log("All hostages rescued!");
                    WinScreen.SetActive(true);
                    yield break;
                }

                Transform nearest = FindClosestHostage(hostages);
                if (nearest == null) yield break;
/////////////////////////////////////////////
                var pathToHostage = AStarPathfinder2D.Instance.FindPath(
                    playerMover.transform.position, nearest.position);
///////////////////////////////////////////
                if (pathToHostage == null || pathToHostage.Count == 0)
                {
                    Debug.LogWarning("No path to hostage: " + nearest.name + ". Skipping.");
                    yield return new WaitForSeconds(0.5f); // استنى شوية قبل ما تحاول تاني
                    continue;
                }

                playerMover.MoveAlongPath(pathToHostage);
                yield return new WaitUntil(() => !playerMover.IsMoving);

            }
            else
            {
                // دلوقتي شايل رهينة لازم نروح للـ Exit
                var pathToExit = AStarPathfinder2D.Instance.FindPath(
                    playerMover.transform.position, exitPoint.position);

                playerMover.MoveAlongPath(pathToExit);
                yield return new WaitUntil(() => !playerMover.IsMoving);

                // وصلنا للـ Exit
                GameManager.Instance.HostageRescued();
                carriedHostage = null;

                yield return new WaitForSeconds(0.5f);
            }

            yield return null; 
        }
    }

/// <summary>
/// //////////
/// </summary>
    public void PickupHostage(Hostage hostage)
    {
        carriedHostage = hostage;
        hostage.gameObject.SetActive(false); // نخفي الرهينة بدل مانمسحها
    }

    public bool IsCarryingHostage()
    {
        return carriedHostage != null;
    }

    private Transform FindClosestHostage(GameObject[] hostages)
    {
        Transform closest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (var hostage in hostages)
        {
            float distance = Vector2.Distance(playerMover.transform.position, hostage.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = hostage.transform;
            }
        }

        return closest;
    }
}
