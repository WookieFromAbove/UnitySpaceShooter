using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField]
    private float _rotationSpeed = 3f;

    [SerializeField]
    private GameObject _explosionPrefab;

    private SpawnManager _spawnManager;

    void Start()
    {
        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        if (_spawnManager == null)
        {
            Debug.Log("SpawnManager is NULL.");
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.back * _rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Laser"))
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

            Destroy(other.gameObject);

            gameObject.GetComponent<Collider2D>().enabled = false;

            _spawnManager.StartSpawning();

            Destroy(this.gameObject, 0.35f);
        }
    }
}
