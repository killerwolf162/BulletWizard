using UnityEngine;

public class UnDecorateBulletCommand : ICommand
{
    private ObjectPool<Bullet> _bulletPool;

    public PlayerController player;
    public IAbilityActor actor { get; private set; }

    public UnDecorateBulletCommand(ObjectPool<Bullet> bulletPool, PlayerController player)
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
        _bulletPool.RequestObject()?.Decorate(new UnDecorator(ElementalTypes.Normal, player.baseDamage, Color.black)); // replace iceDamage with actor.iceDamage(stored in player(?)) so its easier to change values later.
    }
}
