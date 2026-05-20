using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    bool _isGameOver = false;
    bool _isPaused = false;

    public bool IsGameOver => _isGameOver;
    public bool IsPaused => _isPaused;

    void Update()
    {
        restartGame();
    }

    void restartGame()
    {
        if (Input.GetKeyDown(KeyCode.R) && _isGameOver)
        {
            SceneManager.LoadScene(1);
        }
    }

    public void SetPaused(bool paused)
    {
        _isPaused = paused;
    }

    public void gameOver()
    {
        _isGameOver = true;
        _isPaused = true;
    }
}

