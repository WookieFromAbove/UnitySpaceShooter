using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEyeLaser : MonoBehaviour
{
    [SerializeField]
    private float _laserSpeed = 4f;

    private Player _player;

    private bool _isFired = false;

    private Vector3 _playerPosition;
    private Vector3 _targetVector;

    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        {
            if (_player == null)
            {
                Debug.Log("Player is NULL.");
            }
        }
    }

    void Update()
    {
        if (_isFired == false)
        {
            CalculateDirectionToFire();
        }
        else
        {
            CalculateFire();
        }
    }

    private void CalculateDirectionToFire()
    {
        _isFired = true;

        if (_player != null)
        {
            _playerPosition = _player.transform.position;

            _targetVector = (_playerPosition - transform.position).normalized;
        }
    }

    private void CalculateFire()
    {
        if (_player != null)
        {
            transform.Translate(_targetVector * _laserSpeed * Time.deltaTime);
        }
        else
        {
            Destroy(this.gameObject);
        }

        if (transform.position.y > 8f || transform.position.y < -6f)
        {
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }

            Destroy(this.gameObject);
        }
        else if (transform.position.x > 10f || transform.position.x < -10f)
        {
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }

            Destroy(this.gameObject);
        }
    }
}
