using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLaser : MonoBehaviour
{
    [SerializeField]
    private float _laserSpeed;

    void Update()
    {
        CalculateBossFire();
    }

    private void CalculateBossFire()
    {
        transform.Translate(Vector3.up * _laserSpeed * Time.deltaTime, Space.Self);

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
