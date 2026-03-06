using System;

namespace BombGame.RecordEventSpace
{

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ช่องทาง Observer สำหรับแจ้งสถานะตัวละครที่พร้อมให้ Controller (คนหรือบอท) เข้าควบคุม </para>
    /// <para> (EN) : Observer channel for notifying character status available for Controller (Human or Bot) takeover. </para>
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerSessionChannel", menuName = "BombGame/Events/Player Session Channel")]
    public sealed class PlayerSessionChannelSO : ScriptableObject
    {
        // ตัวละครที่ว่างอยู่และตำแหน่ง Transform ของมัน
        public Dictionary<Character, Transform> AvailableBodies { get; private set; } = new();

        public event Action OnSessionUpdated;

        public void RegisterBody(Character id, Transform body)
        {
            if (AvailableBodies.ContainsKey(id)) AvailableBodies[id] = body;
            else AvailableBodies.Add(id, body);
            OnSessionUpdated?.Invoke();
        }

        public Transform GetBody(Character id)
        {
            return AvailableBodies.TryGetValue(id, out var body) ? body : null;
        }
    }

}