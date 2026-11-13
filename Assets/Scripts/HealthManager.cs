using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    private static HealthManager _instance;
    public static HealthManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<HealthManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("HealthManager");
                    _instance = go.AddComponent<HealthManager>();
                }
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }

    public const int MaxLives = 3;
    public int currentLives { get; private set; } = 3;

    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite FullHeartSprite;
    [SerializeField] private Sprite EmptyHeartSprite;

    private Coroutine resetCoroutine;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        currentLives = MaxLives;
    }

    private void Start()
    {
        InitializeSprites();
        FindAndSetupHearts();
        currentLives = MaxLives;
        UpdateHeartsDisplay(false);
    }

    public void InitializeSprites()
    {
        if (FullHeartSprite == null)
        {
            FullHeartSprite = Resources.Load<Sprite>("heart_full");
        }
        if (FullHeartSprite == null)
        {
            FullHeartSprite = LoadSpriteFromFile("heart");
        }

        if (EmptyHeartSprite == null)
        {
            EmptyHeartSprite = Resources.Load<Sprite>("heart_empty");
        }
        if (EmptyHeartSprite == null)
        {
            EmptyHeartSprite = LoadSpriteFromFile("heart (1)");
        }
    }

    private Sprite LoadSpriteFromFile(string fileName)
    {
        string path = System.IO.Path.Combine(Application.dataPath, "png", "Object", fileName + ".png");
        if (System.IO.File.Exists(path))
        {
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(bytes))
            {
                tex.filterMode = FilterMode.Bilinear;
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
        return null;
    }

    public void FindAndSetupHearts()
    {
        var heartList = new List<Image>();
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            Image[] allImages = canvas.GetComponentsInChildren<Image>(true);
            foreach (Image img in allImages)
            {
                if (img.gameObject.name.ToLower().Contains("heart"))
                {
                    heartList.Add(img);
                }
            }
        }

        // Sort by X position (left to right): heart 0 is left, heart 1 is center, heart 2 is right
        heartList.Sort((a, b) => a.rectTransform.anchoredPosition.x.CompareTo(b.rectTransform.anchoredPosition.x));
        hearts = heartList.ToArray();

        // If the scene images already have sprites assigned, capture them
        if (hearts != null && hearts.Length > 0)
        {
            if (FullHeartSprite == null && hearts[0] != null && hearts[0].sprite != null)
            {
                FullHeartSprite = hearts[0].sprite;
            }
            if (EmptyHeartSprite == null && hearts.Length > 2 && hearts[2] != null && hearts[2].sprite != null)
            {
                EmptyHeartSprite = hearts[2].sprite;
            }
        }
    }

    public void UpdateHeartsDisplay(bool animate = false)
    {
        if (hearts == null || hearts.Length == 0)
        {
            FindAndSetupHearts();
        }

        if (hearts == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            if (i < currentLives)
            {
                if (FullHeartSprite != null) hearts[i].sprite = FullHeartSprite;
                hearts[i].color = Color.white;
            }
            else
            {
                if (EmptyHeartSprite != null)
                {
                    hearts[i].sprite = EmptyHeartSprite;
                    hearts[i].color = Color.white;
                }
                else
                {
                    if (FullHeartSprite != null) hearts[i].sprite = FullHeartSprite;
                    hearts[i].color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                }
            }
        }
    }

    // Called on every death event
    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
        }

        UpdateHeartsDisplay(false);

        // When all 3 lives are consumed (0 lives remaining)
        if (currentLives <= 0)
        {
            // Reset to 3 lives and trigger reset animation
            currentLives = MaxLives;
            if (resetCoroutine != null) StopCoroutine(resetCoroutine);
            resetCoroutine = StartCoroutine(AnimateHeartsReset());
        }
    }

    private IEnumerator AnimateHeartsReset()
    {
        // Brief pause showing all hearts empty
        yield return new WaitForSeconds(0.4f);

        // Flash and restore each heart from left to right with a smooth pulse
        if (hearts != null)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] != null)
                {
                    if (FullHeartSprite != null) hearts[i].sprite = FullHeartSprite;
                    hearts[i].color = Color.white;

                    // Scale pulse effect
                    Vector3 origScale = hearts[i].transform.localScale;
                    hearts[i].transform.localScale = origScale * 1.35f;

                    float elapsed = 0f;
                    while (elapsed < 0.15f)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        hearts[i].transform.localScale = Vector3.Lerp(origScale * 1.35f, origScale, elapsed / 0.15f);
                        yield return null;
                    }
                    hearts[i].transform.localScale = origScale;
                }
            }
        }

        UpdateHeartsDisplay(false);
    }

    public void ResetLives()
    {
        currentLives = MaxLives;
        UpdateHeartsDisplay(false);
    }
}
