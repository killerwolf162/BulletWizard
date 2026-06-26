using System.Collections.Generic;
using UnityEngine;

public class StairCase
{
    public static readonly Dictionary<Collider2D, StairCase> collLookup = new Dictionary<Collider2D, StairCase>();

    [Header("General")]
    private Collider2D _col;
    public GameObject _gameObject { get; private set; }

    public StairCase(GameObject gameObject)
    {
        _gameObject = gameObject;

        _col = _gameObject.GetComponent<Collider2D>();

        collLookup[_col] = this;
    }

    public void OnActivation()
    {
        Debug.Log("HIHI you got the Staircase");
    }
}