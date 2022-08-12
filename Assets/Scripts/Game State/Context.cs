using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class Context
{
    [SerializeField]
    NetworkVariable<int> currentIndex = new(0,  writePerm : NetworkVariableWritePermission.Owner);

    State[] turns;
    public State CurrentState { get => turns[currentIndex.Value]; set => turns[currentIndex.Value] = value; }
    public int CurrentIndex { get => currentIndex.Value; set => currentIndex.Value = value; }

    public Context (params State [] states)
    {
        turns = new State[states.Length];
        for (uint i = 0; i < states.Length; ++i)
        {
            turns[i] = states[i];
        }
    }

    public int RequestNext()
    {
        return (currentIndex.Value + 1) % turns.Length;
    }
}
