using UnityEngine;
using BombGame.EnumSpace;

/// <summary>
/// <para> summary : </para>
/// <para> (TH) : ScriptableObject สำหรับเก็บค่าสถานะพื้นฐานของสิ่งมีชีวิตในเกม </para>
/// <para> (EN) : ScriptableObject for storing base statistics of living things in the game. </para>
/// </summary>
[CreateAssetMenu(fileName = "LivingThingsScriptable", menuName = "Scriptable Objects/LivingThingsScriptable")]
public class LivingThingsScriptable : ScriptableObject
{
    [Header("Info")]
    public Character livingName;
    public Charactertype livingType;
    public int livingId;

    [Header("Base Stats")]
    public int baseHp = 3;
    public int baseAtk = 1;
    public float baseSpeed = 5f;

    [Header("Bomb Settings")]
    public int baseBombAmount = 1;
    public int baseExplosionRange = 1;
}