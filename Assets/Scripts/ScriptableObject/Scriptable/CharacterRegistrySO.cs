using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterRegistry", menuName = "BombGame/Data/CharacterRegistry")]
public class CharacterRegistrySO : ScriptableObject
{
    // 🚀 เก็บแบบ GameObject เพื่อความเสถียรสูงสุดในการ Serialize
    [NonSerialized] private Dictionary<Character, GameObject> _activeObjects = new Dictionary<Character, GameObject>();

    [Header("Debug Registry View (Runtime Only)")]
    [SerializeField] private List<RegistryEntry> _inspectorView = new List<RegistryEntry>();

    [Serializable]
    public struct RegistryEntry
    {
        public Character Id;
        public GameObject ObjRef;

        public RegistryEntry(Character id, GameObject obj)
        {
            Id = id;
            ObjRef = obj;
        }
    }

    public void Register(Character type, GameObject obj)
    {
        if (type == Character.None || obj == null) return;
        _activeObjects[type] = obj;
        SyncInspectorView();
    }

    public GameObject GetCharacterObj(Character type)
    {
        if (_activeObjects == null) return null;
        _activeObjects.TryGetValue(type, out var obj);
        return obj;
    }

    // 🚀 ฟังก์ชันหัวใจที่บอทจะเรียกใช้ เพื่อดึง Stats ไปคำนวณ
    public StatsController GetStats(Character type)
    {
        var obj = GetCharacterObj(type);
        return obj != null ? obj.GetComponent<StatsController>() : null;
    }

    public void Clear()
    {
        if (_activeObjects != null) _activeObjects.Clear();
        _inspectorView.Clear();
    }

    private void SyncInspectorView()
    {
        _inspectorView.Clear();
        if (_activeObjects == null) return;
        foreach (var kvp in _activeObjects)
        {
            if (kvp.Value != null)
                _inspectorView.Add(new RegistryEntry(kvp.Key, kvp.Value));
        }
    }

    private void OnEnable() => Clear();
}