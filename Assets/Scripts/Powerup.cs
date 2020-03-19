using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private float _powerupSpeed = 3f;

    [SerializeField]
    private int _powerupID; // 0 = tripple, 1 = health, 2 = speed, 3 = shield, 4 = ammo, 5 = bomb

    [SerializeField]
    private AudioClip _powerupClip;
    
    void Update()
    {
        transform.Translate(Vector3.down * _powerupSpeed * Time.deltaTime, Space.World);

        if (_powerupID == 5)
        {
            int rotationSpeed = 50;

            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }

        if (transform.position.y < -5.5f)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                switch(_powerupID)
                {
                    case 0:
                        player.TrippleShotActive();
                        break;
                    case 1:
                        player.GainLife();
                        break;
                    case 2:
                        player.SpeedBoostActive(); 
                        break;
                    case 3:
                        player.ShieldActive();
                        break;
                    case 4:
                        player.ReloadAmmo();
                        break;
                    case 5:
                        player.BombActive();
                        break;
                }
            }

            PlayPowerupAudio();

            Destroy(this.gameObject);
        }
    }

    public void PlayPowerupAudio()
    {
        Vector3 powerupAudioPosition = new Vector3(0, 1, -10); // position of main camera

        AudioSource.PlayClipAtPoint(_powerupClip, powerupAudioPosition);
    }
}
