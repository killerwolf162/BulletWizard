using UnityEngine;
public class ShootBulletAbility : AbilityBase, ICommand

{

    private ObjectPool<Bullet> _bulletPool;
    protected override float cooldownTime => 0.5f;

    public IAbilityActor actor { get; private set; }

    public ShootBulletAbility(ObjectPool<Bullet> bulletPool)
    {
        this._bulletPool = bulletPool;
        Start();
    }

    public override void Cast()
    {
        _bulletPool.ActivateItem(_bulletPool.RequestObject())?.OnEnableObject();
    }

    public void Execute()
    {
        base.UseAbility();
    }
}
