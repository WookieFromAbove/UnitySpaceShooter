using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private WaveClass[] _wave;

    private float _waitToSpawn = 3f;
    private float _waitToSpawnPowerups;

    private int _totalWaves;
    private int _currentWave;

    private int _totalEnemiesInWave;
    private int _remainingEnemiesInWave;
    private int _currentEnemiesSpawned;

    private int _powerupTableTotal;

    private bool _isSpawning = false;
    private bool _powerupsAvailable = false;
    private bool _spawnPowerups = false;

    [SerializeField]
    private GameObject _enemyContainer;

    private Background _background;

    private bool _isBossWave = false;
    [SerializeField]
    private GameObject _megaVacuum;
    [SerializeField]
    private GameObject _megaHead;
    [SerializeField]
    private GameObject _megaArm;
    [SerializeField]
    private GameObject _planetDruidia;

    [SerializeField]
    private PlanetDruidia _planetDruidiaScript;

    private GameManager _gameManager;

    private Player _player;

    [SerializeField]
    public AudioClip _bossIntroClip;

    [SerializeField]
    private AudioClip _bossStartClip;

    private void Start()
    {
        _background = GameObject.Find("Background").GetComponent<Background>();
        if (_background == null)
        {
            Debug.Log("Background is NULL.");
        }

        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (_gameManager == null)
        {
            Debug.Log("GameManager is NULL.");
        }

        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.Log("Player is NULL.");
        }

        _currentWave = -1;
        _totalWaves = _wave.Length - 1;

        _megaVacuum.SetActive(false);
        _megaHead.SetActive(false);
        _megaArm.SetActive(false);
        _planetDruidia.SetActive(false);
    }

    private void Update()
    {
        if (_isSpawning == true && _isBossWave == false)
        {
            _background.BackgroundScroll();
        }

        if (_spawnPowerups == true)
        {
            SpawnPowerups();
        }
    }

    public void PlayAudio(AudioClip clip)
    {
        Vector3 audioPosition = new Vector3(0, 1, -10); // position of main camera

        AudioSource.PlayClipAtPoint(clip, audioPosition);
    }

    public void StartSpawning()
    {
        _isSpawning = true;

        NextWave();
    }

    public void StartBossWave()
    {
        _isBossWave = true;
        _isSpawning = true;

        _planetDruidia.SetActive(true);

        _megaVacuum.SetActive(true);

        PlayAudio(_bossStartClip);

        _spawnPowerups = true;
    }

    private void NextWave()
    {
        _currentWave++;

        if (_wave[_currentWave].waveName == "Boss Wave")
        {
            StopCoroutine("SpawnPowerupRoutine");

            _isSpawning = false;
            _powerupsAvailable = false;

            Debug.Log("Final challenge!");

            _player._wavesComplete = true;

            return; 
        }
        else
        {
            Debug.Log("Wave: " + _wave[_currentWave].waveName);

            _totalEnemiesInWave = _wave[_currentWave].enemiesPerWave;
            _remainingEnemiesInWave = 0;
            _currentEnemiesSpawned = 0;

            StartCoroutine(SpawnEnemyRoutine());

            if (_spawnPowerups == false)
            {
                _spawnPowerups = true;
            }
        }
    }

    public void CalculateEnemiesRemaining()
    {
        _remainingEnemiesInWave--;

        if (_remainingEnemiesInWave <= 0 && _currentEnemiesSpawned == _totalEnemiesInWave)
        {
            _powerupsAvailable = false;

            StopCoroutine("SpawnPowerupRoutine");

            StartCoroutine(NextWaveRoutine());
        }
    }

    IEnumerator NextWaveRoutine()
    {
        yield return new WaitForSeconds(_waitToSpawn);

        NextWave();
    }

    IEnumerator SpawnEnemyRoutine()
    {
        yield return new WaitForSeconds(_waitToSpawn);

        GameObject[] enemy = _wave[_currentWave].enemyPrefabsInWave;
        float waveRate = _wave[_currentWave].enemySpawnRate;

        while (_isSpawning == true && _currentEnemiesSpawned < _totalEnemiesInWave)
        {
            float spawnRate = (1 / waveRate) * Random.Range(3f, 6f);
            int randomEnemy = Random.Range(0, enemy.Length);

            Vector3 spawnPosition;

            _remainingEnemiesInWave++;
            _currentEnemiesSpawned++;
            

            if (randomEnemy == 0)
            {
                float randomEnemyY = Random.Range(1f, 8.5f);
                float randomEnemyX = Random.Range(-12f, -1f);
                Vector3 randomSpawnY = new Vector3(-12f, randomEnemyY);
                Vector3 randomSpawnX = new Vector3(randomEnemyX, 8.5f);
                List<Vector3> randomSpawnList = new List<Vector3>();
                randomSpawnList.Add(randomSpawnY);
                randomSpawnList.Add(randomSpawnX);

                spawnPosition = randomSpawnList[Random.Range(0, randomSpawnList.Count)];

                randomSpawnList.Clear();
            }
            else
            {
                float randomEnemyX = Random.Range(-9f, 9f);
                spawnPosition = new Vector3(randomEnemyX, 7.5f, 0);
            }

            GameObject newEnemy = Instantiate(enemy[randomEnemy], spawnPosition, Quaternion.identity);

            newEnemy.transform.parent = _enemyContainer.transform;

            yield return new WaitForSeconds(spawnRate);
        }
    }

    private void CalculatePowerupRateTotal()
    {
        _wave[_currentWave].CalculatePowerupTableTotal();

        _powerupTableTotal = _wave[_currentWave].powerupTableTotal;
    }

    private void SpawnPowerups()
    {
        _spawnPowerups = false;

        CalculatePowerupRateTotal();

        StartCoroutine("SpawnPowerupRoutine");
    }

    IEnumerator SpawnPowerupRoutine()
    {
        Debug.Log("PowerupRoutine Started");

        _waitToSpawnPowerups = Random.Range(2, 5);

        yield return new WaitForSeconds(_waitToSpawnPowerups);

        _powerupsAvailable = true;

        WaveClass currentWave = _wave[_currentWave];

        GameObject[] powerup = currentWave.powerupPrefabsInWave;
        int[] powerupRatesInTable = currentWave.powerupRateTable;
        
        while (_isSpawning == true && _powerupsAvailable == true)
        {
            float randomPowerupSpawnRate = currentWave.powerupSpawnRate * Random.Range(2f, 4f);
            int randomPowerupRateTable = Random.Range(0, _powerupTableTotal);
            float randomPowerupX = Random.Range(-9f, 9f);

            Vector3 powerupSpawnPosition = new Vector3(randomPowerupX, 7.5f, 0);

            for (int i = 0; i < powerupRatesInTable.Length; i++)
            {
                if (randomPowerupRateTable <= powerupRatesInTable[i])
                {
                    Instantiate(powerup[i], powerupSpawnPosition, Quaternion.identity);

                    yield return new WaitForSeconds(randomPowerupSpawnRate);
                }
                else
                {
                    randomPowerupRateTable -= powerupRatesInTable[i];
                }
            }
        }
    }

    public void ActivateBossSecondStage()
    {
        StartCoroutine(ActivateBossSecondStageRoutine());
    }

    IEnumerator ActivateBossSecondStageRoutine()
    {
        yield return new WaitForSeconds(3f);

        _planetDruidiaScript._canMove = true;

        yield return new WaitForSeconds(1f);

        _megaHead.SetActive(true);

        yield return new WaitForSeconds(5f);

        _megaArm.SetActive(true);
    }

    public void OnPlayerDeath()
    {
        StopCoroutine("SpawnPowerupRoutine");

        _isSpawning = false;
    }
}
