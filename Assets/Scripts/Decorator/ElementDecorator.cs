using UnityEngine;

public class ElementDecorator : BulletDecorator
{
    private ElementalTypes _bulletType;
    private Color _color;
    private bool _multiplier;

    public override Color Color => _color;

    public ElementDecorator(ElementalTypes bulletType, int damage, Color color, bool multiplier = false)
    {
        this._bulletType = bulletType;
        this.damage = damage;
        this._color = color;
        this._multiplier = multiplier;
    }

    public override IBullet Decorate(IBullet bullet)
    {
        if (bullet.elementalBulletTypes.Contains(_bulletType))
        {
            return bullet;
        }
        if (bullet.elementalBulletTypes.Contains(ElementalTypes.Normal)) // check if bullet is not decorated
        {
            bullet.elementalBulletTypes.Add(_bulletType);
            bullet.elementalBulletTypes.Remove(ElementalTypes.Normal);
            if(_multiplier)
                bullet.damage += damage;
            bullet.color = _color;
            return bullet;
        }

        else // if bullet is already decorted with same or other decoration, return
        {
            bullet.elementalBulletTypes.Clear();
            bullet.elementalBulletTypes.Add(_bulletType);
            bullet.color = _color;
            return bullet;
        }      
    }
}

