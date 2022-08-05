using UnityEngine;

public class Context
{
    int currentIndex;
    State[] turns;
    public State CurrentState { get => turns[currentIndex]; set => turns[currentIndex] = value; }

    public Context (params State [] states)
    {
        currentIndex = 0;
        turns = new State[states.Length];
        for (uint i = 0; i < states.Length; ++i)
        {
            turns[i] = states[i];
        }
    }

    public void RequestNext()
    {
        currentIndex = (currentIndex + 1) % turns.Length;
        Debug.Log($"{CurrentState.PlayerColourState} turn now");
    }
}
