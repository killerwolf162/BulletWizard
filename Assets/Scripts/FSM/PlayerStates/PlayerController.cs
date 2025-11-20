using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerController : IStateRunner, ISceneObject, IAbilityActor, IShooter
{
    [Header("StateMachine")]
    public StateMachine<PlayerController> stateMachine;
    public ScratchPad sharedData => new ScratchPad();
    public PlayerIdle idleState { get; private set; } = new PlayerIdle();
    public PlayerMove moveState { get; private set; } = new PlayerMove();

    public GameObject gameobject { get; private set; }

    public Rigidbody2D rb;
    public Collider2D col;

    private ObjectPool<Bullet> _bulletPool = new ObjectPool<Bullet>(new List<Bullet>() { });

    private InputHandler _inputHandler = new InputHandler();

    private Camera _cam;
    private Vector3 _mousePos;

    private float _damageTimeOut;

    private int _spellCount = 15;
    private int _health = 10;

    public int bonusFireDamage = 1;
    public int bonusIceDamage = 1;
    public int baseDamage = 1;


    public PlayerController(GameObject gameobject)
    {
        this.gameobject = gameobject;
        rb = gameobject.GetComponent<Rigidbody2D>();
        col = gameobject.GetComponent<Collider2D>();
        Start();
    }

    public virtual void Start()
    {
        GameObject bulletObject = Resources.Load("Bullet", typeof(GameObject)) as GameObject;

        if (_bulletPool._inactivePool == null)
        {
            _bulletPool._inactivePool = new List<Bullet>(); // or correct type
        }

        for (int i = 0; i < _spellCount; i++)
        {
            var bulletGO = GameHandler.instance.InstantiateNew(bulletObject);
            var bullet = CreateBullet(bulletGO);
            _bulletPool._inactivePool.Add(bullet);
        }

        GameHandler.instance.Subscribe(this);
        _cam = GameHandler.instance.cam;

        //initialize statemachine and entry state
        stateMachine = new StateMachine<PlayerController>(this);
        stateMachine.SetState(idleState);

        //initialize bullet pool at start
        foreach (Bullet bullet in _bulletPool._inactivePool)
        {
            bullet.OnDie += OnBulletDie;
            bullet.Start();
        }

        //initialize input bindings
        //_inputHandler.BindKeyToCommand(KeyCode.Space, KeypressType.Down, new DashAbility(this));
        _inputHandler.BindKeyToCommand(KeyCode.Space, KeypressType.Down, new FireballAbility(this));
        _inputHandler.BindKeyToCommand(KeyCode.Alpha2, KeypressType.Down, new FireDecorateBulletCommand(_bulletPool, this));
        _inputHandler.BindKeyToCommand(KeyCode.Alpha3, KeypressType.Down, new IceDecorateBulletCommand(_bulletPool, this));
        _inputHandler.BindKeyToCommand(KeyCode.Alpha1, KeypressType.Down, new UnDecorateBulletCommand(_bulletPool, this));
        _inputHandler.BindKeyToCommand(KeyCode.Mouse0, KeypressType.Down, new ShootBulletCommand(_bulletPool));
    }

    public virtual void Update()
    {
        _inputHandler.HandleInput();

        _damageTimeOut -= Time.deltaTime;

        //update loop statemachine
        stateMachine?.Update();

        if (col != null)
        {
            OnCollisionEnter2D(col);
        }
        SetCameraPosition();
    }

    private void TakeDamage(int amount)
    {
        _health -= amount;
        Debug.Log($" damage taken: {amount}, current health: {_health}");
        if (_health <= 0)
            Die();
    }

    private void Die()
    {
        GameHandler.instance.UnSubscribe(this);
        GameHandler.instance.DestroyObject(gameobject);
    }


    public void OnBulletDie(Bullet _bullet)
    {
        _bulletPool.ReturnItemToPool(_bullet);
    }

    public GameObject GameObject()
    {
        return gameobject;
    }

    public Bullet CreateBullet(GameObject bulletObject)
    {
        Bullet bullet = new Bullet(bulletObject, this, baseDamage, Color.black, 15);
        return bullet;
    }

    public Vector2 GetAimDirection()
    {
        _mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        var directionToGive = _mousePos - gameobject.transform.position;

        return directionToGive;
    }

    public Quaternion GetBulletRotation()
    {
        _mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 rotation = _mousePos - gameobject.transform.position;
        float zRotation = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

        return Quaternion.Euler(0, 0, zRotation - 90);
    }

    public Vector2 MoveDirection()
    {
        Vector2 moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        return moveDir;
    }

    public void SetCameraPosition()
    {
        GameHandler.instance.cam.transform.position = gameobject.transform.position + new Vector3(0, 0, -10);
    }

    private void OnCollisionEnter2D(Collider2D collider)
    {
        List<Collider2D> overlappingColliders = new List<Collider2D>();
        ContactFilter2D enemyFilter = new ContactFilter2D();

        collider.Overlap(enemyFilter, overlappingColliders);

        foreach (var otherCollider in overlappingColliders)
        {
            if (otherCollider.tag == "EnemyBullet")
            {
                if (Bullet.collLookup.TryGetValue(otherCollider, out var bullet))
                {
                    TakeDamage(bullet.damage);
                    bullet.Die();
                }
            }
        }
    }
}
