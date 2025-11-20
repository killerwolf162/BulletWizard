using UnityEngine;

public class ElementDecorator : BulletDecorator
{
    private ElementalTypes bulletType;
    private Color color;
    private bool multiplier;

    public ElementDecorator(ElementalTypes bulletType, int damage, Color color, bool multiplier = false)
    {
        this.bulletType = bulletType;
        this.damage = damage;
        this.color = color;
        this.multiplier = multiplier;
    }

    public override IBullet Decorate(IBullet bullet)
    {
        if (bullet.elementalBulletTypes.Contains(bulletType))
        {
            return bullet;
        }
        if (bullet.elementalBulletTypes.Contains(ElementalTypes.Normal)) // check if bullet is not decorated
        {
            bullet.elementalBulletTypes.Add(bulletType);
            bullet.elementalBulletTypes.Remove(ElementalTypes.Normal);
            if(multiplier)
                bullet.damage += damage;
            bullet.color = color;
            return bullet;
        }

        else // if bullet is already decorted with same or other decoration, return
        {
            bullet.elementalBulletTypes.Clear();
            bullet.elementalBulletTypes.Add(bulletType);
            bullet.color = color;
            return bullet;
        }      
    }
}

