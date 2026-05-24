using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] Text scoreText;
    [SerializeField] Text gameOverText;
    [SerializeField] Text restartText;
    [SerializeField] Sprite[] _spriteImg;
    [SerializeField] Image _livesImg;

    GameObject _levelPopupPanel;
    Text _levelTitleText;
    Text _levelMessageText;
    Text _levelContinueText;

    GameManager _gameManager;
    bool _isGameOverShown;

    void Awake()
    {
        // Auto-attach MCQManager if not already on Canvas
        if (GetComponent<MCQManager>() == null)
        {
            gameObject.AddComponent<MCQManager>();
            Debug.Log("[UiManager] MCQManager auto-attached to Canvas");
        }

        BuildLevelPopupUI();
    }

    void Start()
    {
        scoreText.text = "Score : " + 0;
        _livesImg.sprite = _spriteImg[3];
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        HideLevelPopup();
    }

    void BuildLevelPopupUI()
    {
        Transform existing = transform.Find("LevelPopupPanel");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        _levelPopupPanel = new GameObject("LevelPopupPanel");
        _levelPopupPanel.transform.SetParent(transform, false);

        RectTransform panelRect = _levelPopupPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelBg = _levelPopupPanel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.82f);
        panelBg.raycastTarget = false;

        _levelTitleText = CreatePopupText("LevelTitleText", new Vector2(0f, 100f), new Vector2(750f, 90f), 52);
        _levelMessageText = CreatePopupText("LevelMessageText", new Vector2(0f, 10f), new Vector2(750f, 160f), 30);
        _levelContinueText = CreatePopupText("LevelContinueText", new Vector2(0f, -130f), new Vector2(750f, 60f), 26);

        _levelPopupPanel.SetActive(false);
    }

    Text CreatePopupText(string objectName, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(_levelPopupPanel.transform, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.AddComponent<Text>();
        text.font = GetDefaultFont();
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = true;
        text.raycastTarget = false;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return text;
    }

    Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        return font;
    }

    public void ShowLevelPopup(string title, string message)
    {
        if (_levelPopupPanel == null)
        {
            BuildLevelPopupUI();
        }

        _levelPopupPanel.transform.SetAsLastSibling();
        _levelPopupPanel.SetActive(true);

        _levelTitleText.gameObject.SetActive(true);
        _levelMessageText.gameObject.SetActive(true);
        _levelContinueText.gameObject.SetActive(true);

        _levelTitleText.text = title;
        _levelMessageText.text = message;
        _levelContinueText.text = "Press ENTER to continue";

        _levelTitleText.color = Color.white;
        _levelMessageText.color = Color.white;
        _levelContinueText.color = new Color(1f, 0.92f, 0.4f);
    }

    public void HideLevelPopup()
    {
        if (_levelPopupPanel != null)
        {
            _levelPopupPanel.SetActive(false);
        }
    }

    public void updateScore(int score)
    {
        scoreText.text = "Score : " + score;
    }

    public void updateLives(int live)
    {
        _livesImg.sprite = _spriteImg[live];
        if (live == 0)
        {
            gameOver();
        }
    }

    void gameOver()
    {
        HideLevelPopup();
        gameOverText.gameObject.SetActive(true);
        restartText.gameObject.SetActive(true);
        restartText.text = "Press R to Restart";
        _gameManager.gameOver();
        StartCoroutine(gameOverFlickerRouten());
    }

    IEnumerator gameOverFlickerRouten()
    {
        while (true)
        {
            gameOverText.text = "";
            yield return new WaitForSeconds(0.5f);
            gameOverText.text = "Game Over";
            yield return new WaitForSeconds(0.5f);
        }
    }
}
