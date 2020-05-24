using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFireParticleSystem : MonoBehaviour
{
    private Player _player;

    [SerializeField]   
    private GameObject _flame;
    private Collider2D _flameCollider;

    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.Log("Player is NULL.");
        }

        _flameCollider = GetComponent<Collider2D>();
        if (_flameCollider == null)
        {
            Debug.Log("Flame collider is null.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && _player != null)
        {
            if (other.CompareTag("Player"))
            {
                _player.DeductFromScore(5);

                _player.Damage();

                StartCoroutine(FlameCooldownRoutine());
            }
        }
    }

    IEnumerator FlameCooldownRoutine()
    {
        _flameCollider.enabled = false;
        _flame.SetActive(false);

        yield return new WaitForSeconds(3f);

        _flame.SetActive(true);
        _flameCollider.enabled = true;
    }
}