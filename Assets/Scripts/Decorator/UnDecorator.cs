using UnityEngine;

public class UnDecorator : BulletDecorator
{
    private ElementalTypes bulletType;
    private Color color;

    public UnDecorator(ElementalTypes bulletType, int damage, Color color)
    {
        this.bulletType = bulletType;
        this.damage = damage;
        this.color = color;

    }

    public override IBullet Decorate(IBullet bullet)
    {
        if (bullet.elementalBulletTypes.Contains(bulletType)) // checks if bullet is standard
        {
            return bullet;
        }
        else
        {
            bullet.elementalBulletTypes.Clear(); // clear all decorations, reset to standard
            bullet.elementalBulletTypes.Add(bulletType);
            bullet.damage -= damage;
            bullet.color = color;
            return bullet;
        }
    }
}

