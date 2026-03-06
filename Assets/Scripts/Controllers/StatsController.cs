using UnityEngine;
using NaughtyAttributes;
using Genoverrei.DesignPattern;
using Genoverrei.Libary;

namespace BombGame.Logic;

/// <summary>
/// <para> summary_StatsController </para>
/// <para> (TH) : ตัวจัดการค่าสถานะของตัวละคร รวมถึงระบบพลังชีวิต การรับดาเมจ และสถานะอมตะชั่วคราว </para>
/// <para> (EN) : Manager for character statistics including health, damage, and temporary invincibility. </para>
/// </summary>
public sealed class StatsController : MonoBehaviour, ITakeDamageable
{
    #region Variable

    [Header("Assign Data")]
    [SerializeField] private LivingThingsScriptable _statsData;

    [Header("Auto Linked Components")]
    [ReadOnly]
    [SerializeField] private Character _livingName;

    [ReadOnly]
    [SerializeField] private Animator _characterAnimator;

    [ReadOnly]
    [SerializeField] private SpriteRenderer _characterSprite;

    [ReadOnly]
    [SerializeField] private Rigidbody2D _characterRigidbody;

    [ReadOnly]
    [SerializeField] private Collider2D _characterCollider;

    [Header("Damage Settings")]
    [SerializeField] private float _invincibilityTime = 1.2f;

    [SerializeField] private float _flashSpeed = 15f;

    [SerializeField] private GameObject _hitEffectPrefab;

    [Header("Runtime Stats")]
    [ReadOnly]
    [SerializeField] private bool _isInvincible;

    [ReadOnly]
    [SerializeField] private int _currentHp;

    [ReadOnly]
    [SerializeField] private int _currentAtk;

    [ReadOnly]
    [SerializeField] private float _currentSpeed;

    [ReadOnly]
    [SerializeField] private int _currentBombAmount;

    [ReadOnly]
    [SerializeField] private int _currentExplosionRange;

    [ReadOnly]
    [SerializeField] private int _bombsRemaining;

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
        if (_characterRigidbody != null)
        {
            _characterRigidbody.simulated = false;
            _characterRigidbody.bodyType = RigidbodyType2D.Static;
        }
        if (_characterCollider != null) _characterCollider.enabled = false;

        EventBus.Instance.Publish(new CharacterDeathEvent(this.gameObject, _statsData != null ? _statsData.livingType : Charactertype.Bot));
    }

    #endregion //Private Logic
}