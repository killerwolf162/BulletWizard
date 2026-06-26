using UnityEngine;

public class ElementDecorator : BulletDecorator
{
    private ElementalTypes _bulletType;
    private Color _color;
    private float _bulletSpeed;

    public override Color Color => _color;

    public ElementDecorator(ElementalTypes bulletType, int damage, Color color, float bulletSpeed)
    {
        _bulletType = bulletType;
        this.damage = damage;
        _color = color;
        _bulletSpeed = bulletSpeed;
    }

    public override IBullet Decorate(IBullet bullet)
    {
            bullet.elementalBulletTypes.Clear();
            bullet.elementalBulletTypes.Add(_bulletType);
            bullet.damage = damage;
            bullet.bulletSpeed = _bulletSpeed;
            bullet.color = _color;
            return bullet;
    }
}

