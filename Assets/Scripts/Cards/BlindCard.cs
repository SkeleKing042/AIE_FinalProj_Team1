using ILOVEYOU.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILOVEYOU
{
    using Player;
    namespace Cards
    {

        public class BlindCard : MonoBehaviour
        {
            [Tooltip("How long the effect will last for.")] [SerializeField] private float m_time = 3;
            [SerializeField] private int m_popupAmount;
            public void ExecuteEvents(object[] data)
            {
                GameManager manager = (GameManager)data[0];
                PlayerManager player = (PlayerManager)data[1];
                PlayerManager[] targets = manager.GetOtherPlayer(player);

                foreach(PlayerManager target in targets)
                {
                    target.TriggerBlindness(m_popupAmount);
                }
            }
        }
    }
}