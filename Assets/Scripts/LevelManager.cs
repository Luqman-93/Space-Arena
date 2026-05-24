using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static float EnemyMoveSpeed { get; private set; } = 3f;

    const int Level2Score = 50;
    const int Level3Score = 100;

    int _currentLevel = 1;
    bool _isTransitioning;
    bool _level2Triggered;
    bool _level3Triggered;

    SpawnManager _spawnManager;
    UiManager _uiManager;
    GameManager _gameManager;

    void Start()
    {
        _spawnManager = GameObject.Find("SpawnEnemies").GetComponent<SpawnManager>();
        _uiManager = GameObject.Find("Canvas").GetComponent<UiManager>();
        _gameManager = GetComponent<GameManager>();
        StartCoroutine(StartGameFlow());
    }

    IEnumerator StartGameFlow()
    {
        _gameManager.SetPaused(true);
        yield return ShowLevelPopup("Level 1", "Top enemies incoming!\nDestroy them before they reach you.");
        _gameManager.SetPaused(false);
        _spawnManager.BeginLevel(1);
    }

    public void OnScoreChanged(int score)
    {
        if (_isTransitioning || _gameManager.IsGameOver)
        {
            return;
        }

        if (!_level2Triggered && _currentLevel == 1 && score >= Level2Score)
        {
            _level2Triggered = true;
            StartCoroutine(TransitionToLevel(2));
        }
        else if (!_level3Triggered && _currentLevel == 2 && score >= Level3Score)
        {
            _level3Triggered = true;
            StartCoroutine(TransitionToLevel(3));
        }
    }

    IEnumerator TransitionToLevel(int level)
    {
        _isTransitioning = true;
        _spawnManager.StopSpawning();
        _spawnManager.ClearAllEnemies();
        _gameManager.SetPaused(true);

        if (level == 2)
        {
            yield return ShowLevelPopup("Level 2", "Enemy spawn speed increased!\nStay sharp.");
        }
        else if (level == 3)
        {
            yield return ShowLevelPopup("Level 3", "Bottom enemies are coming — watch out!");
        }

        _currentLevel = level;
        _gameManager.SetPaused(false);
        _isTransitioning = false;
        _spawnManager.BeginLevel(level);
    }

    IEnumerator ShowLevelPopup(string title, string message)
    {
        _uiManager.ShowLevelPopup(title, message);

        while (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            yield return null;
        }

        _uiManager.HideLevelPopup();
    }

    public void BeginLevelSettings(int level)
    {
        switch (level)
        {
            case 1:
                EnemyMoveSpeed = 3f;
                break;
            case 2:
                EnemyMoveSpeed = 5f;
                break;
            case 3:
                EnemyMoveSpeed = 6f;
                break;
        }
    }
}

