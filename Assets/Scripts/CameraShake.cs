using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 _cameraPosition;
    private float _cameraPositionZ;

    private Player _player;

    // Start is called before the first frame update
    void Start()
    {
        _cameraPosition = transform.position;
        _cameraPositionZ = transform.position.z;

        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.Log("Player is NULL.");
        }
    }

    // shake camera if shield !active
    public void PlayerHitShake()
    {
        StartCoroutine(CameraShakeRoutine(0.15f, 0.2f));
    }


    // shake camera if shield active
    public void ShieldHitShake()
    {
        StartCoroutine(CameraShakeRoutine(0.1f, 0.2f));
    }

    IEnumerator CameraShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            float cameraX = Random.Range(-0.5f, 0.5f) * magnitude;
            float cameraY = Random.Range(-0.5f, 0.5f) * magnitude;

            transform.position = new Vector3(cameraX, cameraY, _cameraPositionZ);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.position = Vector3.Lerp(transform.position, _cameraPosition, 1f);
    }
}
