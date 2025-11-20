using UnityEngine;

public class UnDecorator : BulletDecorator
{
    private ElementalBulletTypes bulletType;
    private Color color;

    public UnDecorator(ElementalBulletTypes bulletType, int damage, Color color)
    {
        this.bulletType = bulletType;
        this.damage = damage;
        this.color = color;

    }

    public override IBullet Decorate(IBullet bullet)
    {
        if (bullet.elementalBulletTypes.Contains(bulletType)) // checks if bullet is standard
        {
            Debug.Log("Bullet is already normal"); 
            return bullet;
        }
        else
        {
            bullet.elementalBulletTypes.Clear(); // clear all decorations, reset to standard
            bullet.elementalBulletTypes.Add(bulletType);
            bullet.damage -= damage;
            bullet.color = color;
            Debug.Log("Bullet Reset to normal");
            return bullet;
        }
    }
}

