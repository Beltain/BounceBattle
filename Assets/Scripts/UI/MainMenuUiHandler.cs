using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUiHandler : MonoBehaviour
{
    public Color notSelected;
    public Color[] scalingGradientColors;
    public Image[] easyMediumHardButtons;
    public Image[] normalFasterInsaneButtons;
    public enum difficultyButtons {easy, medium, hard };
    public difficultyButtons selectedDifficulty;
    public Image muteIcon;
    public Sprite unmuted;
    public Sprite muted;

    private void Awake()
    {
        ChangeDifficultyButtonSelection(GameController.gameController.gameDifficulty.index);
        ChangeSpeedButtonSelection(GameController.gameController.gameSpeedSettings.index);
        SetMuteButton(GameController.gameController.audioMuted);
    }

    public void ChangeDifficultyButtonSelection (int index)
    {
        switch (index)
        {
            case (0):
                easyMediumHardButtons[0].color = scalingGradientColors[0];
                easyMediumHardButtons[1].color = notSelected;
                easyMediumHardButtons[2].color = notSelected;
                break;
            case (1):
                easyMediumHardButtons[1].color = scalingGradientColors[1];
                easyMediumHardButtons[0].color = notSelected;
                easyMediumHardButtons[2].color = notSelected;
                break;
            case (2):
                easyMediumHardButtons[2].color = scalingGradientColors[2];
                easyMediumHardButtons[1].color = notSelected;
                easyMediumHardButtons[0].color = notSelected;
                break;
            default:
                Debug.Log("Difficulty not set");
                break;
        }
    }

    public void ChangeSpeedButtonSelection(int index)
    {
        switch (index)
        {
            case (0):
                normalFasterInsaneButtons[0].color = scalingGradientColors[0];
                normalFasterInsaneButtons[1].color = notSelected;
                normalFasterInsaneButtons[2].color = notSelected;
                break;
            case (1):
                normalFasterInsaneButtons[1].color = scalingGradientColors[1];
                normalFasterInsaneButtons[0].color = notSelected;
                normalFasterInsaneButtons[2].color = notSelected;
                break;
            case (2):
                normalFasterInsaneButtons[2].color = scalingGradientColors[2];
                normalFasterInsaneButtons[1].color = notSelected;
                normalFasterInsaneButtons[0].color = notSelected;
                break;
            default:
                Debug.Log("Speed not set");
                break;
        }
    }

    public void SetMuteButton(bool state)
    {
        //Method to set the icons for the mute button
        if (state) muteIcon.sprite = muted;
        else muteIcon.sprite = unmuted;
    }

    public void ToggleMute()
    {
        //Method called by a button to toggle the mute state
        GameController.gameController.MuteGame(!GameController.gameController.audioMuted);
        SetMuteButton(GameController.gameController.audioMuted);
    }
}
