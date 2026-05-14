using UnityEngine;

public class UnDecorator : BulletDecorator
{
    private ElementalTypes _bulletType;
    private Color _color;
    private float _bulletSpeed;

    public override Color Color => _color;

    public UnDecorator(ElementalTypes bulletType, int damage, Color color, float bulletSpeed)
    {
        this.damage = damage;
        _bulletType = bulletType;
        _color = color;
        _bulletSpeed = bulletSpeed;

    }



    public override IBullet Decorate(IBullet bullet)
    {
        if (bullet.elementalBulletTypes.Contains(_bulletType)) // checks if bullet is standard
        {
            return bullet;
        }
        else
        {
            bullet.elementalBulletTypes.Clear(); // clear all decorations, reset to standard
            bullet.elementalBulletTypes.Add(_bulletType);
            bullet.damage -= damage;
            bullet.color = _color;
            bullet.bulletSpeed = _bulletSpeed;
            return bullet;
        }
    }
}

