using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    /// <summary>
    /// Repository of commonly used prefabs. Array-based version.
    /// </summary>
    [AddComponentMenu("Gameplay/ObjectPool")]
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        #region member

        /// <summary>
        /// Member class for a prefab entered into the object pool
        /// </summary>
        [Serializable]
        public class ObjectPoolEntry
        {
            /// <summary>
            /// the object to pre instantiate
            /// </summary>
            [SerializeField] public GameObject Prefab;

            /// <summary>
            /// quantity of object to pre-instantiate
            /// </summary>
            [SerializeField] public int PreferredCount;

            /// <summary>
            /// whether to destroy objects when trying to pool them, if the pool is already filled with the amount given as PreferredCount
            /// </summary>
            [SerializeField] public bool DestroySurplus;
        }

        #endregion


        /// <summary>
        /// Member class for a prefab entered into the object pool
        /// </summary>
        [Serializable]
        public class ObjectContainer
        {
            /// <summary>
            /// the object to pre instantiate
            /// </summary>
            [SerializeField] public GameObject ContainerObject;
        }

        /// <summary>
        /// The object prefabs which the pool can handle
        /// by The amount of objects of each type to buffer.
        /// </summary>
        public ObjectPoolEntry[] Entries;

        /// <summary>
        /// The object prefabs which the pool can handle
        /// by The amount of objects of each type to buffer.
        /// </summary>
        public ObjectContainer[] Containers;

        /// <summary>
        /// The pooled objects currently available.
        /// Indexed by the index of the objectPrefabs
        /// </summary>
        [HideInInspector] public List<GameObject>[] Pool;

        /// <summary>
        /// The container object that we will keep unused pooled objects in, so we dont clog up the editor with objects.
        /// </summary>
        protected GameObject ContainerObject;

        private void OnEnable()
        {
            Init();
        }

        // Use this for initialization
        private void Start()
        {
            Init();
        }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            Instance = this;

            if (ContainerObject == null)
                ContainerObject = new GameObject("ObjectPool");

            if (Pool == null)
            {
                Pool = new List<GameObject>[Entries.Length];

                //Loop through the object prefabs and make a new list for each one.
                //We do this because the pool can only support prefabs set to it in the editor,
                //so we can assume the lists of pooled objects are in the same order as object prefabs in the array
                for (int i = 0; i < Entries.Length; i++)
                {
                    var objectPrefab = Entries[i];

                    //create the repository
                    Pool[i] = new List<GameObject>();

                    //fill it
                    for (int n = 0; n < objectPrefab.PreferredCount; n++)
                    {
                        PoolObject(SpawnNewObject(objectPrefab.Prefab));
                    }
                }
            }
        }

        private GameObject SpawnNewObject(GameObject prefab)
        {
            if (prefab == null) return null;
            var newObj = Instantiate(prefab);

            newObj.name = prefab.name;

            return newObj;
        }

        /// <summary>
        /// Gets a new object for the name type provided.  If no object type exists or if onlypooled is true and there is no objects of that type in the pool
        /// then null will be returned.
        /// </summary>
        /// <returns>
        /// The object for type.
        /// </returns>
        /// <param name='objectType'>
        /// Object type.
        /// </param>
        /// <param name='onlyPooled'>
        /// If true, it will only return an object if there is one currently pooled.
        /// </param>
        /// <param name="activate">
        /// If true, the gameobject will be activated before being returned. If false, it will remain deactivated.
        /// </param>
        /// <param name="containerName">
        /// (Optional) Name of the container-gameobject you want the object to be attached to. The container MUST have been added to the Containers-array.
        /// </param>
        public GameObject GetObjectForType(string objectType, bool onlyPooled, bool activate,
            string containerName = null)
        {

            for (int i = 0; i < Entries.Length; i++)
            {
                var prefab = Entries[i].Prefab;

                if (prefab.name != objectType)
                    continue;

                if (Pool[i].Count > 0)
                {
                    GameObject pooledObject = Pool[i][0];

                    Pool[i].RemoveAt(0);

                    pooledObject.transform.parent = null;

                    if (activate) pooledObject.SetActive(true);
                    if (containerName != null) AttachObjectToContainer(pooledObject, containerName);
                    return pooledObject;
                }

                if (onlyPooled) return null;

                GameObject newObj = Instantiate(Entries[i].Prefab);
                newObj.name = Entries[i].Prefab.name;

                if (!activate) newObj.SetActive(false);
                if (containerName != null) AttachObjectToContainer(newObj, containerName);

                return newObj;
            }

            //If we have gotten here, there was no object of the specified type
            return null;
        }

        private void AttachObjectToContainer(GameObject go, string containerName)
        {
            if (string.IsNullOrEmpty(containerName)) return;

            foreach (var t in Containers)
            {
                if (!t.ContainerObject.name.Equals(containerName)) continue;
                go.transform.parent = t.ContainerObject.transform;
            }
        }

        /// <summary>
        /// Convenience-method for getting an active object of the specified type, even if there isn't one ready in the pool.
        /// </summary>
        /// <param name="objectType">
        /// Object type.
        /// </param>
        /// <param name="containerName">
        /// (Optional) Name of the container-gameobject you want the object to be attached to. The container MUST have been added to the Containers-array.
        /// </param>
        /// <returns>
        /// The object.
        /// </returns>
        public GameObject GetActiveObjectForType(string objectType, string containerName = null)
        {
            return GetObjectForType(objectType, false, true, containerName);
        }

        /// <summary>
        /// Convenience-method for getting an object of the specified type, even if there isn't one ready in the pool. The object will start out being inactive.
        /// </summary>
        /// <param name="objectType">
        /// Object type.
        /// </param>
        /// <param name="containerName">
        /// (Optional) Name of the container-gameobject you want the object to be attached to. The container MUST have been added to the Containers-array.
        /// </param>
        /// <returns>
        /// The object for type.
        /// </returns>
        public GameObject GetInactiveObjectForType(string objectType, string containerName = null)
        {
            return GetObjectForType(objectType, false, false, containerName);
        }

        /// <summary>
        /// Pools the object specified.  Will not be pooled if there is no prefab of that type.
        /// </summary>
        /// <param name='obj'>
        /// Object to be pooled.
        /// </param>
        public void PoolObject(GameObject obj)
        {
            if (obj == null) return;
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].Prefab.name != obj.name)
                    continue;

                if (Entries[i].DestroySurplus && Pool[i].Count >= Entries[i].PreferredCount)
                {
                    Destroy(obj);
                }
                else
                {
                    obj.SetActive(false);

                    obj.transform.parent = ContainerObject.transform;

                    Pool[i].Add(obj);
                }
                return;
            }
        }

        /// <summary>
        /// Repopulates all pools, which aren't already at or above their preferred count, with objects of their type, until they've reached their preferred count.
        /// </summary>
        public void RepopulateAllPools()
        {
            for (var i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].PreferredCount <= Pool[i].Count) continue;

                var amountToAdd = Entries[i].PreferredCount - Pool[i].Count;
                for (var j = 0; j < amountToAdd; j++)
                {
                    PoolObject(GetInactiveObjectForType(Entries[i].Prefab.name));
                }
            }
        }

        /// <summary>
        /// Repopulates the pool holding prefabs with the given name, with objects of its type, until it has reached its preferred count.
        /// </summary>
        /// <param name="prefabName"></param>
        public void RepopulatePool(string prefabName)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].Prefab.name != prefabName)
                    continue;
                var amountToAdd = Entries[i].PreferredCount - Pool[i].Count;
                for (var j = 0; j < amountToAdd; j++)
                {
                    PoolObject(GetInactiveObjectForType(Entries[i].Prefab.name));
                }
                return;
            }
        }

        /// <summary>
        /// Purges (destroys) all surplus GameObjects from all pools, so they all end up with precisely their preferred count, or less (if they had less when the method was called).
        /// </summary>
        public void PurgeAllSurplus()
        {
            for (var i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].PreferredCount >= Pool[i].Count) continue;

                var amountToRemove = Pool[i].Count - Entries[i].PreferredCount;
                for (var j = 0; j < amountToRemove; j++)
                {
                    var go = Pool[i][Pool[i].Count - 1];
                    Pool[i].RemoveAt(Pool[i].Count - 1);
                    Destroy(go);
                }
            }
        }

        /// <summary>
        /// Purges (destroys) all surplus GameObjects from the pool holding prefabs with the given name, so they it ends up with precisely its preferred count, or less (if it had less when the method was called).
        /// </summary>
        /// <param name="prefabName"></param>
        public void PurgeSurplus(string prefabName)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].Prefab.name != prefabName)
                    continue;

                var amountToRemove = Pool[i].Count - Entries[i].PreferredCount;
                for (var j = 0; j < amountToRemove; j++)
                {
                    var go = Pool[i][Pool[i].Count - 1];
                    Pool[i].RemoveAt(Pool[i].Count - 1);
                    Destroy(go);
                }
                return;
            }
        }

        /// <summary>
        /// Purges (destroys) all GameObjects from all pools.
        /// </summary>
        public void PurgeAllPools()
        {
            foreach (List<GameObject> poolGOs in Pool)
            {
                for (int j = poolGOs.Count - 1; j >= 0; j--)
                {
                    var go = poolGOs[j];
                    poolGOs.RemoveAt(j);
                    Destroy(go);
                }
            }
        }

        /// <summary>
        /// Destroys any surplus Gameobjects in pools with more than the preferred count, and repopulates any missing GameObjects in pools with less than the preferred count.
        /// </summary>
        public void RebalanceAllPools()
        {
            for (var i = 0; i < Entries.Length; i++)
            {
                // There are fewer than the preferred count in the pool
                if (Entries[i].PreferredCount > Pool[i].Count)
                {
                    var amountToAdd = Entries[i].PreferredCount - Pool[i].Count;
                    for (var j = 0; j < amountToAdd; j++)
                    {
                        PoolObject(GetInactiveObjectForType(Entries[i].Prefab.name));
                    }
                }
                // There are more than the preferred count in the pool
                else if (Entries[i].PreferredCount < Pool[i].Count)
                {
                    var amountToRemove = Pool[i].Count - Entries[i].PreferredCount;
                    for (var j = 0; j < amountToRemove; j++)
                    {
                        var go = Pool[i][Pool[i].Count - 1];
                        Pool[i].RemoveAt(Pool[i].Count - 1);
                        Destroy(go);
                    }
                }
            }
        }
    }
}