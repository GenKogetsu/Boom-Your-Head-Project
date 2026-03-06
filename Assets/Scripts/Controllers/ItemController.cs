using Genoverrei.DesignPattern;

public class ItemController : MonoBehaviour
{
    [SerializeField] private AnimationClip _criticalClip;
    [SerializeField] private float _offset = 0.1f;
    private float _lifeTime;

    private void OnEnable() { _lifeTime = 0; }
    private void Update() { _lifeTime += Time.deltaTime; }

    public void DestroyMeWhenHit()
    {
        // 🛡️ เช็คเงื่อนไขอมตะชั่วคราวตามที่พี่บอก
        // ถ้าเวลาเกิดยังน้อยกว่าช่วง Critical + Offset จะยังไม่ถูกทำลาย
        if (_lifeTime < _criticalClip.length + _offset)
        {
            Debug.Log("Item is protected!");
            return;
        }

        // ถ้าพ้นช่วงอมตะแล้ว ก็กลับเข้า Pool ไป
        ObjectPoolManager.Instance.Release("Item", this);
    }
}