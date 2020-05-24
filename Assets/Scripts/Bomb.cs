using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private Vector3 _spawnPosition = new Vector3(0, 0, 0);

    private Rigidbody2D _bombRigidbody;

    [SerializeField]
    private float _bombSpeed = 7f;

    private Quaternion _bombRotation;

    private bool _rotateBombToEnemy = false;

    [SerializeField]
    private float _rotationSpeed = 80f;
    private float _radius = 1.7f;

    [SerializeField]
    private bool _beginRotation = false;

    [SerializeField]
    private bool _calculateFire = false;

    private List<GameObject> _activeEnemyList = new List<GameObject>();
    private GameObject _closestEnemy = null;

    private Vector3 _targetEnemyVector;

    [SerializeField]
    private GameObject _bombThruster;
    [SerializeField]
    private GameObject _bombThrusterToTarget;


    private Player _player;

    void Start()
    {
        transform.localPosition = _spawnPosition;

        _bombRigidbody = GetComponent<Rigidbody2D>();
        _bombRigidbody.bodyType = RigidbodyType2D.Kinematic;

        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.Log("Player is NULL.");
        }
    }

    void Update()
    {
        if (_beginRotation == false)
        {
            StartCoroutine(GoToRotatePosition());
        }
        else if (_beginRotation == true)
        {
            RotateAroundPlayer();
        }

        if (_calculateFire == true)
        {
            _bombThruster.SetActive(false);
            _bombThrusterToTarget.SetActive(true);

            CalculateFire();

            if (_rotateBombToEnemy == true)
            {
                _bombRotation = Quaternion.LookRotation(transform.forward, _closestEnemy.transform.position);

                transform.localRotation = _bombRotation;
            }
        }
    }

    IEnumerator GoToRotatePosition()
    {
        float waitTime = 0.04f;
        float cruiseSpeed = 1.5f * Time.deltaTime;
        Vector3 rotationPosition = new Vector3(0, 1.75f, 0);

        while (transform.localPosition != rotationPosition && _beginRotation == false)
        {
            yield return new WaitForSeconds(waitTime);

            transform.localPosition = Vector3.Lerp(transform.localPosition, rotationPosition, cruiseSpeed);

            if (transform.localPosition.y >= _radius)
            {
                _beginRotation = true;
            }
        }

        yield return null;
    }

    private void RotateAroundPlayer()
    {
        if (_calculateFire == false)
        {
            Transform playerTransform = _player.transform;
            Vector3 rotationAxis = Vector3.back;
            Vector3 rotationPosition;
            float radiusSpeed = 0.5f;

            // check bomb rotation when player spinning
            transform.position = (transform.position - playerTransform.position).normalized * _radius + playerTransform.position;

            transform.RotateAround(playerTransform.position, rotationAxis, _rotationSpeed * Time.deltaTime);

            rotationPosition = (transform.position - playerTransform.position).normalized * _radius + playerTransform.position;

            transform.position = Vector3.MoveTowards(transform.position, rotationPosition, radiusSpeed * Time.deltaTime);
        }
    }

    private void ActiveEnemyList()
    {
        GameObject[] activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in activeEnemies)
        {
            _activeEnemyList.Add(enemy);
        }
    }

    private void FindClosestEnemy()
    {
        _activeEnemyList.Clear();

        ActiveEnemyList();

        float distance = Mathf.Infinity;

        foreach (GameObject enemy in _activeEnemyList)
        {
            Vector3 enemyDistance = enemy.transform.position - transform.position;
            float currentDistance = enemyDistance.sqrMagnitude;
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();

            if (currentDistance < distance && enemyCollider.enabled == true)
            {
                _closestEnemy = enemy;

                distance = currentDistance;
            }
        }
    }

    public void FireAtEnemy()
    {
        _calculateFire = true;
    }

    private void CalculateFire()
    {
        FindClosestEnemy();

        if (_closestEnemy != null)
        {
            _targetEnemyVector = _closestEnemy.transform.position;

            transform.position = Vector3.MoveTowards(transform.position, _targetEnemyVector, _bombSpeed * Time.deltaTime);

            _rotateBombToEnemy = true;

            if (transform.position.y > 8f || transform.position.y < -5.5f)
            {
                Destroy(this.gameObject);
            }

            if (transform.position.x > 12f || transform.position.x < -12f)
            {
                Destroy(this.gameObject);
            }
        }
        else
        {
            transform.Translate(Vector3.up * _bombSpeed * Time.deltaTime);

            if (transform.position.y > 8f || transform.position.y < -5.5f)
            {
                Destroy(this.gameObject);
            }

            if (transform.position.x > 12f || transform.position.x < -12f)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
