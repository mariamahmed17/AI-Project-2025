using UnityEngine;

// سكربت يتم وضعه على كل رهينة، ويتعامل مع PlayerRescueManager2D
public class Hostage2 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // نتأكد إن اللي لمس الرهينة هو اللاعب
        if (!collision.CompareTag("Player"))
            return;
        
        // نجيب السكربت من اللاعب نفسه
        PlayerRescueManager2D rescueManager = GameObject.FindAnyObjectByType<PlayerRescueManager2D>();
        if (rescueManager == null)
            return;

        // لو مش شايل رهينة بالفعل، نحمل دي
        if (!rescueManager.IsCarryingHostage())
        {
            
            rescueManager.PickupHostage(this);
            Debug.Log($"[Hostage2] Picked up by player: {name}");
            gameObject.SetActive(false);
        }
    }
}
