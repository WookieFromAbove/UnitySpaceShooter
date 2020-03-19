using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private bool _playerCanStart = false;
    private Vector3 _gameStartPosition = new Vector3(0, -6, 0);

    [SerializeField]
    private float _speed = 6.5f;
    [SerializeField]
    private float _thrusters = 1.5f;
    private float _speedBoost = 2f;

    [SerializeField]
    private GameObject _engineThrusters;
    [SerializeField]
    private AudioClip _engineThrustersClip;
    [SerializeField]
    private AudioClip _engineThrustersFailClip;

    private float _maxThrusterTime = 3f;
    private float _remainingThrusterTime;
    private float _thrusterUseRate = 1f;
    private float _thrusterRegenRate = 0.5f;
    private bool _thrusterAvailable = true;

    [SerializeField]
    private Image _thrusterSliderFill;
    private Color32 _fillColor = new Color32(116, 219, 67, 255);

    [SerializeField]
    private int _maxLives = 3;
    private int _currentLives;

    [SerializeField]
    private int _score;

    [SerializeField]
    private float _fireRate = 0.5f;
    private float _canFire = -1f;

    private int _maxLaserAmmo = 15;
    private int _currentLaserAmmo;

    [SerializeField]
    private GameObject[] _weaponsPrefab; // 0 = laser, 1 = tripple, 2 = bomb

    [SerializeField]
    private GameObject _bombContainer;

    [SerializeField]
    private AudioClip[] _weaponsClip; // 0 = laser, 1 = tripple, 2 = bomb

    [SerializeField]
    private AudioClip _ammoEmptyClip;

    private bool _isTrippleShotActive = false;

    private bool _isBombActive = false;
    private int _maxBombsToSpawn = 5;
    [SerializeField]
    private List<GameObject> _activeBombList = new List<GameObject>();
    private Bomb _bombFireScript;

    private bool _isSpeedBoostActive = false;
    private bool _isSpeedBoostPowerDownActive = false;
    [SerializeField]
    private float _speedBoostRoutineTime = 7f;

    [SerializeField]
    private GameObject[] _thrusterVisualizer;

    [SerializeField]
    private GameObject[] _speedBoostVisualizer;

    // Shield
    private bool _isShieldActive = false;
    private int _maxShieldHealth = 3;
    private int _currentShieldHealth;

    [SerializeField]
    private GameObject _shieldVisualizer;
    private SpriteRenderer _shieldRenderer;

    [SerializeField]
    private AudioClip _shieldDeflectClip;
    [SerializeField]
    private AudioClip _shieldPowerDownClip;

    [SerializeField]
    private AudioClip _playerHitClip;

    private AudioSource _playerAudioSource;

    [SerializeField]
    private GameObject[] _damageVisualizer; // 0 = left, 1 = middle, 2 = right
    private GameObject _firstDamage;
    private GameObject _secondDamage;
    private int _currentDamage;

    private CameraShake _mainCameraShake;

    private SpawnManager _spawnManager;

    private UIManager _uIManager;

    private Background _background;

    [SerializeField]
    private AudioSource _alarmAudio;
    
    void Start()
    {
        transform.position = _gameStartPosition;
        _currentLaserAmmo = _maxLaserAmmo;
        _currentLives = _maxLives;
        _remainingThrusterTime = _maxThrusterTime;
        _thrusterSliderFill.color = _fillColor;

        _mainCameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
        if (_mainCameraShake == null)
        {
            Debug.Log("MainCamera is NULL.");
        }

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

        _shieldRenderer = _shieldVisualizer.GetComponent<SpriteRenderer>();
        if (_shieldRenderer == null)
        {
            Debug.Log("ShieldColor is NULL.");
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
        return _currentLives;
    }

    public int CurrentScore()
    {
        return _score;
    }

    public int CurrentAmmo()
    {
        return _currentLaserAmmo;
    }

    public float CurrentEngineThrusterTime()
    {
        return _remainingThrusterTime;
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

        if (_thrusterAvailable == true)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                PlayAudio(_engineThrustersClip);
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                float speed = _speed * _thrusters;

                transform.Translate(direction * speed * Time.deltaTime);

                _engineThrusters.SetActive(true);

                ThrusterTime();
            }
            else if (Input.GetKey(KeyCode.LeftShift) == false)
            {
                _remainingThrusterTime += _thrusterRegenRate * Time.deltaTime;

                _engineThrusters.SetActive(false);

                if (_remainingThrusterTime >= _maxThrusterTime)
                {
                    _remainingThrusterTime = _maxThrusterTime;
                }
            }
        }

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -9.3f, 9.3f), Mathf.Clamp(transform.position.y, -4f, 6f), 0);
    }

    private void ThrusterTime()
    {
        _remainingThrusterTime -= _thrusterUseRate * Time.deltaTime;

        if (_remainingThrusterTime <= 0)
        {
            _engineThrusters.SetActive(false);

            StartCoroutine(ThrusterCooldownRoutine());
        }
    }

    IEnumerator ThrusterCooldownRoutine()
    {
        _thrusterAvailable = false;
        _remainingThrusterTime = 0;

        yield return new WaitForSeconds(5f);

        float cooldownRate = 1f * Time.deltaTime;

        _thrusterSliderFill.color = new Color32(255, 0, 0, 255);

        while (_thrusterAvailable == false)
        {
            _remainingThrusterTime += _thrusterRegenRate * Time.deltaTime;

            _thrusterSliderFill.color = Color32.Lerp(_thrusterSliderFill.color, _fillColor, _thrusterRegenRate * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                PlayAudio(_engineThrustersFailClip);
            }

            if (_remainingThrusterTime >= _maxThrusterTime)
            {
                _remainingThrusterTime = _maxThrusterTime;

                _thrusterSliderFill.color = _fillColor;

                _thrusterAvailable = true;
            }

            yield return new WaitForSeconds(cooldownRate);
        }
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
            else if (_currentLaserAmmo == 0)
            {
                PlayAudio(_ammoEmptyClip);
            }
            else
            {
                Instantiate(_weaponsPrefab[0], transform.position + laserOffset, Quaternion.identity);

                _currentLaserAmmo--;

                WeaponFireAudio(0);
            }
        }

        if (Input.GetKeyDown(KeyCode.X) && _isBombActive == true)
        {
            while (_activeBombList[0] == null)
            {
                _activeBombList.Remove(_activeBombList[0]);
            }

            _activeBombList[0].GetComponent<Bomb>().FireAtEnemy();

            WeaponFireAudio(2);

            _activeBombList.Remove(_activeBombList[0]);

            if (_activeBombList.Count == 0)
            {
                _activeBombList.Clear();

                _isBombActive = false;
            }
        }
    }

    public void ReloadAmmo()
    {
        _currentLaserAmmo = _maxLaserAmmo;
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

        StartCoroutine(SpawnBombRoutine());
    }

    IEnumerator SpawnBombRoutine()
    {
        float rotationSpeed = 115f;
        float rotationTime = rotationSpeed * Time.deltaTime;
        float waitTime = rotationTime / _maxBombsToSpawn;

        while (_isBombActive == true && _activeBombList.Count <= _maxBombsToSpawn)
        {
            GameObject newBomb = Instantiate(_weaponsPrefab[2], transform.position, Quaternion.identity);

            _activeBombList.Add(newBomb);

            newBomb.transform.parent = _bombContainer.transform;

            yield return new WaitForSeconds(waitTime);
        }
    }

    // Coroutine if wanting bombs to replace standard fire for 5 seconds:
    /*
    IEnumerator BombPowerDownRoutine()
    {
        yield return new WaitForSeconds(5f);

        if (_activeBombList.Count > 0)
        {
            _activeBombList[_activeBombList.Count].GetComponent<Bomb>().FireAtEnemy();

            WeaponFireAudio(2);

            _activeBombList.Clear();

            _isBombActive = false;
        }
    }*/

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
        _currentShieldHealth = _maxShieldHealth;

        _isShieldActive = true;

        _shieldRenderer.color = new Color32(255, 0, 248, 255);

        _shieldVisualizer.SetActive(true);
    }

    public void ShieldDamage()
    {
        _currentShieldHealth--;

        if (_currentShieldHealth == 2)
        {
            _shieldRenderer.color = new Color32(255, 0, 100, 255);

            PlayAudio(_shieldDeflectClip);
        }
        
        if (_currentShieldHealth == 1)
        {
            _shieldRenderer.color = new Color32(255, 0, 35, 255);

            PlayAudio(_shieldDeflectClip);
        }
        
        if (_currentShieldHealth == 0)
        {
            _isShieldActive = false;

            _shieldVisualizer.SetActive(false);

            PlayAudio(_shieldPowerDownClip);
        }
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
            ShieldDamage();

            _mainCameraShake.ShieldHitShake();

            return;
        }

        _currentLives--;

        _mainCameraShake.PlayerHitShake();

        if (_currentLives == 1)
        {
            _alarmAudio.Play();
        }

        DamageVisualizer();

        if (_currentLives > 0)
        {
            PlayAudio(_playerHitClip);
        }

        if (_currentLives < 1)
        {
            _spawnManager.OnPlayerDeath();

            _alarmAudio.Stop();

            Destroy(this.gameObject);
        }
    }

    public void DamageVisualizer()
    {
        if (_currentLives == 2)
        {
            int firstRandomDamage = Random.Range(0, _damageVisualizer.Length);
            _currentDamage = firstRandomDamage;

            _firstDamage = _damageVisualizer[firstRandomDamage];
            _firstDamage.SetActive(true);
        }

        if (_currentLives == 1)
        {
            int secondRandomDamage = Random.Range(0, _damageVisualizer.Length);

            while (secondRandomDamage == _currentDamage)
            {
                secondRandomDamage = Random.Range(0, _damageVisualizer.Length);
            }

            _secondDamage = _damageVisualizer[secondRandomDamage];
            _secondDamage.SetActive(true);
        }
    }

    public void GainLife()
    {
        if (_currentLives != _maxLives)
        {
            _currentLives++;

            if (_currentLives == _maxLives)
            {
                _firstDamage.SetActive(false);
            }

            if (_currentLives == _maxLives - 1)
            {
                _secondDamage.SetActive(false);

                _alarmAudio.Stop();
            }
        }
    }
}
