using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;

public class VacuumBehavior : MonoBehaviour
{
    [SerializeField]
    private float _vacuumSpeed = 3f;

    [SerializeField]
    private float _vacuumPower = 2.5f;

    [SerializeField]
    private int _vacMaxHealth = 50;
    private int _vacCurrentHealth;

    private Vector3 _startPosition = new Vector3(0, 11, 0);
    private Quaternion _defaultRotation = Quaternion.Euler(Vector3.zero);
    private bool _isActive = false;
    public bool _isAlive = false;
    private bool _vacuumStartAttack = false;
    private bool _isAttacking = false;

    private AudioSource _vacAudioSource;

    [SerializeField]
    private AudioClip _vacuSuckClip;
    [SerializeField]
    private AudioClip _laserHitClip;

    private int _randomAttackID;

    private bool _playerInRange = false;
    private bool _playerHit = false;

    // CENTER ATTACK
    private int _attackCenterID = 0;
    private Vector3 _centerWaitPosition = new Vector3(0, 6.5f, 0);
    private Vector3 _centerAttackPosition = new Vector3(0, 4, 0);

    // LEFT ATTACK
    private int _attackLeftID = 1;
    private Vector3 _leftWaitPosition = new Vector3(-5.5f, 6.5f, 0);
    private Vector3 _leftAttackPosition = new Vector3(-5.5f, 3.5f, 0); 
    private Quaternion _leftTargetRotation = Quaternion.Euler(0, 0, 19.5f);

    // RIGHT ATTACK
    private int _attackRightID = 2;
    private Vector3 _rightWaitPosition = new Vector3(5.5f, 6.5f, 0);
    private Vector3 _rightAttackPosition = new Vector3(5.5f, 3.5f, 0); 
    private Quaternion _rightTargetRotation = Quaternion.Euler(0, 0, -19.5f);

    [SerializeField]
    private GameObject _target;

    [SerializeField]
    private GameObject _vacuumOffset;
    private Transform _vacTransform;
    private Animator _vacuumAnim;

    [SerializeField]
    private GameObject _vacuShield;

    private Player _player;
    private Rigidbody2D _playerRGB;

    [SerializeField]
    private GameObject _damageContainer;

    [SerializeField]
    private GameObject _damagePrefab;

    [SerializeField]
    private Vector3[] _damagePositions;
    private List<Vector3> _damageList = new List<Vector3>();

    private bool[] _damageChecks = new bool[4];

    [SerializeField]
    private GameObject _vacExplosionPrefab;

    private SpawnManager _spawnManager;

    [SerializeField]
    private GameObject _megaHead;
    [SerializeField]
    private GameObject _megaArm;

    void Start()
    {
        _isAlive = true;

        _vacTransform = _vacuumOffset.transform;

        _vacuumAnim = _vacuumOffset.GetComponent<Animator>();
        if (_vacuumAnim == null)
        {
            Debug.Log("VacuumAnim is NULL.");
        }

        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.Log("Player is NULL.");
        }

        _playerRGB = _player.GetComponent<Rigidbody2D>();
        if (_playerRGB == null)
        {
            Debug.Log("Player Rigidbody is NULL.");
        }

        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        if (_spawnManager == null)
        {
            Debug.Log("SpawnManager is NULL.");
        }

        _vacAudioSource = GetComponent<AudioSource>();
        if (_vacAudioSource == null)
        {
            Debug.Log("AudioSource is NULL.");
        }

        transform.position = _startPosition;

        transform.rotation = _defaultRotation;

        _vacCurrentHealth = _vacMaxHealth;

        _randomAttackID = Random.Range(0, 3);

