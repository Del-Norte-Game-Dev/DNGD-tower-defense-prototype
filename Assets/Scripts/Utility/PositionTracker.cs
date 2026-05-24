using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    [SerializeField] Transform parent;
    [SerializeField] Transform t;

    void LateUpdate()
    {
        Vector3 worldPos = t.position;
        transform.localPosition = parent.InverseTransformPoint(worldPos);
    }
}
