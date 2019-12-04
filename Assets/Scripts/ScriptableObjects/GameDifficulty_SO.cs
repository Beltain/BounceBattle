using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Game_Difficulty_Setting", menuName = "Game/Game Difficulty Setting")]
public class GameDifficulty_SO : ScriptableObject
{
    public int index = 1;
    public float pigMoveSpeedMultiplier = 1f;
    public float pigDetectionRadiusMultiplier = 1f;
    public float pigSpawnMultiplier = 1f;
    public float wolfSpawnMultiplier = 1f;
    public float wolfPowerMultiplier = 1f;
    public float wolfAttackFrequencyMultiplier = 1f;
    public float wolfAttackDamageMultiplier = 1f;
}
