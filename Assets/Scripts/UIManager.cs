using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions.Must;

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

    [SerializeField]
    private GameObject _canvasPanel;

    private GameManager _gameManager;

    private SpawnManager _spawnManager;

    private float _bossIntroClipLength;

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

        _ammoTMP.text = _player.CurrentAmmo() + " / " + _player.MaxAmmo();

        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (_gameManager == null)
        {
            Debug.Log("GameManager is NULL.");
        }

        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        if (_spawnManager == null)
        {
            Debug.Log("SpawnManager is NULL.");
        }

        _bossIntroClipLength = _spawnManager._bossIntroClip.length;
    }

    void Update()
    {
        CurrentScore();
        CurrentAmmo();
        CurrentLives();
        CurrentEngineThrusterTime();
    }

    public void SetScreenToBlack()
    {
        StartCoroutine(SetScreenToBlackRoutine());
    }

    IEnumerator SetScreenToBlackRoutine()
    {
        Image panelImage = _canvasPanel.GetComponent<Image>();

        Color black = new Color(0, 0, 0, 255);
        Color transparent = new Color(0, 0, 0, 0);

        panelImage.color = black;

        _spawnManager.PlayAudio(_spawnManager._bossIntroClip);
        float wait = _bossIntroClipLength + 0.5f;

        yield return new WaitForSeconds(wait);

        _player.StartBossWave();

        panelImage.color = transparent;
    }

    public void CurrentScore()
    {
        _scoreTMP.text = "Space Bucks: $" + _player.CurrentScore();
    }

    private void CurrentAmmo()
    {
        _ammoTMP.text = _player.CurrentAmmo() + " / " + _player.MaxAmmo();
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

    public void PlayerVictorySequence()
    {
        _gameManager.GameOver();

        PlayerVictoryTMPEnabled();
        if (!_flicker)
        {
            StartCoroutine(PlayerVictoryRoutine());
            _flicker = true;
        }

        float restartPosY = _restartTMP.rectTransform.localPosition.y;
        restartPosY -= 25f;
        _restartTMP.rectTransform.localPosition = new Vector3(0, restartPosY, 0);

        RestartTMPEnabled();
    }

    private void PlayerVictoryTMPEnabled()
    {
        _gameOverTMP.gameObject.SetActive(true);

        if (_gameOverTMP.gameObject == isActiveAndEnabled)
        {
            float i = 0f;
            float duration = 0.05f;
            float rate = (1f / duration) * _gameOverTMPScaleSpeed;
            _gameOverTMPMinScale = _gameOverTMP.rectTransform.localScale;
            _gameOverTMPMaxScale = new Vector3(12, 12, 12);

            _gameOverTMP.color = new Color32(27, 183, 241, 255); // Red = 250, 20, 29, 255 ... Blue = 27, 183, 241, 255

            while (i < 1f)
            {
                i += Time.deltaTime * rate;
                _gameOverTMP.rectTransform.localScale = Vector3.Lerp(_gameOverTMPMinScale, _gameOverTMPMaxScale, duration);
            }
        }
    }

    IEnumerator PlayerVictoryRoutine()
    {
        while (true)
        {
            _gameOverTMP.text = "VICTORY!";

            yield return new WaitForSeconds(1f);

            _gameOverTMP.text = "";

            yield return new WaitForSeconds(0.5f);

            _gameOverTMP.text = "MAY THE SCHWARTZ BE WITH \n YOUUUUOUOUOUOUOUOUUOOUU!";

            yield return new WaitForSeconds(1f);

            _gameOverTMP.text = "";

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void RestartTMPEnabled()
    {
        _restartTMP.gameObject.SetActive(true);
    }
}
