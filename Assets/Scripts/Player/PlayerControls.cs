using ILOVEYOU.Management;
using ILOVEYOU.ProjectileSystem;
using ILOVEYOU.Shader;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace ILOVEYOU
{
    namespace Player
    {
        [RequireComponent(typeof(Rigidbody))]
        [RequireComponent(typeof(Collider))]
        public class PlayerControls : MonoBehaviour
        {
            [SerializeField] private bool m_debugging;
            
            private PlayerManager m_plaMa;
            private Rigidbody m_rb;

            [Header("General")]
            [SerializeField] private float m_MaxHealth = 10f;
            private float m_health;
            [SerializeField] private float m_iframesTotal = 1f; //this is in seconds
            private float m_iframesCurrent;
            [SerializeField] private Animator m_anim; //animator should be located on the player model

            [Header("Movement")]
            [SerializeField] private float m_moveSpeed;
            private Vector3 m_moveDir;

            [Header("Shooting")]
            [SerializeField] private float m_damage;
            [SerializeField] private float m_fireRate;
            private Vector3 m_aimDir;
            private bool m_allowShooting;
            private float m_aimMagnitude { get { return m_aimDir.magnitude; } }
            [SerializeField, Range(0f, 1f)] private float m_aimDeadZone;

            //private GameObject m_playerModel;
            private Transform m_facingObject;
            //private GameObject m_patternObject; //gameobject that holds the bullet pattern script
            private BulletPattern m_pattern;

            [Header("Raycasting")]
            [SerializeField] private Vector3 m_boxCastSize;
            [SerializeField] LayerMask m_mask;
            private Collider m_Collider;
            private RaycastHit m_Hit;
            private bool m_hitDetect;

            [Header("Events - for sounds and visuals")]
            [SerializeField] private UnityEvent m_onTakeDamage;
            [SerializeField] private UnityEvent m_onDeath;
            [SerializeField] private UnityEvent m_onShootingDisabled;
            [SerializeField] private UnityEvent m_onShootingEnabled;

            [Header("Context Button - Delegate void stuff")]
            [SerializeField] TextMeshProUGUI m_contextText;
            public delegate void ContextPress();
            private ContextPress m_contextPress;
            private int m_contextPriority; //current priority of the current context action
            /// <summary>
            /// Sets the context press function to whatever function is put in the ContextPress
            /// </summary>
            /// <param name="context">function for the context button to attatch to</param>
            /// <param name="priority">what priority this action has, the higher the priority, the less likely it will be overwritten</param>
            /// <param name="contextText">what display for the context text</param>
            public void SetContext(ContextPress context, int priority, string contextText)
            {
                if (priority > m_contextPriority && context != m_contextPress) { m_contextPress = context; m_contextText.text = contextText; }
            }
            //possible TODO: think about perhaps having multiple context actions at once
            /// <summary>
            /// removes the set context provided and resets values
            /// </summary>
            /// <param name="context">function to remove</param>
            public void RemoveSetContext(ContextPress context)
            {
                m_contextPress -= context;
                m_contextPriority = 0;
                m_contextText.text = "";
            }
            //might not need this one
            /// <summary>
            /// removes the all contexts provided and resets values
            /// </summary>
            public void RemoveAllContext()
            {
                m_contextPress = null;
                m_contextPriority = 0;
                m_contextText.text = "";
            }
            private void Awake()
            {
                m_facingObject = transform.GetChild(0);
                m_pattern = m_facingObject.GetComponent<BulletPattern>();
                m_Collider = GetComponent<Collider>();
                m_plaMa = GetComponent<PlayerManager>();
                m_rb = GetComponent<Rigidbody>();
                m_health = m_MaxHealth;
                UpdateHealthBar();
                m_allowShooting = true;
            }
            public bool Startup()
            {
                if (m_debugging) Debug.Log($"There's nothing to start in {this}. It was all done in Awake.");


                return true;
            }


            /// <summary>
            /// Changes a stat
            /// </summary>
            /// <param name="index"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool ChangeStat(int index, float value)
            {
                switch (index)
                {
                    case 0:
                        m_moveSpeed += value;
                        break;
                    case 1:
                        m_MaxHealth += value;
                        break;
                    case 2:
                        m_pattern.AddDamage(value);
                        break;
                    case 3:
                        m_pattern.AddFireSpeed(value);
                        break;
                }
                return true;
            }

            public void ChangeWeapon(BulletPatternObject obj)
            {
                m_pattern.ChangePattern(obj);
            }
            public void Update()
            {
                Color tmp_color = Color.blue;
                if (m_aimMagnitude >= m_aimDeadZone && m_allowShooting)
                {
                    if (m_debugging) Debug.Log($"{gameObject} is firing");
                    if (m_debugging) tmp_color = Color.red;

                    //raycasts in front of the player for a target for the bullets
                    m_hitDetect = Physics.BoxCast(m_Collider.bounds.center, m_boxCastSize * 0.5f, m_facingObject.transform.forward, out m_Hit, m_facingObject.transform.rotation, 200f, m_mask);

                    //adds target to pattern script
                    if (m_hitDetect) m_pattern.AddTarget(m_Hit.collider.transform);
                    else m_pattern.AddTarget(null);

                    //updates cooldowns
                    m_pattern.PatternUpdate();

                    //update animator
                    m_anim.SetBool("Shooting", true);
                }
                else
                {
                    //update animator
                    m_anim.SetBool("Shooting", false);
                }
                m_iframesCurrent = Mathf.Clamp(m_iframesCurrent - Time.deltaTime, 0f, m_iframesTotal);
                if (m_debugging) Debug.DrawRay(transform.position, 5 * m_aimMagnitude * m_aimDir, tmp_color);
                //Apply direction to the player object
                m_rb.velocity = m_moveDir * m_moveSpeed;
            }
            public void OnMove(InputValue value)
            {
                //Get the direction from the given input
                m_moveDir = value.Get<Vector2>();
                m_moveDir = new Vector3(m_moveDir.x, 0, m_moveDir.y);
                if (m_debugging) Debug.Log($"Moving {gameObject} by {m_moveDir}.");

                //Converts movement into angle
                float moveAngle = Mathf.Rad2Deg * Mathf.Atan2(m_moveDir.x, m_moveDir.z);
                //gets quaternions to convert to vectors
                Quaternion moveQ = Quaternion.Euler(0f, moveAngle, 0f);
                Quaternion shotQ = m_facingObject.rotation;
                //sets required animation variables
                m_anim.SetFloat("moveX", (moveQ * Quaternion.Inverse(shotQ) * Vector3.forward * m_moveDir.magnitude).x);
                m_anim.SetFloat("moveZ", (moveQ * Quaternion.Inverse(shotQ) * Vector3.forward * m_moveDir.magnitude).z);
            }
            public void OnFire(InputValue value)
            {
                //Get the direction of the right stick
                m_aimDir = value.Get<Vector2>();
                if (m_aimDir == Vector3.zero) return;
                //Apply it to the x & z axis
                m_aimDir = new Vector3(m_aimDir.x, 0, m_aimDir.y);
                if (m_debugging) Debug.Log($"{gameObject} is aiming towards {m_aimDir}.");

                //find and apply rotation to the pattern object
                Vector3 relativePos = (transform.position + m_aimDir) - transform.position;
                //gets the required rotation for the shooting
                Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
                m_facingObject.transform.rotation = rotation;
                //m_playerModel.transform.rotation = rotation;
            }
            public void OnContextButton(InputValue value)
            {
                //Debug.Log("HEwwo!!!!");
                m_contextPress?.Invoke();
            }
            /// <summary>
            /// makes the player take the damage oh noooo this is bad
            /// </summary>
            public void TakeDamage(float damage)
            {
                if (m_iframesCurrent > 0) return;
                m_health -= damage;
                m_iframesCurrent = m_iframesTotal;
                UpdateHealthBar();
                //reset all timed tasks when damaged
                m_plaMa.GetTaskManager.UpdateTimers(true);
                if (m_health <= 0)
                {
                    GameManager.Instance.PlayerDeath(m_plaMa);
                    m_onDeath.Invoke();
                }
                else
                {
                    m_onTakeDamage.Invoke();
                }
                
            }

            public void UpdateHealthBar()
            {
                float current = m_health / m_MaxHealth;
                m_plaMa.UpdateHealthBar(current);
            }

            private void OnDrawGizmos()
            {
                if (m_debugging)
                {
                    Gizmos.color = Color.red;

                    //Check if there has been a hit yet
                    if (m_hitDetect)
                    {
                        //Draw a Ray forward from GameObject toward the hit
                        Gizmos.DrawRay(m_facingObject.transform.position, m_facingObject.transform.forward * m_Hit.distance);
                        //Draw a cube that extends to where the hit exists
                        Gizmos.DrawWireCube(m_facingObject.transform.position + m_facingObject.transform.forward * m_Hit.distance, m_boxCastSize);
                    }
                    //If there hasn't been a hit yet, draw the ray at the maximum distance
                    else
                    {
                        //Draw a Ray forward from GameObject toward the maximum distance
                        Gizmos.DrawRay(m_facingObject.transform.position, m_facingObject.transform.forward * 200f);
                        //Draw a cube at the maximum distance
                        Gizmos.DrawWireCube(m_facingObject.transform.position + m_facingObject.transform.forward * 200f, m_boxCastSize);
                    }
                }
            }
            public void DisableShooting(float time)
            {
                m_allowShooting = false;
                m_plaMa.GetLog.LogInput($"<color=\"red\">Debugger disabled.</color> Rebooting in {time} seconds");
                m_onShootingDisabled.Invoke();
                //CancelInvoke();
                //m_plaMa.GetLevelManager.GetParticleSpawner.SpawnParticleTime(m_debuffParticleTemp, transform, time);
                //Invoke("ReenableShooting", time);
                //return true;
            }
            public void ReenableShooting()
            {
                m_allowShooting = true;
                m_plaMa.GetLog.LogInput($"Debugger rebooted, please enjoy your free trial!");
                m_onShootingEnabled.Invoke();
            }

        }
    }
}
