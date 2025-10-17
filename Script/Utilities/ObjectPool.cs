using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CandyProject
{
    public class ObjectPool
    {
        private readonly GameObject prefab;
        private readonly Transform parent;

        private readonly Queue<GameObject> objects = new Queue<GameObject>();

        public ObjectPool(GameObject prefab, Transform parent, int initialSize)
        {
            this.prefab = prefab;
            this.parent = parent;

            for(int i = 0; i <= initialSize; i++)
            {
               var obj = GameObject.Instantiate(prefab, parent);
                obj.SetActive(false);
                objects.Enqueue(obj);

            }
        }


        public GameObject Get()
        {
            if (objects.Count <= 0)
            {
                AddObjectToPool();
            }
            var obj = objects.Dequeue();
            obj.SetActive(true);

            return obj;


        }

        public void Return(GameObject obj)
        {
            objects.Enqueue(obj);
            obj.transform.SetParent(parent);
            obj.SetActive(false);

        }

        private void AddObjectToPool()
        {
            var obj = GameObject.Instantiate(prefab, parent);
            obj.SetActive(false);
            objects.Enqueue(obj);
        }


    }
}
