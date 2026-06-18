using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class BuildButtonManager : GenericSingleton<BuildButtonManager>
{
    public static event Action OnButtonPressed;

    private BuildingButton selectedButton;

    [SerializeField] private Color selectedColor;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var inputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        if (inputModule != null)
            inputModule.move = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelectedButton(BuildingButton button)
    {
        selectedButton = button;
    }

    public BuildingButton GetSelectedBuildButton()
    {
        return selectedButton;
    }
    public Color GetSelectedColor()
    {
        return selectedColor;
    }

    public void ButtonPressed()
    {
        OnButtonPressed?.Invoke();
    }
}
