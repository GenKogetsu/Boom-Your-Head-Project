using UnityEngine;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

public sealed class BombTriggerSensor : MonoBehaviour
{
    [SerializeField] private BombChannelSO _bombChannel;
    [SerializeField] private AnimationClip _lifeTime;
    [SerializeField] private LayerMask _mapLayer, _livingLayer, _itemLayer, _bombLayer;

    private void OnEnable() => Invoke(nameof(ExecuteRelease), _lifeTime != null ? _lifeTime.length : 0.5f);
    private void OnDisable() => CancelInvoke();

    private void OnTriggerEnter2D(Collider2D other)
    {
        string layerName = LayerMask.LayerToName(other.gameObject.layer);

        // 🚀 ใช้ Bitwise เช็ค Layer เพื่อความเร็ว
        int bit = 1 << other.gameObject.layer;

        if ((bit & _mapLayer) != 0) _bombChannel.RaiseExplosionHit(Vector3Int.RoundToInt(transform.position), other);
        else if ((bit & _livingLayer) != 0) 
        {
            var stats = other.GetComponentInParent<ITakeDamageable>();
            stats?.TakeDamage(1);
        }
        else if ((bit & _itemLayer) != 0)
        {
            // 🚀 ไอเทมก็ต้องใช้ชื่อตัวเองเป็น Key ในการกลับ Pool
            string itemKey = other.gameObject.name.Replace("(Clone)", "").Trim();
            ObjectPoolManager.Instance.Release(itemKey, other);
        }
        else if ((bit & _bombLayer) != 0)
        {
            if (other.TryGetComponent<BombController>(out var bomb)) bomb.ForceExplode();
        }
    }

    private void ExecuteRelease() => ObjectPoolManager.Instance.Release("Explosion", this);
}