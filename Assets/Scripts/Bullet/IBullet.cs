using System.Collections.Generic;
using UnityEngine;

public interface IBullet : IPoolable
{
    public int damage { get; set; }

    public Color color { get; set; }

    public HashSet<ElementalTypes> elementalBulletTypes { get; set; }

    public float bulletSpeed { get; set; }

    public void Decorate(BulletDecorator decorator);
}

