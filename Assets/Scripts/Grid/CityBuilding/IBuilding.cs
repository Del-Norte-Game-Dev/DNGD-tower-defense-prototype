using UnityEngine;

public interface IBuilding
{
    void Init() { }
    bool CanPlace(Vector3 worldPos) => true;
    void TakeDamage(int damage){}
}
