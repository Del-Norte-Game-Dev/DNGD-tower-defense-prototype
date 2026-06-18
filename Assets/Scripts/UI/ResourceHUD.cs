using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceHUD : MonoBehaviour
{
    [SerializeField] private Vector3 HUDPos;
    [SerializeField] private ResourceDatabase database;
    [SerializeField] private ResourceSlotUI slotPrefab;
    [SerializeField] private Transform container;

    private readonly List<ResourceSlotUI> spawnedSlots = new();

    private void Start()
    {
        BuildHUD();
    }
    private void BuildHUD()
    {
        foreach (Resource res in database.resources)
        {
            if (res == null) continue;

            ResourceSlotUI slot = Instantiate(slotPrefab, container);
            slot.Setup(res.type, database);

            spawnedSlots.Add(slot);
        }
    }
}