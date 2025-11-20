using UnityEngine;

public class Fireball : ISceneObject
{
    public GameObject gameobject { get; private set; }

    private Vector2 _startPosition;
    private Vector2 _direction;

    private float _aliveTime = 2f;

    public Fireball(Vector2 position, Vector2 direction)
    {
        _startPosition = position;
        _direction = direction;
        Start();
    }

    public void Start()
    {
        GameObject fireballObject = Resources.Load("Fireball", typeof(GameObject)) as GameObject;
        gameobject = GameHandler.instance.InstantiateNew(fireballObject);
        gameobject.transform.position = _startPosition;
        GameHandler.instance.Subscribe(this);
    }

    public void Update()
    {
        //count down alive timer to destroy fireball
        _aliveTime -= Time.deltaTime;

        if (_aliveTime < 0)
        {
            GameHandler.instance.DestroyObject(gameobject);
            GameHandler.instance.UnSubscribe(this);
        }

        //move fireball
        gameobject.transform.position += (Vector3)_direction * Time.deltaTime;
    }
}
