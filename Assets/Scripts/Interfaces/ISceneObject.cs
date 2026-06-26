using UnityEngine;

public interface ISceneObject : IUpdateable
{
    public GameObject _gameObject { get; }
}
