using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class Bootstrapper : MonoBehaviour {
    MonoBehaviour[] orderedServices;

    void Awake() {
        orderedServices = GetComponentsInChildren<MonoBehaviour>();
        if (orderedServices == null) return;
        
        foreach (var mb in orderedServices) {
            if (mb is IInitializable init) init.InitializeSingleton();
        }
    }
}