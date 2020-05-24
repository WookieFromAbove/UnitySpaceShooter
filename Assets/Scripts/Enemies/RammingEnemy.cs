using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RammingEnemy : MonoBehaviour
{
    [SerializeField]
    private float _enemySpeed = 6f;
    private float _speedIncrease = 1.25f;

    [SerializeField]
    private int _enemyID = 0;

    private Player _player;
    private SpawnManager _spawnManager;

    private Animator _enemyAnim;

    [SerializeField]
    private AudioClip _explosionClip;

    private AudioSource _enemyAudio;

    private bool _isEnemyDead = false;

    [SerializeField]
    private GameObject _enemyShieldVisualizer;
    private bool _isEnemyShieldActive = false;
    [SerializeField]
    private AudioClip _enemyShieldPowerdownClip;

    private int _randomAttack;

    private bool _playerInRange = false;

    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.Log("Player is NULL.");
        }

        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        if (_spawnManager == null)
        {
            Debug.Log("SpawnManager is NULL.");
        }

        _enemyAnim = GetComponent<Animator>();
        if (_enemyAnim == null)
        {
            Debug.Log("Animator is NULL.");
        }

        _enemyAudio = GetComponent<AudioSource>();
        if (_enemyAudio == null)
        {
            Debug.Log("Enemy AudioSource is NULL.");
        }

        RandomShieldActivate();
        SetRandomAttack();
    }

    void Update()
    {
        CalculateMovement();
    }

    private void RandomShieldActivate()
    {
        int randomShield = Random.Range(0, 3);

        if (randomShield == 0 && _enemyShieldVisualizer != null)
        {
            EnemyShieldActive();
        }
    }

    private void SetRandomAttack()
    {
        _randomAttack = Random.Range(0, 3);
    }

    private void CalculateMovement()
    {
        float searchArea = 3.5f;

        RaycastHit2D[] hitDetection = Physics2D.RaycastAll(transform.position, Vector2.down, searchArea);

        foreach (RaycastHit2D obj in hitDetection)
        {
            if (obj.collider.gameObject.CompareTag("Player"))
            {
                if (_randomAttack == 0)
                {
                    _playerInRange = true;
                }
            }
        }

        Debug.DrawRay(transform.position, Vector3.down * searchArea);

        if (_playerInRange == false)
        {
            transform.eulerAngles = new Vector3(0, 0, 45);

            transform.Translate(Vector3.up * -1 * _enemySpeed * Time.deltaTime);
        }
        else if (_playerInRange == true)
        {
            transform.Translate(Vector3.down * _enemySpeed * _speedIncrease * Time.deltaTime, Space.World);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, 0), _enemySpeed);
        }

        if (transform.position.y < -5.5f)
        {
            if (_isEnemyDead == true)
            {
                transform.Translate(Vector3.down * _enemySpeed * Time.deltaTime, Space.World);
            }
            else
            {
                _playerInRange = false;

                float randomEnemyY = Random.Range(1f, 8.5f);
                float randomEnemyX = Random.Range(-12f, -1f);
                Vector3 randomSpawnY = new Vector3(-12f, randomEnemyY);
                Vector3 randomSpawnX = new Vector3(randomEnemyX, 8.5f);
                List<Vector3> randomEnemySpawn = new List<Vector3>();
                randomEnemySpawn.Add(randomSpawnY);
                randomEnemySpawn.Add(randomSpawnX);

                Vector3 spawnPosition = randomEnemySpawn[Random.Range(0, randomEnemySpawn.Count)];

                transform.position = spawnPosition;

                SetRandomAttack();

                randomEnemySpawn.Clear();
            }
        }
    }

    private void EnemyShieldActive()
    {
        _isEnemyShieldActive = true;

        if (_enemyShieldVisualizer != null)
        {
            _enemyShieldVisualizer.SetActive(true);
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && _player != null)
        {
            switch (other.tag)
            {
                case "Player":

                    _player.DeductFromScore(5);

                    _player.Damage();

                    EnemyDamage();

                    break;

                case "Laser":

                    _player.AddToScore(10);

                    Destroy(other.gameObject);

                    EnemyDamage();

                    break;

                case "Bomb":

                    _player.AddToScore(20);

                    Destroy(other.gameObject);

                    EnemyDamage();

                    break;

                default:

                    break;
            }
        }
    }

    private void EnemyDamage()
    {
        if (_isEnemyShieldActive == true)
        {
            _isEnemyShieldActive = false;

            _enemyShieldVisualizer.SetActive(false);

            EnemyShieldAudio();

            return;
        }
        else
        {
            StartCoroutine(OnEnemyDeathRoutine());
        }
    }

    IEnumerator OnEnemyDeathRoutine()
    {
        _enemySpeed /= 2f;

        _isEnemyDead = true;

        _enemyAnim.SetTrigger("OnEnemyDeath");

        EnemyExplosionAudio();

        gameObject.GetComponent<Collider2D>().enabled = false;

        _spawnManager.CalculateEnemiesRemaining();

        yield return new WaitForSeconds(2.5f);

        Destroy(this.gameObject);
    }

    void EnemyShieldAudio()
    {
        _enemyAudio.clip = _enemyShieldPowerdownClip;
        _enemyAudio.Play();
    }

    void EnemyExplosionAudio()
    {
        _enemyAudio.clip = _explosionClip;
        _enemyAudio.Play();
    }
}
