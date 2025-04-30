using UnityEngine;

public class ExitTrigger2D : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("Tag المستخدم للـ Player GameObject")]
    public string playerTag = "Player";

    public PlayerRescueManager2D rescueManager;

    void OnTriggerEnter2D(Collider2D col)
    {
        // تأكد إن الكوليد دخل هو الـ Player
        if (!col.CompareTag(playerTag))
            return;

        // جلب سكربت إدارة الإنقاذ من اللاعب
        if (rescueManager == null)
            rescueManager = col.GetComponent<PlayerRescueManager2D>();

        // إذا الرايسكيو مانجر موجود واللاعب شايل رهينة، نفّذ التسليم
        if (rescueManager != null && rescueManager.IsCarryingHostage())
        {
            rescueManager.DeliverCarriedHostage();
        }
    }
}
