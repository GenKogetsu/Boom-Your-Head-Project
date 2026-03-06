using UnityEngine;
using NaughtyAttributes;
using BombGame.EnumSpace;

namespace BombGame.Controller;

/// <summary>
/// <para> summary_ExplosionAnimationController </para>
/// <para> (TH) : ควบคุมแอนิเมชันของไฟระเบิดในแต่ละส่วน (Start, Middle, End) และจัดการทิศทางตาม Logic ดั้งเดิม </para>
/// <para> (EN) : Controls explosion animations for each part (Start, Middle, End) and handles rotation based on original logic. </para>
/// </summary>
public sealed class ExplosionAnimationController : MonoBehaviour
{
    #region Variable

    [SerializeField] private Animator _startAnimator;

    [SerializeField] private Animator _middleAnimator;

    [SerializeField] private Animator _endAnimator;

    [ReadOnly]
    [SerializeField] private Vector2 _direction;

    [ReadOnly]
    [SerializeField] private BombPart _currentPart;

    #endregion //Variable

    #region Public Methods

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ตั้งค่าเริ่มต้นให้กับไฟระเบิด กำหนดทิศทาง และเปิดใช้งาน Animator ที่ถูกต้อง </para>
    /// <para> (EN) : Initializes the explosion effect, sets direction, and activates the correct animator. </para>
    /// </summary>
    public void Setup(BombPart part, Vector2 direction)
    {
        _currentPart = part;
        _direction = direction;

        SetDirection(direction);
        SetActiveAnimator(part);
    }

    /// <summary>
    /// <para> (TH) : เลือกเปิด Animator ตามส่วนของไฟระเบิด (Start, Middle, End) และสั่ง Trigger แอนิเมชัน </para>
    /// </summary>
    public void SetActiveAnimator(BombPart part)
    {
        // ปิด Animator ทุกตัวก่อนเพื่อให้พร้อมใช้งานใหม่จาก Pool
        _startAnimator.gameObject.SetActive(false);
        _middleAnimator.gameObject.SetActive(false);
        _endAnimator.gameObject.SetActive(false);

        if (part == BombPart.Start)
        {
            _startAnimator.gameObject.SetActive(true);
            _startAnimator.SetTrigger("Start");
        }
        else if (part == BombPart.Middle)
        {
            _middleAnimator.gameObject.SetActive(true);
            _middleAnimator.SetTrigger("Middle");
        }
        else if (part == BombPart.End)
        {
            _endAnimator.gameObject.SetActive(true);
            _endAnimator.SetTrigger("End");
        }
    }

    /// <summary>
    /// <para> (TH) : ปรับมุมหมุนของ Object ตามทิศทางที่ได้รับจากการคำนวณ </para>
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        float angle = Mathf.Atan2(direction.y, direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    #endregion //Public Methods
}