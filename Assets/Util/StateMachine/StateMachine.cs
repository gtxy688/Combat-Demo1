using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状态管理类
/// </summary>
/// <typeparam name="T">管理哪一类人的状态(Enemy/Player)</typeparam>
public class StateMachine<T>
{
    T _owner;

    public State<T> CurrentState { get; private set; }

    public StateMachine(T owner)
    {
        _owner = owner;
    }

    public void ChangeState(State<T> newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter(_owner);
    }

    public void Execute()
    {
        CurrentState.Execute();
    }
}