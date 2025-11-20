using UnityEngine;

public class FireDecorateBulletCommand : ICommand
{

    private ObjectPool<Bullet> _bulletPool;
    public PlayerController player;

    public IAbilityActor actor { get; private set; }

    public FireDecorateBulletCommand(ObjectPool<Bullet> bulletPool, PlayerController player)
    {
        this._bulletPool = bulletPool;
        this.player = player;
    }

   
    public void Execute()
    {
        DecorateBullet();
    }

    private void DecorateBullet()
    {
        _bulletPool.RequestObject()?.Decorate(new ElementDecorator(ElementalBulletTypes.Fire, player.bonusFireDamage + player.baseDamage, Color.red)); // replace fireDamage with actor.fireDamage(stored in player(?)) so its easier to change values later.
    }
}
