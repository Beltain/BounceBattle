using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Status_Profile", menuName = "Status/Status Profile")]
public class StatusProfile_SO : ScriptableObject
{
    [Header("UI")]
    public GameObject prefab;

    [Header("Health")]
    public Vector2 h_MaxValue;
    public float h_MinValue;
    public float h_RegenRate;
    public bool h_AllowRegen;
    public bool h_AllowChange;

    [Space(10)]
    [Header("Stamina")]
    public Vector2 s_MaxValue;
    public float s_MinValue;
    public float s_RegenRate;
    public bool s_AllowRegen;
    public bool s_AllowChange;
}
