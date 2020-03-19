using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _enemyPrefab;

    [SerializeField]
    private GameObject _enemyContainer;

    [SerializeField]
    private GameObject[] _powerupPrefabs;

    [SerializeField]
    private int[] _powerupRateTable =
    {
        18, // trippleShot
        18, // health
        18, // speedBoost
        18, // shield
        18, // ammo
        10 // bomb
    };

    private int _powerupTableTotal;

    private bool _stopSpawning = false;

    private bool _isSpawning = false;
    
    private Background _background;

    private void Start()
    {
        _background = GameObject.Find("Background").GetComponent<Background>();
        if (_background == null)
        {
            Debug.Log("Background is NULL.");
        }

        CalculatePowerupRateTotal();
    }

    private void Update()
    {
        if (_isSpawning == true)
        {
            _background.BackgroundScroll();
        }
    }

    public void StartSpawning()
    {
        _isSpawning = true;

        StartCoroutine(SpawnEnemyRoutine());

        StartCoroutine(SpawnPowerupRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        yield return new WaitForSeconds(3f);

        while(_stopSpawning == false)
        {
            float randomEnemyX = Random.Range(-9f, 9f);
            Vector3 _spawnPosition = new Vector3(randomEnemyX, 7.5f, 0);

            int randomEnemy = Random.Range(0, _enemyPrefab.Length);

            GameObject newEnemy = Instantiate(_enemyPrefab[randomEnemy], _spawnPosition, Quaternion.identity);

            newEnemy.transform.parent = _enemyContainer.transform;

            yield return new WaitForSeconds(Random.Range(3f, 6f));
        }
    }

    private void CalculatePowerupRateTotal()
    {
        foreach (int rate in _powerupRateTable)
        {
            _powerupTableTotal += rate;
        }
    }

    IEnumerator SpawnPowerupRoutine()
    {
        yield return new WaitForSeconds(3f);

        while (_stopSpawning == false)
        {
            float randomPowerupX = Random.Range(-9f, 9f);
            Vector3 _spawnPowerupPosition = new Vector3(randomPowerupX, 7.5f, 0);

            int randomPowerup = Random.Range(0, _powerupTableTotal);

            for (int i = 0; i < _powerupRateTable.Length; i++)
            {
                if (randomPowerup <= _powerupRateTable[i])
                {
                    Instantiate(_powerupPrefabs[i], _spawnPowerupPosition, Quaternion.identity);

                    yield return new WaitForSeconds(Random.Range(5f, 10f));
                }
                else
                {
                    randomPowerup -= _powerupRateTable[i];
                }
            }
        }
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }
}
