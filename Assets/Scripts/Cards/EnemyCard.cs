using ILOVEYOU.Management;
using ILOVEYOU.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ILOVEYOU 
{ 
    namespace Cards
    {
        public class EnemyCard : MonoBehaviour
        {

            [Tooltip("Which group id to spawn from")] [SerializeField] private int m_enemyGroup = 0;
            [Tooltip("How many enemies to spawn")] [SerializeField] private int m_enemyCount = 0;
            public void ExecuteEvents(object[] data)
            {
                //get required data
                GameManager manager = (GameManager)data[0];
                PlayerManager player = (PlayerManager)data[1];
                PlayerManager target = manager.GetOtherPlayer(player);

                target.GetLevelManager.GetSpawner.SpawnRandomNumberOfEnemiesFromGroup(m_enemyGroup, m_enemyCount);
                //target.GetLevelManager.GetSpawner.SpawnEnemyWave();
            }
        }
    }

}