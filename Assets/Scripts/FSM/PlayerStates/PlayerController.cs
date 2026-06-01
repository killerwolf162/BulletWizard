using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerController : IStateRunner, ISceneObject, IAbilityActor, IShooter
{
    internal const int WALL_LAYER_MASK = 1 << 7;

    public event Action<int> ManaChanged;
    public event Action<int> HealthChanged;
    public event Action<int> MaxHealthChanged;
    public event Action<int> MaxManaChanged;

    public Rigidbody2D rb;
    public Collider2D col;

    [Header("StateMachine")]
    public StateMachine<PlayerController> stateMachine;
    public ScratchPad sharedData => new ScratchPad();
    public PlayerIdle idleState { get; private set; } = new PlayerIdle();
    public PlayerMove moveState { get; private set; } = new PlayerMove();

    [Header("Player things")]
    public GameObject _gameObject { get; private set; }
    public BulletDecorator activeDecorator { get; set; }
    public FireballAbility FireballAbility { get; private set; }
    public ShootBulletAbility ShootBulletAbility { get; private set; }
    private List<ElementData> elementData = new List<ElementData>();
    private ObjectPool<Bullet> _bulletPool = new ObjectPool<Bullet>(new List<Bullet>() { });
    private InputHandler _inputHandler = new InputHandler();
    private Camera _cam;
    private ElementData _basicElement;
    private ElementData _fireData;
    private ElementData _iceData;
    private ElementData _airData;



    [Header("Gameplay variables")]
    public int Health => _health;
    public int MaxHealth => _maxHealth;
    public int Mana => _mana;
    public int MaxMana => _maxMana;

    public int baseDamage = 1;
    public float baseBulletSpeed = 5f;
    public int baseManaCost = 1;
    public int elementalManaCost = 2;
    public int fireballManaCost = 4;

    private int _spellCount = 15;
    private int _health;
    private int _maxHealth = 10;
    private int _mana;
    private int _maxMana = 10;
    private int _manaRecoveryAmount = 1;
    private float _manaRecoveryInterval = 1f;   
    private float _manaRecoveryTimer = 0f;

    [Header("Misc")]
    public Color NextBulletColor => activeDecorator?.Color ?? Color.black;
    public ItemChest chestInRange { get; private set; }
    public StairCase staircaseInRange { get; private set; }
    private Vector3 _mousePos;   

    public PlayerController(GameObject gameobject, List<ElementData> elementList)
    {
        this._gameObject = gameobject;
        rb = gameobject.GetComponent<Rigidbody2D>();
        col = gameobject.GetComponent<Collider2D>();

        elementData.AddRange(elementList);
        _basicElement = elementData[0];
        _fireData = elementData[1];
        _iceData = elementData[2];
        _airData = elementData[3];

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

        _health = _maxHealth;
        _mana = _maxMana;

        //Set starting decorator so player can shoot at start
        var startingDecorator = new ElementDecorator(_basicElement.type, _basicElement.elementalDmg, _basicElement.elementColor, _basicElement.bulletSpeed);
        activeDecorator = startingDecorator;

        //initialize input bindings
        //_inputHandler.BindKeyToCommand(KeyCode.Space, KeypressType.Down, new DashAbility(this));
        _inputHandler.BindKeyToCommand(KeyCode.Space, KeypressType.Down, FireballAbility = new FireballAbility(this));
        _inputHandler.BindKeyToCommand(KeyCode.Mouse0, KeypressType.Down, ShootBulletAbility = new ShootBulletAbility(_bulletPool, this));
        _inputHandler.BindKeyToCommand(KeyCode.E, KeypressType.Down, new InteractCommand(this));
        _inputHandler.BindKeyToCommand(KeyCode.Alpha1, KeypressType.Down, new SetDecoratorCommand(this, new ElementDecorator(_basicElement.type, _basicElement.elementalDmg, _basicElement.elementColor, _basicElement.bulletSpeed)));
        _inputHandler.BindKeyToCommand(KeyCode.Alpha2, KeypressType.Down, new SetDecoratorCommand(this, new ElementDecorator(_fireData.type, _fireData.elementalDmg, _fireData.elementColor, _fireData.bulletSpeed)));
        _inputHandler.BindKeyToCommand(KeyCode.Alpha3, KeypressType.Down, new SetDecoratorCommand(this, new ElementDecorator(_iceData.type, _iceData.elementalDmg, _iceData.elementColor, _iceData.bulletSpeed)));      
    }

    public virtual void Update()
    {
        _inputHandler.HandleInput();

        Vector3 previousPos = _gameObject.transform.position;

        // Mana regen Tick
        if(_mana < _maxMana)
        {
            _manaRecoveryTimer += Time.deltaTime;
            if(_manaRecoveryTimer >= _manaRecoveryInterval)
            {
                _manaRecoveryTimer = 0f;
                ModifyMana(_manaRecoveryAmount);
            }
        }

        //update loop statemachine
        stateMachine?.Update();
        if (col != null) HandleCollision();
        SetCameraPosition();
    }

    public void IncreaseMaxHP(int amount)
    {
        _maxHealth += amount;
        MaxHealthChanged?.Invoke(_maxHealth);
        ModifyHealth(amount);
    }

    public void IncreaseMaxMana(int amount)
    {
        _maxMana += amount;
        MaxManaChanged?.Invoke(_maxMana);
    }

    public void ModifyHealth(int amount)
    {
        _health = Mathf.Clamp(_health + amount, 0, _maxHealth);
        HealthChanged?.Invoke(_health);

        if (_health == 0)
            Die();
    }

    public void ModifyMana(int amount)
    {
        _mana = Mathf.Clamp(_mana + amount, 0, _maxMana);
        ManaChanged?.Invoke( _mana);
    }

    private void Die()
    {
        GameHandler.instance.UnSubscribe(this);
        _bulletPool?.DestroyAll(b => { GameHandler.instance.DestroyObject(b._gameObject); });
        GameHandler.instance.PlayerDied();
        GameHandler.instance.DestroyObject(_gameObject);
    }


    public void OnBulletDie(Bullet _bullet)
    {
        _bulletPool.ReturnItemToPool(_bullet);
    }

    public GameObject GameObject()
    {
        return _gameObject;
    }

    public Bullet CreateBullet(GameObject bulletObject)
    {
        Bullet bullet = new Bullet(bulletObject, this, baseDamage, Color.black, 15);
        return bullet;
    }

    public Vector2 GetAimDirection()
    {
        _mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        var directionToGive = _mousePos - _gameObject.transform.position;

        return directionToGive;
    }

    public Quaternion GetBulletRotation()
    {
        _mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 rotation = _mousePos - _gameObject.transform.position;
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
        GameHandler.instance.cam.transform.position = _gameObject.transform.position + new Vector3(0, 0, -10);
    }

    private void HandleCollision()
    {
        var overlappingColliders = new List<Collider2D>();
        var filter = new ContactFilter2D
        {
            useTriggers = true
        };
        col.Overlap(filter, overlappingColliders);

        HandleBulletCollision(overlappingColliders);
        HandleItemCollision(overlappingColliders);
    }

    private void HandleBulletCollision(List<Collider2D> colliders)
    {
        foreach (var otherCollider in colliders)
        {
            if (otherCollider.CompareTag("EnemyBullet"))
            {
                if (Bullet.collLookup.TryGetValue(otherCollider, out var bullet))
                {
                    ModifyHealth(-bullet.damage);
                    bullet.Die();
                }
            }
        }
    }

    public void HandleItemCollision(List<Collider2D> colliders)
    {
        ItemChest foundChest = null;
        StairCase foundStaircase = null;

        foreach (var otherCollider in colliders)
        {
            if (otherCollider.CompareTag("Staircase"))
            {
                if (StairCase.collLookup.TryGetValue(otherCollider, out var staircase))
                    foundStaircase = staircase;
            }
            if (otherCollider.CompareTag("ItemChest"))
            {
                if (ItemChest.collLookup.TryGetValue(otherCollider, out var chest))
                    foundChest = chest;
            }
        }

        chestInRange = foundChest;
        staircaseInRange = foundStaircase;
    }
}
