using System.Collections.Generic;
using UnityEngine;
using BombGame.Manager;

namespace Genoverrei.Libary;

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : อินเทอร์เฟซสำหรับวัตถุที่ต้องการใช้งานระบบค้นหาเส้นทาง (A* Pathfinding) </para>
/// <para> (EN) : Interface for objects requiring the A* pathfinding system. </para>
/// </summary>
public interface IPathfindable
{
    Vector2Int CurrentGridPosition { get; }
    IMapProvider MapProvider { get; }

    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ฟังก์ชันสำหรับเรียกใช้งานการค้นหาเส้นทาง (Explicit Implementation) </para>
    /// <para> (EN) : Function to invoke pathfinding (Explicit Implementation). </para>
    /// </summary>
    Vector2Int GetNextPath(Vector2Int targetPosition);
}

/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : ความสามารถในการค้นหาเส้นทางด้วยอัลกอริทึม A* สำหรับวัตถุที่เป็น IPathfindable </para>
/// <para> (EN) : A* pathfinding ability for IPathfindable objects. </para>
/// </summary>
public static class PathfindAbility<T> where T : IPathfindable
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : จุดรวมตรรกะ A* (ถูกเรียกใช้โดย Execute ของคลาสที่สืบทอด IPathfindable) </para>
    /// <para> (EN) : Central A* logic (called by the Execute method of IPathfindable classes). </para>
    /// </summary>
    public static Vector2Int Execute(T actor, Vector2Int targetPosition)
    {
        Vector2Int startPos = actor.CurrentGridPosition;
        IMapProvider map = actor.MapProvider;

        if (startPos == targetPosition || map == null) return startPos;

        // --- Logic A* เหมือนเดิมเป๊ะ ---
        List<AStarNode> openList = new List<AStarNode> { new AStarNode(startPos) };
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        while (openList.Count > 0)
        {
            AStarNode currentNode = GetLowestFCostNode(openList);
            openList.Remove(currentNode);
            closedList.Add(currentNode.Position);

            if (currentNode.Position == targetPosition)
                return ExecuteRetracePath(startPos, currentNode);

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                Vector2Int neighborPos = currentNode.Position + dir;
                if (!map.IsWalkable(neighborPos) || closedList.Contains(neighborPos)) continue;

                AStarNode neighborNode = new AStarNode(neighborPos, currentNode);
                neighborNode.GCost = currentNode.GCost + 1;
                neighborNode.HCost = CalculateDistance(neighborPos, targetPosition);

                AStarNode existingNode = openList.Find(n => n.Position == neighborPos);
                if (existingNode == null || neighborNode.GCost < existingNode.GCost)
                {
                    if (existingNode == null) openList.Add(neighborNode);
                    else { existingNode.GCost = neighborNode.GCost; existingNode.Parent = currentNode; }
                }
            }
        }
        return startPos;
    }

    // [Private Helper Methods: CalculateDistance, GetLowestFCostNode, ExecuteRetracePath เหมือนเดิม]
    private static int CalculateDistance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    private static AStarNode GetLowestFCostNode(List<AStarNode> nodes) { /* ... */ return nodes[0]; } // ย่อไว้
    private static Vector2Int ExecuteRetracePath(Vector2Int start, AStarNode end) { /* ... */ return end.Position; }

    private class AStarNode
    {
        public Vector2Int Position; public AStarNode Parent;
        public int GCost; public int HCost; public int FCost => GCost + HCost;
        public AStarNode(Vector2Int pos, AStarNode par = null) { Position = pos; Parent = par; }
    }
}