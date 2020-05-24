using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetDruidia : MonoBehaviour
{
    private Vector3 _startPosition = new Vector3(0.6f, -11.5f, 0);

    private Vector3 _secondaryPosition = new Vector3(0.6f, -14f, 0);

    public bool _canMove = false;

    [SerializeField]
    private float _rotationSpeed = 1.5f;

    private VacuumBehavior _megaVacuum;

    [SerializeField]
    private GameObject _megaHead;
    [SerializeField]
    private GameObject _megaArm;

    void Start()
    {
        if (_megaVacuum != null)
        {
            _megaVacuum = GameObject.Find("MegaVacuum").GetComponent<VacuumBehavior>();
            if (_megaVacuum == null)
            {
                Debug.Log("MegaVacuum is NULL.");
            }
        }

        transform.position = _startPosition;
    }

    void Update()
    {
        CalculateRotation();

        if (_canMove == true)
        {
            MoveToSecondaryPos();
        }
    }

    private void CalculateRotation()
    {
        transform.Rotate(Vector3.back * _rotationSpeed * Time.deltaTime);
    }

    private void MoveToSecondaryPos()
    {
        _canMove = false;

        StartCoroutine(MoveToSecondaryPosRoutine());
    }

    IEnumerator MoveToSecondaryPosRoutine()
    {
        float waitTime = 0.04f;
        float speed = 1f;
        float cruiseSpeed = speed * Time.deltaTime;

        bool _isClose = false;

        yield return new WaitForSeconds(1f);

        while (transform.position != _secondaryPosition && _isClose == false)
        {
            yield return new WaitForSeconds(waitTime);

            transform.position = Vector3.Lerp(transform.position, _secondaryPosition, cruiseSpeed);

            if (transform.position.y <= -13.75f)
            {
                _isClose = true;
            }
        }

        yield return null;
    }
}
