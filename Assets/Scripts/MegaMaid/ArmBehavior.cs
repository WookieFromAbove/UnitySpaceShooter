using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class ArmBehavior : MonoBehaviour
{
    [SerializeField]
    private float _armSpeed = 4f;

    [SerializeField]
    private float _rotationSpeed = 50f;

    private Vector3 _startPosition;

    private bool _isSpawned = false;
    private float _randomYSpawn;
    private float _maxXPosition = 13f;

    private Player _player;
    private bool _playerHit = false;

    private bool _isDead = false;

    [SerializeField]
    private GameObject _explosionPrefab;

    void Start()
    {
        CalculateStartPosition();

        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.Log("Player is NULL.");
        }
    }

    void Update()
    {
        if (_isDead == false)
        {
            CalculateMovement();

            CalculateRotation();
        }
    }

    private void CalculateStartPosition()
    {
        _randomYSpawn = Random.Range(-2.65f, 4.6f);
        _startPosition = new Vector3(-13f, _randomYSpawn, 0);

        transform.position = _startPosition;

        _isSpawned = true;
    }

    private void CalculateMovement()
    {
        transform.Translate(Vector3.right * _armSpeed * Time.deltaTime, Space.World);

        if (transform.position.x > _maxXPosition)
        {
            if (_isSpawned == true)
            {
                StartCoroutine(RespawnArmRoutine());
            }
        }
    }

    IEnumerator RespawnArmRoutine()
    {
        _isSpawned = false;

        float randomWait = Random.Range(2, 4);

        yield return new WaitForSeconds(randomWait);

        _randomYSpawn = Random.Range(-2.65f, 4.6f);

        transform.position = new Vector3(-13f, _randomYSpawn, 0);

        _isSpawned = true;
    }

    private void CalculateRotation()
    {
        transform.Rotate(Vector3.back * _rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null)
        {
            switch (other.tag)
            {
                case "Player":

                    if (_playerHit == false)
                    {
                        StartCoroutine(DamagePlayerRoutine());
                    }
                    // play sound?

                    break;

                case "Laser":

                    Destroy(other.gameObject);

                    break;

                default:

                    break;
            }
        }
    }

    IEnumerator DamagePlayerRoutine()
    {
        _playerHit = true;

        float damageCooldown = 1f;

        _player.DeductFromScore(15);

        _player.Damage();

        yield return new WaitForSeconds(damageCooldown);

        _playerHit = false;
    }

    public void OnBossDeath()
    {
        _isDead = true;

        StartCoroutine(OnBossDeathRoutine());
    }

    IEnumerator OnBossDeathRoutine()
    {
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

        gameObject.GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(0.35f);

        Destroy(this.gameObject);
    }
}
