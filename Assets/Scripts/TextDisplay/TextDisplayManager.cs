using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

//Call TextDisplayManager.New(...).Build() from main thread.
public class TextDisplayManager : PersistentGenericSingleton<TextDisplayManager>
{
    [SerializeField] private Canvas canvas;
    private GameObject container;
    [SerializeField] private TMP_FontAsset defaultFont;

    protected override void Awake()
    {
        base.Awake();
        if (defaultFont == null)
        {
            defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
    }

    private GameObject GetOrCreateContainer()
    {
        if (container == null)
        {
            container = new GameObject("3D Text Container");
        }
        return container;
    }
    private Canvas GetOrCreateCanvas()
    {
        if (canvas != null) return canvas;

        GameObject canvasGO = new GameObject("TextDisplay Canvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        return canvas;
    }

    private void SetupUpdater(GameObject textObject, TextDisplay td, Func<string> trackedProvider, Action onClick, bool draggable, bool isUI)
    {
        if (trackedProvider == null && onClick == null && !draggable) return;

        if (isUI)
        {
            var updater = textObject.AddComponent<TextDisplayUIUpdater>();
            updater.Init(td, trackedProvider);
            updater.SetDraggable(draggable);
            updater.SetOnClick(onClick);
        }
        else
        {
            var updater = textObject.AddComponent<TextDisplayUpdater>();
            updater.Init(td, trackedProvider);
            updater.SetDraggable(draggable);
            updater.SetOnClick(onClick);
        }
    }

    private void SetupAutoDestroy(GameObject textObject, float autoDestroySeconds)
    {
        if (autoDestroySeconds <= 0f) return;
        var ad = textObject.AddComponent<AutoDestroy>();
        ad.LifeSeconds = autoDestroySeconds;
    }

    ///<summary>Call TextDisplayParent.New() ... .Build() instead</summary>
    public TextDisplay Create3D(Vector3 position, float size, string initialText = null, Func<string> trackedProvider = null, Transform parent = null, System.Action onClick = null, bool draggable = false, float autoDestroySeconds = 0f)
    {
        GameObject containerObj = parent != null ? parent.gameObject : GetOrCreateContainer();
        GameObject textObject = new GameObject("Text Display");

        if (parent != null)
        {
            textObject.transform.localPosition = position;
            textObject.transform.SetParent(parent, false);
        }
        else
        {
            textObject.transform.SetParent(containerObj.transform, true);
            textObject.transform.position = position;
        }

        TextMeshPro tmp = textObject.AddComponent<TextMeshPro>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = Mathf.Max(1, Mathf.RoundToInt(8 * Mathf.Max(0.1f, size)));
        tmp.color = Color.white;
        if (!string.IsNullOrEmpty(initialText)) tmp.text = initialText;

        var mr = textObject.GetComponent<MeshRenderer>();
        textObject.layer = 5;
        if (mr != null && defaultFont != null && defaultFont.material != null)
            mr.sharedMaterial = defaultFont.material;

        TextDisplay td = new TextDisplay(textObject, tmp, trackedProvider);
        SetupUpdater(textObject, td, trackedProvider, onClick, draggable, false);
        SetupAutoDestroy(textObject, autoDestroySeconds);

        return td;
    }

    ///<summary>Create 2D world text (TextMeshPro in world space). Position is in world coordinates with sorting order for depth.</summary>
    public TextDisplay Create2DWorld(Vector3 position, float size, string initialText = null, Func<string> trackedProvider = null, Transform parent = null, System.Action onClick = null, bool draggable = false, float autoDestroySeconds = 0f, int sortingOrder = 0)
    {
        GameObject containerObj = parent != null ? parent.gameObject : GetOrCreateContainer();
        GameObject textObject = new GameObject("2D Text");

        if (parent != null)
        {
            textObject.transform.localPosition = position;
            textObject.transform.SetParent(parent, false);
        }
        else
        {
            textObject.transform.SetParent(containerObj.transform, true);
            textObject.transform.position = position;
        }

        TextMeshPro tmp = textObject.AddComponent<TextMeshPro>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = Mathf.Max(1, Mathf.RoundToInt(8 * Mathf.Max(0.1f, size)));
        tmp.color = Color.white;
        if (!string.IsNullOrEmpty(initialText)) tmp.text = initialText;

        var mr = textObject.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingOrder = sortingOrder;
            if (defaultFont != null && defaultFont.material != null)
                mr.sharedMaterial = defaultFont.material;
        }

        TextDisplay td = new TextDisplay(textObject, tmp, trackedProvider);
        SetupUpdater(textObject, td, trackedProvider, onClick, draggable, false);
        SetupAutoDestroy(textObject, autoDestroySeconds);

        return td;
    }

    ///<summary>Create a UI Text (TextMeshProUGUI) under a Canvas. anchoredPosition is in canvas local coordinates (RectTransform.anchoredPosition).</summary>
    public TextDisplay CreateUI(Vector2 anchoredPosition, float size, string initialText = null, Func<string> trackedProvider = null, Canvas parentCanvas = null, Action onClick = null, bool draggable = false, float autoDestroySeconds = 0f)
    {
        Canvas targetCanvas = parentCanvas ?? GetOrCreateCanvas();

        GameObject textObject = new GameObject("Text UI");
        textObject.transform.SetParent(targetCanvas.transform, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(200f, 50f);
        rect.localScale = Vector3.one;

        targetCanvas.sortingOrder = 100;

        if (textObject.GetComponent<CanvasRenderer>() == null)
            textObject.AddComponent<CanvasRenderer>();

        TextMeshProUGUI tmpui = textObject.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmpui.font = defaultFont;
        tmpui.alignment = TextAlignmentOptions.Center;
        tmpui.fontSize = Mathf.Max(1, Mathf.RoundToInt(size * 36f));
        tmpui.color = Color.white;
        tmpui.raycastTarget = true;
        if (!string.IsNullOrEmpty(initialText)) tmpui.text = initialText;

        TextDisplay td = new TextDisplay(textObject, tmpui, trackedProvider);
        SetupUpdater(textObject, td, trackedProvider, onClick, draggable, true);
        SetupAutoDestroy(textObject, autoDestroySeconds);

        return td;
    }

    public class Builder
    {
        private Vector3 position;
        private float size;
        private string initialText;
        private Func<string> trackedProvider;
        private Transform parent;
        private Action onClick;
        private bool draggable = false;
        private float autoDestroySeconds = 0f;
        private int sortingOrder = 0;

        private int buildType = 0; // 0 = 3D, 1 = 2D World, 2 = UI
        private Vector2 uiAnchoredPosition;
        private Canvas uiCanvas = null;

        public Builder(Vector3 position, float size)
        {
            this.position = position;
            this.size = size;
        }

        // 2D World builder ctor
        public Builder(Vector3 position, float size, bool is2DWorld)
        {
            this.position = position;
            this.size = size;
            this.buildType = is2DWorld ? 1 : 0;
        }

        // UI builder ctor
        public Builder(Vector2 anchoredPosition, float size, Canvas parentCanvas = null)
        {
            this.buildType = 2;
            this.uiAnchoredPosition = anchoredPosition;
            this.size = size;
            this.uiCanvas = parentCanvas;
        }

        public Builder WithInitialText(string text) { initialText = text; return this; }
        public Builder WithTrackedProvider(Func<string> provider) { trackedProvider = provider; return this; }
        public Builder WithParent(Transform p) { parent = p; return this; }
        public Builder WithOnClick(Action a) { onClick = a; return this; }
        public Builder WithDraggable(bool d = true) { draggable = d; return this; }
        public Builder WithAutoDestroy(float s) { autoDestroySeconds = s; return this; }
        public Builder WithSortingOrder(int order) { sortingOrder = order; return this; }

        public TextDisplay Build()
        {
            switch (buildType)
            {
                case 1: // 2D World
                    return TextDisplayManager.Instance.Create2DWorld(position, size, initialText, trackedProvider, parent, onClick, draggable, autoDestroySeconds, sortingOrder);
                case 2: // UI
                    return TextDisplayManager.Instance.CreateUI(uiAnchoredPosition, size, initialText, trackedProvider, uiCanvas, onClick, draggable, autoDestroySeconds);
                default: // 3D World
                    return TextDisplayManager.Instance.Create3D(position, size, initialText, trackedProvider, parent, onClick, draggable, autoDestroySeconds);
            }
        }
    }
    public static Builder New3D(Vector3 position, float size) => new Builder(position, size);
    public static Builder New2DWorld(Vector3 position, float size) => new Builder(position, size, true);
    public static Builder NewUI(Vector2 anchoredPosition, float size, Canvas parentCanvas = null) => new Builder(anchoredPosition, size, parentCanvas);
}