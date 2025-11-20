using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : IStateRunner, ISceneObject, IShooter
{
    [Header("General")]
    private Rigidbody2D rb;
    private Collider2D col;
    private EnemySpawner _spawner;
    public GameObject gameobject { get; private set; }
    private ElementalTypes _elementType;

    [Header("StateMachine")]
    private StateMachine<EnemyBehaviour> stateMachine;
    public ScratchPad sharedData => new ScratchPad();
    public EnemyIdle idleState { get; private set; } = new EnemyIdle();
    public EnemyPatrol patrolState { get; private set; } = new EnemyPatrol();
    public EnemyChase chaseState { get; private set; } = new EnemyChase();
    public EnemyAttack attackState { get; private set; } = new EnemyAttack();

    [Header("Checks")]
    const int PLAYER_LAYER = 3;
    public float chaseRange = 4f;
    public float attackRange = 1f;
    public bool inChaseRange;
    public bool inAttackRange;

    [Header("AI")]
    public Transform _player;

    [Header("Stats")]
    private string _name;
    private float _speed;
    private int _damage;
    private int _health;
    private int _bulletSpeed;

    [Header("BulletPool")]
    private ObjectPool<Bullet> _bulletPool = new ObjectPool<Bullet>(new List<Bullet>() { });
    private int _spellCount = 2;

    //constructor
    public EnemyBehaviour(ElementalTypes type, EnemySpawner spawner, GameObject gameobject, Transform Player, EnemyData data, Vector2 position)
    {
        _player = Player;
        _elementType = type;
        _name = data.enemyName;
        _speed = data.moveSpeed;
        _damage = data.damage;
        _health = data.health;
        _bulletSpeed = 2;
        _spawner = spawner;
        this.gameobject = gameobject;
        gameobject.transform.position = position;

        Start();
    }

    public virtual void Start()
    {
        GameObject bulletObject = Resources.Load("spooderBullet", typeof(GameObject)) as GameObject;
        if (_bulletPool._inactivePool == null)
        {
            _bulletPool._inactivePool = new List<Bullet>();
        }

        for (int i = 0; i < _spellCount; i++)
        {
            var bulletGO = GameHandler.instance.InstantiateNew(bulletObject);
            var bullet = CreateBullet(bulletGO);

            if (_elementType == ElementalTypes.Fire)
                bullet.Decorate(new ElementDecorator(ElementalTypes.Fire, _damage, Color.red, true));
            else if (_elementType == ElementalTypes.Ice)
                bullet.Decorate(new ElementDecorator(ElementalTypes.Ice, _damage, Color.blue, true));

            _bulletPool._inactivePool.Add(bullet);
        }

        rb = gameobject.GetComponent<Rigidbody2D>();
        col = gameobject.GetComponent<Collider2D>();

        GameHandler.instance.Subscribe(this);

        //initialize statemachine and entry state
        stateMachine = new StateMachine<EnemyBehaviour>(this);
        stateMachine.SetState(idleState);

        foreach (Bullet bullet in _bulletPool._inactivePool)
        {
            bullet.OnDie += OnBulletDie;
            bullet.Start();
        }
    }

    public virtual void Update()
    {
        if (gameobject == null)
        {
            GameHandler.instance.UnSubscribe(this);
        }

        //update loop statemachine
        stateMachine?.Update();
        CheckPlayerInRange();
        if (col != null)
        {
            OnCollisionEnter2D(col);
        }
    }

    //check if the enemy gives chase or attacks
    public void CheckPlayerInRange()
    {
        if (Vector3.Distance(gameobject.transform.position, _player.transform.position) < attackRange)
            stateMachine.SetState(attackState);
        else if (Vector3.Distance(gameobject.transform.position, _player.transform.position) < chaseRange)
            stateMachine.SetState(chaseState);
    }

    private void TakeDamage(int amount)
    {
        _health -= amount;
        if (_health <= 0)
            Die();
    }

    public GameObject GameObject()
    {
        return gameobject;
    }

    public Bullet CreateBullet(GameObject bulletObject)
    {
        Bullet bullet = new Bullet(bulletObject, this, _damage, bulletObject.GetComponent<SpriteRenderer>().color, 5);
        bullet.gameobject.name = "SpiderBullet";
        return bullet;
    }

    public void OnEnemeyShootBullet()
    {
        _bulletPool.ActivateItem(_bulletPool.RequestObject())?.OnEnableObject();
    }

    public void OnBulletDie(Bullet _bullet)
    {
        if (_bulletPool != null)
            _bulletPool.ReturnItemToPool(_bullet);
    }

    public Vector2 GetAimDirection()
    {
        var playerPos = _player.transform.position;
        var directionToGive = playerPos - gameobject.transform.position;

        return directionToGive;
    }

    public Quaternion GetBulletRotation()
    {
        var playerPos = _player.transform.position;
        Vector3 rotation = playerPos - gameobject.transform.position;
        float zRotation = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

        return Quaternion.Euler(0, 0, zRotation - 90);
    }

    private void Die()
    {
        _spawner.ownedEnemies.Remove(this);
        GameHandler.instance.UnSubscribe(this);

        _bulletPool?.DestroyAll(b =>
        {
            GameHandler.instance.DestroyObject(b.gameobject);
        });
        _bulletPool = null;
        GameHandler.instance.DestroyObject(gameobject);
    }

    private void OnCollisionEnter2D(Collider2D collider)
    {
        List<Collider2D> overlappingColliders = new List<Collider2D>();
        ContactFilter2D enemyFilter = new ContactFilter2D();

        collider.Overlap(enemyFilter, overlappingColliders);

        foreach (Collider2D otherCollider in overlappingColliders)
        {

            if (otherCollider.tag == "Fireball")
            {
                GameHandler.instance.UnSubscribe(this);
                GameHandler.instance.DestroyObject(gameobject);
            }
            if (otherCollider.tag == "Bullet")
            {
                if (Bullet.collLookup.TryGetValue(otherCollider, out var bullet))
                {
                    if (bullet.elementalBulletTypes.Contains(ElementalTypes.Normal))
                    {
                        TakeDamage(bullet.damage);
                        bullet.Die();
                    }
                    else if (bullet.elementalBulletTypes.Contains(_elementType))
                    {
                        bullet.Die();
                        Debug.Log("The enemy has the same element, it does no damage!");
                    }
                    else if (!bullet.elementalBulletTypes.Contains(_elementType))
                    {
                        TakeDamage(bullet.damage*2);
                        bullet.Die();
                        Debug.Log("The enemy has a different element, it does 2x damage!");
                    }



                }
            }
        }
    }
}


