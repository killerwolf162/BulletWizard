using System;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : IBullet, ISceneObject
{
    private const int WALL_LAYER_MASK = 1 << 7;

    public static readonly Dictionary<Collider2D, Bullet> collLookup = new Dictionary<Collider2D, Bullet>();

    public HashSet<ElementalTypes> elementalBulletTypes { get; set; } = new HashSet<ElementalTypes>() { ElementalTypes.Normal };
    public int damage { get; set; }
    public Color color { get; set; }
    public bool active { get; set; }

    public event Action<Bullet> OnDie;

    public GameObject gameobject => bullet;
    public GameObject bullet;

    private IShooter _shooter;

    private Rigidbody2D _rig;
    private SpriteRenderer _rend;
    private Collider2D _col;

    private float timer = 0f;
    private float timeOutTime = 2f;
    private int _bulletSpeed;

    public Bullet(GameObject bulletPrefab, IShooter shooter, int damage, Color color, int bulletSpeed = 2)
    {
        bullet = bulletPrefab;
        this.damage = damage;
        this.color = color;
        _bulletSpeed = bulletSpeed;
        _col = bullet.GetComponent<Collider2D>();
        collLookup.Add(_col, this);
        _shooter = shooter;
    }

    public void Start()
    {
        OnDisableObject();

        if (_rig == null)
            _rig = bullet.GetComponent<Rigidbody2D>();
        if (_rend == null)
            _rend = bullet.GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer > timeOutTime)
        {
            Die();
        }

        if (_col != null)
            CheckWallHit();
    }

    public void Die()
    {
        OnDie?.Invoke(this);
    }

    public void Decorate(BulletDecorator decorator)
    {
        decorator.Decorate(this);
    }

    public void OnEnableObject()
    {
        bullet.SetActive(true);
        collLookup[_col] = this;
        GameHandler.instance.Subscribe(this);

        bullet.transform.position = _shooter.GameObject().transform.position;
        bullet.transform.rotation = _shooter.GetBulletRotation();

        _rend.color = color;
        _rig.AddForce(_shooter.GetAimDirection().normalized * _bulletSpeed, ForceMode2D.Impulse);
    }

    public void OnDisableObject()
    {
        GameHandler.instance.UnSubscribe(this);

        if (_col != null)
            collLookup.Remove(_col);

        bullet.SetActive(false);

        timer = 0;
    }

    private void CheckWallHit()
    {
        if (_col.IsTouchingLayers(WALL_LAYER_MASK))
        {
            Die();
        }
    }
}
