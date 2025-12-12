using System.Collections.Generic;
using Entity.Enemy;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private List<EnemyController> _activeEnemies;

    void Awake()
    {
        _activeEnemies = new List<EnemyController>();
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterEnemy(EnemyController enemy)
    {
        if (!_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(EnemyController enemy)
    {
        _activeEnemies.Remove(enemy);
    }

    public List<EnemyController> GetActiveEnemies()
    {
        return  _activeEnemies;
    }

    public int GetEnemyCount()
    {
        return _activeEnemies.Count;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
