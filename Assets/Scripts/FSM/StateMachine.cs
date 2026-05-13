public class StateMachine<T> where T : IStateRunner
{
    private T owner;
    private IState<T> currentState;
    public IState<T> CurrentState => currentState;

    public StateMachine(T _owner)
    {
        owner = _owner;
    }

    public void Update()
    {
        if (currentState != null)
        {
            currentState.Update(owner);
        }
    }

    public void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.FixedUpdate(owner);
        }
    }

    public void SetState(IState<T> newState)
    {
        if (currentState != null)
        {
            currentState.Complete(owner);
            currentState.onSwitch -= SetState;
        }

        newState.Start(owner);
        newState.onSwitch += SetState;

        currentState = newState;
    }
}