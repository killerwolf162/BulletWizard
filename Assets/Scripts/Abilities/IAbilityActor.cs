using UnityEngine;

public interface IAbilityActor
{
    public abstract GameObject GameObject();
    public abstract Vector2 GetAimDirection();
    public abstract Vector2 MoveDirection();
}
