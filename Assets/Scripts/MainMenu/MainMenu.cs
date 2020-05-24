using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _spaceStop;

    private float _textOutlineMin = 0.224f;
    private float _textOutlineMax = 0.5f;

    private Background _background;

    private void Start()
    {
        StartCoroutine(SpaceStopTextFlickerRoutine());

        _background = GameObject.Find("Background").GetComponent<Background>();
        if (_background == null)
        {
            Debug.Log("Background is NULL.");
        }
    }

    private void Update()
    {
        _background.BackgroundScroll();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(2); // main game scene
    }

    

    public void LoadMainMenuControls()
    {
        SceneManager.LoadScene(1); // main menu controls
    }

    IEnumerator SpaceStopTextFlickerRoutine()
    {
        while (true)
        {
            float randomTime = Random.Range(0, 2f);

            _spaceStop.outlineWidth = _textOutlineMin;

            yield return new WaitForSeconds(randomTime);

            _spaceStop.outlineWidth = _textOutlineMax;

            yield return new WaitForSeconds(randomTime);
        }
    }
}
