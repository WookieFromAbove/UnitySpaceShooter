using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _scoreTMP;

    [SerializeField]
    private TextMeshProUGUI _ammoTMP;

    private Player _player;

    [SerializeField]
    private Image _livesImage;

    [SerializeField]
    private Sprite[] _livesSprites;

    [SerializeField]
    public Slider _thrusterBar;

    private bool _isPlayerDead = false;

    [SerializeField]
    private TextMeshProUGUI _gameOverTMP;

    [SerializeField]
    private Vector3 _gameOverTMPMaxScale;
    private Vector3 _gameOverTMPMinScale;
    private float _gameOverTMPScaleSpeed = 0.1f;

    private bool _flicker = false;

    [SerializeField]
    private TextMeshProUGUI _restartTMP;

    private GameManager _gameManager;

    void Start()
    {
        _scoreTMP.text = "Space Bucks: $" + 0;

        _gameOverTMP.gameObject.SetActive(false);
        _restartTMP.gameObject.SetActive(false);

        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.Log("Player is NULL.");
        }

        _ammoTMP.text = _player.CurrentAmmo() + " /";

        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (_gameManager == null)
        {
            Debug.Log("GameManager is NULL.");
        }
    }

    void Update()
    {
        CurrentScore();
        CurrentAmmo();
        CurrentLives();
        CurrentEngineThrusterTime();
    }

    public void CurrentScore()
    {
        _scoreTMP.text = "Space Bucks: $" + _player.CurrentScore();
    }

    private void CurrentAmmo()
    {
        _ammoTMP.text = _player.CurrentAmmo() + " /";
    }

    public void CurrentLives()
    {
        _livesImage.sprite = _livesSprites[_player.CurrentLives()];

        if (_player.CurrentLives() <= 0)
        {
            _isPlayerDead = true;

            if (_isPlayerDead == true)
            {
                PlayerDeathSequence();
            }
        }
    }

    private void CurrentEngineThrusterTime()
    {
        _thrusterBar.value = _player.CurrentEngineThrusterTime();
    }

    private void PlayerDeathSequence()
    {
        _gameManager.GameOver();

        GameOverTMPEnabled();
        if (!_flicker)
        {
            StartCoroutine(GameOverTMPFlickerRoutine());
            _flicker = true;
        }

        RestartTMPEnabled();
    }

    public void GameOverTMPEnabled()
    {
        _gameOverTMP.gameObject.SetActive(true);

        if (_gameOverTMP.gameObject == isActiveAndEnabled)
        {
            float i = 0f;
            float duration = 0.05f;
            float rate = (1f / duration) * _gameOverTMPScaleSpeed;
            _gameOverTMPMinScale = _gameOverTMP.rectTransform.localScale;

            while (i < 1f)
            {
                i += Time.deltaTime * rate;
                _gameOverTMP.rectTransform.localScale = Vector3.Lerp(_gameOverTMPMinScale, _gameOverTMPMaxScale, duration);
            }
        }
    }

    IEnumerator GameOverTMPFlickerRoutine()
    {
        while (true)
        {
            _gameOverTMP.text = "SPACEBALL CITY IS SAVED!";

            yield return new WaitForSeconds(0.5f);

            _gameOverTMP.text = "";

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void RestartTMPEnabled()
    {
        _restartTMP.gameObject.SetActive(true);
    }
}
