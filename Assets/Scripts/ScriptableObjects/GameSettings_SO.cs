using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New_GameSettings_Profile", menuName = "Game/Game Settings Profile")]
public class GameSettings_SO : ScriptableObject
{
    public float attackAngle = 70f;
    public float maxAttackPower = 5f;
    public float attackPowerScaler = 130f;

    public float boundsRadius = 2f;
    public float pigDetectionRadius = 9f;
    public Vector2 aimReticuleClamps = new Vector2(2f, 8f);

    public float maxPullDistance = 16f;

    public GameObject chickenPrefab;
    public GameObject[] pigPrefabs;
    public Vector2 pigAmount = new Vector2(2,4);
    public GameObject[] wolfPrefabs;
    public Vector2 wolfAmount = new Vector2(4,6);

    public float levelRadius = 50f;

    public GameObject trailParticlesPrefab;
    public GameObject explosionParticlesPrefab;
    public GameObject parryParticlesPrefab;
    public GameObject powerUpParticlePrefab;

    public AudioClip powerUpSound;
    public AudioClip parrySound;

    public AudioClip clickSound;
    public AudioClip gameOverWinSound;
    public AudioClip gameOverLoseSound;
    public AudioClip bountyTallySound;

}
