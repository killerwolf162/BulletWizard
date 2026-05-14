using UnityEngine;

public class IceDecorateBulletCommand : ICommand
{

    private ObjectPool<Bullet> _bulletPool;
    public PlayerController player;
    public IAbilityActor actor { get; private set; }
    public IceDecorateBulletCommand(ObjectPool<Bullet> bulletPool, PlayerController player)
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
        //_bulletPool.RequestObject()?.Decorate(new ElementDecorator(ElementalTypes.Ice, player.bonusIceDamage + player.baseDamage, Color.blue, 5f));
    }
}
