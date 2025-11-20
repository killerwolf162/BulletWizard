public class FireballAbility : AbilityBase, ICommand
{
    public IAbilityActor actor { get; private set; }

    protected override float cooldownTime => 3f;

    public FireballAbility(IAbilityActor actor)
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
        Fireball fireball = new Fireball(actor.GameObject().transform.position, actor.GetAimDirection());
    }
}
