using UnityEngine;
using System;
using TMPro;

public class GridDebugDrawer : MonoBehaviour
{
    private IGridDebug grid;
    private int width, height;

    // text
    private TextMeshPro[,] textArray;
    private string[,] textCache;
    private GameObject textRoot;
    private bool textEnabled;

    //sprite
    private GameObject spriteRoot;
    private SpriteRenderer[,] spriteArray;
    private Func<int, int, Sprite> spriteFunc;
    private Func<int, int, float> rotationFunc;
    private bool spriteEnabled;

    //color
    private GameObject colorGrid;
    private Texture2D texture;
    private SpriteRenderer colorRenderer;
    private Color[,] cachedColors;
    private Func<int, int, Color> colorFunc;
    private bool colorEnabled;
    private bool textureDirty;

    public void Initialize(IGridDebug grid)
    {
        this.grid = grid;

        width = grid.GetWidth();
        height = grid.GetHeight();

        SetupColorTexture();
        OnGridRefresh();
    }

    public void HideAll()
    {
        SetText(false);
        SetSprites(false);
        SetColor(false);
    }

    
    #region color
    private void SetupColorTexture()
    {
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        cachedColors = new Color[width, height];

        colorGrid = new GameObject("ColorGrid");
        colorGrid.transform.SetParent(transform);
        colorRenderer = colorGrid.AddComponent<SpriteRenderer>();

        colorRenderer.material = new Material(Shader.Find("Sprites/Default"));
        colorRenderer.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            Vector2.one * 0.5f,
            1f
        );

        colorRenderer.transform.position =
            grid.GetWorldPositionCorner(0, 0) +
            new Vector3(width, height, 0) * grid.GetCellSize() * 0.5f;

