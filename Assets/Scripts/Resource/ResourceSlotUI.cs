using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;

    private ResourceType resourceType;
    private ResourceDatabase database;

    public void Setup(ResourceType type, ResourceDatabase db)
    {
        resourceType = type;
        database = db;

        var res = database.Get(type);

        if (res != null)
        {
            icon.sprite = res.icon;
            icon.color = res.UIColor;
        }

        ResourceManager.Instance.OnResourceChanged += OnResourceChanged;

        Refresh(ResourceManager.Instance.GetAmount(type));
    }

    private void OnResourceChanged(ResourceType type, int amount)
    {
        if (type != resourceType) return;
        Refresh(amount);
    }

    private void Refresh(int amount)
    {
        amountText.text = amount.ToString();
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourceChanged -= OnResourceChanged;
    }
}