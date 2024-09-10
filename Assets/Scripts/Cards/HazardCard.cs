using ILOVEYOU.EnemySystem;
using ILOVEYOU.Hazards;
using ILOVEYOU.Management;
using ILOVEYOU.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILOVEYOU
{
    namespace Cards
    {

        public class HazardCard : MonoBehaviour
        {
            [Tooltip("How long the effect will last for. 0 seconds will enable the hazard without it turning off.")][SerializeField] private float m_time;
            [Tooltip("What hazards will be enabled when this card is selected. If none is inputted, activate all hazards instead.")][SerializeField] private HazardTypes[] m_hazardType;
            [Header("Temporary Hazard Creation")]
            [Tooltip("If this hazard card should create extra hazards around the player")][SerializeField] private bool m_createObjects = false;
            [Tooltip("How many objects to create")] [SerializeField] private int m_objectCount = 0;
            [Tooltip("How long the object will last for. 0 seconds will enable the hazard without it turning off.")][SerializeField] private float m_objectLifetime = 1f;
            [Tooltip("Hazard objects that will be created in the scene around the player")] [SerializeField] private GameObject[] m_hazardObjects;
            [Tooltip("Mask that the hazards will collide with when spawning")][SerializeField] private LayerMask m_mask;
            public void ExecuteEvents(object[] data)
            {
                PlayerManager player = (PlayerManager)data[1];

                foreach (PlayerManager target in GameManager.Instance.GetOtherPlayer(player))
                {
                    //creates the requested objects if true
                    if (m_createObjects)
                    {

                        //gets the position of the player
                        Vector3 enemyPos = target.transform.position;

                        for (int i = 0; i < m_objectCount; i++)
                        {
                            GameObject obj = Instantiate(m_hazardObjects[Random.Range(0, m_hazardObjects.Length)]);
                            bool spawnSuccess = false;

                            for (int j = 0; j < 100; j++)
                            {
                                obj.transform.position = new(enemyPos.x + Random.Range(-10, 10), enemyPos.y, enemyPos.z + Random.Range(-10, 10));

                                if (!Physics.CheckSphere(obj.transform.position + new Vector3(0f, 1f), 1f, m_mask))
                                {
                                    spawnSuccess = true;
                                    break;
                                }
                            }

                            if (!spawnSuccess)
                            {
                                Destroy(obj);
                                continue;
                            }

                            obj.GetComponent<HazardObject>().SetTrigger();

                            if (m_objectLifetime > 0) target.GetLevelManager.GetComponent<HazardManager>().AddHazard(obj.GetComponent<HazardObject>(), m_objectLifetime);
                            else target.GetLevelManager.GetComponent<HazardManager>().AddHazard(obj.GetComponent<HazardObject>());
                        }
                    }


                    //if there aren't any types assigned to the card, activate all hazards
                    if (m_hazardType.Length == 0)
                    {
                        //if time is inputted, activate hazards for set amount of time
                        if (m_time > 0) target.GetLevelManager.GetComponent<HazardManager>().EnableAllHazards(m_time);
                        //else toggle all hazards on
                        else target.GetLevelManager.GetComponent<HazardManager>().EnableAllHazards();
                    }
                    else
                    {
                        //if time is inputted, activate hazards for set amount of time
                        if (m_time > 0)
                        {
                            foreach (HazardTypes type in m_hazardType)
                            {
                                target.GetLevelManager.GetComponent<HazardManager>().EnableTypeHazards(type, m_time);
                            }
                        }
                        //else toggle all hazards on
                        else
                        {
                            foreach (HazardTypes type in m_hazardType)
                            {
                                target.GetLevelManager.GetComponent<HazardManager>().EnableTypeHazards(type);
                            }
                        }
                    }
                }
                        
            }
        }
    }
}