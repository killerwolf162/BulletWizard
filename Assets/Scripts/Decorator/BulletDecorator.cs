public abstract class BulletDecorator
{
    public int damage { get; set; }

    public BulletDecorator()
    {

    }

    public abstract IBullet Decorate(IBullet bullet);

}

