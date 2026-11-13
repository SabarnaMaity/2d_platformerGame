using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<UIManager>(FindObjectsInactive.Include);
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }

    public GameObject mobileControls;

    public bool fadeToBlack, fadeFromBlack;
    public Image blackScreen;
    public float fadeSpeed = 2f;

    // Player reference
    public PlayerController playerController;

    private static Sprite _leftSprite;
    private static Sprite _rightSprite;
    private static Sprite _upSprite;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        if (playerController == null)
        {
            playerController = Object.FindAnyObjectByType<PlayerController>();
        }

        SetupMobileButtons();
        EnableMobileControls();
    }

    public void DisableMobileControls()
    {
        if (mobileControls != null)
        {
            mobileControls.SetActive(false);
        }
    }

    public void EnableMobileControls()
    {
        if (mobileControls != null)
        {
            SetupMobileButtons();
            mobileControls.SetActive(true);
        }
    }

    public void SetupMobileButtons()
    {
        if (mobileControls == null)
        {
            Transform existing = transform.Find("MobileControls");
            if (existing != null)
            {
                mobileControls = existing.gameObject;
            }
            else
            {
                mobileControls = new GameObject("MobileControls", typeof(RectTransform));
                mobileControls.transform.SetParent(transform, false);
            }
        }

        mobileControls.layer = 5; // UI layer

        RectTransform mcRect = mobileControls.GetComponent<RectTransform>();
        if (mcRect != null)
        {
            // Stretch to full Canvas
            mcRect.anchorMin = Vector2.zero;
            mcRect.anchorMax = Vector2.one;
            mcRect.offsetMin = Vector2.zero;
            mcRect.offsetMax = Vector2.zero;
            mcRect.pivot = new Vector2(0.5f, 0.5f);
            mcRect.localScale = Vector3.one;
            mcRect.localPosition = Vector3.zero;
        }

        // Clean up any old buttons if they exist
        Transform oldBtn = mobileControls.transform.Find("Btn_Left");
        if (oldBtn != null)
        {
            Image existingImg = oldBtn.GetComponent<Image>();
            if (existingImg != null && existingImg.sprite != null && existingImg.sprite.name.Contains("leftarrow"))
            {
                // Already setup with leftarrow sprite
                return;
            }

            for (int i = mobileControls.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(mobileControls.transform.GetChild(i).gameObject);
            }
        }

        // Load custom arrow PNG sprites (Assets/png/Object/ or Assets/Resources/)
        Sprite sprLeft = LoadArrowSprite("leftarrow");
        Sprite sprRight = LoadArrowSprite("rightarrow");
        Sprite sprUp = LoadArrowSprite("uparrow");

        // 1. Left Arrow Button (Bottom-Left)
        GameObject btnLeft = CreateArrowButton(mobileControls.transform, "Btn_Left", sprLeft,
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(150f, 150f), new Vector2(160f, 160f));
        AddEventTrigger(btnLeft, EventTriggerType.PointerDown, () => OnLeftButton(true));
        AddEventTrigger(btnLeft, EventTriggerType.PointerUp, () => OnLeftButton(false));
        AddEventTrigger(btnLeft, EventTriggerType.PointerExit, () => OnLeftButton(false));
        AddEventTrigger(btnLeft, EventTriggerType.Cancel, () => OnLeftButton(false));
        AddEventTrigger(btnLeft, EventTriggerType.Deselect, () => OnLeftButton(false));

        // 2. Right Arrow Button (Bottom-Left)
        GameObject btnRight = CreateArrowButton(mobileControls.transform, "Btn_Right", sprRight,
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(350f, 150f), new Vector2(160f, 160f));
        AddEventTrigger(btnRight, EventTriggerType.PointerDown, () => OnRightButton(true));
        AddEventTrigger(btnRight, EventTriggerType.PointerUp, () => OnRightButton(false));
        AddEventTrigger(btnRight, EventTriggerType.PointerExit, () => OnRightButton(false));
        AddEventTrigger(btnRight, EventTriggerType.Cancel, () => OnRightButton(false));
        AddEventTrigger(btnRight, EventTriggerType.Deselect, () => OnRightButton(false));

        // 3. Jump Arrow Button (Bottom-Right, uparrow.png)
        GameObject btnJump = CreateArrowButton(mobileControls.transform, "Btn_Jump", sprUp,
            new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-170f, 160f), new Vector2(190f, 190f));
        AddEventTrigger(btnJump, EventTriggerType.PointerDown, () => OnJumpButton());
    }

    private GameObject CreateArrowButton(Transform parent, string name, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(EventTrigger));
        obj.layer = 5;
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = obj.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;
        img.color = new Color(1f, 1f, 1f, 0.95f);
        img.raycastTarget = true;

        Button btn = obj.GetComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        cb.pressedColor = new Color(0.65f, 0.65f, 0.65f, 1f);
        btn.colors = cb;

        return obj;
    }

    private void AddEventTrigger(GameObject target, EventTriggerType eventType, UnityEngine.Events.UnityAction callback)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener((data) => { callback(); });
        trigger.triggers.Add(entry);
    }

    public void OnLeftButton(bool isDown)
    {
        EnsurePlayerController();
        if (playerController != null)
        {
            playerController.SetLeftHeld(isDown);
        }
    }

    public void OnRightButton(bool isDown)
    {
        EnsurePlayerController();
        if (playerController != null)
        {
            playerController.SetRightHeld(isDown);
        }
    }

    public void OnJumpButton()
    {
        EnsurePlayerController();
        if (playerController != null)
        {
            playerController.MobileJump();
        }
    }

    private void EnsurePlayerController()
    {
        if (playerController == null)
        {
            playerController = Object.FindAnyObjectByType<PlayerController>();
        }
    }

    public static Sprite LoadArrowSprite(string name)
    {
        if (name == "leftarrow" && _leftSprite != null) return _leftSprite;
        if (name == "rightarrow" && _rightSprite != null) return _rightSprite;
        if (name == "uparrow" && _upSprite != null) return _upSprite;

        Sprite sp = null;

        // 1. Try loading from Resources folder (works in APK builds and Editor)
        sp = Resources.Load<Sprite>(name);
        if (sp == null)
        {
            Sprite[] all = Resources.LoadAll<Sprite>(name);
            if (all != null && all.Length > 0 && all[0] != null)
            {
                sp = all[0];
            }
        }

        // 2. Fallback: Load directly from Assets/png/Object/ file
        if (sp == null)
        {
            string path = System.IO.Path.Combine(Application.dataPath, "png", "Object", name + ".png");
            if (System.IO.File.Exists(path))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(bytes))
                {
                    tex.filterMode = FilterMode.Bilinear;
                    sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    sp.name = name;
                }
            }
        }

        if (name == "leftarrow") _leftSprite = sp;
        else if (name == "rightarrow") _rightSprite = sp;
        else if (name == "uparrow") _upSprite = sp;

        return sp;
    }

    private void Update()
    {
        UpdateFade();
    }

    private void UpdateFade()
    {
        if (fadeToBlack)
        {
            FadeToBlack();
        }
        else if (fadeFromBlack)
        {
            FadeFromBlack();
        }
    }

    private void FadeToBlack()
    {
        FadeScreen(1f);

        if (blackScreen != null && blackScreen.color.a >= 1f)
        {
            fadeToBlack = false;
        }
    }

    private void FadeFromBlack()
    {
        FadeScreen(0f);

        if (blackScreen != null && blackScreen.color.a <= 0f)
        {
            EnableMobileControls();
            fadeFromBlack = false;
        }
    }

    private void FadeScreen(float targetAlpha)
    {
        if (blackScreen == null) return;
        Color currentColor = blackScreen.color;
        float newAlpha = Mathf.MoveTowards(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
        blackScreen.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
    }
}
