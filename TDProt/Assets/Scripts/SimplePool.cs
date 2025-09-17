// SimplePool.cs
using System.Collections.Generic;
using UnityEngine;

public class SimplePool : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 8;

    private Queue<GameObject> _pool = new Queue<GameObject>();

    public void Init(GameObject prefabObj, int initial = 8)
    {
        prefab = prefabObj;
        initialSize = initial;
        for (int i = 0; i < initialSize; i++)
        {
            var o = Instantiate(prefab, transform);
            o.SetActive(false);
            _pool.Enqueue(o);
        }
    }

    public GameObject Get(Vector3 position)
    {
        GameObject obj;
        if (_pool.Count > 0) obj = _pool.Dequeue();
        else obj = Instantiate(prefab, transform);

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);
        return obj;
    }

    public void Release(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        _pool.Enqueue(obj);
    }
}
