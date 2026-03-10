using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapRandomWalker : MonoBehaviour
{
    public Tilemap targetTilemap;
    public float speed = 5f;

    [Header("Sprite Settings")]
    [Tooltip("ติ๊กถูกถ้าภาพ Sprite ต้นฉบับหันหน้าไปทางขวา / เอาออกถ้าหันไปทางซ้าย")]
    public bool initialFacingRight = true;

    [Header("Random Settings")]
    [Tooltip("ระยะที่ต้องการให้บีบขอบเขตการสุ่มเข้ามาจากขอบ Tilemap")]
    public Vector2 boundsOffset = new Vector2(1f, 1f);

    [Tooltip("ปรับแต่งจุดศูนย์กลางเป้าหมาย (เช่น ปรับให้เท้าเหยียบพื้นพอดี)")]
    public Vector3 centerOffset = Vector3.zero;

    private Vector3 targetPosition;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (targetTilemap != null)
        {
            SetRandomTarget();
        }
    }

    void Update()
    {
        if (targetTilemap == null) return;

        // เคลื่อนที่ไปยังเป้าหมาย
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        HandleSpriteFlip();

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            SetRandomTarget();
        }
    }

    private void HandleSpriteFlip()
    {
        bool targetIsRight = targetPosition.x > transform.position.x;
        if (initialFacingRight)
        {
            spriteRenderer.flipX = !targetIsRight;
        }
        else
        {
            spriteRenderer.flipX = targetIsRight;
        }
    }

    /// <summary>
    /// สุ่มพิกัดและปรับจูนด้วย centerOffset เพื่อความแม่นยำ
    /// </summary>
    private void SetRandomTarget()
    {
        BoundsInt bounds = targetTilemap.cellBounds;

        float minX = bounds.xMin + boundsOffset.x;
        float maxX = bounds.xMax - boundsOffset.x;
        float minY = bounds.yMin + boundsOffset.y;
        float maxY = bounds.yMax - boundsOffset.y;

        if (minX > maxX) maxX = minX;
        if (minY > maxY) maxY = minY;

        int randomX = Mathf.RoundToInt(Random.Range(minX, maxX));
        int randomY = Mathf.RoundToInt(Random.Range(minY, maxY));

        Vector3Int cellPosition = new Vector3Int(randomX, randomY, 0);

        // รับพิกัด World Space จาก Tilemap และบวกเพิ่มด้วย centerOffset ที่กำหนด
        targetPosition = targetTilemap.GetCellCenterWorld(cellPosition) + centerOffset;
    }

    // วาดเส้นไกด์ในหน้า Scene เพื่อช่วยในการตั้งค่า Offset
    private void OnDrawGizmosSelected()
    {
        if (targetTilemap == null) return;

        Gizmos.color = Color.green;
        BoundsInt b = targetTilemap.cellBounds;
        Vector3 size = new Vector3(b.size.x - (boundsOffset.x * 2), b.size.y - (boundsOffset.y * 2), 0);
        Vector3 center = targetTilemap.GetCellCenterWorld(new Vector3Int((int)b.center.x, (int)b.center.y, 0));

        // วาดกรอบพื้นที่สุ่ม
        Gizmos.DrawWireCube(center, size);

        // วาดจุดเป้าหมายปัจจุบัน
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, 0.3f);
    }
}