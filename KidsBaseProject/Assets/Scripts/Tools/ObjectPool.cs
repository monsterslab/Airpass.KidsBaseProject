using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Tools.Utility;

[Serializable] public class GameObjectUEvent : UnityEvent<GameObject> { }
namespace Tools.ObjectPooling
{
    public class ObjectPool : MonoBehaviour
    {
        ObjectPool()
        {
            Initialize();
        }

        [Header("Settings")]
        public GameObject poolingObject;        // The pooling object prefab.
        public int initPoolSize = 20;                   // Amount of pool object while initialize.

        // Variables
        public List<PoolObject> ActivePoolObjects { get { return activePoolObjects.Values.ToList(); } }

        private List<PoolObject> poolQueue = new List<PoolObject>();
        private Dictionary<string, PoolObject> activePoolObjects = new Dictionary<string, PoolObject>();

        /// <summary>
        /// Get an Object from pool.
        /// </summary>
        /// <param name="_position">The object position while get the object.</param>
        /// <param name="_quaternion">The object rotation while get the object.</param>
        /// <param name="_scale">The object scale while get the object.</param>
        /// <param name="_scale">The object name while get the object.</param>
        /// <returns>The pool Object.</returns>
        public PoolObject GetObject(Vector3 _position = default, Quaternion _quaternion = default, Vector3 _scale = default, string _name = "")
        {
            PoolObject temp;
            if (poolQueue.Count != 0)
            {
                temp = poolQueue[0];
                poolQueue.RemoveAt(0);
            }
            else
            {
                temp = Instantiate(poolingObject, transform).TryAddComponent<PoolObject>();
            }
            temp.objPool = this;
            temp.gameObject.SetActive(true);
            temp.transform.position = (_position == Vector3.zero ? temp.transform.position : _position);
            temp.transform.rotation = (_quaternion.eulerAngles == Vector3.zero ? temp.transform.rotation : _quaternion);
            temp.transform.localScale = (_scale == Vector3.zero ? temp.transform.localScale : _scale);
            temp.name = (_name == "") ? string.Format($"NoNamePob({activePoolObjects.Count}：{temp.transform.GetSiblingIndex()})") : _name;
            activePoolObjects.Add(temp.name, temp);
            //temp.onObjectActive?.Invoke();
            //OnGetObject?.Invoke(temp.gameObject);

            return temp;
        }

        /// <summary>
        /// Get a specific poolObject by name or spawn a specifec poolObject by name.
        /// </summary>
        /// <param name="_name">Name for poolObject.</param>
        /// <returns>The pool Object.</returns>
        public PoolObject GetObject(string _name)
        {
            PoolObject temp = null;
            if (activePoolObjects.ContainsKey(_name))
            {
                temp = activePoolObjects[_name];
            }
            else
            {
                for (int i = 0; i < poolQueue.Count; ++i)
                {
                    if (poolQueue[i].name == _name)
                    {
                        temp = poolQueue[i];
                        temp.gameObject.SetActive(true);
                        poolQueue.RemoveAt(i);
                        activePoolObjects.Add(_name, temp);
                        break;
                    }
                }
                if (temp == null)
                {
                    temp = GetObject(default, default, default, _name);
                }
            }
            return temp;
        }

        /// <summary>
        /// Check a specific poolObject by name has been active or not.
        /// </summary>
        /// <param name="_name"></param>
        /// <returns>Acitcve or not or doesn't exsict.</returns>
        public bool ContainsActivedObject(string _name)
        {
            return activePoolObjects.ContainsKey(_name);
        }

        /// <summary>
        /// Recycle gameObject to pool if its an poolObject.
        /// </summary>
        /// <param name="_obj">PoolObject</param>
        public void ObjectRecycle(PoolObject _obj)
        {
            if (_obj)
            {
                _obj.gameObject.SetActive(false);
                _obj.transform.SetParent(transform);
                _obj.transform.localPosition = new Vector3(0, 0, 0);
                poolQueue.Add(_obj.GetComponent<PoolObject>());
                activePoolObjects.Remove(_obj.name);
            }
        }

        /// <summary>
        /// Recycle all actived poolObjects.
        /// </summary>
        public void RecycleAll()
        {
            foreach (PoolObject temp in activePoolObjects.Values.ToList())
            {
                //poolQueue.Enqueue(temp);
                temp.Recycle();
            }
        }

        public void Initialize(GameObject poolObject = null)
        {
            poolingObject = poolObject ?? poolingObject;
            if (poolingObject != null)
            {
                // Initialize the pool.
                foreach (Transform child in transform)
                {
                    Destroy(child);
                }

                for (int i = 0; i < initPoolSize; ++i)
                {
                    GameObject temp = Instantiate(poolingObject, transform);
                    if (temp.GetComponent<PoolObject>() == null)
                    {
                        temp.AddComponent<PoolObject>();
                    }
                    temp.SetActive(false);
                    poolQueue.Add(temp.GetComponent<PoolObject>());
                }
            }
        }
    }
}