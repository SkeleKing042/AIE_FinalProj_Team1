using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ILOVEYOU
{
    namespace Management
    {

        public class MainMenuScene : MonoBehaviour
        {
            [SerializeField] private string m_sceneName;
            private int m_lastPlayerCount = 0;
            [SerializeField] private string m_stringToPassOnJoin;
            [SerializeField] private UnityEvent<string, float> m_onPlayerJoined;
            [SerializeField] private string m_stringToPassOnLeave;
            [SerializeField] private UnityEvent<string, float> m_onPlayerLeft;

            [SerializeField] private Animator[] m_playerIndis;

            public void Awake()
            {
                Time.timeScale = 1;
                GameManager.ResetScore();
                for(int i = 0; i < ControllerManager.Instance.ControllerCount && i < m_playerIndis.Length; i++)
                {
                    m_playerIndis[i].SetTrigger(m_stringToPassOnJoin);
                }
            }
            private void Update()
            {
                if(ControllerManager.Instance.ControllerCount > m_lastPlayerCount)
                {
                    //controller added
                    m_playerIndis[ControllerManager.Instance.MostRecentID].SetTrigger(m_stringToPassOnJoin);
                }
                else if(ControllerManager.Instance.ControllerCount < m_lastPlayerCount)
                {
                    //controller removed
                    m_playerIndis[ControllerManager.Instance.MostRecentID].SetTrigger(m_stringToPassOnLeave);
                }
                m_lastPlayerCount = (int)ControllerManager.Instance.ControllerCount;
            }
            public void TriggerSceneChange()
            {
                if (ControllerManager.Instance.ControllerCount == 2)
                    StartCoroutine(_changeScene());
                else
                {
                    Debug.LogWarning("Not enough controllers connected.");
                }
            }
            private IEnumerator _changeScene()
            {
                //unload the current scene - this won't affect this object
                //SceneManager.UnloadSceneAsync(gameObject.scene);
                //load the next scene
                SceneManager.LoadSceneAsync(m_sceneName);
                yield return 0;
            }
            public void QuitApp()
            {
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#else
                Application.Quit();
#endif
            }
        }
    }
}