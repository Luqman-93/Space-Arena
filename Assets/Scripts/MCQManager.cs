using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MCQManager : MonoBehaviour
{
    [Serializable]
    public struct QuestionData
    {
        public string question;
        public string[] options;
        public int correctIndex;

        public QuestionData(string q, string[] opts, int correct)
        {
            question = q;
            options = opts;
            correctIndex = correct;
        }
    }

    UiManager _uiManager;
    GameManager _gameManager;
    SpawnManager _spawnManager;
    Player _player;

    List<QuestionData> _allQuestions = new List<QuestionData>();
    List<QuestionData> _roundQuestions = new List<QuestionData>();

    GameObject _mcqPanel;
    GameObject _choicePanel;
    GameObject _questionPanel;
    GameObject _resultPanel;

    Text _titleText;
    Text _messageText;

    Text _counterText;
    Text _questionText;
    Text[] _optionTexts = new Text[4];
    Button[] _optionButtons = new Button[4];
    Text _answerResultText;
    Text _resultSummaryText;

    int _currentQuestionIndex;
    int _correctCount;
    bool _isAwaitingAnswer;

    void Awake()
    {
        _uiManager = GetComponent<UiManager>();
        if (_uiManager == null) Debug.LogError("[MCQManager] UiManager not on same Canvas!");
        
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        if (_gameManager == null) Debug.LogError("[MCQManager] Game_Manager not found!");
        
        _spawnManager = GameObject.Find("SpawnEnemies").GetComponent<SpawnManager>();
        if (_spawnManager == null) Debug.LogError("[MCQManager] SpawnEnemies not found!");
        
        _player = GameObject.FindObjectOfType<Player>();
        if (_player == null) Debug.LogError("[MCQManager] Player not found!");

        BuildAllQuestions();
        Debug.Log("[MCQManager] Initialized with " + _allQuestions.Count + " questions");
        BuildUI();
        Debug.Log("[MCQManager] UI built");
    }

    void BuildAllQuestions()
    {
        _allQuestions = new List<QuestionData>
        {
            new QuestionData("What is the closest planet to the Sun?", new[] {"Venus","Mercury","Earth","Mars"}, 1),
            new QuestionData("How many planets are in our Solar System?", new[] {"7","9","8","10"}, 2),
            new QuestionData("What is the largest planet in our Solar System?", new[] {"Saturn","Neptune","Uranus","Jupiter"}, 3),
            new QuestionData("What is the name of Earth's natural satellite?", new[] {"Titan","Europa","The Moon","Phobos"}, 2),
            new QuestionData("Which planet is known as the Red Planet?", new[] {"Venus","Jupiter","Mars","Mercury"}, 2),
            new QuestionData("What is the name of the galaxy that contains our Solar System?", new[] {"Andromeda","Triangulum","Whirlpool","Milky Way"}, 3),
            new QuestionData("Approximately how long does light from the Sun take to reach Earth?", new[] {"1 minute","8 minutes","1 hour","24 minutes"}, 1),
            new QuestionData("What is the largest moon of Saturn?", new[] {"Titan","Ganymede","Europa","Callisto"}, 0),
            new QuestionData("Which planet has the most rings?", new[] {"Jupiter","Uranus","Neptune","Saturn"}, 3),
            new QuestionData("What force keeps planets in orbit around the Sun?", new[] {"Magnetism","Friction","Gravity","Electricity"}, 2),
            new QuestionData("What is a light year a measurement of?", new[] {"Time","Speed","Distance","Mass"}, 2),
            new QuestionData("What is the hottest planet in our Solar System?", new[] {"Mercury","Mars","Jupiter","Venus"}, 3),
            new QuestionData("Who was the first human to walk on the Moon?", new[] {"Buzz Aldrin","Yuri Gagarin","Neil Armstrong","John Glenn"}, 2),
            new QuestionData("What is the name of NASA's most famous space telescope?", new[] {"Spitzer","Hubble","Chandra","Kepler"}, 1),
            new QuestionData("What type of star is our Sun?", new[] {"Red Giant","White Dwarf","Neutron Star","Yellow Dwarf"}, 3),
        };
    }

    void BuildUI()
    {
        Transform parent = transform;

        // Main panel
        _mcqPanel = new GameObject("MCQPanel");
        _mcqPanel.transform.SetParent(parent, false);
        RectTransform mcqRect = _mcqPanel.AddComponent<RectTransform>();
        mcqRect.anchorMin = Vector2.zero;
        mcqRect.anchorMax = Vector2.one;
        mcqRect.offsetMin = Vector2.zero;
        mcqRect.offsetMax = Vector2.zero;

        Image bg = _mcqPanel.AddComponent<Image>();
        // try to reuse a scene background sprite if available
        Sprite bgSprite = null;
        GameObject bgObj = GameObject.Find("Background");
        if (bgObj != null)
        {
            var sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr != null) bgSprite = sr.sprite;
            var img = bgObj.GetComponent<Image>();
            if (img != null) bgSprite = img.sprite;
        }
        if (bgSprite != null)
        {
            bg.sprite = bgSprite;
            bg.type = Image.Type.Simple;
            bg.preserveAspect = true;
        }
        else
        {
            bg.color = new Color(0f,0f,0f,0.7f);
        }

        // overlay for readability
        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(_mcqPanel.transform, false);
        RectTransform or = overlay.AddComponent<RectTransform>();
        or.anchorMin = Vector2.zero;
        or.anchorMax = Vector2.one;
        or.offsetMin = Vector2.zero;
        or.offsetMax = Vector2.zero;
        Image oimg = overlay.AddComponent<Image>();
        oimg.color = new Color(0f,0f,0f,0.7f);

        // Choice Panel
        _choicePanel = new GameObject("ChoicePanel");
        _choicePanel.transform.SetParent(_mcqPanel.transform, false);
        RectTransform cp = _choicePanel.AddComponent<RectTransform>();
        cp.anchorMin = Vector2.zero;
        cp.anchorMax = Vector2.one;
        cp.offsetMin = Vector2.zero;
        cp.offsetMax = Vector2.zero;

        _titleText = CreateText("ChoiceTitle", _choicePanel.transform, new Vector2(0,80), new Vector2(600,40), 26);
        _messageText = CreateText("ChoiceMessage", _choicePanel.transform, new Vector2(0,30), new Vector2(650,40), 16);
        Text choicePrompt = CreateText("ChoicePrompt", _choicePanel.transform, new Vector2(0,-30), new Vector2(650,40), 14);
        _titleText.text = "YOU LOST ALL LIVES!";
        _messageText.text = "Do you want a second chance?";
        choicePrompt.text = "Press P to try MCQ (Second Chance) or Q to Quit";

        // Question Panel
        _questionPanel = new GameObject("QuestionPanel");
        _questionPanel.transform.SetParent(_mcqPanel.transform, false);
        RectTransform qp = _questionPanel.AddComponent<RectTransform>();
        qp.anchorMin = Vector2.zero;
        qp.anchorMax = Vector2.one;
        qp.offsetMin = Vector2.zero;
        qp.offsetMax = Vector2.zero;

        _counterText = CreateText("CounterText", _questionPanel.transform, new Vector2(0,120), new Vector2(500,30), 16);
        _questionText = CreateText("QuestionText", _questionPanel.transform, new Vector2(0,50), new Vector2(800,80), 18);

        // answer buttons 2x2
        float by = -30f;
        for (int i = 0; i < 4; i++)
        {
            GameObject btn = new GameObject("OptionButton" + i);
            btn.transform.SetParent(_questionPanel.transform, false);
            RectTransform br = btn.AddComponent<RectTransform>();
            br.anchorMin = new Vector2(0.5f, 0.5f);
            br.anchorMax = new Vector2(0.5f, 0.5f);
            br.pivot = new Vector2(0.5f, 0.5f);
            br.sizeDelta = new Vector2(380,70);
            int col = i % 2;
            int row = i / 2;
            br.anchoredPosition = new Vector2((col==0? -200f:200f), by - row*90f);
            Button b = btn.AddComponent<Button>();
            Image bi = btn.AddComponent<Image>();
            bi.color = new Color(0.12f,0.12f,0.12f,0.95f);
            int idx = i;
            b.onClick.AddListener(() => { OnAnswerSelected(idx); });

            Text label = CreateText("OptionText"+i, btn.transform, new Vector2(0,0), new Vector2(350,60), 16);
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            _optionTexts[i] = label;
            _optionButtons[i] = b;
        }

        _answerResultText = CreateText("AnswerResult", _questionPanel.transform, new Vector2(0,-220), new Vector2(600,25), 16);

        // Result Panel
        _resultPanel = new GameObject("ResultPanel");
        _resultPanel.transform.SetParent(_mcqPanel.transform, false);
        RectTransform rp = _resultPanel.AddComponent<RectTransform>();
        rp.anchorMin = Vector2.zero;
        rp.anchorMax = Vector2.one;
        rp.offsetMin = Vector2.zero;
        rp.offsetMax = Vector2.zero;

        _resultSummaryText = CreateText("ResultSummary", _resultPanel.transform, new Vector2(0,20), new Vector2(650,70), 18);

        // initially hide
        _mcqPanel.SetActive(false);
        _questionPanel.SetActive(false);
        _resultPanel.SetActive(false);
    }

    Text CreateText(string name, Transform parent, Vector2 pos, Vector2 size, int fontSize)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Text t = go.AddComponent<Text>();
        
        // Load font with fallback
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        t.font = font;
        
        t.fontSize = fontSize;
        t.fontStyle = FontStyle.Bold;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.supportRichText = true;
        t.raycastTarget = false;
        
        Shadow s = go.AddComponent<Shadow>();
        s.effectColor = new Color(0f,0f,0f,0.9f);
        s.effectDistance = new Vector2(2f,-2f);
        Debug.Log("[MCQManager] Created text: " + name);
        return t;
    }

    public void TriggerMCQ()
    {
        Debug.Log("[MCQManager] TriggerMCQ called");
        // Always refresh player reference in case it was recreated
        _player = GameObject.FindObjectOfType<Player>();
        if (_player != null)
        {
            _player.gameObject.SetActive(false);
            Debug.Log("[MCQManager] Player hidden (refreshed)");
        }
        
        // Pause game and stop spawning
        if (_gameManager != null)
        {
            _gameManager.SetPaused(true);
        }
        if (_spawnManager != null)
        {
            _spawnManager.StopSpawning();
        }

        if (_mcqPanel != null)
        {
            _mcqPanel.SetActive(true);
            _mcqPanel.transform.SetAsLastSibling();
            Debug.Log("[MCQManager] Panel activated");
            ShowChoiceScreen();
        }
        else
        {
            Debug.LogError("[MCQManager] _mcqPanel is NULL!");
        }
    }

    void ShowChoiceScreen()
    {
        _choicePanel.SetActive(true);
        _questionPanel.SetActive(false);
        _resultPanel.SetActive(false);
        _isAwaitingAnswer = false;
    }

    void Update()
    {
        if (!_mcqPanel.activeInHierarchy) return;

        if (_choicePanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                // Quit: trigger normal game over
                _mcqPanel.SetActive(false);
                _uiManager.updateLives(0);
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                _choicePanel.SetActive(false);
                StartRound();
            }
        }
        else if (_questionPanel.activeInHierarchy && _isAwaitingAnswer)
        {
            if (Input.GetKeyDown(KeyCode.A)) OnAnswerSelected(0);
            if (Input.GetKeyDown(KeyCode.B)) OnAnswerSelected(1);
            if (Input.GetKeyDown(KeyCode.C)) OnAnswerSelected(2);
            if (Input.GetKeyDown(KeyCode.D)) OnAnswerSelected(3);
        }
    }

    void StartRound()
    {
        // Shuffle and take first 6
        _roundQuestions = _allQuestions.OrderBy(x => Guid.NewGuid()).Take(6).ToList();
        _currentQuestionIndex = 0;
        _correctCount = 0;
        ShowQuestion();
    }

    void ShowQuestion()
    {
        _questionPanel.SetActive(true);
        _resultPanel.SetActive(false);
        _isAwaitingAnswer = true;

        var q = _roundQuestions[_currentQuestionIndex];
        _counterText.text = $"Question {_currentQuestionIndex + 1} of {_roundQuestions.Count}";
        _questionText.text = q.question;
        for (int i = 0; i < 4; i++)
        {
            _optionTexts[i].text = (char)('A' + i) + ": " + q.options[i];
            _optionButtons[i].interactable = true;
            _optionButtons[i].GetComponent<Image>().color = new Color(0.12f,0.12f,0.12f,0.95f);
        }
        _answerResultText.text = "";
    }

    void OnAnswerSelected(int index)
    {
        if (!_isAwaitingAnswer) return;
        _isAwaitingAnswer = false;
        // disable buttons
        for (int i = 0; i < 4; i++) _optionButtons[i].interactable = false;

        var q = _roundQuestions[_currentQuestionIndex];
        bool correct = index == q.correctIndex;
        if (correct)
        {
            _correctCount++;
            _optionButtons[index].GetComponent<Image>().color = Color.green;
            _answerResultText.text = "CORRECT";
        }
        else
        {
            _optionButtons[index].GetComponent<Image>().color = Color.red;
            // highlight correct
            _optionButtons[q.correctIndex].GetComponent<Image>().color = Color.green;
            _answerResultText.text = "WRONG";
        }

        StartCoroutine(AdvanceAfterDelay());
    }

    IEnumerator AdvanceAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        _currentQuestionIndex++;
        if (_currentQuestionIndex < _roundQuestions.Count)
        {
            ShowQuestion();
        }
        else
        {
            ShowResults();
        }
    }

    void ShowResults()
    {
        _questionPanel.SetActive(false);
        _resultPanel.SetActive(true);

        int restored = 0;
        // Use thresholds so near-perfect rounds also reward lives
        if (_correctCount >= 5) restored = 3;
        else if (_correctCount >= 4) restored = 2;
        else if (_correctCount >= 2) restored = 1;
        else restored = 0;

        _resultSummaryText.text = $"{_correctCount} / {_roundQuestions.Count} Correct\nLives restored: {restored}\n" + (restored>0?"Good job! Get back out there!":"Not enough correct. Game Over.");

        StartCoroutine(HandlePostResult(restored));
    }

    IEnumerator HandlePostResult(int restored)
    {
        yield return new WaitForSeconds(2f);

        if (restored > 0)
        {
            Debug.Log($"[MCQManager] Restoring {restored} lives");

            if (_player == null)
            {
                _player = GameObject.FindObjectOfType<Player>();
                Debug.Log(_player == null ? "[MCQManager] Player still null when restoring" : "[MCQManager] Re-found Player for restore");
            }

            if (_player != null)
            {
                // Ensure player active before restoring
                _player.gameObject.SetActive(true);
                _player.RestoreLives(restored);
            }

            // Update UI and resume
            if (_uiManager != null)
            {
                // Use the player's actual lives value (in case logic changes)
                if (_player != null)
                {
                    _uiManager.updateLives(_player.Lives);
                }
                else
                {
                    _uiManager.updateLives(restored);
                }
            }

            if (_gameManager != null)
            {
                _gameManager.SetPaused(false);
            }

            if (_spawnManager != null)
            {
                _spawnManager.BeginLevel(_spawnManager.CurrentLevel);
            }
            // hide mcq UI and resume gameplay
            _mcqPanel.SetActive(false);
        }
        else
        {
            // Do NOT trigger game over automatically. Return to the choice screen
            // so the player can choose to quit (Q) or try MCQ again (P).
            ShowChoiceScreen();
        }
    }
}
