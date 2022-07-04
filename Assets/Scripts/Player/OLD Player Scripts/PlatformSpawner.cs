using Assets.Scripts.Utils;
using UnityEngine;
using Utils;

namespace Entities
{
    public class PlatformSpawner : SingletonBehaviour<PlatformSpawner>
    {
        public GameObject PlatformPrefab;

        public void DestroyAllPlatforms()
        {
            var platforms = GameObject.FindGameObjectsWithTag("Platform");
            for (int i = 0; i < platforms.Length; i++)
            {
                DestroyPlatform(platforms[i]);
            }
        }

        public GameObject SpawnPlatform(Vector3 position, int tileSizeX, int tileSizeY)
        {
            var platform = ObjectPool.Instance.GetActiveObjectForType("Platform", "PlatformContainer");

            var platformController = platform.GetComponent<PlatformController>();
            platformController.SetupPlatform(position, tileSizeX, tileSizeY);

            return platform;
        }

        public static void DestroyPlatform(GameObject platformGO, bool countAsPlatform = true)
        {
            var platformController = platformGO.GetComponent<PlatformController>();
            platformController.ResetPlatform();

            ObjectPool.Instance.PoolObject(platformGO);
        }
    }
}