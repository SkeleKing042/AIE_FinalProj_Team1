using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SnapInput<Vector2> m_moveAction;
    [SerializeField] private float m_speed;
    void Start()
    {
        //Enable keybinds
        m_moveAction.Bindings.Enable();
    }
    public void Update()
    {
        //Check keybind inputs
        m_moveAction.Check();
    }
    public void MovePlayer(Vector2 dir)
    {
        //Move based on given direction.
        transform.position += new Vector3(dir.x, 0, dir.y) * m_speed * Time.deltaTime;
    }
}
