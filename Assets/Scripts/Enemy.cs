using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _enemySpeed = 4f;
    private float _speedIncrease = 1.25f;

    [SerializeField]
    private int _enemyID; // 0 = Ram, 1 = BigGun, 2 = Spin

    private Player _player;
    private SpawnManager _spawnManager;

    private Animator _enemyAnim;

    [SerializeField]
    private AudioClip _explosionClip;

    private AudioSource _enemyAudio;

    private bool _isEnemyDead = false;

    [SerializeField]
    private float _fireRate;
    private bool _canFire = false;

    [SerializeField]
    private GameObject _enemyLaserPrefab;
    private Vector3 _enemyLaserOffset;

    [SerializeField]
    private GameObject _enemySideShotPrefab;

    [SerializeField]
    private AudioClip _enemyWeaponsClip;

    [SerializeField]
    private AudioClip _enemyDestroyPowerupClip;

    [SerializeField]
    private Collider2D[] _spinningFlameCollider;

    [SerializeField]
    private GameObject _enemyShieldVisualizer;
    private bool _isEnemyShieldActive = false;
    [SerializeField]
    private AudioClip _enemyShieldPowerdownClip;

    private Vector3 _startPosition;
    private Vector3 _enemyAxis;
    private float _randomChangeX;

    private bool _playerInRange = false;
    private bool _playerBehindEnemy = false;
    private bool _powerupInRange = false;

    private List<GameObject> _playerLaserList = new List<GameObject>();
    private bool _dodgeLaser = false;
    private bool _dodgeRight = false;
    private bool _dodgeLeft = false;

    private int _randomAttack;
    private int _randomAvoid;
    private int _randomFireBackwards;
    private int _randomFireAtPowerup;

    private void Start()
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

        _startPosition = transform.position;
        _enemyAxis = transform.right;
        _randomChangeX = Random.Range(2f, 4f);

        if (_enemyID == 0) // RammingEnemy
        {
            int randomShield = Random.Range(0, 3);

            if (randomShield == 0 && _enemyShieldVisualizer != null)
            {
                EnemyShieldActive();
            }
        }

        RandomAttack();
        RandomAvoid();
        RandomFireBackwards();
        RandomFireAtPowerup();
    }

    void Update()
    {
        if (_dodgeLaser == true)
        {
            if (_dodgeRight == true)
            {
                transform.Translate(Vector3.right * _enemySpeed * Time.deltaTime);
            }
            else if (_dodgeLeft == true)
            {
                transform.Translate(Vector3.left * _enemySpeed * Time.deltaTime);
            }
        }
        else
        {
            CalculateMovement();
        }

        CalculateEnemyFire();
    }

    private void CalculateMovement()
    {
        if (_enemyID == 0) // RammingEnemy
        {
            // shoot ray down 5
            // if player hits it
            // enemy moves down to hit it
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

                    RandomAttack();

                    randomEnemySpawn.Clear();
                }
            }
        }
        else
        {
            if (_enemyID == 1) // BigGunEnemy
            {
                float searchArea = 2f;

                Collider2D[] detectLaser = Physics2D.OverlapCircleAll(transform.position, searchArea);

                foreach (Collider2D obj in detectLaser)
                {
                    if (obj.gameObject.CompareTag("Laser"))
                    {
                        if (_randomAvoid == 0 && _dodgeLaser == false)
                        {
                            DodgeLaser();
                        }
                    }
                }

                float moveRate = 1f;
                float moveSize = _randomChangeX;
                
                _startPosition += Vector3.down * _enemySpeed * Time.deltaTime;
                transform.position = _startPosition + _enemyAxis * Mathf.Sin(Time.time * moveSize) * moveRate;
            }
            else
            {
                transform.Translate(Vector3.down * _enemySpeed * Time.deltaTime, Space.World);
            }

            if (_enemyID == 2) // SpinningEnemy
            {
                int rotationSpeed = 65;

                transform.Rotate(Vector3.back * rotationSpeed * Time.deltaTime);

                foreach (var fire in _spinningFlameCollider)
                {
                    if (fire != null)
                    {
                        fire.gameObject.SetActive(true);
                    }
                }
            }

            if (transform.position.y < -5.5f)
            {
                if (_isEnemyDead == true)
                {
                    transform.Translate(Vector3.down * _enemySpeed * Time.deltaTime, Space.World);
                }
                else
                {
                    float randomX = Random.Range(-9f, 9f);

                    transform.position = new Vector3(randomX, 7.5f, 0);

                    _startPosition = transform.position;

                    RandomAvoid();
                    RandomFireBackwards();
                    RandomFireAtPowerup();
                }
            }
        }
    }

    private void RandomAttack()
    {
        _randomAttack = Random.Range(0, 3);
    }

    private void RandomAvoid()
    {
        _randomAvoid = Random.Range(0, 2);
    }

    private void RandomFireBackwards()
    {
        _randomFireBackwards = Random.Range(0, 2);
    }

    private void RandomFireAtPowerup()
    {
        _randomFireAtPowerup = Random.Range(0, 2);
    }

    private void CalculateEnemyFire()
    {
        if (_enemyID == 1) // BigGunEnemy
        {
            if (_canFire == false)
            {
                _enemyLaserOffset = new Vector3(0.025f, -0.9f, 0);

                StartCoroutine(EnemyFireRoutine());
            }

            float playerSearch = 7f;

            RaycastHit2D[] playerDetection = Physics2D.RaycastAll(transform.position, Vector2.up, playerSearch);
            Debug.DrawRay(transform.position, Vector3.up * playerSearch);
            
            foreach (RaycastHit2D obj in playerDetection)
            {
                if (obj.collider.gameObject.CompareTag("Player"))
                {
                    if (_playerBehindEnemy == false)
                    {
                        if (_randomFireBackwards == 0)
                        {
                            StartCoroutine(EnemyRearFireRoutine());
                        }
                    }
                }
            }

            float powerupSearch = 2.5f;

            RaycastHit2D[] powerupDetection = Physics2D.RaycastAll(transform.position, Vector2.down, powerupSearch);
            Debug.DrawRay(transform.position, Vector3.down * powerupSearch);

            foreach (RaycastHit2D obj in powerupDetection)
            {
                if (obj.collider.gameObject.CompareTag("PlayerPowerup"))
                {
                    if (_powerupInRange == false)
                    {
                        if (_randomFireAtPowerup == 0)
                        {
                            StartCoroutine(EnemyDestroyPowerupRoutine());
                        }
                    }
                }
            }
        }
    }

    private void DodgeLaser()
    {
        _dodgeLaser = true;

        StartCoroutine(DodgeLaserRoutine());
    }

    IEnumerator DodgeLaserRoutine()
    {
        if (_isEnemyDead == false)
        {
            GameObject[] laser = GameObject.FindGameObjectsWithTag("Laser");
            foreach (var item in laser)
            {
                _playerLaserList.Add(item);
            }

            if (transform.position.x - _playerLaserList[0].transform.position.x >= 0)
            {
                _dodgeRight = true;
            }
            else if (transform.position.x - _playerLaserList[0].transform.position.x < 0)
            {
                _dodgeLeft = true;
            }

            yield return new WaitForSeconds(0.5f);

            if (_dodgeRight == true)
            {
                _dodgeRight = false;
            }

            if (_dodgeLeft == true)
            {
                _dodgeLeft = false;
            }

            _playerLaserList.Clear();

            _startPosition = transform.position;

            _dodgeLaser = false;
        }
    }

    IEnumerator EnemyFireRoutine()
    {
        while (_isEnemyDead == false)
        {
            _canFire = true;

            _fireRate = Random.Range(3, 5);

            Instantiate(_enemyLaserPrefab, transform.position + _enemyLaserOffset, Quaternion.identity);

            EnemyWeaponsAudio();

            yield return new WaitForSeconds(_fireRate);

            _canFire = false;
        }
    }

    IEnumerator EnemyRearFireRoutine()
    {
        if (_isEnemyDead == false)
        {
            _playerBehindEnemy = true;

            int rearReload = Random.Range(3, 5);

            GameObject rearFire = Instantiate(_enemyLaserPrefab, transform.position, Quaternion.Euler(0, 0, 180));
            rearFire.GetComponent<SpriteRenderer>().color = new Color32(0, 255, 66, 255);

            EnemyWeaponsAudio();

            yield return new WaitForSeconds(rearReload);

            _playerBehindEnemy = false;
        }
    }

    IEnumerator EnemyDestroyPowerupRoutine()
    {
        if (_isEnemyDead == false && _enemySideShotPrefab != null)
        {
            _powerupInRange = true;

            int sideReload = Random.Range(3, 5);

            Instantiate(_enemySideShotPrefab, transform.position, Quaternion.identity);

            EnemyDestroyPowerupAudio();

            yield return new WaitForSeconds(sideReload);

            _powerupInRange = false;
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

    public void EnemyShieldActive()
    {
        _isEnemyShieldActive = true;

        if (_enemyShieldVisualizer != null)
        {
            _enemyShieldVisualizer.SetActive(true);
        }
        
    }

    public void EnemyDamage()
    {
        if (_isEnemyShieldActive == true)
        {
            _isEnemyShieldActive = false;

            _enemyShieldVisualizer.SetActive(false);

            EnemyShieldAudio();

            return;
        }

        // start enemy death coroutine
        StartCoroutine(OnEnemyDeathRoutine());
    }

    IEnumerator OnEnemyDeathRoutine()
    {
        _enemySpeed /= 2f;

        _isEnemyDead = true;

        _enemyAnim.SetTrigger("OnEnemyDeath");

        EnemyExplosionAudio();

        gameObject.GetComponent<Collider2D>().enabled = false;

        _spawnManager.CalculateEnemiesRemaining();

        if (_enemyID == 2)
        {
            foreach (var fire in _spinningFlameCollider)
            {
                if (fire != null)
                {
                    Destroy(fire.gameObject);
                }
            }
        }

        yield return new WaitForSeconds(2.5f);

        Destroy(this.gameObject);
    }

    void EnemyExplosionAudio()
    {
        _enemyAudio.clip = _explosionClip;
        _enemyAudio.Play();
    }

    void EnemyWeaponsAudio()
    {
        if (_enemyWeaponsClip != null)
        {
            _enemyAudio.clip = _enemyWeaponsClip;
            _enemyAudio.Play();
        }
    }

    void EnemyDestroyPowerupAudio()
    {
        _enemyAudio.clip = _enemyDestroyPowerupClip;
        _enemyAudio.Play();
    }

    void EnemyShieldAudio()
    {
        _enemyAudio.clip = _enemyShieldPowerdownClip;
        _enemyAudio.Play();
    }
}
