using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _isGameOver;

    public int _nextScene;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && _isGameOver == true)
        {
            
            LoadScene(2);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void NextScene(int scene)
    {
        _nextScene = scene;
    }

    public void LoadScene(int scene) // Main Menu = 0, Controls = 1, Game = 2, Boss = 3
    {
        NextScene(scene);

        SceneManager.LoadScene(scene);
    }

    public void GameOver()
    {
        _isGameOver = true;
    }
}
