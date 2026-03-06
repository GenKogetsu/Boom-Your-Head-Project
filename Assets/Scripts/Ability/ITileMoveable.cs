using Genoverrei.Libary;
using UnityEngine;

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

    void Move(Vector2 input);
}

public static class TileMoveAbility
{
    public static void ExecuteUpdate(ITileMoveable actor, float deltaTime)
    {
        if (actor.IsMoving)
        {
            Vector2 newPos = Vector2.MoveTowards(actor.Rigidbody.position, actor.TargetPosition, actor.MoveSpeed * deltaTime);
            actor.Rigidbody.MovePosition(newPos);

            if (actor.MoveInputValue != Vector2.zero && Vector2.Distance(actor.Rigidbody.position, actor.TargetPosition) < 0.15f)
            {
                ProcessMoveRequest(actor);
            }

            if (Vector2.Distance(actor.Rigidbody.position, actor.TargetPosition) < 0.005f)
            {
                actor.Rigidbody.position = actor.TargetPosition;
                if (actor.MoveInputValue == Vector2.zero) actor.IsMoving = false;
                else ProcessMoveRequest(actor);
            }
        }
        else if (actor.MoveInputValue != Vector2.zero)
        {
            ProcessMoveRequest(actor);
        }
    }

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
            // 🚀 คืนชีพระบบเลี้ยวเข้าซอย (Corner Nudging) ของพี่
            if (!TryApplyNudge(actor, actor.MoveInputValue, dir))
            {
                if (Vector2.Distance(actor.Rigidbody.position, actor.TargetPosition) < 0.005f)
                    actor.IsMoving = false;
            }
        }
    }

    public static bool IsPositionOccupied(ITileMoveable actor, Vector2 targetPos)
    {
        return Physics2D.OverlapBox(targetPos, actor.CollisionCheckSize, 0f, actor.CollisionLayer) != null;
    }

    public static Vector2 SnapToGrid(Vector2 pos, float offsetX = 0, float offsetY = 0)
    {
        return new Vector2(Mathf.Round(pos.x - offsetX) + offsetX, Mathf.Round(pos.y - offsetY) + offsetY);
    }

    public static Vector2 GetDiscreteDirection(Vector2 input)
    {
        if (input == Vector2.zero) return Vector2.zero;
        return Mathf.Abs(input.x) > Mathf.Abs(input.y)
            ? new Vector2(input.x > 0 ? 1 : -1, 0)
            : new Vector2(0, input.y > 0 ? 1 : -1);
    }

    private static bool TryApplyNudge(ITileMoveable actor, Vector2 rawInput, Vector2 discreteDir)
    {
        if (!(Mathf.Abs(rawInput.x) > 0.2f && Mathf.Abs(rawInput.y) > 0.2f)) return false;

        Vector2 nudgeDir = discreteDir.x != 0
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