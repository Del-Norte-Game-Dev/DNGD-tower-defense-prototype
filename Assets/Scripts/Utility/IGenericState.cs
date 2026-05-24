using UnityEngine;

public interface IGenericState<TContext>
{
    public void Enter(TContext context);
    public void Exit(TContext context);
    public void Tick(TContext context, float deltaTime);
    public void FixedTick(TContext context, float fixedDletaTime);
}
