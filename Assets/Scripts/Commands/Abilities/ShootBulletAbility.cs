using UnityEngine;
public class ShootBulletAbility : AbilityBase, ICommand

{

    private ObjectPool<Bullet> _bulletPool;
    private PlayerController _player;
    protected override float cooldownTime => 0.5f;

    public IAbilityActor actor { get; private set; }

    public ShootBulletAbility(ObjectPool<Bullet> bulletPool, PlayerController player)
    {
        _bulletPool = bulletPool;
        _player = player;
        Start();
    }

    public override void Cast()
    {
        var bullet = _bulletPool.RequestObject();
        if (bullet == null) return;
        if (_player.activeDecorator == null) return;

        _player.activeDecorator?.Decorate(bullet);
        _bulletPool.ActivateItem(_bulletPool.RequestObject())?.OnEnableObject();
    }

    public void Execute()
    {
        base.UseAbility();
    }
}
