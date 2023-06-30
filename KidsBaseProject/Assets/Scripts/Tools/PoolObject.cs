using UnityEngine;

namespace Tools.ObjectPooling
{
    public class PoolObject : MonoBehaviour
    {
        [HideInInspector]
        public ObjectPool objPool;

        /// <summary>
        /// Recycle this object to pool.
        /// </summary>
        public void Recycle()
        {
            objPool.ObjectRecycle(this);
        }
    }
}