
using System.Collections.Generic;
using UnityEngine;

namespace CandyProject
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        public Dictionary<GameObject, ObjectPool> pools = new Dictionary<GameObject, ObjectPool>();

        public void CreatePool(GameObject prefab, int initialSize)
        {
            if (pools.ContainsKey(prefab)) return;

            var objectPool = new ObjectPool(prefab, transform, initialSize);
            pools.Add(prefab, objectPool);
        }

        public GameObject Get(GameObject prefab)
        {
            if (!pools.ContainsKey(prefab))
            {
                CreatePool(prefab, 5);
            }
            return pools[prefab].Get();
        }

        public void Return(GameObject prefab, GameObject obj)
        {
            if (!pools.ContainsKey(prefab))
            {
                CreatePool(prefab, 5);
            }
            pools[prefab].Return(obj);
        }
    }
}
