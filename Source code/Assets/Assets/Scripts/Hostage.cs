using UnityEngine;

public class Hostage : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            RescueManager2D rescueManager = FindObjectOfType<RescueManager2D>();

            if (rescueManager != null && !rescueManager.IsCarryingHostage())
            {
                rescueManager.PickupHostage(this);
            }
        }
    }
}
