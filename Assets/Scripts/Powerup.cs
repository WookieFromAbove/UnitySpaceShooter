using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private float _powerupSpeed = 3f;

    [SerializeField]
    private int _powerupID; // 0 = tripple, 1 = health, 2 = speed, 3 = shield, 4 = ammo, 5 = bomb, 6 = spin (negative)

    [SerializeField]
    private AudioClip _powerupClip;

    [SerializeField]
    private AudioClip _powerupExplosionClip;

    private Player _player;
    public bool _moveToPlayer = false;

    private void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.Log("Player is null.");
        }
    }

    void Update()
    {
        if (_moveToPlayer == false)
        {
            transform.Translate(Vector3.down * _powerupSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            float boostedSpeed = 8f;

            transform.position = Vector3.MoveTowards(transform.position, _player.transform.position, boostedSpeed * Time.deltaTime);
        }

        if (_powerupID == 5)
        {
            int rotationSpeed = 50;

            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }

        if (_powerupID == 6)
        {
            float rotationSpeed = 3f;
            float maxRotation = 45f;

            transform.rotation = Quaternion.Euler(0, 0, maxRotation * Mathf.Sin(Time.time * rotationSpeed));
        }

        if (transform.position.y < -5.5f)
        {
            Destroy(this.gameObject);
        }
    }

    public void MoveToPlayer()
    {
        if (_player != null)
        {
            _moveToPlayer = true;
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
                    case 6:
                        player.SpinPlayer();
                        break;
                }
            }

            PlayPowerupAudio(_powerupClip);

            Destroy(this.gameObject);
        }
        else if (other.CompareTag("EnemyPowerupShot") && _powerupID != 6) // neg powerup
        {
            PlayPowerupAudio(_powerupExplosionClip);

            Destroy(other.gameObject);

            Destroy(this.gameObject);
        }
    }

    public void PlayPowerupAudio(AudioClip clip)
    {
        Vector3 powerupAudioPosition = new Vector3(0, 1, -10); // position of main camera

        AudioSource.PlayClipAtPoint(clip, powerupAudioPosition);
    }
}
