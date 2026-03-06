using NaughtyAttributes;
using UnityEngine;

public class OnHitItem : MonoBehaviour
{
    [ReadOnly]
    [SerializeField] private StatsController _stats;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _stats = GetComponentInParent<StatsController>();
    }
#endif

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("ItemLayer")) return;

        _stats.OnHitItem(other.tag);

        Destroy(other.gameObject);
    }
}
