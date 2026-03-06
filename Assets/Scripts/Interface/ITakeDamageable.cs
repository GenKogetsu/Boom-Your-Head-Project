using UnityEngine;
using System;
using System.Collections;

namespace Genoverrei.Libary;

/// <summary>
/// <para> summary_ITakeDamageable </para>
/// <para> (TH) : อินเตอร์เฟสสำหรับวัตถุที่สามารถรับดาเมจได้ </para>
/// <para> (EN) : Interface for objects that can take damage. </para>
/// </summary>
public interface ITakeDamageable : IAbility
{
    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : เริ่มกระบวนการรับดาเมจ รวมถึงสถานะอมตะและเอฟเฟกต์ </para>
    /// <para> (EN) : Starts damage intake process including invincibility. </para>
    /// </summary>
    void TakeDamage(int amount);

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : คำนวณการลดลงของพลังชีวิตโดยตรง </para>
    /// <para> (EN) : Directly calculates health reduction. </para>
    /// </summary>
    void ApplyDamage(int amount);

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : สถานะอมตะชั่วคราว </para>
    /// <para> (EN) : Temporary invincibility state. </para>
    /// </summary>
    bool IsInvincible { get; set; }

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ตัวแสดงผลภาพสำหรับทำเอฟเฟกต์กระพริบ </para>
    /// <para> (EN) : Sprite renderer for flashing effects. </para>
    /// </summary>
    SpriteRenderer SpriteRenderer { get; }

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ตัวรัน Coroutine สำหรับจัดการเวลาสถานะอมตะ </para>
    /// <para> (EN) : MonoBehaviour to run invincibility coroutines. </para>
    /// </summary>
    MonoBehaviour CoroutineRunner { get; }
}

/// <summary>
/// <para> summary_DamageAbility </para>
/// <para> (TH) : คลาส Static Helper สำหรับระบบรับดาเมจและเอฟเฟกต์อมตะ </para>
/// <para> (EN) : Static helper for damage system and invincibility effects. </para>
/// </summary>
public static class DamageAbility<T> where T : MonoBehaviour, ITakeDamageable
{
    #region Public Methods

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ประมวลผลการรับดาเมจพร้อมเริ่มสถานะอมตะและ Callback </para>
    /// <para> (EN) : Processes damage, starts invincibility, and triggers callbacks. </para>
    /// </summary>
    public static void TakeDamage(ITakeDamageable target, int amount, float invTime = 1f, float flashSpeed = 10f, Action onHit = null)
    {
        if (target == null || target.IsInvincible || target.CoroutineRunner == null) return;

        target.ApplyDamage(amount);
        onHit?.Invoke();

        if (target.CoroutineRunner.gameObject.activeInHierarchy)
        {
            target.CoroutineRunner.StartCoroutine(InvincibleRoutine(target, invTime, flashSpeed));
        }
    }

    #endregion //Public Methods

    #region Private Logic

    private static IEnumerator InvincibleRoutine(ITakeDamageable target, float time, float speed)
    {
        target.IsInvincible = true;
        float t = 0f;

        Color originalColor = target.SpriteRenderer != null ? target.SpriteRenderer.color : Color.white;

        while (t < time)
        {
            if (target.CoroutineRunner == null || target.SpriteRenderer == null) yield break;

            float alpha = Mathf.PingPong(Time.time * speed, 1f);
            SetAlpha(target.SpriteRenderer, alpha, originalColor);

            t += Time.deltaTime;
            yield return null;
        }

        if (target.SpriteRenderer != null)
        {
            SetAlpha(target.SpriteRenderer, 1f, originalColor);
        }

        target.IsInvincible = false;
    }

    private static void SetAlpha(SpriteRenderer sr, float a, Color originalColor)
    {
        if (sr == null) return;

        Color c = originalColor;
        c.a = Mathf.Lerp(0.3f, 1f, a);
        sr.color = c;
    }

    #endregion //Private Logic
}