        for (int i = 0; i < _damageChecks.Length; i++)
        {
            _damageChecks[i] = false;
        }
    }

    void Update()
    {
        if (_isActive == false)
        {
            MoveToStart();
        }

        if (_vacuumStartAttack == true)
        {
            CalculateAttack();
        }

        if (_isAttacking == true)
        {
            OperationVacuSuck();

            if (_playerInRange == true)
            {
                CalculatePlayerInRange();
            }
            else
            {
                PlayerDefaultVelocity();
            }
        }
        else
        {
            PlayerDefaultVelocity();
            
            if (_vacuShield.activeInHierarchy == false)
            {
                _vacuShield.SetActive(true);
            }
        }

        if (_isAlive == false)
        {
            PlayerDefaultVelocity();
        }
    }

    private void MoveToStart()
    {
        //_isActive = true;

        //StartCoroutine(MoveToStartRoutine());

        transform.position = Vector3.MoveTowards(transform.position, _centerWaitPosition, 1f * Time.deltaTime);

        if (transform.position.y == _centerWaitPosition.y)
        {
            _isActive = true;
            _vacuumStartAttack = true;

            return;
        }
    }

    IEnumerator MoveToStartRoutine()
    {
        float waitTime = 0.04f;
        float speed = 1f;
        float cruiseSpeed = speed * Time.deltaTime;
        Vector3 startPosition = new Vector3(0, 6.5f, 0);

        while (transform.position != startPosition && _vacuumStartAttack == false)
        {
            yield return new WaitForSeconds(waitTime);

            transform.position = Vector3.Lerp(transform.position, startPosition, cruiseSpeed);

            if (transform.position.y <= 6.75f)
            {
                _vacuumStartAttack = true;
            }
        }

        yield return null;
    }

    private void PlayAudio(AudioClip clip)
    {
        Vector3 audioPosition = new Vector3(0, 1, -10); // position of main camera

        AudioSource.PlayClipAtPoint(clip, audioPosition);
    }

    private void CalculateAttack()
    {
        _vacuumStartAttack = false;

        StartCoroutine(VacuumAttackRoutine());
    }
    
    IEnumerator VacuumAttackRoutine()
    {
        float waitTime = 0.02f;
        float attackTime = 4f;
        float timeToNextTarget = 2f;

        float wiggleRoomY = 0.2f;

        bool moveUp = false;

        while (_isActive == true)
        {
            // CENTER ATTACK
            if (_randomAttackID == _attackCenterID)
            {
                if (_isAttacking == false)  // move from start pos y to 6.5
                {
                    yield return new WaitForSeconds(waitTime);

                    transform.position = Vector3.MoveTowards(transform.position, _centerWaitPosition, _vacuumSpeed * Time.deltaTime); 

                    if (moveUp == false)
                    {
                        if (Vector3.Distance(transform.position, _centerWaitPosition) <= wiggleRoomY)  
                        {
                            _isAttacking = true;
                        }
                    }
                    else if (moveUp == true)
                    {
                        if (Vector3.Distance(transform.position, _centerWaitPosition) <= wiggleRoomY)
                        {
                            moveUp = false;

                            yield return new WaitForSeconds(timeToNextTarget);

                            while(_randomAttackID == _attackCenterID)
                            {
                                _randomAttackID = Random.Range(0, 3);
                            }
                        }
                    }
                }
                else
                {
                    yield return new WaitForSeconds(waitTime); // move y from 6.5 to 4

                    transform.position = Vector3.MoveTowards(transform.position, _centerAttackPosition, _vacuumSpeed * Time.deltaTime);  

                    if (Vector3.Distance(transform.position, _centerAttackPosition) <= wiggleRoomY)
                    {
                        PlayAudio(_vacuSuckClip);

                        if (_vacuShield.activeInHierarchy == true)
                        {
                            _vacuShield.SetActive(false);
                        }

                        yield return new WaitForSeconds(attackTime);

                        if (_vacuShield.activeInHierarchy == false)
                        {
                            _vacuShield.SetActive(true);
                        }

                        _isAttacking = false;

                        if (_vacuumAnim.GetBool("VacuSuckActive") == true)
                        {
                            _vacuumAnim.SetBool("VacuSuckActive", false);
                        }

                        moveUp = true;
                    }
                }
            }

            // LEFT ATTACK
            if (_randomAttackID == _attackLeftID)
            {
                if (_isAttacking == false)  // move from start pos y to 6.5
                {
                    yield return new WaitForSeconds(waitTime);

                    transform.position = Vector3.MoveTowards(transform.position, _leftWaitPosition, _vacuumSpeed * Time.deltaTime);

                    if (moveUp == false)
                    {
                        if (Vector3.Distance(transform.position, _leftWaitPosition) <= wiggleRoomY) // < 6.7
                        {
                            _isAttacking = true;
                        }
                    }
                    else if (moveUp == true)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, _defaultRotation, _vacuumSpeed * Time.deltaTime);

                        if (Vector3.Distance(transform.position, _leftWaitPosition) <= wiggleRoomY)
                        {
                            transform.rotation = _defaultRotation;

                            moveUp = false;

                            yield return new WaitForSeconds(timeToNextTarget);

                            while (_randomAttackID == _attackLeftID)
                            {
                                _randomAttackID = Random.Range(0, 3);
                            }
                        }
                    }
                }
                else
                {
                    yield return new WaitForSeconds(waitTime); // move y from 6.5 to 4

                    transform.position = Vector3.MoveTowards(transform.position, _leftAttackPosition, _vacuumSpeed * Time.deltaTime);

                    transform.rotation = Quaternion.Slerp(transform.rotation, _leftTargetRotation, _vacuumSpeed * Time.deltaTime);

                    if (Vector3.Distance(transform.position, _leftAttackPosition) <= wiggleRoomY) // < 4.2
                    {
                        PlayAudio(_vacuSuckClip);

                        if (_vacuShield.activeInHierarchy == true)
                        {
                            _vacuShield.SetActive(false);
                        }

                        yield return new WaitForSeconds(attackTime);

                        if (_vacuShield.activeInHierarchy == false)
                        {
                            _vacuShield.SetActive(true);
                        }

                        _isAttacking = false;

                        if (_vacuumAnim.GetBool("VacuSuckActive") == true)
                        {
                            _vacuumAnim.SetBool("VacuSuckActive", false);
                        }

                        moveUp = true;
                    }
                }
            }

            // RIGHT ATTACK
            if (_randomAttackID == _attackRightID)
            {
                if (_isAttacking == false)  // move from start pos y to 6.5
                {
                    yield return new WaitForSeconds(waitTime);

                    transform.position = Vector3.MoveTowards(transform.position, _rightWaitPosition, _vacuumSpeed * Time.deltaTime);

                    if (moveUp == false)
                    {
                        if (Vector3.Distance(transform.position, _rightWaitPosition) <= wiggleRoomY) // < 6.7
                        {
                            _isAttacking = true;
                        }
                    }
                    else if (moveUp == true)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, _defaultRotation, _vacuumSpeed * Time.deltaTime);

                        if (Vector3.Distance(transform.position, _rightWaitPosition) <= wiggleRoomY)
                        {
                            transform.rotation = _defaultRotation;

                            moveUp = false;

                            yield return new WaitForSeconds(timeToNextTarget);

                            while (_randomAttackID == _attackRightID)
                            {
                                _randomAttackID = Random.Range(0, 3);
                            }
                        }
                    }
                }
                else
                {
                    yield return new WaitForSeconds(waitTime); // move y from 6.5 to 4

                    transform.position = Vector3.MoveTowards(transform.position, _rightAttackPosition, _vacuumSpeed * Time.deltaTime);

                    transform.rotation = Quaternion.Slerp(transform.rotation, _rightTargetRotation, _vacuumSpeed * Time.deltaTime);

                    if (Vector3.Distance(transform.position, _rightAttackPosition) <= wiggleRoomY) // < 4.2
                    {
                        PlayAudio(_vacuSuckClip);

                        if (_vacuShield.activeInHierarchy == true)
                        {
                            _vacuShield.SetActive(false);
                        }

                        yield return new WaitForSeconds(attackTime);

                        if (_vacuShield.activeInHierarchy == false)
                        {
                            _vacuShield.SetActive(true);
                        }

                        _isAttacking = false;

                        if (_vacuumAnim.GetBool("VacuSuckActive") == true)
                        {
                            _vacuumAnim.SetBool("VacuSuckActive", false);
                        }

                        moveUp = true;
                    }
                }
            }
        }
    }

    private void OperationVacuSuck()
    {
        if (_vacuumAnim.GetBool("VacuSuckActive") == false)
        {
            _vacuumAnim.SetBool("VacuSuckActive", true);
        }

        float detectHit = 5.5f;

        RaycastHit2D[] forwardHit = Physics2D.RaycastAll(_vacTransform.position, -_vacTransform.up, detectHit);
        Debug.DrawRay(_vacTransform.position, -_vacTransform.up * detectHit, Color.green);

        foreach (RaycastHit2D obj in forwardHit)
        {
            if (obj.transform.CompareTag("Player"))
            {
                SuckUpPlayer();
            }
        }

        Vector2 leftHalf = (-_vacTransform.up - _vacTransform.right).normalized;

        RaycastHit2D[] leftHalfHit = Physics2D.RaycastAll(_vacTransform.position, leftHalf, detectHit);
        Debug.DrawRay(_vacTransform.position, leftHalf * detectHit, Color.yellow);

        foreach (RaycastHit2D obj in leftHalfHit)
        {
            if (obj.transform.CompareTag("Player"))
            {
                SuckUpPlayer();
            }
        }

        Vector2 leftQtr = Quaternion.Euler(0, 0, -22.5f) * -_vacTransform.up;

        RaycastHit2D[] leftQtrHit = Physics2D.RaycastAll(_vacTransform.position, leftQtr, detectHit);
        Debug.DrawRay(_vacTransform.position, leftQtr * detectHit, Color.cyan);

        foreach (RaycastHit2D obj in leftQtrHit)
        {
            if (obj.transform.CompareTag("Player"))
            {
                SuckUpPlayer();
            }
        }

        Vector2 rightHalf = (-_vacTransform.up + _vacTransform.right).normalized;

        RaycastHit2D[] rightHalfHit = Physics2D.RaycastAll(_vacTransform.position, rightHalf, detectHit);
        Debug.DrawRay(_vacTransform.position, rightHalf * detectHit, Color.red);

        foreach (RaycastHit2D obj in rightHalfHit)
        {
            if (obj.transform.CompareTag("Player"))
            {
                SuckUpPlayer();
            }
        }

        Vector2 rightQtr = Quaternion.Euler(0, 0, 22.5f) * -_vacTransform.up;

        RaycastHit2D[] rightQtrHit = Physics2D.RaycastAll(_vacTransform.position, rightQtr, detectHit);
        Debug.DrawRay(_vacTransform.position, rightQtr * detectHit, Color.blue);

        foreach (RaycastHit2D obj in rightQtrHit)
        {
            if (obj.transform.CompareTag("Player"))
            {
                SuckUpPlayer();
            }
        }
    }

    private void SuckUpPlayer()
    {
        _playerInRange = true;

        Vector3 dir = _vacTransform.position - _player.transform.position;
        dir = dir.normalized;

        _playerRGB.AddForce(dir * _vacuumPower);
    }

    private void CalculatePlayerInRange()
    {
        Vector3 playerPos = _player.transform.position;

        float maxY = _vacTransform.position.y + 0.5f;
        float maxX = _vacTransform.position.x + 1.5f;
        float minX = _vacTransform.position.x - 1.5f;

        if (playerPos.y >= _vacTransform.position.y && playerPos.y <= maxY && playerPos.x <= maxX && playerPos.x >= minX)
        {
            playerPos.y = _vacTransform.position.y;

            _player.transform.position = playerPos;
        }
        else
        {
            _playerInRange = false;
        }
    }

    private void PlayerDefaultVelocity()
    {
        if (_player != null)
        {
            if (_playerInRange == true)
            {
                _playerInRange = false;
            }

            if (_playerRGB.velocity != Vector2.zero)
            {
                _playerRGB.velocity = Vector2.zero;
            }

            if (_playerRGB.angularVelocity != 0)
            {
                _playerRGB.angularVelocity = 0;
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && _player != null)
        {
            switch (other.tag)
            {
                case "Player":

                    StartCoroutine(DamagePlayerRoutine());

                    break;

                case "Laser":

                    int damage = Random.Range(5, 10);

                    VacuumDamage(damage);

                    Destroy(other.gameObject);

                    break;

                default:

                    break;
            }
        }
    }

    IEnumerator DamagePlayerRoutine()
    {
        float damageCooldown = 1.5f;

        if (_playerHit == false)
        {
            _playerHit = true;

            _player.DeductFromScore(20);

            _player.Damage();

            yield return new WaitForSeconds(damageCooldown);

            _playerHit = false;
        }
    }

    private void VacuumDamage(int damage)
    {
        float firstCheck = _vacMaxHealth * 0.75f;
        float secondCheck = _vacMaxHealth * 0.5f;
        float thirdCheck = _vacMaxHealth * 0.25f;

        if (_vacuShield.activeInHierarchy == true)
        {
            return;
        }
        else
        {
            _vacCurrentHealth -= damage;

            _player.AddToScore(damage);

            if (_vacCurrentHealth >= firstCheck && _damageChecks[0] == false)
            {
                _damageChecks[0] = true;

                DamageVisualizer();
            }
            else if (_vacCurrentHealth >= secondCheck && _vacCurrentHealth < firstCheck && _damageChecks[1] == false)
            {
                _damageChecks[1] = true;

                DamageVisualizer();
            }
            else if (_vacCurrentHealth >= thirdCheck && _vacCurrentHealth < secondCheck && _damageChecks[2] == false)
            {
                _damageChecks[2] = true;

                DamageVisualizer();
            }
            else if (_vacCurrentHealth < thirdCheck && _vacCurrentHealth > 0 && _damageChecks[3] == false)
            {
                _damageChecks[3] = true;

                DamageVisualizer();
            }

            if (_vacCurrentHealth <= 0)
            {
                _vacCurrentHealth = 0;

                StartCoroutine(OnVacuumDeathRoutine());
            }

            PlayAudio(_laserHitClip);

            Debug.Log(_vacCurrentHealth);
        }
    }

    private void DamageVisualizer()
    {
        if (_damageList.Count.Equals(_damagePositions.Length))
        {
            return;
        }
        else
        {
            int randomPosition = Random.Range(0, _damagePositions.Length);

            while (_damageList.Contains(_damagePositions[randomPosition]))
            {
                randomPosition = Random.Range(0, _damagePositions.Length);
            }

            _damageList.Add(_damagePositions[randomPosition]);

            GameObject newDamage = Instantiate(_damagePrefab, _damagePositions[randomPosition], Quaternion.Euler(0, 0, 180f));

            newDamage.transform.SetParent(_damageContainer.transform, false);
        }
    }

    IEnumerator OnVacuumDeathRoutine()
    {
        PlayerDefaultVelocity();

        _isAlive = false;

        Instantiate(_vacExplosionPrefab, transform.position, Quaternion.identity);

        gameObject.GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(0.35f);

        _spawnManager.ActivateBossSecondStage();

        Destroy(this.gameObject);
    }
}
