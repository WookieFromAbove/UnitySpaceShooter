using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private bool _playerCanStart = false;
    private Vector3 _gameStartPosition = new Vector3(0, -6, 0);

    [SerializeField]
    private float _speed = 6.5f;
    private float _speedBoost = 1.5f;

    [SerializeField]
    private int _lives = 3;

    [SerializeField]
    private int _score;

    [SerializeField]
    private float _fireRate = 0.5f;
    private float _canFire = -1f;

    [SerializeField]
    private GameObject[] _weaponsPrefab; // 0 = laser, 1 = tripple, 2 = bomb

    [SerializeField]
    private AudioClip[] _weaponsClip; // 0 = laser, 1 = tripple, 2 = bomb

    private bool _isTrippleShotActive = false;

    private bool _isBombActive = false;

    private bool _isSpeedBoostActive = false;
    private bool _isSpeedBoostPowerDownActive = false;
    [SerializeField]
    private float _speedBoostRoutineTime = 7f;

    [SerializeField]
    private GameObject[] _thrusterVisualizer;

    [SerializeField]
    private GameObject[] _speedBoostVisualizer;

    private bool _isShieldActive = false;

    [SerializeField]
    private GameObject _shieldVisualizer;

    [SerializeField]
    private AudioClip _shieldPowerDownClip;

    [SerializeField]
    private AudioClip _playerHitClip;

    private AudioSource _playerAudioSource;

    [SerializeField]
    private GameObject[] _damageVisualizer; // 0 = left, 1 = middle, 2 = right

    private SpawnManager _spawnManager;

    private UIManager _uIManager;

    private Background _background;

    [SerializeField]
    private AudioSource _alarmAudio;
    
    void Start()
    {
        transform.position = _gameStartPosition;

        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        if (_spawnManager == null)
        {
            Debug.LogError("SpawnManager is NULL.");
        }

        _uIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (_uIManager == null)
        {
            Debug.Log("UIManager is NULL.");
        }

        _background = GameObject.Find("Background").GetComponent<Background>();
        if (_background == null)
        {
            Debug.Log("Background is NULL.");
        }

        _playerAudioSource = GetComponent<AudioSource>();
        if (_playerAudioSource == null)
        {
            Debug.Log("Player AudioSource is NULL.");
        }
    }

    void Update()
    {
        if (_playerCanStart == false)
        {
            StartCoroutine(GoToStartPosition());
        }
        else
        {
            CalculateMovement();

            CalculateFire();
        }
    }

    IEnumerator GoToStartPosition()
    {
        float waitTime = 0.04f;
        float cruiseSpeed = waitTime * Time.deltaTime;
        Vector3 startPosition = new Vector3(0, -2, 0);

        while (transform.position != startPosition && _playerCanStart == false)
        {
            yield return new WaitForSeconds(waitTime);

            transform.position = Vector3.Lerp(transform.position, startPosition, cruiseSpeed);

            if (transform.position.y >= -2.05f)
            {
                _playerCanStart = true;
            }
        }

        yield return null;
    }

    public int CurrentLives()
    {
        return _lives;
    }

    public int CurrentScore()
    {
        return _score;
    }

    public void AddToScore(int points)
    {
        _score += points;
    }

    public void DeductFromScore(int points)
    {
        if (_isShieldActive == true)
        {
            return;
        }

        _score -= points;
    }

    private void PlayAudio(AudioClip clip)
    {
        Vector3 audioPosition = new Vector3(0, 1, -10); // position of main camera

        AudioSource.PlayClipAtPoint(clip, audioPosition);
    }

    public void WeaponFireAudio(int clipID)
    {
        _playerAudioSource.clip = _weaponsClip[clipID];

        _playerAudioSource.Play();
    }

    private void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);

        transform.Translate(direction * _speed * Time.deltaTime);

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -9.3f, 9.3f), Mathf.Clamp(transform.position.y, -4f, 6f), 0);
    }

    private void CalculateFire()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
        {
            _canFire = Time.time + _fireRate;

            var laserOffset = new Vector3(0, 0.66f, 0);

            if (_isTrippleShotActive == true)
            {
                Instantiate(_weaponsPrefab[1], transform.position, Quaternion.identity);

                WeaponFireAudio(1);
            }
            else
            {
                Instantiate(_weaponsPrefab[0], transform.position + laserOffset, Quaternion.identity);

                WeaponFireAudio(0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.X) && _isBombActive == true)
        {
            var bigSchwartzOffset = new Vector3(0.03f, 1.6f, 0);

            Instantiate(_weaponsPrefab[2], transform.position + bigSchwartzOffset, Quaternion.identity);

            WeaponFireAudio(2);

            _isBombActive = false;
        }
    }

    public void TrippleShotActive()
    {
        _isTrippleShotActive = true;

        StartCoroutine(TrippleShotPowerDownRoutine());
    }

    IEnumerator TrippleShotPowerDownRoutine()
    {
        yield return new WaitForSeconds(5f);

        _isTrippleShotActive = false;
    }

    public void BombActive()
    {
        _isBombActive = true;
    }

    public void SpeedBoostActive()
    {
        if (_isSpeedBoostPowerDownActive == false)
        {
            _isSpeedBoostActive = true;

            _speed *= _speedBoost;

            _background.IncreaseScrollSpeed();

            for (int i = 0; i < _speedBoostVisualizer.Length; i++)
            {
                _speedBoostVisualizer[i].SetActive(true);
            }

            for (int i = 0; i < _thrusterVisualizer.Length; i++)
            {
                _thrusterVisualizer[i].SetActive(false);
            }

            StartCoroutine(SpeedBoostPowerDownRoutine());
        }
    }

    IEnumerator SpeedBoostPowerDownRoutine()
    {
        _isSpeedBoostPowerDownActive = true;

        yield return new WaitForSeconds(_speedBoostRoutineTime);

        _isSpeedBoostActive = false;

        _speed /= _speedBoost;

        _background.DecreaseScrollSpeed();

        for (int i = 0; i < _thrusterVisualizer.Length; i++)
        {
            _thrusterVisualizer[i].SetActive(true);
        }

        for (int i = 0; i < _speedBoostVisualizer.Length; i++)
        {
            _speedBoostVisualizer[i].SetActive(false);
        }

        _isSpeedBoostPowerDownActive = false;
    }

    public void ShieldActive()
    {
        _isShieldActive = true;

        _shieldVisualizer.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyLaser"))
        {
            DeductFromScore(10);

            Damage();

            Destroy(other.gameObject);
        }
    }

    public void Damage()
    {
        if (_isShieldActive == true)
        {
            _isShieldActive = false;

            _shieldVisualizer.SetActive(false);

            PlayAudio(_shieldPowerDownClip);

            return;
        }

        _lives--;

        if (_lives == 1)
        {
            _alarmAudio.Play();
        }

        DamageVisualizer();

        if (_lives > 0)
        {
            PlayAudio(_playerHitClip);
        }

        if (_lives < 1)
        {
            _spawnManager.OnPlayerDeath();

            _alarmAudio.Stop();

            Destroy(this.gameObject);
        }
    }

    public void DamageVisualizer()
    {
        int randomDamage = Random.Range(0, _damageVisualizer.Length);

        int previousDamage = randomDamage;

        if (_lives == 2)
        {
            _damageVisualizer[randomDamage].SetActive(true);
        }

        if (_lives == 1)
        {
            while (randomDamage == previousDamage)
            {
                randomDamage = Random.Range(0, _damageVisualizer.Length);
            }

            _damageVisualizer[randomDamage].SetActive(true);
        }
    }
}
