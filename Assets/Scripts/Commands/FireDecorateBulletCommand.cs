using UnityEngine;

public class FireDecorateBulletCommand : ICommand
{

    private ObjectPool<Bullet> _bulletPool;
    public PlayerController player;
    bool multiplier;

    public IAbilityActor actor { get; private set; }

    public FireDecorateBulletCommand(ObjectPool<Bullet> bulletPool, PlayerController player, bool multiplier = true)
    {
        this._bulletPool = bulletPool;
        this.player = player;
        this.multiplier = multiplier;
    }

   
    public void Execute()
    {
        DecorateBullet();
    }

    private void DecorateBullet()
    {
        _bulletPool.RequestObject()?.Decorate(new ElementDecorator(ElementalTypes.Fire, player.bonusFireDamage + player.baseDamage, Color.red, multiplier));
    }
}
