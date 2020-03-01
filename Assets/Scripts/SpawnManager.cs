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

    IEnumerator SpawnPowerupRoutine()
    {
        yield return new WaitForSeconds(3f);

        while (_stopSpawning == false)
        {
            float randomPowerupX = Random.Range(-9f, 9f);
            Vector3 _spawnPowerupPosition = new Vector3(randomPowerupX, 7.5f, 0);

            int randomPowerup = Random.Range(0, _powerupPrefabs.Length);

            Instantiate(_powerupPrefabs[randomPowerup], _spawnPowerupPosition, Quaternion.identity);
            
            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }
}
