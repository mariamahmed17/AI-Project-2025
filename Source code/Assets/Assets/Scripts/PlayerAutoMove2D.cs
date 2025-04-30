using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAutoMove2D : MonoBehaviour
{
    public float moveSpeed = 3f;

    private Rigidbody2D rb;
    private List<Vector3> path;
    private int targetIndex;
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MoveAlongPath(List<Vector3> newPath)
    {
        if (newPath == null || newPath.Count == 0)
        {
            IsMoving = false;
            return;
        }

        path = newPath;
        targetIndex = 0;
        IsMoving = true;
        StopAllCoroutines();
        StartCoroutine(FollowPath());
    }

    private IEnumerator FollowPath()
    {
        if (!GameManager.Instance.loss)
        {
            while (targetIndex < path.Count)
            {
                Vector3 targetPos = path[targetIndex];
                while (Vector2.Distance(transform.position, targetPos) > 0.1f)
                {
                    Vector2 dir = (targetPos - transform.position).normalized;
                    rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
                    yield return new WaitForFixedUpdate();
                }

                transform.position = targetPos;
                targetIndex++;
                yield return null;
            }

            IsMoving = false;
        }
    }
}
