public class FireballAbility : AbilityBase, ICommand
{
    public IAbilityActor actor { get; private set; }
    protected override float cooldownTime => 3f;
    private PlayerController _player;

    public FireballAbility(PlayerController player)
    {
        actor = player;
        _player = player;
        manaCost = _player.fireballManaCost;
        Start();
    }

    public void Execute()
    {
        base.UseAbility();
    }

    public override void Cast()
    {
        if (_player.Mana < manaCost) return;

        _player.ModifyMana(-manaCost);
        activeCooldown = cooldownTime;
        Fireball fireball = new Fireball(actor.GameObject().transform.position, actor.GetAimDirection());
    }
}
