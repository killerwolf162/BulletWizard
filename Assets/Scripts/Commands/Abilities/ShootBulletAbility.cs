using UnityEngine;
using System;
public class ShootBulletAbility : AbilityBase, ICommand

{
    public IAbilityActor actor { get; private set; }
    protected override float cooldownTime => 0.5f;

    private ObjectPool<Bullet> _bulletPool;
    private PlayerController _player;
 
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
        
        _player.activeDecorator?.Decorate(bullet);

        //check elemental type, adjust manacost
        manaCost = bullet.elementalBulletTypes.Contains(ElementalTypes.Normal)
        ? _player.baseManaCost
        : _player.elementalManaCost;

        if (_player.Mana < manaCost) return;

        _player.ModifyMana(-manaCost);
        activeCooldown = cooldownTime;
        _bulletPool.ActivateItem(bullet)?.OnEnableObject();
    }

    public void Execute()
    {
        if (_player.activeDecorator == null) return;
        base.UseAbility();
    }
}
