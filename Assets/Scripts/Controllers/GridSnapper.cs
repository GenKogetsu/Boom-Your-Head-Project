using UnityEngine;

[ExecuteAlways]
public class GridSnapper : MonoBehaviour
{
    [SerializeField] private bool _enableSnap = true;

    private void LateUpdate()
    {
        if (_enableSnap)
        {
            SnapToCenter();
        }
    }
        
    private void SnapToCenter()
    {
        Vector3 pos = transform.position;

        float snapX = Mathf.Round(pos.x);
        float snapY = Mathf.Round(pos.y);

        if (pos.x != snapX || pos.y != snapY)
        {
            transform.position = new Vector3(snapX, snapY, pos.z);
        }
    }
    private void OnValidate()
    {
        if (_enableSnap)
        {
            SnapToCenter();
        }
    }
}