using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace HonorServer
{
    class PhysicsWorld
    {
        private List<GameObject> gameObjects;
        private List<Spawner> spawners;
        private Action<GameObject> objectSpawnedHandler;
        private Action<GameObject> objectDespawnedHandler;
        private Action<GameObject> objectUpdatedHandler;
        private Timer collisionTimer;

        public PhysicsWorld(Action<GameObject> objectSpawnedHandler, Action<GameObject> objectDespawnedHandler, Action<GameObject> objectUpdatedHandler)
        {
            this.gameObjects = new List<GameObject>();
            this.spawners = new List<Spawner>();
            this.objectSpawnedHandler = objectSpawnedHandler;
            this.objectDespawnedHandler = objectDespawnedHandler;
            this.objectUpdatedHandler = objectUpdatedHandler;
            this.collisionTimer = new Timer();

            ConfigureCollisionTimer();
        }

        public GameObject CreateObject(string name, float size, string color)
        {
            GameObject gameObject = GameObject.Create(name, size, color);

            OnObjectSpawned(gameObject);

            return gameObject;
        }

        public Spawner CreateSpawner(GameObject templateObject, int maxObjectsToSpawn, int spawnIntervalInMs)
        {
            Spawner spawner = Spawner.Create(templateObject, maxObjectsToSpawn, spawnIntervalInMs, PublishObjectSpawnedToParent, PublishObjectDespawnedToParent);

            spawners.Add(spawner);

            return spawner;
        }

        public GameObject[] GetGameObject()
        {
            return gameObjects.ToArray();
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

            foreach (Spawner spawner in spawners)
            {
                spawner.OnObjectDespawned(gameObject);
            }
        }

        public void PublishObjectUpdatedToParent(GameObject gameObject)
        {
            OnObjectUpdated(gameObject);

            objectUpdatedHandler.Invoke(gameObject);
        }

        public void PublishObjectUpdatedToChildren(GameObject gameObject)
        {
            OnObjectUpdated(gameObject);
        }

        public void OnObjectSpawned(GameObject gameObject)
        {
            lock (gameObjects)
            {
                gameObjects.Add(gameObject);
            }
        }

        public void OnObjectDespawned(GameObject gameObject)
        {
            lock (gameObjects)
            {
                gameObjects.Remove(gameObject);
            }
        }

        public void OnObjectUpdated(GameObject gameObject)
        {
        }

        public void CheckForCollisions()
        {
            Queue<GameObject> collisionIslands = new Queue<GameObject>();

            lock (gameObjects)
            {
                foreach (GameObject firstGameObject in gameObjects)
                {
                    foreach (GameObject secondGameObject in gameObjects)
                    {
                        if (firstGameObject != secondGameObject)
                        {
                            float distanceX = firstGameObject.GetPosX() - secondGameObject.GetPosX();
                            float distanceY = firstGameObject.GetPosY() - secondGameObject.GetPosY();
                            float distance = (float)Math.Sqrt((distanceX * distanceX) + (distanceY * distanceY));

                            float r = firstGameObject.GetSize() + secondGameObject.GetSize();

                            if (r > distance)
                            {
                                collisionIslands.Enqueue(firstGameObject);
                                collisionIslands.Enqueue(secondGameObject);
                            }
                        }
                    }
                }
            }

            while (collisionIslands.Count > 0)
            {
                GameObject firstGameObject = collisionIslands.Dequeue();
                GameObject secondGameObject = collisionIslands.Dequeue();

                OnCollision(firstGameObject, secondGameObject);
            }
        }

        public void Start()
        {
            if (!collisionTimer.Enabled)
            {
                collisionTimer.Enabled = true;
            }
        }

        public void Stop()
        {
            if (collisionTimer.Enabled)
            {
                foreach (Spawner spawner in spawners)
                {
                    spawner.Stop();
                }
            }
        }

        private void ConfigureCollisionTimer()
        {
            collisionTimer.Interval = 10;
            collisionTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                CheckForCollisions();
            };
        }

        private void OnCollision(GameObject firstGameObject, GameObject secondGameObject)
        {
            if (firstGameObject.GetSize() > secondGameObject.GetSize())
            {
                PublishObjectDespawnedToChildren(secondGameObject);
                PublishObjectDespawnedToParent(secondGameObject);

                if (firstGameObject.GetIsPlayerObject())
                {
                    int scoreIncrease = 1;

                    if (secondGameObject.GetIsPlayerObject())
                    {
                        scoreIncrease = secondGameObject.GetScore();
                    }

                    firstGameObject.SetScore(firstGameObject.GetScore() + scoreIncrease);
                    firstGameObject.SetSize(firstGameObject.GetScore() / 10.0f);

                    PublishObjectUpdatedToParent(firstGameObject);
                }
            }
            else if (secondGameObject.GetSize() > firstGameObject.GetSize())
            {
                PublishObjectDespawnedToChildren(firstGameObject);
                PublishObjectDespawnedToParent(firstGameObject);

                if (secondGameObject.GetIsPlayerObject())
                {
                    int scoreIncrease = 1;

                    if (firstGameObject.GetIsPlayerObject())
                    {
                        scoreIncrease = firstGameObject.GetScore();
                    }

                    secondGameObject.SetScore(secondGameObject.GetScore() + scoreIncrease);
                    secondGameObject.SetSize(secondGameObject.GetScore() / 10.0f);

                    PublishObjectUpdatedToParent(secondGameObject);
                }
            }
        }
    }
}
