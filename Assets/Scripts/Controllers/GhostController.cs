using Genoverrei.Libary;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class GhostController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LivingThings"))
        {
            var target = other.GetComponentInParent<ITakeDamageable>();
            if (target != null) target.TakeDamage(1);
        }

        if (other.TryGetComponent<BombController>(out var bomb))
        {
            bomb.ForceExplode();
        }
    }
}
