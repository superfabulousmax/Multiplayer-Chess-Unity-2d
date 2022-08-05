
public abstract class State
{
    public abstract PlayerColour PlayerColourState { get; }
    public abstract bool Handle();
}
