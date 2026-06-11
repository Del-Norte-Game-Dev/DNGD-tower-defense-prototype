using UnityEngine;

public abstract class BuildingBehavior : MonoBehaviour, IBuilding
{
    public virtual void Init() { }

    public virtual bool CanPlace(Vector3 worldPos) => true;

    public virtual void TakeDamage(float damage) { }
}
