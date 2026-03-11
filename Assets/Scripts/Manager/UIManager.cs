using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace BombGame.UI
{
    public sealed class UIManager : MonoBehaviour
    {
        [Header("Ryuwen Stats Reference")]
        [SerializeField] private StatsController _ryuwenStats;
        [SerializeField] private GameObject _ryuwenOut;
        [SerializeField] private TextMeshProUGUI _ryuwenHp, _ryuwenBomb, _ryuwenSpeed, _ryuwenRange;

        [Header("Edigan Stats Reference")]
        [SerializeField] private StatsController _ediganStats;
        [SerializeField] private GameObject _ediganOut;
        [SerializeField] private TextMeshProUGUI _ediganHp, _ediganBomb, _ediganSpeed, _ediganRange;

        [Header("Baboon Stats Reference")]
        [SerializeField] private StatsController _baboonStats;
        [SerializeField] private GameObject _baboonOut;
        [SerializeField] private TextMeshProUGUI _baboonHp, _baboonBomb, _baboonSpeed, _baboonRange;

        [Header("Terbi Stats Reference")]
        [SerializeField] private StatsController _terbiStats;
        [SerializeField] private GameObject _terbiOut;
        [SerializeField] private TextMeshProUGUI _terbiHp, _terbiBomb, _terbiSpeed, _terbiRange;

        private void Update()
        {
            UpdatePlayerUI(_ryuwenStats, _ryuwenOut, _ryuwenHp, _ryuwenBomb, _ryuwenSpeed, _ryuwenRange);
            UpdatePlayerUI(_ediganStats, _ediganOut, _ediganHp, _ediganBomb, _ediganSpeed, _ediganRange);
            UpdatePlayerUI(_baboonStats, _baboonOut, _baboonHp, _baboonBomb, _baboonSpeed, _baboonRange);
            UpdatePlayerUI(_terbiStats, _terbiOut, _terbiHp, _terbiBomb, _terbiSpeed, _terbiRange);
        }

        private void UpdatePlayerUI(StatsController stats, GameObject outImg, TextMeshProUGUI hp, TextMeshProUGUI bomb, TextMeshProUGUI speed, TextMeshProUGUI range)
        {
            if (stats == null) return;

            if (hp) hp.text = stats.CurrentHp.ToString();
            if (bomb) bomb.text = stats.BombsRemaining.ToString();

            int speedVal = (int)Mathf.Floor(stats.CurrentSpeed);
            if (speed) speed.text = speedVal.ToString();

            if (range) range.text = stats.CurrentExplosionRange.ToString();

            if (outImg)
            {
                bool isDead = stats.CurrentHp <= 0;
                if (outImg.activeSelf != isDead) outImg.SetActive(isDead);
            }
        }
    }
}