using ILOVEYOU.Environment;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

namespace ILOVEYOU
{
    namespace Management
    {

        public class MenuManager : MonoBehaviour
        {
            [SerializeField] private string m_gameManagerSceneName;
            [SerializeField] private string m_playerSceneName;

            [SerializeField] private UnityEvent<int> m_EventOnJoin;
            [SerializeField] private UnityEvent m_OnStartGameFail;
            [SerializeField] private UnityEvent m_OnLoadSceneStart;
            [SerializeField] private UnityEvent m_OnSceneLoaded;
            public void OnPlayerJoined(PlayerInput input)
            {
                GameManager.AddPlayerInput(input);
                m_EventOnJoin.Invoke(GameManager.NumberOfPlayers);
                Debug.Log($"PLAYER JOINED // TOTAL = {GameManager.NumberOfPlayers}");
            }
            public void TriggerStart()
            {
                if (GameManager.NumberOfPlayers >= 2)
                    StartCoroutine(_startGame());
                else
                    m_OnStartGameFail.Invoke();
            }
            private IEnumerator _startGame()
            {
                m_OnLoadSceneStart.Invoke();
                SceneManager.LoadScene(m_gameManagerSceneName, LoadSceneMode.Additive);
                //Load scenes for each player
                for(int i = 0; i < GameManager.NumberOfPlayers; i++)
                {
                    SceneManager.LoadSceneAsync(m_playerSceneName, LoadSceneMode.Additive);
                }
                //wait till the end of frame when the scenes have been loaded
                yield return new WaitForSeconds(0.1f);

                LevelManager[] managers = FindObjectsOfType<LevelManager>();
                for (int i = 0; i < GameManager.NumberOfPlayers; i++)
                {
                    //start level manager
                    if (!managers[i].Startup(i))
                    {
                        //manager failed
                        Debug.LogError($"{managers[i]} has failed, aborting...");
                        Destroy(this);
                        yield return null;
                    }
                }

                GameManager.Instance.SetInstances(managers);

                m_OnSceneLoaded.Invoke();
                //levels have been loaded!!

                yield return null;
            }
            public void UnloadThisScene()
            {
                SceneManager.UnloadSceneAsync(0);
            }

            private void Update()
            {
               
            }
        }
    }
}
