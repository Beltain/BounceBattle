using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Game_Speed", menuName = "Game/Game Speed Profile")]
public class GameSpeed_SO : ScriptableObject
{
    public int index = 0;
    public float statusRegenMultiplier = 1f;
    public float wolfAttackFrequencyMultiplier = 1f;
}
