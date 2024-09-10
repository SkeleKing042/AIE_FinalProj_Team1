using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game Settings", menuName = "Create new game settings")]
public class GameSettings : ScriptableObject
{
    [Header("Difficulty")]
    [SerializeField] private float m_timeToMaxDifficulty;
    public float TimeToMaxDifficulty { get { return m_timeToMaxDifficulty; } }
    [SerializeField] private AnimationCurve m_spawnRateOverTime;
    public AnimationCurve SpawnRate { get { return m_spawnRateOverTime; } }

}
