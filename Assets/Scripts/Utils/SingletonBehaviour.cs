using UnityEngine;

namespace Utils
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this as T;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            OnAwake();
        }

        protected virtual void OnAwake() { }
    }
}