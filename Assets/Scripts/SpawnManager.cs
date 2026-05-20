using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] GameObject _topEnemy;
    [SerializeField] GameObject _topEnemyContainer;
    [SerializeField] GameObject _bottomEnemy;
    [SerializeField] GameObject _bottomEnemyContainer;

    bool _isSpawning;
    int _currentLevel = 1;

    float _topSpawnTime = 2.5f;
    float _bottomSpawnTime = 2f;

    LevelManager _levelManager;

    void Awake()
    {
        _levelManager = GameObject.Find("Game_Manager").GetComponent<LevelManager>();
        ClearAllEnemies();
    }

    public void BeginLevel(int level)
    {
        _currentLevel = level;
        _isSpawning = true;
        ApplyLevelSettings(level);
        _levelManager.BeginLevelSettings(level);
        StopAllCoroutines();
        StartCoroutine(spawningFromTheTop());

        if (level >= 3)
        {
            StartCoroutine(spawningFromTheBottom());
        }
    }

    public void StopSpawning()
    {
        _isSpawning = false;
        StopAllCoroutines();
    }

    public void ClearAllEnemies()
    {
        ClearContainer(_topEnemyContainer);
        ClearContainer(_bottomEnemyContainer);
    }

    void ClearContainer(GameObject container)
    {
        if (container == null)
        {
            return;
        }

        Transform parent = container.transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    void ApplyLevelSettings(int level)
    {
        switch (level)
        {
            case 1:
                _topSpawnTime = 2.5f;
                break;
            case 2:
                _topSpawnTime = 1.2f;
                break;
            case 3:
                _topSpawnTime = 0.9f;
                _bottomSpawnTime = 1.5f;
                break;
        }
    }

    IEnumerator spawningFromTheTop()
    {
        while (_isSpawning)
        {
            if (_topEnemy != null && _topEnemyContainer != null)
            {
                Vector3 spawnPos = new Vector3(Random.Range(-7f, 7f), 7f, 0f);
                Instantiate(_topEnemy, spawnPos, Quaternion.identity, _topEnemyContainer.transform);
            }
            yield return new WaitForSeconds(_topSpawnTime);
        }
    }

    IEnumerator spawningFromTheBottom()
    {
        while (_isSpawning && _currentLevel >= 3)
        {
            if (_bottomEnemy != null && _bottomEnemyContainer != null)
            {
                Vector3 spawnPos = new Vector3(Random.Range(-7f, 7f), -7f, 0f);
                Instantiate(_bottomEnemy, spawnPos, Quaternion.identity, _bottomEnemyContainer.transform);
            }
            yield return new WaitForSeconds(_bottomSpawnTime);
        }
    }

    public void onPlayerDeath()
    {
        StopSpawning();
    }
}
