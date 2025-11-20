using UnityEngine;

public interface ISceneObject : IUpdateable
{
    public GameObject gameobject { get; }
}
