using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiController : MonoBehaviour
{
    public static UiController uiController;
    [SerializeField]private GameObject[] bouncersInScene = null;
    [SerializeField] private List<GameObject> statusBarPrefabs = null;

    public enum uiStates { MainMenu, PauseMenu, Gameplay, GameOver };
    public uiStates uiState;
    public Canvas hud;
    public Canvas mainMenu;
    public Canvas pauseMenu;
    public Canvas gameOverMenu;

    public GameOverUiHandler gameOverUiHandler;
    public MainMenuUiHandler mainMenuUiHandler;

    public void Awake()
    {
        uiController = this;
        gameOverUiHandler = GetComponent<GameOverUiHandler>();
        mainMenuUiHandler = GetComponent<MainMenuUiHandler>();
    }

    public void FindBouncersInScene()
    {
        bouncersInScene = null;
        bouncersInScene = GameObject.FindGameObjectsWithTag("Bouncer");
        Debug.Log(bouncersInScene[0].name);
    }

    public void GenerateStatusUI()
    {
        statusBarPrefabs = new List<GameObject>();
        FindBouncersInScene();
        foreach(GameObject bonObj in bouncersInScene)
        {
            Bouncer bon = bonObj.GetComponent<Bouncer>();
            GameObject currentStatusBarPrefab = Object.Instantiate(bon.stats.prefab, hud.gameObject.transform);
            currentStatusBarPrefab.GetComponent<StatusBarUiHandler>().statusTarget = bon;
            if (statusBarPrefabs == null) statusBarPrefabs = new List<GameObject>();
            statusBarPrefabs.Add(currentStatusBarPrefab);
        }
    }

    public void DestroyStatusUi()
    {
        foreach(GameObject stat in statusBarPrefabs)
        {
            Destroy(stat);
        }
    }

    public void ChangeUI(uiStates state)
    {
        uiState = state;
        switch (uiState)
        {
            case(uiStates.MainMenu):
                mainMenu.enabled = true;
                hud.enabled = false;
                pauseMenu.enabled = false;
                gameOverMenu.enabled = false;
                break;
            case(uiStates.PauseMenu):
                mainMenu.enabled = false;
                hud.enabled = false;
                pauseMenu.enabled = true;
                gameOverMenu.enabled = false;
                break;
            case (uiStates.Gameplay):
                mainMenu.enabled = false;
                hud.enabled = true;
                pauseMenu.enabled = false;
                gameOverMenu.enabled = false;
                break;
            case (uiStates.GameOver):
                mainMenu.enabled = false;
                hud.enabled = false;
                pauseMenu.enabled = false;
                gameOverMenu.enabled = true;
                break;
            default:
                Debug.Log("Couldnt get the UI state value entered");
                break;
        }
    }

    public void PlayClickSound()
    {
        GameController.gameController.universalAudioSource.PlayOneShot(GameController.gameController.gameSettings.clickSound);
    }

}
