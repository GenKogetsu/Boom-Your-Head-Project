using UnityEngine;
using Genoverrei.DesignPattern;

namespace Genoverrei.Libary;

/// <summary>
/// <para> summary : </para>
/// <para> (TH) : กำหนดฟังก์ชันสำหรับ Object แบบ Tile-based ที่เคลื่อนที่ใน 2D 
/// รวมถึงความเร็ว, การตรวจจับการชน และการควบคุมการเคลื่อนที่ </para>
/// <para> (EN) : Defines functionality for a tile-based object in 2D, 
/// including speed, collision detection, and movement control. </para>
/// </summary>
/// <remarks>
/// <para> remarks : </para>
/// <para> (TH) : ใช้สำหรับ Entity ที่เคลื่อนที่ด้วย User Input หรือ AI บนระบบ Grid 
/// โดยจะเช็ค Collision ตาม Layer และขนาดที่กำหนด มี Property ควบคุมสถานะ, 
/// Target Position และ Input ต่างๆ ส่วนพฤติกรรมเฉพาะขึ้นอยู่กับคลาสที่ Implement </para>
/// <para> (EN) : Used for entities moved by input or AI in a grid system. 
/// Movement is subject to collision checks via layers and specified size. 
/// Provides properties for state, target position, and input values. </para>
/// </remarks>
public interface ITileMoveable : IAbility
{
    float MoveSpeed { get; }
    Rigidbody2D Rigidbody { get; }
    Vector2 TargetPosition { get; set; }
    bool IsMoving { get; set; }
    LayerMask CollisionLayer { get; }
    Vector2 CollisionCheckSize { get; }
    Vector2 MoveInputValue { get; }

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : เคลื่อนย้าย Object ตามทิศทางของ Input ที่ระบุ </para>
    /// <para> (EN) : Moves the object based on the specified directional input. </para>
    /// </summary>
    /// <param name="input">
    /// <para> param : </para>
    /// <para> (TH) : Vector2 ที่แทนทิศทางและขนาดของการเคลื่อนที่ โดยค่า X และ Y 
    /// จะกำหนดการเคลื่อนที่ในแนวแกน X และ Y ตามลำดับ </para>
    /// <para> (EN) : A Vector2 representing the direction and magnitude of movement. 
    /// The X and Y components determine horizontal and vertical movement. </para>
    /// </param>
    void Move(Vector2 input);

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : อัปเดตการเคลื่อนที่ของ Object ตามเวลาที่ผ่านไปจากการอัปเดตครั้งล่าสุด </para>
    /// <para> (EN) : Updates the object's movement based on the elapsed time 
    /// since the last update. </para>
    /// </summary>
    /// <param name="deltaTime">
    /// <para> param : </para>
    /// <para> (TH) : ระยะเวลา (วินาที) ที่ผ่านไปนับจากการอัปเดตก่อนหน้า 
    /// โดยค่าที่ใช้ต้องไม่เป็นลบ </para>
    /// <para> (EN) : The time, in seconds, that has elapsed since the 
    /// previous update. Must be a non-negative value. </para>
    /// </param>
    void UpdateMovement(float deltaTime);
}

/// <summary>
/// <para> summary_TileMoveAbility </para>
/// <para> (TH) : คลาส Static Helper สำหรับจัดการ Grid-based Movement และ Collision Detection สำหรับ Actor บน Tile ในสภาพแวดล้อม 2D </para>
/// <para> (EN) : Static helper for managing grid-based movement and collision detection for tile-moveable actors in 2D. </para>
/// </summary>
public static class TileMoveAbility<T> where T : MonoBehaviour, ITileMoveable
{
    #region Public Methods

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : อัปเดตตำแหน่งปัจจุบันและจัดการตรรกะการเคลื่อนที่บน Grid </para>
    /// <para> (EN) : Updates the current position and handles grid-based movement logic. </para>
    /// </summary>
    public static void ExecuteUpdate(T actor, float deltaTime)
    {
        if (actor.IsMoving)
        {
            Vector2 newPos = Vector2.MoveTowards(actor.Rigidbody.position, actor.TargetPosition, actor.MoveSpeed * deltaTime);
            actor.Rigidbody.MovePosition(newPos);

            if (actor.MoveInputValue != Vector2.zero && Vector2.Distance(actor.Rigidbody.position, actor.TargetPosition) < 0.15f)
                ProcessMoveRequest(actor);

            if (Vector2.Distance(actor.Rigidbody.position, actor.TargetPosition) < 0.005f)
            {
                actor.Rigidbody.position = actor.TargetPosition;
                if (actor.MoveInputValue == Vector2.zero) actor.IsMoving = false;
                else ProcessMoveRequest(actor);
            }
        }
        else if (actor.MoveInputValue != Vector2.zero) ProcessMoveRequest(actor);
    }

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ประมวลผลการร้องขอเคลื่อนที่ไปยังช่องถัดไปตามทิศทาง Input </para>
    /// <para> (EN) : Processes a movement request to the next tile based on input direction. </para>
    /// </summary>
    public static void ProcessMoveRequest(T actor)
    {
        Vector2 dir = GetDiscreteDirection(actor.MoveInputValue);
        Vector2 nextPos = actor.TargetPosition + dir;

        if (!IsPositionOccupied(actor, nextPos))
        {
            actor.TargetPosition = nextPos;
            actor.IsMoving = true;
        }
    }

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ตรวจสอบว่าพิกัดเป้าหมายถูกครอบครองโดยสิ่งกีดขวางหรือไม่ </para>
    /// <para> (EN) : Checks if the target coordinates are occupied by an obstacle. </para>
    /// </summary>
    public static bool IsPositionOccupied(T actor, Vector2 targetPos) =>
        Physics2D.OverlapBox(targetPos, actor.CollisionCheckSize, 0f, actor.CollisionLayer) != null;

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : แปลง Vector2 ให้เป็นทิศทางแบบไม่ต่อเนื่อง (4 ทิศทางหลัก) </para>
    /// <para> (EN) : Converts Vector2 into a discrete direction (4 primary directions). </para>
    /// </summary>
    public static Vector2 GetDiscreteDirection(Vector2 input)
    {
        if (input == Vector2.zero) return Vector2.zero;
        return Mathf.Abs(input.x) > Mathf.Abs(input.y) ? new Vector2(input.x > 0 ? 1 : -1, 0) : new Vector2(0, input.y > 0 ? 1 : -1);
    }

    /// <summary>
    /// <para> summary : </para>
    /// <para> (TH) : ปรับตำแหน่งพิกัดให้ตรงกับจุดกึ่งกลางของ Grid </para>
    /// <para> (EN) : Snaps the coordinate position to the grid center. </para>
    /// </summary>
    public static Vector2 SnapToGrid(Vector2 pos) => new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));

    #endregion //Public Methods
}