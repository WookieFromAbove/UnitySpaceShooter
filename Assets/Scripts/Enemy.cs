using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _enemySpeed = 4f;
    private float _speedIncrease = 1.25f;

    [SerializeField]
    private int _enemyID; // 0 = Ram, 1 = BigGun, 2 = Spin

    private Player _player;

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
    private AudioClip _enemyWeaponsClip;

    private void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.Log("Player is NULL.");
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
    }

    void Update()
    {
        CalculateMovement();

        CalculateEnemyFire();
    }

    private void CalculateMovement()
    {
        transform.Translate(Vector3.down * _enemySpeed * Time.deltaTime, Space.World);

        if (_enemyID == 2)
        {
            int rotationSpeed = 65;

            transform.Rotate(Vector3.back * rotationSpeed * Time.deltaTime);
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
            }
        }
    }

    private void CalculateEnemyFire()
    {
        if (_enemyID == 1 && _canFire == false)
        {
            _enemyLaserOffset = new Vector3(0.025f, -0.9f, 0);

            StartCoroutine(EnemyFireRoutine());
        }
        else
        {
            return;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_player != null)
            {
                _player.DeductFromScore(5);

                _player.Damage();
            }

            StartCoroutine(OnEnemyDeathRoutine());
        }
        else if (other.CompareTag("Laser"))
        {
            if (_player != null)
            {
                _player.AddToScore(10);
            }

            Destroy(other.gameObject);

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
}
