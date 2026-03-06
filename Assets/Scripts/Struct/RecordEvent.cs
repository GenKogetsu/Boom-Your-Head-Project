using BombGame.EnumSpace;
using Genoverrei.DesignPattern;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BombGame.RecoreEventSpace;

/// <summary>
/// <para> summary : </para>
/// <para> (TH) : รวมข้อมูลเหตุการณ์ (Events) ทั้งหมดที่เกิดขึ้นในเกม เพื่อใช้สื่อสารผ่าน EventBus </para>
/// <para> (EN) : Collection of all gameplay events used for communication via EventBus. </para>
/// </summary>

// --- Combat & Bomb Group ---

/// <summary>
/// <para> summary : </para>
/// <para> (TH) : เมื่อ Character ทำการกระทำบางอย่าง (เช่น วางระเบิด หรือ เคลื่อนที่) </para>
/// <para> (EN) : When a character performs an action. </para>
/// </summary>
public record struct CharacterAction(Character SignalTarget, ActionType Action, IEvent Event) : ISignal;

/// <summary>
/// <para> summary : </para>
/// <para> (TH) : ข้อมูลทิศทางการเคลื่อนที่ของตัวละคร </para>
/// <para> (EN) : Movement direction event data. </para>
/// </summary>
public record struct MoveInputEvent(Vector2 Direction) : IEvent;

/// <summary>
/// <para> summary : </para>
/// <para> (TH) : เมื่อมีการวางระเบิดลงบน Grid </para>
/// <para> (EN) : When a bomb is placed on the grid. </para>
/// </summary>
public record struct BombPlantedEvent(Vector2Int Position, int Radius, float FuseTime) : IEvent;

/// <summary>
/// <para> summary : </para>
/// <para> (TH) : ข้อมูลเหตุการณ์เมื่อระเบิดทำงาน ส่งต่อตำแหน่งและข้อมูลฉากเพื่อคำนวณแรงระเบิด </para>
/// <para> (EN) : Event data for bomb explosion, passing position and scene data for calculation. </para>
/// </summary>
public record struct BombExplodedEvent(Vector2Int Position,int Radius, List<Tilemap> SolidTilemaps, List<Tilemap> DestructibleTilemaps) : IEvent;

/// <summary>
/// <para> summary : </para>
/// <para> (TH) : เมื่อ Character ตาย (ส่งข้อมูล GameObject และประเภทเพื่อไปจัดการต่อ) </para>
/// <para> (EN) : When an Character dies. </para>
/// </summary>
public record struct CharacterDeathEvent(GameObject Victim, Charactertype Type) : IEvent;

// --- Environment Group ---

/// <summary>
/// <para> summary : </para>
/// <para> (TH) : เมื่อประเภทของ Tile บนแผนที่เปลี่ยนไป (เช่น กำแพงถูกทำลาย) </para>
/// <para> (EN) : When a tile type on the map changes. </para>
/// </summary>
public record struct TileChangedEvent(Vector2Int GridPos, TileType NewType) : IEvent;