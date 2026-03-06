using UnityEngine;
using BombGame.EnumSpace;

namespace BombGame.Data
{
    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ??????????????????????????????? ???? ???????????? ??????????????? ??????????????? </para>
    /// <para> (EN) : Current game session data, including player count, selected characters, and current stage. </para>
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Game Session Data")]
    public class GameSessionData : ScriptableObject
    {
        #region Variable

        [Header("Player Setup")]
        [SerializeField] private int _playerCount = 1;

        [SerializeField] private Character _firstPlayerCharacter;

        [SerializeField] private Character _secondPlayerCharacter;

        [Header("Stage")]
        [SerializeField] private int _currentStageIndex = 0;

        #endregion //Variable

        #region Properties

        public int PlayerCount
        {
            get => _playerCount;
            set => _playerCount = Mathf.Clamp(value, 1, 2);
        }

        public Character FirstPlayerCharacter
        {
            get => _firstPlayerCharacter;
            set => _firstPlayerCharacter = value;
        }

        public Character SecondPlayerCharacter
        {
            get => _secondPlayerCharacter;
            set => _secondPlayerCharacter = value;
        }

        public int CurrentStageIndex
        {
            get => _currentStageIndex;
            set => _currentStageIndex = value;
        }

        #endregion //Properties

        #region Public Methods

        /// <summary>
        /// <para> summary : </para>
        /// <para> (TH) : ????????????????????????????????????? </para>
        /// <para> (EN) : Resets session data to default values. </para>
        /// </summary>
        public void ResetSession()
        {
            _playerCount = 1;
            _firstPlayerCharacter = default;
            _secondPlayerCharacter = default;
            _currentStageIndex = 0;
        }

        #endregion //Public Methods
    }
}

