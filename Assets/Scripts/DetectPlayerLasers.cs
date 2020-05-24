using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectPlayerLasers : MonoBehaviour
{
    private BigGunEnemy _bigGunEnemy;

    void Start()
    {
        _bigGunEnemy = gameObject.GetComponentInParent<BigGunEnemy>();
        if (_bigGunEnemy == null)
        {
            Debug.Log("Parent is NULL.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_bigGunEnemy._randomAvoid == 0)
        {
            if (other != null && other.CompareTag("Laser"))
            {
                _bigGunEnemy.DodgeLaser();
            }
        }
    }
}