        colorRenderer.sortingOrder = -1;
    }

    public void SetColor(bool enabled, Func<int, int, Color> func = null)
    {
        colorEnabled = enabled;

        if (!enabled)
        {
            if (colorRenderer != null)
                colorRenderer.gameObject.SetActive(false);

            grid.OnGridObjectChanged -= OnGridChanged;
            grid.OnGridFullRefresh -= OnGridRefresh;
            return;
        }

        colorFunc = func ?? colorFunc;

        if (texture == null)
            SetupColorTexture();

        RebuildColor();

        colorRenderer.gameObject.SetActive(true);

        grid.OnGridObjectChanged -= OnGridChanged;
        grid.OnGridFullRefresh -= OnGridRefresh;
        grid.OnGridObjectChanged += OnGridChanged;
        grid.OnGridFullRefresh += OnGridRefresh;
    }

    private void RebuildColor()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            cachedColors[x, y] = colorFunc(x, y);

        ApplyTexture();
    }

    private void ApplyTexture()
    {
        if (texture == null) return;

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            texture.SetPixel(x, y, cachedColors[x, y]);

        textureDirty = true;
    }
    #endregion

    #region text

    private void CreateText()
    {
        textArray = new TextMeshPro[width, height];
        textCache = new string[width, height];
        textRoot = new GameObject("TextGrid");
        textRoot.transform.SetParent(transform);

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            var td = TextDisplayManager
                .New2DWorld(
                    grid.GetWorldPositionCorner(x, y) + Vector3.one * 0.5f,
                    0.5f * grid.GetCellSize())
                .WithInitialText(grid.GetDebugValue(x, y))
                .Build();

            var tmp = (TextMeshPro)td.textComponent;
            tmp.enableAutoSizing = false;
            tmp.autoSizeTextContainer = false;
            tmp.isTextObjectScaleStatic = true;
            tmp.richText = false;
            tmp.SetText(textCache[x, y] = grid.GetDebugValue(x, y));

            var go = tmp.gameObject;
            go.transform.SetParent(textRoot.transform, false);
            textArray[x, y] = tmp;
        }
    }

    public void SetText(bool enabled)
    {
        textEnabled = enabled;

        if (enabled)
        {
            if (textArray == null)
            {
                CreateText();
            }
            else if (textRoot != null)
            {
                textRoot.SetActive(true);
            }

            grid.OnGridObjectChanged -= OnGridChanged;
            grid.OnGridFullRefresh -= OnGridRefresh;
            grid.OnGridObjectChanged += OnGridChanged;
            grid.OnGridFullRefresh += OnGridRefresh;

            if (textArray != null)
                RefreshAllText();
        }
        else
        {
            ClearText();

            grid.OnGridObjectChanged -= OnGridChanged;
            grid.OnGridFullRefresh -= OnGridRefresh;
        }
    }

    private void UpdateText(int x, int y)
    {
        if (!textEnabled || textArray == null) return;

        string newText = grid.GetDebugValue(x, y);
        if (textCache[x, y] == newText) return;

        textCache[x, y] = newText;
        textArray[x, y].SetText(newText);
    }

    private void RefreshAllText()
    {
        if (textArray == null) return;

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            UpdateText(x, y);
    }

    private void ClearText()
    {
        if (textRoot != null)
            textRoot.SetActive(false);
    }

    #endregion

    #region sprite
    private void CreateSprites()
    {
        spriteArray = new SpriteRenderer[width, height];
        spriteRoot = new GameObject("SpriteGrid");
        spriteRoot.transform.SetParent(transform);

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            GameObject go = new GameObject($"S_{x}_{y}");
            go.transform.parent = spriteRoot.transform;

            go.transform.position =
                grid.GetWorldPositionCorner(x, y) +
                Vector3.one * 0.5f * grid.GetCellSize();

            go.transform.localScale = Vector3.one * grid.GetCellSize();

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spriteFunc(x, y);

            spriteArray[x, y] = sr;
        }
    }

    public void SetSprites(bool enabled,
        Func<int, int, Sprite> func = null,
        Func<int, int, float> rot = null)
    {
        spriteEnabled = enabled;

        if (enabled)
        {
            spriteFunc = func ?? spriteFunc;
            rotationFunc = rot ?? rotationFunc;

            if (spriteArray == null)
            {
                CreateSprites();
            }
            else if (spriteArray != null)
            {
                spriteRoot.SetActive(true);
            }

            grid.OnGridObjectChanged -= OnGridChanged;
            grid.OnGridFullRefresh -= OnGridRefresh;
            grid.OnGridObjectChanged += OnGridChanged;
            grid.OnGridFullRefresh += OnGridRefresh;
        }
        else
        {
            ClearSprites();

            grid.OnGridObjectChanged -= OnGridChanged;
            grid.OnGridFullRefresh -= OnGridRefresh;
        }
    }

    private void UpdateSprite(int x, int y)
    {
        if (!spriteEnabled || spriteArray == null) return;

        spriteArray[x, y].sprite = spriteFunc(x, y);

        if (rotationFunc != null)
            spriteArray[x, y].transform.rotation =
                Quaternion.Euler(0, 0, rotationFunc(x, y));
    }

    private void ClearSprites()
    {
        if (spriteRoot != null)
            spriteRoot.SetActive(false);
    }
    #endregion

    #region events
    private void OnGridChanged(int x, int y)
    {
        if (colorEnabled && colorFunc != null)
        {
            cachedColors[x, y] = colorFunc(x, y);
            texture.SetPixel(x, y, cachedColors[x, y]);
            textureDirty = true;
        }

        if (textEnabled) UpdateText(x, y);
        if (spriteEnabled) UpdateSprite(x, y);
    }

    private void OnGridRefresh()
    {
        if (colorEnabled && colorFunc != null)
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                {
                    cachedColors[x, y] = colorFunc(x, y);
                    texture.SetPixel(x, y, cachedColors[x, y]);
                }
            textureDirty = true;
        }

        if (textEnabled)
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                UpdateText(x, y);
        } 
        if (spriteEnabled)
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                UpdateSprite(x, y);
        } 
    }
    #endregion

    private void LateUpdate()
    {
        if (textureDirty && texture != null)
        {
            texture.Apply(false);
            textureDirty = false;
        }
    }
}