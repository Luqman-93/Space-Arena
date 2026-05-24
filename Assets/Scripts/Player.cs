using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    //[SerializeField] GameObject _liser;
    int _lives = 3;
    int _score = 0;
    SpawnManager _spawnManager;
    UiManager _uiManager;
    GameManager _gameManager;
    LevelManager _levelManager;
    MCQManager _mcqManager;
    [SerializeField] GameObject _laser;
    [SerializeField] AudioClip _laserClip;
    AudioSource _audioSource;
    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _laserClip;     
        _spawnManager = GameObject.Find("SpawnEnemies").GetComponent<SpawnManager>();
        _uiManager = GameObject.Find("Canvas").GetComponent<UiManager>();
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        _levelManager = GameObject.Find("Game_Manager").GetComponent<LevelManager>();
        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            _mcqManager = canvas.GetComponent<MCQManager>();
            if (_mcqManager == null)
            {
                Debug.LogError("MCQManager NOT found on Canvas!");
            }
            else
            {
                Debug.Log("MCQManager found and linked to Player");
            }
        }
        else
        {
            Debug.LogError("Canvas GameObject NOT found!");
        }
    }

    void Update()
    {
        if (_gameManager.IsPaused || _gameManager.IsGameOver)
        {
            return;
        }

        movement();
        fire();
    }

    void movement()
    {
        float xAxis = Input.GetAxisRaw("H");
        float yAxis = Input.GetAxisRaw("V");
        float speed = 6f;
        Vector3 move = new Vector3(xAxis, yAxis, 0f) * speed * Time.deltaTime;
        
        transform.Translate(move);

        if (transform.position.x >= 8)
        {
            transform.position = new Vector3(8f,transform.position.y,0f);
        }
        else if (transform.position.x <= -8)
        {
            transform.position = new Vector3(-8f, transform.position.y, 0f);
        }
        if (transform.position.y >= 4)
        {
            transform.position = new Vector3(transform.position.x, 4f, 0f);
        }
        else if (transform.position.y <= -4)
        {
            transform.position = new Vector3(transform.position.x, -4f, 0f);
        }
    }
    void fire()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(_laser, transform.position, Quaternion.identity);
            _audioSource.Play();
        }

    }
    public void damage()
    {
        _lives--;
        if (_lives > 0)
        {
            _uiManager.updateLives(_lives);
        }
        else
        {
            if (_mcqManager != null)
            {
                Debug.Log("Lives ended, triggering MCQManager.TriggerMCQ()");
                _mcqManager.TriggerMCQ();
            }
            else
            {
                Debug.LogError("MCQManager is NULL! Using fallback death.");
                // Fallback: if MCQManager not found, perform normal death
                Destroy(this.gameObject);
                _spawnManager.onPlayerDeath();
            }
        }
    }

    public void RestoreLives(int lives)
    {
        Debug.Log($"[Player] RestoreLives called: {lives}");
        // Add restored lives to current lives (so repeated MCQ rewards accumulate)
        _lives += lives;
        // Clamp to reasonable bounds (0..3) to match UI sprite indices
        _lives = Mathf.Clamp(_lives, 0, 3);
        _uiManager.updateLives(_lives);
        if (lives > 0)
        {
            transform.position = Vector3.zero;
            gameObject.SetActive(true);
        }
    }
    public int Lives
    {
        get { return _lives; }
    }
    public void addScore(int points)
    {
        _score += points;
        _uiManager.updateScore(_score);
        _levelManager.OnScoreChanged(_score);
    }
}
