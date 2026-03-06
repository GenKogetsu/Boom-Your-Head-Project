using Genoverrei.Libary;
using UnityEngine;

namespace Genoverrei.Libary
{
    /// <summary>
    /// <para> (TH) : อินเตอร์เฟซสำหรับตัวละครหรือวัตถุที่ต้องการเคลื่อนที่บน Grid </para>
    /// </summary>
    public interface ITileMoveable : IAbility
    {
        float MoveSpeed { get; }
        Rigidbody2D Rigidbody { get; }
        Vector2 TargetPosition { get; set; }
        bool IsMoving { get; set; }
        LayerMask CollisionLayer { get; }
        Vector2 CollisionCheckSize { get; }
        Vector2 MoveInputValue { get; }
        float OffsetX { get; }
        float OffsetY { get; }

        // 🚀 เปลี่ยนจาก Singleton มาใช้ MapChannel (สายไฟเชื่อมต่อ MapManager)
        Genoverrei.DesignPattern.MapChannelSO MapChannel { get; }

        void Move(Vector2 input);
    }

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ความสามารถในการเคลื่อนที่แบบ Grid-Based พร้อมระบบ Corner Nudging โดยไม่ใช้ Singleton </para>
    /// </summary>
    public static class TileMoveAbility
    {
        /// <summary>
        /// (TH) : อัปเดตตำแหน่งและจัดการคิวการเดิน (เรียกใน FixedUpdate)
        /// </summary>
        public static void ExecuteUpdate(ITileMoveable actor, float deltaTime)
        {
            if (actor.IsMoving)
            {
                // 1. เคลื่อนที่ Rigidbody ไปยังพิกัดเป้าหมาย
                Vector2 newPos = Vector2.MoveTowards(actor.Rigidbody.position, actor.TargetPosition, actor.MoveSpeed * deltaTime);
                actor.Rigidbody.MovePosition(newPos);

                float distanceToTarget = Vector2.Distance(actor.Rigidbody.position, actor.TargetPosition);

                // 2. ถ้าใกล้ถึงเป้าหมายและยังกดเดินอยู่ ให้เตรียมคำนวณช่องถัดไป (Buffer)
                if (actor.MoveInputValue != Vector2.zero && distanceToTarget < 0.15f)
                {
                    ProcessMoveRequest(actor);
                }

                // 3. เมื่อถึงตำแหน่งเป้าหมายแบบสนิท
                if (distanceToTarget < 0.005f)
                {
                    actor.Rigidbody.position = actor.TargetPosition;

                    if (actor.MoveInputValue == Vector2.zero)
                    {
                        actor.IsMoving = false;
                    }
                    else
                    {
                        ProcessMoveRequest(actor);
                    }
                }
            }
            else if (actor.MoveInputValue != Vector2.zero)
            {
                // ถ้าอยู่นิ่งแต่มีการกดปุ่ม ให้เริ่มคำนวณการเดิน
                ProcessMoveRequest(actor);
            }
        }

        /// <summary>
        /// (TH) : ประมวลผลคำขอเดิน ตรวจสอบช่องว่าง และระบบเลี้ยวเข้าซอย
        /// </summary>
        public static void ProcessMoveRequest(ITileMoveable actor)
        {
            Vector2 dir = GetDiscreteDirection(actor.MoveInputValue);
            Vector2 nextPos = actor.TargetPosition + dir;

            if (!IsPositionOccupied(actor, nextPos))
            {
                actor.TargetPosition = nextPos;
                actor.IsMoving = true;
            }
            else
            {
                // 🚀 ระบบเลี้ยวเข้าซอย (Corner Nudging)
                if (!TryApplyNudge(actor, actor.MoveInputValue, dir))
                {
                    if (Vector2.Distance(actor.Rigidbody.position, actor.TargetPosition) < 0.005f)
                    {
                        actor.IsMoving = false;
                    }
                }
            }
        }

        /// <summary>
        /// (TH) : ตรวจสอบสิ่งกีดขวางผ่าน MapChannel และ Physics Overlap
        /// </summary>
        public static bool IsPositionOccupied(ITileMoveable actor, Vector2 targetPos)
        {
            // 🛡️ STEP 1: เช็คผ่านระบบแผนที่ (เลิกใช้ MapManager.Instance แล้วใช้สายไฟแทน)
            if (actor.MapChannel != null)
            {
                // ปัดเศษพิกัดเพื่อให้ตรงกับตำแหน่ง Tile ใน Grid
                Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(targetPos.x), Mathf.RoundToInt(targetPos.y));

                // ถาม MapManager ผ่านสายไฟว่าช่องนี้เดินได้ไหม
                if (!actor.MapChannel.IsWalkable(gridPos))
                {
                    return true;
                }
            }

            // 🛡️ STEP 2: เช็คผ่าน Physics (เช่น ระเบิด, ตัวละครอื่น)
            // ใช้ค่าที่ส่งมาจาก Actor (แนะนำ 0.8 ถึง 0.9 เพื่อให้เดินรอดช่องแคบได้)
            Collider2D hit = Physics2D.OverlapBox(targetPos, actor.CollisionCheckSize, 0f, actor.CollisionLayer);

            // ต้องชนสิ่งของที่ไม่ใช่ตัวเอง (ห้ามใช้ null propagation กับ Unity Object)
            if (hit != null)
            {
                if (hit.gameObject != actor.Rigidbody.gameObject)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// (TH) : ปัดตำแหน่งปัจจุบันให้ลงล็อก Grid
        /// </summary>
        public static Vector2 SnapToGrid(Vector2 pos, float offsetX = 0, float offsetY = 0)
        {
            return new Vector2(Mathf.Round(pos.x - offsetX) + offsetX, Mathf.Round(pos.y - offsetY) + offsetY);
        }

        /// <summary>
        /// (TH) : แปลงทิศทาง Input ให้เหลือแค่ 4 ทิศหลัก
        /// </summary>
        public static Vector2 GetDiscreteDirection(Vector2 input)
        {
            if (input == Vector2.zero) return Vector2.zero;

            return Mathf.Abs(input.x) > Mathf.Abs(input.y)
                ? new Vector2(input.x > 0 ? 1 : -1, 0)
                : new Vector2(0, input.y > 0 ? 1 : -1);
        }

        /// <summary>
        /// (TH) : ระบบช่วยเลี้ยวเมื่อเดินเบียดขอบกำแพง
        /// </summary>
        private static bool TryApplyNudge(ITileMoveable actor, Vector2 rawInput, Vector2 discreteDir)
        {
            // ทำงานเมื่อมีการกดปุ่มเฉียง (Diagonal)
            if (Mathf.Abs(rawInput.x) < 0.2f || Mathf.Abs(rawInput.y) < 0.2f) return false;

            Vector2 nudgeDir = (discreteDir.x != 0)
                ? new Vector2(0, rawInput.y > 0 ? 1 : -1)
                : new Vector2(rawInput.x > 0 ? 1 : -1, 0);

            Vector2 checkSide = actor.TargetPosition + nudgeDir;

            if (!IsPositionOccupied(actor, checkSide) && !IsPositionOccupied(actor, checkSide + discreteDir))
            {
                actor.TargetPosition = checkSide;
                actor.IsMoving = true;
                return true;
            }
            return false;
        }
    }
}