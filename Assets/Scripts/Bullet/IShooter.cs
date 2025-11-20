using UnityEngine;

public interface IShooter
{

    public abstract GameObject GameObject();
    public abstract Vector2 GetAimDirection();
    public abstract Quaternion GetBulletRotation();

}

