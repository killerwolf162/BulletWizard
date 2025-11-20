using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : IPoolable
{
    public List<T> _activePool = new List<T>();
    public List<T> _inactivePool = new List<T>();

    public ObjectPool(List<T> initPool)
    {
        _inactivePool.AddRange(initPool);
    }

    private T AddNewObjectToPool()
    {
        T instance = (T)Activator.CreateInstance(typeof(T));
        _inactivePool.Add(instance);
        return instance;
    }

    public T RequestObject()
    {
        if (_inactivePool.Count > 0)
        {
            return _inactivePool[0];
        }
        return default(T);
    }

    public T ActivateItem(T item)
    {
        item.active = true;
        if (_inactivePool.Contains(item))
        {
            _inactivePool.Remove(item);
        }
        _activePool.Add(item);
        return item;
    }

    public void ReturnItemToPool(T item)
    {
        if (_activePool.Contains(item))
        {
            _activePool.Remove(item);
        }
        item.OnDisableObject();
        item.active = false;
        _inactivePool.Add(item);
    }
}