using UnityEngine;
using NaughtyAttributes;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

/// <summary>
/// <para> (TH) : ตัวจัดการค่าสถานะของตัวละคร รวมถึงระบบพลังชีวิต การรับดาเมจ สถานะอมตะชั่วคราว และการรีเซ็ตค่าสถานะ </para>
/// <para> (EN) : Manager for character statistics including health, damage, temporary invincibility, and status resetting. </para>
/// </summary>
public sealed class StatsController : MonoBehaviour, ITakeDamageable
{
    #region Variable

    [Header("Assign Data")]
    [SerializeField] private LivingThingsScriptable _statsData;

    [Header("Auto Linked Components")]
    [ReadOnly][SerializeField] private Character _livingName;
    [ReadOnly][SerializeField] private Animator _characterAnimator;
    [ReadOnly][SerializeField] private SpriteRenderer _characterSprite;
    [ReadOnly][SerializeField] private Rigidbody2D _characterRigidbody;
    [ReadOnly][SerializeField] private Collider2D _characterCollider;

    [Header("Damage Settings")]
    [SerializeField] private float _invincibilityTime = 1.2f;
    [SerializeField] private float _flashSpeed = 15f;
    [SerializeField] private GameObject _hitEffectPrefab;

    [Header("Runtime Stats")]
    [ReadOnly][SerializeField] private bool _isInvincible;
    [ReadOnly][SerializeField] private int _currentHp;
    [ReadOnly][SerializeField] private int _currentAtk;
    [ReadOnly][SerializeField] private float _currentSpeed;
    [ReadOnly][SerializeField] private int _currentBombAmount;
    [ReadOnly][SerializeField] private int _currentExplosionRange;
    [ReadOnly][SerializeField] private int _bombsRemaining;

    #endregion //Variable

    #region ITakeDamageable Properties

    public bool IsInvincible { get => _isInvincible; set => _isInvincible = value; }
    public SpriteRenderer SpriteRenderer => _characterSprite;
    public MonoBehaviour CoroutineRunner => this;

    #endregion //ITakeDamageable Properties

    #region Explicit Interface Implementation

    void ITakeDamageable.TakeDamage(int amount) => ExecuteTakeDamage(amount);
    void ITakeDamageable.ApplyDamage(int amount) => ExecuteApplyDamage(amount);

    #endregion //Explicit Interface Implementation

    #region Properties

    public Character LivingName => _livingName;

    public int CurrentHp
    {
        get => _currentHp;
        private set
        {
            _currentHp = Mathf.Max(0, value);
            OnHpChanged();
            if (_currentHp <= 0) OnDeath();
        }
    }

    public float CurrentSpeed { get => _currentSpeed; set => _currentSpeed = Mathf.Max(0, value); }
    public int CurrentBombAmount { get => _currentBombAmount; set => _currentBombAmount = Mathf.Max(0, value); }
    public int CurrentExplosionRange { get => _currentExplosionRange; set => _currentExplosionRange = Mathf.Max(0, value); }
    public int CurrentAtk { get => _currentAtk; set => _currentAtk = Mathf.Max(0, value); }
    public int BombsRemaining { get => _bombsRemaining; set => _bombsRemaining = Mathf.Clamp(value, 0, _currentBombAmount); }

    #endregion //Properties

    #region Unity Lifecycle

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_characterAnimator == null) _characterAnimator = GetComponentInChildren<Animator>();
        if (_characterSprite == null) _characterSprite = GetComponentInChildren<SpriteRenderer>();
        if (_characterRigidbody == null) _characterRigidbody = GetComponentInChildren<Rigidbody2D>();
        if (_characterCollider == null) _characterCollider = GetComponentInChildren<Collider2D>();
        if (_statsData != null) _livingName = _statsData.livingName;
    }
