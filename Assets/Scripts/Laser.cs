using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _laserSpeed = 8f;

    [SerializeField]
    private bool _isPlayerLaser = true;

    void Update()
    {
        if (_isPlayerLaser == true)
        {
            CalculatePlayerFire();
        }
        else if (_isPlayerLaser == false)
        {
            CalculateEnemyFire();
        }
        else
        {
            Debug.LogError("Assign laser.");
        }
    }

    void CalculatePlayerFire()
    {
        transform.Translate(Vector3.up * _laserSpeed * Time.deltaTime);

        if (transform.position.y > 8f)
        {
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }

            Destroy(this.gameObject);
        }
    }

    void CalculateEnemyFire()
    {
        transform.Translate(Vector3.down * _laserSpeed * Time.deltaTime);

        if (transform.position.y < -6f)
        {
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }

            Destroy(this.gameObject);
        }
    }
}
