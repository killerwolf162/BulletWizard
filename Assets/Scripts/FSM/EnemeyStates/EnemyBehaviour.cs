using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : IStateRunner, ISceneObject, IShooter
{
    [Header("General")]
    private Rigidbody2D rb;
    private Collider2D col;
    private AbstractSpawner _spawner;
    public GameObject gameobject { get; private set; }
    private ElementalTypes _elementType;

    [Header("StateMachine")]
    private StateMachine<EnemyBehaviour> _stateMachine;
    public ScratchPad sharedData => new ScratchPad();
    public EnemyIdle idleState { get; private set; } = new EnemyIdle();
    public EnemyPatrol patrolState { get; private set; } = new EnemyPatrol();
    public EnemyChase chaseState { get; private set; } = new EnemyChase();
    public EnemyAttack attackState { get; private set; } = new EnemyAttack();

    [Header("Checks")]
    const int PLAYER_LAYER = 3;
    internal const int WALL_LAYER_MASK = 1 << 7;
    public float chaseRange = 8f;
    public float soundRange = 6f;
    public float attackRange = 6f;
    public float chaseThreshold = 2f;
    public bool hasReachedThreshold;
    public bool inChaseRange;
    public bool inAttackRange;
    private bool _alwaysChase = false;

    [Header("AI")]
    public Transform _player;

    [Header("Stats")]
    public float Speed => _speed;
    private string _name;
    private float _speed;
    private int _damage;
    private int _health;
    private int _score;
    private float _bulletSpeed = 5f;

    [Header("BulletPool")]
    private ObjectPool<Bullet> _bulletPool = new ObjectPool<Bullet>(new List<Bullet>() { });
    private int _spellCount = 5;

    public IReadOnlyList<Vector3> PatrolPoints => _spawner?.PatrolPoints;
    public EnemyBehaviour(ElementalTypes type, AbstractSpawner spawner, GameObject gameobject, Transform Player, EnemyData data, Vector2 position, bool alwaysChase)
    {
        _player = Player;
        _elementType = type;
        _name = data.enemyName;
        _speed = data.moveSpeed;
        _damage = data.damage;
        _health = data.health;
        _score = data.score;
        _spawner = spawner;
        _alwaysChase = alwaysChase;
        this.gameobject = gameobject;
        gameobject.transform.position = position;

        Start();
    }

    public virtual void Start()
    {
        GameHandler.instance.OnPlayerDied += DisableAI;

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
                bullet.Decorate(new ElementDecorator(ElementalTypes.Fire, _damage, Color.red, _bulletSpeed));
            else if (_elementType == ElementalTypes.Ice)
                bullet.Decorate(new ElementDecorator(ElementalTypes.Ice, _damage, Color.blue, _bulletSpeed));

            _bulletPool._inactivePool.Add(bullet);
        }

        rb = gameobject.GetComponent<Rigidbody2D>();
        col = gameobject.GetComponent<Collider2D>();

        GameHandler.instance.Subscribe(this);

        //initialize statemachine and entry state
        _stateMachine = new StateMachine<EnemyBehaviour>(this);
        _stateMachine.SetState(idleState);

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

        Vector3 previousPos = gameobject.transform.position;

        //update loop statemachine.
        CheckPlayerInRange();
        _stateMachine?.Update();


        if (col != null)
        {
            HandleProjectileHits();
        }

    }

    private void DisableAI()
    {
        Debug.Log("DisableAI");
        _player = null;
        _stateMachine.SetState(idleState);
    }

    //check if the enemy gives chase or attacks
    public void CheckPlayerInRange()
    {
        if (gameobject == null || _player == null)
        {
            inChaseRange = false;
            inAttackRange = false;
            hasReachedThreshold = false;
            return;
        }

        float distance = Vector3.Distance(gameobject.transform.position, _player.transform.position);
        bool canSee = distance <= chaseRange && HasLineOfSightToPlayer();
        bool canHear = distance <= soundRange;

        bool shouldChase = canSee || canHear;
        inChaseRange = _alwaysChase || shouldChase;

        inAttackRange = distance <= attackRange && canSee;
        hasReachedThreshold = distance <= chaseThreshold;
    }

    private bool HasLineOfSightToPlayer()
    {
        Vector3 origin = gameobject.transform.position;
        Vector3 target = _player.position;
        Vector3 dir = target - origin;
        float distance = dir.magnitude;

        if (distance <= 0.01f)
            return true;

        dir /= distance;

        // If the ray hits a wall, there is no line of sight.
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, distance, WALL_LAYER_MASK);
        return hit.collider == null;
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
        _spawner?.UnregisterEnemy(this);
        GameHandler.instance.UnSubscribe(this);
        GameHandler.instance.OnPlayerDied -= DisableAI;

        _bulletPool?.DestroyAll(b => { GameHandler.instance.DestroyObject(b.gameobject); });

        _bulletPool = null;
        GameHandler.instance.ModifyScore(_score);
        GameHandler.instance.DestroyObject(gameobject);
    }

    private void HandleProjectileHits()
    {
        var overlappingColliders = new List<Collider2D>();
        var filter = new ContactFilter2D
        {
            useTriggers = true
        };
        col.Overlap(filter, overlappingColliders);

        foreach (Collider2D otherCollider in overlappingColliders)
        {
            if (otherCollider.CompareTag("Fireball"))
            {
                Die();
                return;
            }

            if (otherCollider.CompareTag("Bullet"))
            {
                if (Bullet.collLookup.TryGetValue(otherCollider, out var bullet))
                {
                    if (bullet.elementalBulletTypes.Contains(ElementalTypes.Normal) || bullet.elementalBulletTypes.Contains(ElementalTypes.Air)) //normal and air bullet always do normal dmg
                    {
                        TakeDamage(bullet.damage);
                        bullet.Die();
                        return;
                    }

                    if (_elementType == ElementalTypes.Normal) // if normal spider always take normal bullet dmg
                    {
                        TakeDamage(bullet.damage);
                        bullet.Die();
                        return;
                    }

                    if (bullet.elementalBulletTypes.Contains(_elementType)) // if bullet same element as spider take no dmg
                    {
                        bullet.Die();
                    }
                    else
                    {
                        TakeDamage(bullet.damage * 2); // oterwise bullet opposite element, take double dmg
                        bullet.Die();
                    }
                }
            }
        }
    }
}


