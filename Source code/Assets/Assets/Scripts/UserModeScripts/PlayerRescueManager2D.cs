using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerRescueManager2D : MonoBehaviour
{
    [Header("Tags & References")]
    public string hostageTag = "Hostage";
    public string exitTag = "Exit";
    public GameObject onAllRescued;
    public TextMeshProUGUI rescuedCounterText;

    private Hostage2 carriedHostage;
    private int rescuedCount;
    private int totalHostages;
    private bool initialized;

    void Start()
    {
        StartCoroutine(InitializeAfterSpawning());
    }

    IEnumerator InitializeAfterSpawning()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag(hostageTag).Length > 0);
        totalHostages = GameObject.FindGameObjectsWithTag(hostageTag).Length;
        Debug.Log($"[RescueManager] Initialized. Total hostages: {totalHostages}");

        if (onAllRescued != null)
            onAllRescued.SetActive(false);

        UpdateUI();
        initialized = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        HandleTrigger(col);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // لو الـ Exit مش Trigger
        HandleCollision(col.collider);
    }

    private void HandleTrigger(Collider2D col)
    {
        if (!initialized) return;
        Debug.Log($"[RescueManager] Trigger Enter with {col.name} (Tag={col.tag}), carrying={carriedHostage != null}");

        if (col.CompareTag(hostageTag) && carriedHostage == null)
        {
            carriedHostage = col.GetComponent<Hostage2>();
            if (carriedHostage != null)
            {
                col.gameObject.SetActive(false);
                Debug.Log($"[RescueManager] Picked up hostage: {carriedHostage.name}");
            }
        }
        else if (col.CompareTag(exitTag) && carriedHostage != null)
        {
            DeliverHostage();
        }
    }

    private void HandleCollision(Collider2D col)
    {
        if (!initialized) return;
        Debug.Log($"[RescueManager] Collision Enter with {col.name} (Tag={col.tag}), carrying={carriedHostage != null}");
        if (col.CompareTag(exitTag) && carriedHostage != null)
        {
            DeliverHostage();
        }
    }

    private void DeliverHostage()
    {
        rescuedCount++;
        Debug.Log($"[RescueManager] Delivered hostage. Rescued: {rescuedCount}/{totalHostages}");
        GameManager.Instance.HostageRescued();
        carriedHostage = null;
        UpdateUI();

        if (rescuedCount >= totalHostages && onAllRescued != null)
        {
            onAllRescued.SetActive(true);

            Debug.Log("[RescueManager] All hostages rescued! Activated.");
        }
    }
    // في أسفل الكلاس PlayerRescueManager2D
    public void DeliverCarriedHostage()
    {
        if (carriedHostage == null) return;
        rescuedCount++;
        Debug.Log($"[RescueManager] Delivered hostage. Rescued: {rescuedCount}/{totalHostages}");
        GameManager.Instance.HostageRescued();
        carriedHostage = null;
        UpdateUI();
        if (rescuedCount >= totalHostages && onAllRescued != null)
        {
            onAllRescued.SetActive(true);
            GameManager.Instance.loss = true;
            Debug.Log("[RescueManager] All hostages rescued! Activated.");
        }
    }
    public void PickupHostage(Hostage2 h)
    {
        carriedHostage = h;
        Debug.Log($"[RescueManager] Hostage picked up via external call: {h.name}");
    }
    void UpdateUI()
    {
        if (rescuedCounterText != null)
            rescuedCounterText.text = initialized
                ? $"Rescued: {rescuedCount}/{totalHostages}"
                : "Rescued: 0/"+ totalHostages;
    }

    // دوال للمزامنة الخارجية
    public bool IsCarryingHostage() => carriedHostage != null;
    public int GetRescuedCount() => rescuedCount;
}