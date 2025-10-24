using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状态基类
/// </summary>
/// <typeparam name="T">哪一类人的状态(Enemy/Player)</typeparam>
public class State<T>: MonoBehaviour
{
    public virtual void Enter(T owner) { }

    public virtual void Execute() { }

    public virtual void Exit() { }
}
