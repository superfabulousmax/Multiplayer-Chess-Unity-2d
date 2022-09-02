using System;
using UnityEngine;

[Serializable]
public class Context
{
    [SerializeField]
    int currentIndex;

    State[] turns;
    public State CurrentState { get => turns[currentIndex]; set => turns[currentIndex] = value; }
    public int CurrentIndex { get => currentIndex; set => currentIndex = value; }

    public Context (params State [] states)
    {
        currentIndex = 0;
        turns = new State[states.Length];
        for (uint i = 0; i < states.Length; ++i)
        {
            turns[i] = states[i];
        }
    }

    public int RequestNext()
    {
        return (currentIndex + 1) % turns.Length;
    }
}
