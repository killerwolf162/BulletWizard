using UnityEngine;

public class DashAbility : AbilityBase, ICommand
{
    public IAbilityActor actor { get; private set; }

    protected override float cooldownTime => 2f;

    public DashAbility(IAbilityActor actor)
    {
        this.actor = actor;
        Start();
    }

    public void Execute()
    {
        base.UseAbility();
    }

    public override void Cast()
    {
        Debug.Log($"{actor.GameObject().name} is dashing");
        Transform actorTransform = actor.GameObject().transform;
        actorTransform.position += (Vector3)actor.MoveDirection();
    }
}
