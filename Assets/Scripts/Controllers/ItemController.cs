using UnityEngine;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

public class ItemController : MonoBehaviour
{
    [Header("Pool Settings")]
    [Tooltip("ลาก Prefab ตัวต้นฉบับมาใส่ที่นี่เพื่อใช้เป็น Key")]
    [SerializeField] private GameObject _poolkey;

    [Header("Invincible Settings")]
    [SerializeField] private AnimationClip _criticalClip;
    [SerializeField] private float _offset = 0.1f;

    private float _lifeTime;

    private void OnEnable()
    {
        _lifeTime = 0;
    }

    private void Update()
    {
        _lifeTime += Time.deltaTime;
    }

    public void DestroyMeWhenHit()
    {
        // 🛡️ ป้องกันกรณีลืมลาก Animation Clip ใส่มาใน Inspector
        float protectionDuration = (_criticalClip != null) ? _criticalClip.length + _offset : _offset;

        if (_lifeTime < protectionDuration)
        {
            Debug.Log($"<b><color=#81C784>[Item]</color></b> {name} is protected! Time: {_lifeTime:F2}");
            return;
        }

        ExecuteRelease();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // เช็คว่าคนชนคือ Player/LivingThings หรือไม่
        var stats = other.GetComponentInParent<StatsController>();
        if (stats != null)
        {
            stats.OnHitItem(gameObject.tag);

            // 🚀 เก็บของกินแล้ว ไม่ต้องรอช่วงอมตะ สั่งคืน Pool ทันที
            ExecuteRelease();
        }
    }

    private void ExecuteRelease()
    {
        // 🚀 ดึงชื่อจาก Prefab ที่ลากมาใส่ หรือชื่อตัวเองแบบตัด (Clone)
        string key = (_poolkey != null) ? _poolkey.name : gameObject.name.Replace("(Clone)", "").Trim();

        ObjectPoolManager.Instance.Release(key, this);
    }
}