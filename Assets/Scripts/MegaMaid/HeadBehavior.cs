using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class HeadBehavior : MonoBehaviour
{
    private Player _player;
    private Rigidbody2D _playerRGB;
    private bool _playerHit = false;

    private Vector3 _startPosition = new Vector3(0, 9.25f, 0);
    private Vector3 _centerPosition = new Vector3(0, 2, 0);

    private Quaternion _originalRotation;
    private Quaternion _attackRotation = Quaternion.Euler(0, 0, 180);
    [SerializeField]
    private float _rotationSpeed = 50f;
    private float _rotationTime = 0.02f;

    private bool _isActive = false;
    private bool _onDeath = false;

    private float _waitToAttack = 2f;

    private bool _startAttack = false;
    private bool _isAttacking = false;
    private bool _isFacingUp = true;

    private float _cannonAttack;
    private float _eyeAttack;

    private bool _canFire = false;
    private float _fireRate;

    [SerializeField]
    private GameObject[] _cannons = new GameObject[7];
    [SerializeField]
    private GameObject _bossLaserPrefab;

    [SerializeField]
    private GameObject _bossEyeLaserPrefab;

    private Vector3 _floatingPosition;
    private float _floatingY;
    [SerializeField]
    private float _floatingAmplitude = 0.25f;
    private float _floatingSpeed = 1f;

    [SerializeField]
    private int _headMaxHealth = 50;
    private int _headCurrentHealth;

    [SerializeField]
    private GameObject _damageContainer;

    [SerializeField]
    private GameObject _damagePrefab;

    [SerializeField]
    private Vector3[] _damagePositions;
    private List<Vector3> _damageList = new List<Vector3>();

    private bool[] _damageChecks = new bool[4];

    [SerializeField]
    private GameObject _explosionPrefab;

    [SerializeField]
    private Vector3 _bossMinScale;

    [SerializeField]
    private GameObject _megaArm;

    private AudioSource _headAudioSource;

    [SerializeField]
    private AudioClip _startClip;
    [SerializeField]
    private AudioClip _eyeShotClip;
    [SerializeField]
    private AudioClip _cannonShotClip;
    [SerializeField]
    private AudioClip _laserHitClip;
    [SerializeField]
    private AudioClip _keepFiringClip;
    [SerializeField]
    private AudioClip _onDestroyClip;

    private UIManager _uIManager;

    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.Log("Player is NULL.");
        }
        _playerRGB = _player.GetComponent<Rigidbody2D>();

        _headAudioSource = gameObject.GetComponent<AudioSource>();
        if (_headAudioSource == null)
        {
            Debug.Log("AudioSource is NULL.");
        }

        _uIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (_uIManager == null)
        {
            Debug.Log("UIManager is NULL.");
        }

        transform.position = _startPosition;
        _originalRotation = transform.rotation;

        _floatingPosition = _centerPosition;
        _floatingY = _centerPosition.y;

        _headCurrentHealth = _headMaxHealth;

        _isActive = true;

        for (int i = 0; i < _damageChecks.Length; i++)
        {
            _damageChecks[i] = false;
        }


        StartAudio(_startClip);
    }

    void Update()
    {
        if (_isActive == true)
        {
            if (_startAttack == false)
            {
                MoveToCenter();
            }
            else
            {
                CalculateFloating();

                if (_isAttacking == false)
                {
                    CalculateAttack();
                }
            }
        }
        else
        {
            OnBossDeath();
        }

        PlayerDefaultVelocity();
    }

    private void PlayAudio(AudioClip clip)
    {
        Vector3 audioPosition = new Vector3(0, 1, -10); // position of main camera

        AudioSource.PlayClipAtPoint(clip, audioPosition);
    }

    public void StartAudio(AudioClip clip)
    {
        _headAudioSource.clip = clip;

        _headAudioSource.Play();
    }

    private void MoveToCenter()
    {
        transform.position = Vector3.MoveTowards(transform.position, _centerPosition, 1f * Time.deltaTime);

        if (transform.position.y <= _centerPosition.y + _floatingAmplitude)
        {
            _startAttack = true;

            return;
        }
    }

    private void CalculateFloating()
    {
        _floatingPosition.y = _floatingY + _floatingAmplitude * Mathf.Sin(_floatingSpeed * Time.time);

        transform.position = _floatingPosition;
    }

    private void OriginalRotation()
    {
        if (transform.rotation != _originalRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _originalRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    private void AttackRotation()
    {
        if (transform.rotation != _attackRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _attackRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    private void CalculateAttack()
    {
        _isAttacking = true;

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(_waitToAttack);

        _rotationTime *= Time.deltaTime;

        while (_isActive == true && _isAttacking == true)
        {
            if (_isFacingUp == false)
            {
                AttackRotation();

                if (transform.rotation == _attackRotation)
                {
                    CalculateCannonFire();

                    _cannonAttack = Random.Range(10f, 15f);

                    yield return new WaitForSeconds(_cannonAttack);

                    _isFacingUp = true;
                }

                yield return new WaitForSeconds(_rotationTime);
            }
            else if (_isFacingUp == true)
            {
                OriginalRotation();

                if (transform.rotation == _originalRotation)
                {
                    CalculateEyeFire();

                    _eyeAttack = Random.Range(15f, 20f);

                    yield return new WaitForSeconds(_eyeAttack);

                    _isFacingUp = false;
                }

                yield return new WaitForSeconds(_rotationTime);
            }
        }
    }

    private void CalculateCannonFire()
    {
        if (_isAttacking == true && _canFire == false)
        {
            StartCoroutine(BossCannonFireRoutine());
        }
    }

    IEnumerator BossCannonFireRoutine()
    {
        while (_isActive == true && _isFacingUp == false && _player != null)
        {
            _canFire = true;

            _fireRate = Random.Range(1, 2);

            foreach (var cannon in _cannons)
            {
                GameObject laser = Instantiate(_bossLaserPrefab, cannon.transform.position, Quaternion.identity);

                laser.transform.rotation = cannon.transform.rotation;
            }

            PlayAudio(_cannonShotClip);

            yield return new WaitForSeconds(_fireRate);

            _canFire = false;
        }
    }

    private void CalculateEyeFire()
    {
        if (_isAttacking == true && _isFacingUp == true && _canFire == false)
        {
            StartCoroutine(BossEyeFireRoutine());
        }
    }

    IEnumerator BossEyeFireRoutine()
    {
        while (_isActive == true && _isFacingUp == true && _player != null)
        {
            _canFire = true;

            _fireRate = 0.5f;

            Vector3 rightOffset = new Vector3(0.4f, -0.6f, 0);
            Vector3 leftOffset = new Vector3(-0.4f, -0.6f, 0);

            Instantiate(_bossEyeLaserPrefab, transform.position + rightOffset, Quaternion.identity);
            Instantiate(_bossEyeLaserPrefab, transform.position + leftOffset, Quaternion.identity);

            PlayAudio(_eyeShotClip);

            yield return new WaitForSeconds(_fireRate);

            _canFire = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && _player != null && _startAttack == true)
        {
            switch (other.tag)
            {
                case "Player":

                    StartCoroutine(DamagePlayerRoutine());

                    break;

                case "Laser":

                    int damage = Random.Range(5, 10);

                    HeadDamage(damage);

                    _player.AddToScore(damage);

                    Destroy(other.gameObject);

                    break;

                default:

                    break;
            }
        }
    }

    IEnumerator DamagePlayerRoutine()
    {
        float damageCooldown = 1.5f;

        if (_playerHit == false)
        {
            _playerHit = true;

            _player.DeductFromScore(20);

            _player.Damage();

            yield return new WaitForSeconds(damageCooldown);

            _playerHit = false;
        }
    }

    private void HeadDamage(int damage)
    {
        float damageThreshold = _headMaxHealth * 0.9f;
        float firstCheck = _headMaxHealth * 0.75f;
        float secondCheck = _headMaxHealth * 0.5f;
        float thirdCheck = _headMaxHealth * 0.25f;

        _headCurrentHealth -= damage;

        _player.AddToScore(damage);

        if (_headCurrentHealth >= firstCheck && _headCurrentHealth < damageThreshold && _damageChecks[0] == false)
        {
            _damageChecks[0] = true;

            DamageVisualizer();
        }
        else if (_headCurrentHealth >= secondCheck && _headCurrentHealth < firstCheck && _damageChecks[1] == false)
        {
            _damageChecks[1] = true;

            DamageVisualizer();
        }
        else if (_headCurrentHealth >= thirdCheck && _headCurrentHealth < secondCheck && _damageChecks[2] == false)
        {
            _damageChecks[2] = true;

            DamageVisualizer();
        }
        else if (_headCurrentHealth < thirdCheck && _headCurrentHealth > 0 && _damageChecks[3] == false)
        {
            _damageChecks[3] = true;

            PlayAudio(_keepFiringClip);

            DamageVisualizer();
        }

        if (_headCurrentHealth <= 0)
        {
            _headCurrentHealth = 0;

            _isActive = false;
        }

        PlayAudio(_laserHitClip);

        Debug.Log(_headCurrentHealth);
    }

    private void DamageVisualizer()
    {
        if (_damageList.Count.Equals(_damagePositions.Length))
        {
            return;
        }
        else
        {
            int randomPosition = Random.Range(0, _damagePositions.Length);

            while (_damageList.Contains(_damagePositions[randomPosition]))
            {
                randomPosition = Random.Range(0, _damagePositions.Length);
            }

            _damageList.Add(_damagePositions[randomPosition]);

            GameObject newDamage = Instantiate(_damagePrefab, _damagePositions[randomPosition], Quaternion.Euler(0, 0, 180f));

            newDamage.transform.SetParent(_damageContainer.transform, false);
        }
    }

    private void OnBossDeath()
    {
        transform.Rotate(Vector3.back * _rotationSpeed * Time.deltaTime);

        transform.Translate(Vector3.up * 2f * Time.deltaTime, Space.World);

        if (transform.position.y > 12f)
        {
            _uIManager.PlayerVictorySequence();

            Destroy(this.gameObject);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, _bossMinScale, 0.5f * Time.deltaTime);

        if (_onDeath == false)
        {
            _onDeath = true;

            StartCoroutine(OnBossDeathRoutine());

            _megaArm.GetComponent<ArmBehavior>().OnBossDeath();
        }
    }

    IEnumerator OnBossDeathRoutine()
    {
        gameObject.GetComponent<Collider2D>().enabled = false;

        for (int i = 0; i < _damagePositions.Length; i++)
        {
            GameObject explosion = Instantiate(_explosionPrefab, _damagePositions[i], Quaternion.identity);

            explosion.transform.localScale = new Vector3(0.25f, 0.25f, 0);

            explosion.transform.SetParent(_damageContainer.transform, false);

            yield return new WaitForSeconds(0.25f);
        }

        PlayAudio(_onDestroyClip);
    }

    private void PlayerDefaultVelocity()
    {
        if (_player != null)
        {

            if (_playerRGB.velocity != Vector2.zero)
            {
                _playerRGB.velocity = Vector2.zero;
            }

            if (_playerRGB.angularVelocity != 0)
            {
                _playerRGB.angularVelocity = 0;
            }
        }
    }
}
