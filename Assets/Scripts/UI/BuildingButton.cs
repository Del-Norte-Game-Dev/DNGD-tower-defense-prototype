using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    [SerializeField] private BuildingData buildingData;

    private void Start()
    {
        BuildButtonManager.OnButtonPressed += UpdateButtonState;
    }

    public void SelectBuilding()
    {
        EventSystem.current.SetSelectedGameObject(null);


        if (BuildManager.Instance.GetBuildingData() == buildingData)
        {
            BuildManager.Instance.SelectBuilding(null);
            BuildButtonManager.Instance.SetSelectedButton(null);
        }
        else
        {
            BuildManager.Instance.SelectBuilding(buildingData);
            BuildButtonManager.Instance.SetSelectedButton(this);
        }

        BuildButtonManager.Instance.ButtonPressed();
    }

    private void UpdateButtonState()
    {
        if (BuildButtonManager.Instance.GetSelectedBuildButton() == this)
        {
            GetComponent<Image>().color = BuildButtonManager.Instance.GetSelectedColor();
        }
        else
        {
            GetComponent<Image>().color = GetComponent<Button>().colors.normalColor;
        }
    }
}
