using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    //Generic
    public GameSettings_SO gameSettings;
    public GameDifficulty_SO gameDifficulty;
    public GameSpeed_SO gameSpeedSettings;
    public GameSpeed_SO[] selectableSpeedSettings;
    public GameDifficulty_SO[] selectableDifficulties;
    public static GameController gameController;

    public bool audioMuted = false;
    public AudioSource universalAudioSource;

    public Bouncer player;
    public List<EnemyAI> wolves;
    public List<PigPowerUpAI> pigs;

    [Tooltip("index 0: Wolves, index 1: pigs")]public int[] bounties = { 0, 0};

    public bool canInteract = true;
    public bool gamePaused = false;
    public bool gameOver = false;
    private IEnumerator AimHandler;

    //Game Sequencing
    [Tooltip("Set a menu and game rotation for the camera")] public Vector3[] CameraRotations;
    public float gameTime = 0f;
    private IEnumerator GameTimeCounter;


    private void Awake()
    {
        //Assign constants
        gameController = this;
        universalAudioSource = GetComponent<AudioSource>();
        //Set the menu as the default UI state
        UiController.uiController.ChangeUI(UiController.uiStates.MainMenu);
        //Mute the game if it's set
        MuteGame(audioMuted);
    }

    private void Update()
    {
        if(canInteract)CheckMouse();
    }

    public void MuteGame(bool state)
    {
        //Mute/Unmute the game
        if (state) Camera.main.GetComponent<AudioListener>().enabled = false;
        else Camera.main.GetComponent<AudioListener>().enabled = true;
        audioMuted = state;
    }

    public void ChangeDifficulty(int index)
    {
        //Change the difficulty scriptable object assigned based on passed in index number
        foreach(GameDifficulty_SO difficultyLevel in selectableDifficulties)
        {
            if (difficultyLevel.index == index) gameDifficulty = difficultyLevel;
        }
    }

    public void ChangeSpeedSettings(int index)
    {
        //Change the speedSettings scriptable object assigned based on passed in index number
        foreach (GameSpeed_SO speedSettings in selectableSpeedSettings)
        {
            if (speedSettings.index == index) gameSpeedSettings = speedSettings;
        }
    }

    private void CheckMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(player != null)
                if (player.isControllable)
                {
                    if (AimHandler != null) StopCoroutine(AimHandler);
                    AimHandler = Aim(player.gameObject/*hitInfo.collider.gameObject*/);
                    StartCoroutine(AimHandler);
                }
            //OLD METHOD THAT WORKED ON THE PLAYER ACTUALLY CLICKING THE CONTROLLABLE IN THE SCENE:
            //Shoot a ray on the selectable layer and if it finds a selectable object, start an aim routine with it
            //int selectableLayermask = 1 << 9;
            //RaycastHit hitInfo;
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //if(Physics.Raycast(ray, out hitInfo, 1000f, selectableLayermask))
            //{
                //if (hitInfo.collider.gameObject.GetComponent<Bouncer>().isControllable)
                //{
                    //if (AimHandler != null) StopCoroutine(AimHandler);
                    //AimHandler = Aim(hitInfo.collider.gameObject);
                    //StartCoroutine(AimHandler);
                //}
            //}
        }
    }

    IEnumerator Aim(GameObject selected)
    {
        Bouncer selectedBouncer = selected.GetComponent<Bouncer>();
        Transform selectedOrigin;

        Vector3 lookAngle;
        float aimStrength = 0f;

        selectedBouncer.EnableAimReticule(true);
        selectedBouncer.aiming = true;

        while (!Input.GetMouseButtonUp(0))
        {
            selectedOrigin = selected.GetComponent<Transform>();

            //Face select to opposite of mouse
            lookAngle = -(getMousePointInWorld() - selectedOrigin.position);
            selectedOrigin.LookAt(new Vector3(lookAngle.x*100f, selectedOrigin.position.y, lookAngle.z*100f));

            //Creating a float to lock the max pull based on stamina allowance
            float maxPullAllowByStamina = Mathf.Clamp(selectedBouncer.stamina.currentValue / gameSettings.maxAttackPower, 0f, 1f);
            //Change aim reticule size and set aim strength
            aimStrength =  Mathf.Clamp(Vector3.Distance(getMousePointInWorld(), selectedOrigin.position)/gameSettings.maxPullDistance, 0f, maxPullAllowByStamina);
            selectedBouncer.AdjustAimReticuleSizeAndColor(aimStrength);

            yield return null;
        }

        //Release attack
        selectedBouncer.LaunchBouncer(aimStrength);
        selectedBouncer.EnableAimReticule(false);
        selectedBouncer.aiming = false;
        
    }

    private Vector3 getMousePointInWorld()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 1.0F;
        Ray cameraRay = Camera.main.ScreenPointToRay(mousePos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;
        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            return cameraRay.GetPoint(rayLength);
        }
        Debug.Log("Error fetching mouse world point, ray not hitting plane?");
        return Vector3.zero;
    }

    private void GenerateLevel()
    {
        //Spawn player
        Vector2 circlePoint = Random.insideUnitCircle * gameSettings.levelRadius;
        player = Instantiate(gameSettings.chickenPrefab, new Vector3(circlePoint.x, gameSettings.chickenPrefab.transform.position.y, circlePoint.y), transform.rotation).GetComponent<Bouncer>();

        //Spawn Enemies
        wolves = new List<EnemyAI>();
        int enemyCount = Mathf.RoundToInt((Random.Range(gameSettings.wolfAmount.x, gameSettings.wolfAmount.y))*gameDifficulty.wolfSpawnMultiplier);
        for(int i = 0; i < enemyCount; i++)
        {
            //Get a random enemy from the wolf prefabs list in game settings
            GameObject wolfPrefab = gameSettings.wolfPrefabs[Random.Range(0, gameSettings.wolfPrefabs.Length)];

            //Loop until the random spawn position is not overlapping with another character, then spawn the wolf
            while (true)
            {
                circlePoint = Random.insideUnitCircle * gameSettings.levelRadius;
                Vector3 circlePointV3 = new Vector3(circlePoint.x, wolfPrefab.transform.position.y, circlePoint.y);
                //if it instersects with the player, ensure this loops again
                if (Vector3.Distance(player.transform.position, circlePointV3) > gameSettings.boundsRadius * 2f)
                {
                    //if it intersects with another wolf, ensure this loops again
                    bool intersects = false;
                    foreach (EnemyAI wolf in wolves)
                    {
                        if (Vector3.Distance(wolf.transform.position, circlePointV3) < gameSettings.boundsRadius * 2f)
                        {
                            intersects = true;
                            break;
                        }
                    }
                    //Spawn it here
                    if (!intersects)
                    {
                        wolves.Add(Instantiate(wolfPrefab, circlePointV3, transform.rotation).GetComponent<EnemyAI>());
                        break;
                    }
                }
            }
        }

        //Spawn Pigs
        pigs = new List<PigPowerUpAI>();
        int pigCount = Mathf.RoundToInt((Random.Range(gameSettings.pigAmount.x, gameSettings.pigAmount.y))*gameDifficulty.pigSpawnMultiplier);
        for (int i = 0; i < pigCount; i++)
        {
            //Get a random pig from the pig prefabs list in game settings
            GameObject pigPrefab = gameSettings.pigPrefabs[Random.Range(0, gameSettings.pigPrefabs.Length)];

            //Loop until the random spawn position is not overlapping with another character, then spawn the wolf
            while (true)
            {
                circlePoint = Random.insideUnitCircle * gameSettings.levelRadius;
                Vector3 circlePointV3 = new Vector3(circlePoint.x, pigPrefab.transform.position.y, circlePoint.y);
                //if it instersects with the player, ensure this loops again
                if (Vector3.Distance(player.transform.position, circlePointV3) > gameSettings.boundsRadius * 3f)
                {
                    //if it intersects with another wolf, ensure this loops again
                    bool intersects = false;
                    foreach (EnemyAI wolf in wolves)
                    {
                        if (Vector3.Distance(wolf.transform.position, circlePointV3) < gameSettings.boundsRadius * 3f)
                        {
                            intersects = true;
                            break;
                        }
                    }
                    foreach (PigPowerUpAI pig in pigs)
                    {
                        if (Vector3.Distance(pig.transform.position, circlePointV3) < gameSettings.boundsRadius * 3f)
                        {
                            intersects = true;
                            break;
                        }
                    }
                    //Spawn it here
                    if (!intersects)
                    {
                        pigs.Add(Instantiate(pigPrefab, circlePointV3, transform.rotation).GetComponent<PigPowerUpAI>());
                        break;
                    }
                }
            }
        }
        if (GameTimeCounter != null) StopCoroutine(GameTimeCounter);
        GameTimeCounter = CountGameTime();
        StartCoroutine(GameTimeCounter);
    }

    private void DestroyLevel()
    {
        //Clear bounties
        bounties[0] = 0;
        bounties[1] = 0;
        //Kill all entities
        foreach (EnemyAI wolf in wolves)
        {
            Destroy(wolf.gameObject);
        }
        foreach (PigPowerUpAI pig in pigs)
        {
            Destroy(pig.gameObject);
        }
        if(player != null)Destroy(player.gameObject);
        //Remove old status UI
        UiController.uiController.DestroyStatusUi();
        if (GameTimeCounter != null) StopCoroutine(GameTimeCounter);
    }

    IEnumerator CountGameTime()
    {
        gameTime = 0f;
        while (true)
        {
            gameTime += Time.deltaTime;
            yield return null;
        }
    }

    public void StartGame()
    {
        StartCoroutine(StartGameSequence());
    }

    IEnumerator StartGameSequence()
    {
        //Move the camera down to show the battlefield and then hide menu UI, and generate the level
        UiController.uiController.ChangeUI(UiController.uiStates.Gameplay);
        yield return ToggleCamera(UiController.uiStates.Gameplay, 1f);
        GenerateLevel();
        yield return null;
        //Generate the ui for the characters
        UiController.uiController.GenerateStatusUI();
        yield return null;

        gameOver = false;
        canInteract = true;
    }

    public void TogglePauseGame()
    {
        StartCoroutine(TogglePauseSequencer());
    }

    IEnumerator TogglePauseSequencer()
    {
        gamePaused = !gamePaused;
        canInteract = !gamePaused;
        if (gamePaused)
        {
            //Pause code
            yield return ToggleCamera(UiController.uiStates.PauseMenu, 5f);
            Time.timeScale = 0f;
            UiController.uiController.ChangeUI(UiController.uiStates.PauseMenu);
        }
        else
        {
            //Play code
            yield return ToggleCamera(UiController.uiStates.Gameplay, 10f);
            Time.timeScale = 1f;
            UiController.uiController.ChangeUI(UiController.uiStates.Gameplay);
        }
    }

    public void ReloadGame()
    {
        //Remove all the old stuff and replace it with a fresh game
        UiController.uiController.ChangeUI(UiController.uiStates.Gameplay);
        DestroyLevel();
        gamePaused = false;
        canInteract = true;
        Time.timeScale = 1f;
        StartGame();
    }

    public void BackToMainMenu()
    {
        //Remove all entities and bring the main menu back up
        DestroyLevel();
        gamePaused = false;
        canInteract = true;
        Time.timeScale = 1f;
        UiController.uiController.ChangeUI(UiController.uiStates.MainMenu);
    }

    IEnumerator ToggleCamera(UiController.uiStates menuChange, float speed)
    {
        //One function to toggle the camera to all states
        Vector3 fromVector = Vector3.zero;
        Vector3 toVector = Vector3.zero;
        
        switch (menuChange)
        {
            case (UiController.uiStates.Gameplay):
                fromVector = CameraRotations[0];
                toVector = CameraRotations[1];
                break;
            case (UiController.uiStates.MainMenu):
                fromVector = CameraRotations[1];
                toVector = CameraRotations[0];
                break;
            case (UiController.uiStates.PauseMenu):
                fromVector = CameraRotations[1];
                toVector = CameraRotations[0];
                break;
            case (UiController.uiStates.GameOver):
                fromVector = CameraRotations[1];
                toVector = CameraRotations[0];
                break;
        }

        for (float i = 0f; i < 1f; i += Time.unscaledDeltaTime * speed)
        {
            Camera.main.transform.rotation = Quaternion.Euler(Vector3.Slerp(fromVector, toVector, i));
            yield return null;
        }
        Camera.main.transform.rotation = Quaternion.Euler(toVector);
        
    }

    public void GameOver(bool winState)
    {
        gameOver = true;
        canInteract = false;

        //Stop counting game time
        if (GameTimeCounter != null) StopCoroutine(GameTimeCounter);
        if(AimHandler != null)StopCoroutine(AimHandler);

        //GAMEOVER SEQUENCE
        Debug.Log("Gameover sequence");

        //Reset and show the Gameover Ui Sequence
        StartCoroutine(ToggleCamera(UiController.uiStates.GameOver, 2f));
        UiController.uiController.gameOverUiHandler.ResetRunDown();
        UiController.uiController.ChangeUI(UiController.uiStates.GameOver);
        UiController.uiController.gameOverUiHandler.PlayGameOverRundown(winState, new Vector2(bounties[0], bounties[1]), gameTime);
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
