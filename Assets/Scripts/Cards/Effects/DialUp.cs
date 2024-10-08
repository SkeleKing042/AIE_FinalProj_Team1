using ILOVEYOU.BuffSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILOVEYOU
{
    namespace Cards
    {
        public class DialUp : MonoBehaviour
        {
            private float m_time = 0f;
            private BuffDataSystem m_buffDataSystem;

            // Start is called before the first frame update
            void Start()
            {
                m_buffDataSystem = GetComponentInParent<BuffDataSystem>();
            }

            // Update is called once per frame
            void Update()
            {
                m_time -= Time.deltaTime;

                if (m_time <= 0f)
                {
                    float range = Random.Range(0.05f, 1f);

                    //do thing
                    m_buffDataSystem.GiveBuff(new BuffDataSystem.BuffData("Slowness", 6969, 0, false, range, 0f, -10, 0, 0));

                    m_time = Random.Range(0.1f, 1f) + range;
                }
            }
        }
    }
}


