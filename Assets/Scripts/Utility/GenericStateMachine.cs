using System;

public class GenericStateMachine<IGenericState, TContext> where IGenericState : IGenericState<TContext>
{
    private IGenericState currentState;

    public void Initialize(IGenericState startingState, TContext context)
    {
        currentState = startingState;
        currentState?.Enter(context);
    }

    public void ChangeState(IGenericState newState, TContext context)
    {
        currentState?.Exit(context);
        // Debug.Log($"Changed from {currentState.ToString()} to {newState.ToString()}");
        currentState = newState;
        currentState?.Enter(context);
    }

    public void Tick(TContext context, float deltaTime)
    {
        currentState?.Tick(context, deltaTime);
    }

    public void FixedTick(TContext context, float fixedDeltaTime)
    {
        currentState?.FixedTick(context, fixedDeltaTime);
    }

    public string GetStateName()
    {
        return currentState.ToString();
    }

    ///<summary>
    ///return concrete state type
    ///</summary>
    public Type GetStateType()
    {
        return currentState.GetType();
    }
}
