using UnityEngine;
using System.Collections.Generic;

namespace Genoverrei.Libary;

/// <summary>
/// <para> summary_ObjectPool </para>
/// <para> (TH) : ?????????? Pool ?????????? Generic ????????????????????????????????????????? </para>
/// <para> (EN) : Generic object pooling system with dynamic growth and automatic cleanup. </para>
/// </summary>
public class ObjectPool<T> where T : Component
{
    #region Variable

    private readonly T _prefab;
    private readonly int _maxSize;
    private readonly Transform _container;
    private readonly Queue<T> _pool = new();

    #endregion //Variable

    #region Constructor

    public ObjectPool(T prefab, int maxSize, Transform container = null)
    {
        _prefab = prefab;
        _maxSize = maxSize;
        _container = container;
    }

    #endregion //Constructor

    #region Public Methods

    public T Get()
    {
        T item = _pool.Count > 0 ? _pool.Dequeue() : Object.Instantiate(_prefab, _container);
        item.gameObject.SetActive(true);
        return item;
    }

    public void Return(T item)
    {
        if (item == null) return;

        if (_pool.Count >= _maxSize)
        {
            Object.Destroy(item.gameObject);
            return;
        }

        item.gameObject.SetActive(false);
        _pool.Enqueue(item);
    }

    #endregion //Public Methods
}