using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple non-ordered memory-efficient collection of GameObjects.
/// Automatically grows as needed.   Never shrinks.
/// </summary>
public class GameObjectBag
{
    private readonly Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    public Dictionary<int, GameObject>.ValueCollection Items
    {
        get { return _objects.Values; }
    }

    public void Add(GameObject gobj)
    {
        var key = gobj.GetInstanceID();

        if (_objects.ContainsKey(key))
            return;

        _objects.Add(key, gobj);
    }

    public void Remove(GameObject gobj)
    {
        var key = gobj.GetInstanceID();

        if (_objects.ContainsKey(key))
            _objects.Remove(key);
    }
}