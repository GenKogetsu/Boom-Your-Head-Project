using UnityEngine;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

/// <summary>
/// <para> (TH) : เซนเซอร์ไฟระเบิด ตรวจจับการชนและใช้ PoolKey ในการคืนวัตถุกลับเข้าคลัง </para>
/// </summary>
public sealed class BombTriggerSensor : MonoBehaviour
{
    #region Variable

    [Header("Pool Settings")]
    [Tooltip("ลาก Prefab ลูกไฟต้นฉบับมาใส่ที่นี่ (เพื่อให้ชื่อ Key ตรงกับ PoolManager)")]
    [SerializeField] private GameObject _poolKey;

    [Header("Observer")]
    [SerializeField] private BombChannelSO _bombChannel;

    [Header("Lifecycle Settings")]
    [SerializeField] private AnimationClip _lifeTime;

    [Header("Layer Settings")]
    [SerializeField] private LayerMask _mapLayer;
    [SerializeField] private LayerMask _livingLayer;
    [SerializeField] private LayerMask _itemLayer;
    [SerializeField] private LayerMask _bombLayer;

    #endregion //Variable

    #region Unity Lifecycle

    private void OnEnable()
    {
        // ⏳ ตั้งเวลาคืน Pool ตามความยาว Animation
        float duration = (_lifeTime != null) ? _lifeTime.length : 0.5f;
        Invoke(nameof(ExecuteRelease), duration);
    }

    private void OnDisable() => CancelInvoke();

    private void OnTriggerEnter2D(Collider2D other)
    {
        int hitBit = 1 << other.gameObject.layer;
        Vector3Int gridPos = Vector3Int.RoundToInt(transform.position);

        // 1. 🛡️ เช็คเลเยอร์แผนที่
        if ((hitBit & _mapLayer) != 0)
        {
            _bombChannel.RaiseExplosionHit(gridPos, other);
            return;
        }

        // 2. 💣 เช็คเลเยอร์ระเบิด
        if ((hitBit & _bombLayer) != 0)
        {
            if (other.TryGetComponent<BombController>(out var bomb))
                bomb.ForceExplode();
            return;
        }

        // 3. 🍎 เช็คเลเยอร์ไอเทม
        if ((hitBit & _itemLayer) != 0)
        {
            if (other.TryGetComponent<ItemController>(out var item))
            {
                item.DestroyMeWhenHit();
            }
            return;
        }

        // 4. 🤺 เช็คเลเยอร์สิ่งมีชีวิต
        if ((hitBit & _livingLayer) != 0)
        {
            var stats = other.GetComponentInParent<ITakeDamageable>();
            stats?.TakeDamage(1);
        }
    }

    #endregion //Unity Lifecycle

    #region Private Logic

    private void ExecuteRelease()
    {
        // 🚀 ดึงชื่อจาก Prefab ที่ลากมาใส่ หรือ Fallback ไปที่ชื่อตัวเองตัด Clone
        string key = (_poolKey != null) ? _poolKey.name : gameObject.name.Replace("(Clone)", "").Trim();

        ObjectPoolManager.Instance.Release(key, this);
    }

    #endregion //Private Logic
}