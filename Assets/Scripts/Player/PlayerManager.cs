using ILOVEYOU.Cards;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ILOVEYOU
{
    namespace Player
    {
        public class PlayerManager : MonoBehaviour
        {
            private List<Task> m_tasks = new List<Task>();
            public int NumberOfTasks { get { return m_tasks.Count; } }
            [HideInInspector] public int TaskCompletionPoints;
            private DisruptCard[] m_cardsHeld;
            public bool CardsInHand { get { return m_cardsHeld.Length > 0; } }
            [SerializeField] private float m_cardTimeout;
            [SerializeField] private float m_baseCardWidth;
            public bool Startup()
            {
                //Reset variables
                TaskCompletionPoints = 0;
                m_cardsHeld = new DisruptCard[0];
                Debug.Log("PlayerManager started successfully");
                return true;
            }
            #region Task Management
            /// <summary>
            /// Creates a new class to add to the list
            /// </summary>
            /// <param name="type"></param>
            /// <param name="cap"></param>
            /// <returns></returns>
            public bool AddTask(TaskType type, float cap)
            {
                m_tasks.Add(new Task(type, cap));
                VerifyTaskList();
                return true;
            }
            /// <summary>
            /// Checks if any tasks are complete and removes them from the list
            /// </summary>
            /// <returns></returns>
            private bool VerifyTaskList()
            {
                List<Task> completeTasks = new List<Task>();
                //Get a list of all the completed tasks
                foreach(Task task in m_tasks)
                {
                    if (task.IsComplete)
                        completeTasks.Add(task);
                }
                //Remove those completed tasks from the main list
                foreach(Task task in completeTasks)
                {
                    m_tasks.Remove(task);
                    TaskCompletionPoints++;
                }
                return true;
            }
            /// <summary>
            /// Returns all the indexes of tasks matching the given type
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public int[] GetMatchingTasks(TaskType type)
            {
                //Create a list of indexes
                List<int> indexes = new List<int>();
                //If the type of the current iteration matches the requested type...
                //...saved its index
                for (int i = 0; i < m_tasks.Count; i++)
                {
                    if (m_tasks[i].GetTaskType == type)
                    {
                        indexes.Add(i);
                    }
                }
                return indexes.ToArray();
            }
            /// <summary>
            /// Updates the value of a task at a given index
            /// </summary>
            /// <param name="index"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool UpdateTask(int index, float value)
            {
                bool x = m_tasks[index].UpdateTask(value);
                VerifyTaskList();
                return x;
            }
            /// <summary>
            /// Resets the task at the given index
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public bool ResetTask(int index)
            {
                bool x = m_tasks[index].ResetTask();
                VerifyTaskList();
                return x;
            }
            /// <summary>
            /// Updates any kill type tasks
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool UpdateKillTrackers(float value)
            {
                foreach (Task task in m_tasks)
                {
                    if (task.GetTaskType == TaskType.Kills)
                        task.UpdateTask(value);
                }
                VerifyTaskList();
                return true;
            }

            /// <summary>
            /// Updates any time type tasks
            /// </summary>
            /// <param name="doReset"></param>
            /// <returns></returns>
            public bool UpdateTimers(bool doReset)
            {
                foreach (Task task in m_tasks)
                {
                    if (task.GetTaskType == TaskType.Time)
                    {
                        if (doReset)
                            task.ResetTask();
                        else
                            task.UpdateTask(Time.deltaTime);
                    }
                }
                VerifyTaskList();
                return true;
            }
            #endregion
            #region Card Management
            /// <summary>
            /// Save a given hand as the cards held by this player
            /// </summary>
            /// <param name="cards"></param>
            public void CollectHand(DisruptCard[] cards)
            {
                Debug.Log("Hand dealt, setting up cards.");
                //Copy the given array to this hand
                m_cardsHeld = new DisruptCard[cards.Length];
                cards.CopyTo(m_cardsHeld, 0);
                //Set up each card
                foreach(DisruptCard card in m_cardsHeld)
                {
                    Debug.Log("Readying discard function to card.");
                    card.transform.SetParent(transform);
                    card.m_playerHandToDiscard.AddListener(delegate { DiscardHand(); });
                }
                //To stop stockpiling, delete the cards after a set time
                Invoke("DiscardHand", m_cardTimeout);
                DisplayHand(cards);
            }
            /// <summary>
            /// Destroys the player's hand card objects
            /// </summary>
            public void DiscardHand()
            {
                foreach(DisruptCard card in m_cardsHeld)
                {
                    Destroy(card.gameObject);
                }
                m_cardsHeld = new DisruptCard[0];
            }
            /// <summary>
            /// Takes a player's input to select a card
            /// </summary>
            /// <param name="value"></param>
            public void OnSelectCard(InputValue value)
            {
                //If the hand is empty, don't continue
                if (!CardsInHand)
                    return;
                //Get the vector of the face buttons
                Vector2 selection = value.Get<Vector2>();
                Debug.Log($"The inputed value {selection}");
                //This index will be used to choose a card
                int index = -1;
                switch (selection)
                {
                    //Top card / Y
                    case Vector2 v when v.Equals(new Vector2(0.0f, 1.0f)):
                        index = 1;
                        break;
                    //Left card / X
                    case Vector2 v when v.Equals(new Vector2(-1.0f, 0.0f)):
                        index = 0;
                        break;
                    //Right card / B
                    case Vector2 v when v.Equals(new Vector2(1.0f, 0.0f)):
                        index = 2;
                        break;
                }
                //Trigger the effects of the chosen card.
                m_cardsHeld[index].Trigger();
            }

            private void DisplayHand(DisruptCard[] targets)
            {
                Transform camera = GetComponentInChildren<Camera>().transform;
                bool isOdd = targets.Length % 2 == 1;

                float offset = 0;
                if (!isOdd)
                    offset = m_baseCardWidth / 2;

                for (int i = -targets.Length / 2; i <= targets.Length / 2; i++)
                {
                    targets[i + targets.Length / 2].transform.localPosition = Vector3.zero;
                    targets[i + targets.Length / 2].transform.localPosition += new Vector3(m_baseCardWidth * i,0,0);
                    if(i < 0)
                    {
                        targets[i + targets.Length / 2].transform.localPosition -= new Vector3(offset,0,0);
                    }
                    if(i > 0)
                    {
                        targets[i + targets.Length / 2].transform.localPosition += new Vector3(offset,0,0);
                    }
                }

/*                float totalX = 0;
                foreach (Transform pos in targets)
                {
                    totalX += pos.sizeDelta.x * pos.localScale.x;
                }

                float indivX = totalX / targets.Length;

                    if (!isOdd && i == 0)
                        i++;
                    targets[i].anchoredPosition = new Vector2(indivX * i, targets[i].anchoredPosition.y);
                    if (isOdd)
                    {
                        if(i < 0)
                        {
                            targets[i].anchoredPosition
                        }
                    }
                }*/


            }
            #endregion
        }
    }
}