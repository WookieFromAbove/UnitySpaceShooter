using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class BigGunEnemy : MonoBehaviour
{
    [SerializeField]
    private float _enemySpeed = 4.5f;

    [SerializeField]
    private int _enemyID = 1;

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

    private Vector3 _startPosition;
    private Vector3 _enemyAxis;
    private float _randomChangeX;

    private bool _playerBehindEnemy = false;
    private bool _powerupInRange = false;

    private List<GameObject> _playerLaserList = new List<GameObject>();
    private bool _dodgeLaser = false;
    private bool _dodgeRight = false;
    private bool _dodgeLeft = false;

    public int _randomAvoid;
    private int _randomFireBackwards;
    private int _randomFireAtPowerup;

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

        _startPosition = transform.position;
        _enemyAxis = transform.right;
        _randomChangeX = Random.Range(2f, 4f);

        RandomAvoid();
        RandomFireBackwards();
        RandomFireAtPowerup();
    }

    void Update()
    {
        CalculateMovement();

        if (_dodgeRight == true)
        {
            DodgeRight();
        }
        
        if (_dodgeLeft == true)
        {
            DodgeLeft();
        }

        CalculateEnemyFire();
        CalculateEnemyRearFire();
        CalculateEnemyDestroyPowerup();
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

    private void CalculateMovement()
    {
        if (_isEnemyDead == false)
        {
            DetectLaser();

            float moveRate = 1f;

            _startPosition += Vector3.down * _enemySpeed * Time.deltaTime;
            transform.position = _startPosition + _enemyAxis * Mathf.Sin(Time.time * _randomChangeX) * moveRate;

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
        else
        {
            transform.Translate(Vector3.down * _enemySpeed * Time.deltaTime, Space.World);
        }
        
    }
    
    private void DetectLaser()
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
    }

    public void DodgeLaser()
    {
        _dodgeLaser = true;

        GameObject[] laser = GameObject.FindGameObjectsWithTag("Laser");
        foreach (var item in laser)
        {
            _playerLaserList.Add(item);
        }

        StartCoroutine(DodgeLaserRoutine());
    }

    IEnumerator DodgeLaserRoutine()
    {
        if (_isEnemyDead == false)
        {
            while (_playerLaserList[0] == null)
            {
                _playerLaserList.Remove(_playerLaserList[0]);
            }

            float xValue = transform.position.x - 0;
            float laserValue = _playerLaserList[0].transform.position.x - 0;

            if (xValue > laserValue && xValue >= 0)
            {
                _dodgeRight = true;
            }
            else
            {
                _dodgeLeft = true;
            }

            yield return new WaitForSeconds(1f);

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

            yield return new WaitForSeconds(2f);

            _dodgeLaser = false;
        }
    }

    private void DodgeRight()
    {
        transform.Translate(Vector3.right * _enemySpeed * Time.deltaTime);
    }

    private void DodgeLeft()
    {
        transform.Translate(Vector3.left * _enemySpeed * Time.deltaTime);
    }

    private void CalculateEnemyFire()
    {
        if (_canFire == false)
        {
            _enemyLaserOffset = new Vector3(0.025f, -0.9f, 0);

            StartCoroutine(EnemyFireRoutine());
        }
    }

    IEnumerator EnemyFireRoutine()
    {
        if (_isEnemyDead == false)
        {
            _canFire = true;

            _fireRate = Random.Range(3, 5);

            Instantiate(_enemyLaserPrefab, transform.position + _enemyLaserOffset, Quaternion.identity);

            EnemyWeaponsAudio();

            yield return new WaitForSeconds(_fireRate);

            _canFire = false;
        }
    }

    private void CalculateEnemyRearFire()
    {
        float playerSearch = 7f;

        RaycastHit2D[] playerDetection = Physics2D.RaycastAll(transform.position, Vector2.up, playerSearch);
        Debug.DrawRay(transform.position, Vector3.up * playerSearch);

        foreach (RaycastHit2D obj in playerDetection)
        {
            if (obj.collider.gameObject.CompareTag("Player"))
            {
                if (_playerBehindEnemy == false && _randomFireBackwards == 0)
                {
                    _playerBehindEnemy = true;

                    StartCoroutine(EnemyRearFireRoutine());
                }
            }
        }
    }

    IEnumerator EnemyRearFireRoutine()
    {
        if (_isEnemyDead == false)
        {
            int rearReload = Random.Range(3, 5);

            GameObject rearFire = Instantiate(_enemyLaserPrefab, transform.position, Quaternion.Euler(0, 0, 180));
            rearFire.GetComponent<SpriteRenderer>().color = new Color32(0, 255, 66, 255);

            EnemyWeaponsAudio();

            yield return new WaitForSeconds(rearReload);

            _playerBehindEnemy = false;
        }
    }

    private void CalculateEnemyDestroyPowerup()
    {
        float powerupSearch = 2.5f;

        RaycastHit2D[] powerupDetection = Physics2D.RaycastAll(transform.position, Vector2.down, powerupSearch);
        Debug.DrawRay(transform.position, Vector3.down * powerupSearch);

        foreach (RaycastHit2D obj in powerupDetection)
        {
            if (obj.collider.gameObject.CompareTag("PlayerPowerup"))
            {
                if (_powerupInRange == false && _randomFireAtPowerup == 0)
                {
                    _powerupInRange = true;

                    StartCoroutine(EnemyDestroyPowerupRoutine());
                }
            }
        }
    }

    IEnumerator EnemyDestroyPowerupRoutine()
    {
        if (_isEnemyDead == false && _enemySideShotPrefab != null)
        {
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

    private void EnemyDamage()
    {
        StartCoroutine(OnEnemyDeathRoutine());
    }

    IEnumerator OnEnemyDeathRoutine()
    {
        _isEnemyDead = true;

        _enemySpeed /= 2f;

        _enemyAnim.SetTrigger("OnEnemyDeath");

        EnemyExplosionAudio();

        gameObject.GetComponent<Collider2D>().enabled = false;

        _spawnManager.CalculateEnemiesRemaining();

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
        _enemyAudio.clip = _enemyWeaponsClip;
        _enemyAudio.Play();
    }

    void EnemyDestroyPowerupAudio()
    {
        _enemyAudio.clip = _enemyDestroyPowerupClip;
        _enemyAudio.Play();
    }
}
