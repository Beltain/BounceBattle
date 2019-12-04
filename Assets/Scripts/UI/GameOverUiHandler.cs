using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUiHandler : MonoBehaviour
{
    [SerializeField] protected GameObject[] bountiesPrefabs;
    [SerializeField] protected List<GameObject> spawnedBountyPrefabs = new List<GameObject>();
    [SerializeField] protected Text[] gameOverText;
    [SerializeField] protected string victoryText;
    [SerializeField] protected string[] failureText;
    [SerializeField] protected GameObject bountyText;
    [SerializeField] protected Transform bountyBox;
    [SerializeField] protected GameObject timeText;
    [SerializeField] protected Text[] timeVal;
    [SerializeField] protected GameObject[] otherButtons;
    protected float currentXOffset = 0f;

    private void Awake()
    {

    }

    public void ResetRunDown()
    {
        //Remove bounty images
        foreach(GameObject spawnedPrefab in spawnedBountyPrefabs)
        {
            Destroy(spawnedPrefab);
        }
        spawnedBountyPrefabs = new List<GameObject>();
        currentXOffset = 0f;
        //Hide all UI objects individually to show again
        foreach(Text overText in gameOverText)
        {
            overText.text = "";
        }
        bountyText.SetActive(false);
        foreach(Text timeValue in timeVal)
        {
            timeValue.text = "";
        }
        timeText.SetActive(false);
        foreach(GameObject button in otherButtons)
        {
            button.SetActive(false);
        }
    }

    public void PlayGameOverRundown(bool winState, Vector2 bountiesVal, float gameTime)
    {
        StartCoroutine(RunDownGameOver(winState, bountiesVal, gameTime));
    }

    IEnumerator RunDownGameOver(bool winState, Vector2 bountiesVal, float gameTime)
    {
        ResetRunDown();
        yield return new WaitForSeconds(0.5f);
        //Display either VICTORY or GAMEOVER texts
        if (winState)
        {
            //Play gameOver sound
            GameController.gameController.universalAudioSource.PlayOneShot(GameController.gameController.gameSettings.gameOverWinSound);
            for (int i = 0; i < victoryText.Length; i++)
            {
                foreach (Text overText in gameOverText)
                {
                    overText.text += victoryText[i];
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            //Play gameOver sound
            GameController.gameController.universalAudioSource.PlayOneShot(GameController.gameController.gameSettings.gameOverLoseSound);
            foreach (string seg in failureText)
            {
                foreach (Text overText in gameOverText)
                {
                    overText.text += seg;
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
        yield return new WaitForSeconds(0.6f);

        //Show Bounty
        bountyText.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        //Spawn in bounty representing icons one by one, starting at an offset value of x
        for(int i = 0; i < bountiesVal.x; i++)
        {
            GameObject currentIcon = Instantiate(bountiesPrefabs[0], bountyBox);
            spawnedBountyPrefabs.Add(currentIcon);
            currentIcon.transform.Translate(new Vector3(currentXOffset, 0f, 0f));
            currentXOffset += 21f;
            //Play tally sound
            GameController.gameController.universalAudioSource.PlayOneShot(GameController.gameController.gameSettings.bountyTallySound);
            yield return new WaitForSeconds(0.1f);
        }
        for (int i = 0; i < bountiesVal.y; i++)
        {
            GameObject currentIcon = Instantiate(bountiesPrefabs[1], bountyBox);
            spawnedBountyPrefabs.Add(currentIcon);
            currentIcon.transform.Translate(new Vector3(currentXOffset, 0f, 0f));
            currentXOffset += 21f;
            //Play tally sound
            GameController.gameController.universalAudioSource.PlayOneShot(GameController.gameController.gameSettings.bountyTallySound);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.6f);

        //Show Time Text Section
        timeText.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        //CONVERT TIME (Could be its own function in gamecontroller later if needed)
        gameTime = Mathf.RoundToInt(gameTime);
        string seconds = (gameTime % 60).ToString();
        if (seconds.Length < 1) seconds = "00";
        else if (seconds.Length < 2) seconds = "0" + seconds;
        string minutes = (Mathf.FloorToInt(gameTime / 60f)).ToString();
        if (minutes.Length < 1) minutes = "00";
        if (minutes.Length < 2) minutes = "0" + minutes;
        //Show time
        if (gameTime > 3600f)
        {
            foreach (Text timeValue in timeVal)
            {
                timeValue.text = "Over an hour somehow???";
            }
        }
        else
        {
            foreach (Text timeValue in timeVal)
            {
                timeValue.text = minutes + ":" + seconds;
            }
        }
        //Play sound with time reveal
        GameController.gameController.universalAudioSource.PlayOneShot(GameController.gameController.gameSettings.bountyTallySound);
        yield return new WaitForSeconds(0.6f);

        //Show buttons
        foreach (GameObject button in otherButtons)
        {
            button.SetActive(true);
        }
    }
}
