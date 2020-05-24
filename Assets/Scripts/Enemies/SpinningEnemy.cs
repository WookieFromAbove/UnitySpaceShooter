using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningEnemy : MonoBehaviour
{
    [SerializeField]
    private float _enemySpeed = 3f;

    [SerializeField]
    private int _enemyID = 2;

    private Player _player;
    private SpawnManager _spawnManager;

    private Animator _enemyAnim;

    [SerializeField]
    private AudioClip _explosionClip;

    private AudioSource _enemyAudio;

    private bool _isEnemyDead = false;

    [SerializeField]
    private Collider2D[] _spinningFlameCollider;

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
    }

    void Update()
    {
        CalculateMovement();
    }

    private void CalculateMovement()
    {
        transform.Translate(Vector3.down * _enemySpeed * Time.deltaTime, Space.World);

        int rotationSpeed = 65;

        transform.Rotate(Vector3.back * rotationSpeed * Time.deltaTime);

        foreach (var fire in _spinningFlameCollider)
        {
            if (fire != null && fire.enabled == false)
            {
                fire.gameObject.SetActive(true);
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
            }
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
        _enemySpeed /= 2f;

        _isEnemyDead = true;

        _enemyAnim.SetTrigger("OnEnemyDeath");

        EnemyExplosionAudio();

        gameObject.GetComponent<Collider2D>().enabled = false;

        _spawnManager.CalculateEnemiesRemaining();

        foreach (var fire in _spinningFlameCollider)
        {
            if (fire != null)
            {
                Destroy(fire.gameObject);
            }
        }

        yield return new WaitForSeconds(2.5f);

        Destroy(this.gameObject);
    }

    private void EnemyExplosionAudio()
    {
        _enemyAudio.clip = _explosionClip;
        _enemyAudio.Play();
    }
}
