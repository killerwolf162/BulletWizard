using UnityEngine;

public abstract class BulletDecorator
{
    public int damage { get; set; }
    public abstract Color Color { get; }

    public BulletDecorator()
    {

    }

    public abstract IBullet Decorate(IBullet bullet);

}

