using System;

namespace BombGame.RecordEventSpace
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ช่องทางสื่อสารให้ตัวละครส่งพิกัดเพื่อขอให้สร้างระเบิด </para>
    /// <para> (EN) : Communication channel for characters to send coordinates requesting a bomb spawn. </para>
    /// </summary>
    [CreateAssetMenu(fileName = "BombSpawnChannel", menuName = "BombGame/Events/Bomb Spawn Channel")]
    public sealed class BombSpawnChannelSO : ScriptableObject
    {
        public event Action<Vector2Int> OnBombSpawnRequested;

        public void RaiseEvent(Vector2Int position)
        {
            OnBombSpawnRequested?.Invoke(position);
        }
    }
}

