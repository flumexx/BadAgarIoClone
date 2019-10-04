using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace HonorServer
{
    class Spawner
    {
        private GameObject templateObject;
        private int maxObjectsToSpawn;
        private int spawnIntervalInMs;
        private Action<GameObject> objectSpawnedHandler;
        private Action<GameObject> objectDespawnedHandler;
        private List<GameObject> spawnedObjects;
        private Timer timer;

        private Spawner(GameObject templateObject, int maxObjectsToSpawn, int spawnIntervalInMs, Action<GameObject> objectSpawnedHandler, Action<GameObject> objectDespawnedHandler)
        {
            this.templateObject = templateObject;
            this.maxObjectsToSpawn = maxObjectsToSpawn;
            this.spawnIntervalInMs = spawnIntervalInMs;
            this.objectSpawnedHandler = objectSpawnedHandler;
            this.objectDespawnedHandler = objectDespawnedHandler;
            this.spawnedObjects = new List<GameObject>();
            this.timer = new Timer();

            ConfigureTimer();
        }

        public static Spawner Create(GameObject templateObject, int maxObjectsToSpawn, int spawnIntervalInMs, Action<GameObject> objectSpawnedHandler, Action<GameObject> objectDespawnedHandler)
        {
            return new Spawner(templateObject, maxObjectsToSpawn, spawnIntervalInMs, objectSpawnedHandler, objectDespawnedHandler);
        }

        public GameObject GetTemplateObject()
        {
            return templateObject;
        }

        public int GetMaxObjectsToSpawn()
        {
            return maxObjectsToSpawn;
        }

        public int GetSpawnIntervalInMs()
        {
            return spawnIntervalInMs;
        }

        public GameObject[] GetSpawnedObjects()
        {
            return spawnedObjects.ToArray();
        }

        public void Start()
        {
            if (!timer.Enabled)
            {
                timer.Enabled = true;
            }
        }

        public void Stop()
        {
            if (timer.Enabled)
            {
                timer.Enabled = false;
            }
        }

        public void PublishObjectSpawnedToParent(GameObject gameObject)
        {
            OnObjectSpawned(gameObject);

            objectSpawnedHandler.Invoke(gameObject);
        }

        public void PublishObjectSpawnedToChildren(GameObject gameObject)
        {
            OnObjectSpawned(gameObject);
        }

        public void PublishObjectDespawnedToParent(GameObject gameObject)
        {
            OnObjectDespawned(gameObject);

            objectDespawnedHandler.Invoke(gameObject);
        }

        public void PublishObjectDespawnedToChildren(GameObject gameObject)
        {
            OnObjectDespawned(gameObject);
        }

        public void OnObjectSpawned(GameObject gameObject)
        {

        }

        public void OnObjectDespawned(GameObject gameObject)
        {
            spawnedObjects.Remove(gameObject);
        }

        private void ConfigureTimer()
        {
            timer.Interval = spawnIntervalInMs;
            timer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                if (spawnedObjects.Count < maxObjectsToSpawn)
                {
                    GameObject gameObject = GameObject.Create(templateObject.GetName(), templateObject.GetSize(), templateObject.GetColor());

                    spawnedObjects.Add(gameObject);

                    PublishObjectSpawnedToParent(gameObject);
                }
            };
        }
    }
}