#endif

    private void Awake() => SyncData();

    #endregion //Unity Lifecycle

    #region Public Methods

    /// <summary>
    /// <para> (TH) : ดึงข้อมูลตั้งต้นจาก ScriptableObject กลับมาทับค่า Runtime ปัจจุบัน </para>
    /// <para> (EN) : Syncs current runtime stats with initial values from the ScriptableObject. </para>
    /// </summary>
    public void SyncData()
    {
        if (_statsData == null) return;
        _livingName = _statsData.livingName;
        _currentHp = _statsData.baseHp;
        _currentAtk = _statsData.baseAtk;
        _currentSpeed = _statsData.baseSpeed;
        _currentBombAmount = _statsData.baseBombAmount;
        _currentExplosionRange = _statsData.baseExplosionRange;
        _bombsRemaining = _currentBombAmount;
    }

    /// <summary>
    /// <para> (TH) : รีเซ็ตค่าสถานะและเปิดการทำงานของ Component กลับเป็นค่าเริ่มต้น (ใช้ตอน Spawn ใหม่) </para>
    /// <para> (EN) : Resets stats and re-enables components to default state (Used on Respawn). </para>
    /// </summary>
    public void ResetStats()
    {
        SyncData();
        IsInvincible = false;

        // คืนค่าการแสดงผล
        if (_characterSprite != null)
        {
            Color c = _characterSprite.color;
            c.a = 1f;
            _characterSprite.color = c;
        }

        // เปิด Physics
        if (_characterRigidbody != null)
        {
            _characterRigidbody.simulated = true;
            _characterRigidbody.bodyType = RigidbodyType2D.Dynamic;
        }

        if (_characterCollider != null) _characterCollider.enabled = true;

        // เปิด Logic และ Animator
        this.enabled = true;
        if (_characterAnimator != null)
        {
            _characterAnimator.enabled = true;
            _characterAnimator.SetInteger("Hp", _currentHp);
            _characterAnimator.Play("Idle"); // กลับไปท่าเริ่มต้น
        }

        Debug.Log($"<b><color=#4FC3F7>[Stats Reset]</color></b> {name} has been restored to initial state.");
    }

    public void OnHitItem(string itemTag)
    {
        switch (itemTag)
        {
            case "HpItem": CurrentHp++; break;
            case "SpeedItem": CurrentSpeed++; break;
            case "BombAmountItem": CurrentBombAmount++; BombsRemaining++; break;
            case "ExplosionRangeItem": CurrentExplosionRange++; break;
        }
    }

    #endregion //Public Methods

    #region Private Logic

    private void ExecuteTakeDamage(int amount)
    {
        DamageAbility<StatsController>.TakeDamage(this, amount, _invincibilityTime, _flashSpeed, onHit: () => {
            SpawnHitEffect();
        });
    }

    private void ExecuteApplyDamage(int amount)
    {
        if (IsInvincible) return;
        CurrentHp -= amount;
    }

    private void SpawnHitEffect()
    {
        if (_hitEffectPrefab != null) Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
    }

    private void OnHpChanged()
    {
#if UNITY_EDITOR
        Debug.Log($"<b><color=#FF5252>[Stats]</color></b> {name} HP: <color=#81C784>{_currentHp}</color>");
#endif
        if (_characterAnimator != null) _characterAnimator.SetInteger("Hp", _currentHp);
    }

    private void OnDeath()
    {
        // ปิด Physics และการควบคุม แต่ไม่ Destroy เพื่อให้ Registry ยังอ้างอิงถึงได้
        if (_characterRigidbody != null)
        {
            _characterRigidbody.simulated = false;
            _characterRigidbody.bodyType = RigidbodyType2D.Static;
        }

        if (_characterCollider != null) _characterCollider.enabled = false;

        // ส่ง Event แจ้งว่าตายแล้ว
        EventBus.Instance.Publish(new CharacterDeathEvent(this.gameObject, _statsData.livingName));

        // ปิดสคริปต์ตัวเองเพื่อหยุด Logic การทำงาน
        this.enabled = false;

        Debug.Log($"<b><color=#FF5252>[Death]</color></b> {name} is dead. Logic disabled for pooling/registry.");
    }

    #endregion //Private Logic
}