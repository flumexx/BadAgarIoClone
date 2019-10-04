using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Fleck;

namespace HonorServer
{
    class GameServer
    {
        private bool isRunning;
        private WebSocketServer server;
        private List<WebSocketClient> clients;
        private PhysicsWorld physicsWorld;

        public GameServer()
        {
            this.isRunning = false;
        }

        public void Start(int port)
        {
            if (!isRunning)
            {
                isRunning = true;
                clients = new List<WebSocketClient>();
                physicsWorld = new PhysicsWorld(PublishObjectSpawnedToParent, PublishObjectDespawnedToParent, PublishObjectUpdatedToParent);
                server = new WebSocketServer("ws://0.0.0.0:" + port);

                Spawner spawner = physicsWorld.CreateSpawner(GameObject.Create(" ", 1, "#fff"), 20, 3000);

                physicsWorld.Start();
                spawner.Start();

                server.ListenerSocket.NoDelay = true;
                server.Start(clientConnection =>
                {
                    clientConnection.OnOpen = () =>
                    {
                        WebSocketClient client = new WebSocketClient(clientConnection);

                        lock (clients)
                        {
                            clients.Add(client);
                        }
                        
                        OnClientConnected(client);
                    };
                    clientConnection.OnClose = () =>
                    {
                        WebSocketClient client = null;

                        lock (clients)
                        {
                            clients.ForEach((otherClient) =>
                            {
                                if (otherClient.GetConnection() == clientConnection)
                                {
                                    client = otherClient;
                                }
                            });

                            if (client != null)
                            {
                                clients.Remove(client);
                            }
                        }

                        OnClientDisconnected(client);
                    };
                    clientConnection.OnMessage = message =>
                    {
                        WebSocketClient client = null;

                        lock (clients)
                        {
                            clients.ForEach((otherClient) =>
                            {
                                if (otherClient.GetConnection() == clientConnection)
                                {
                                    client = otherClient;
                                }
                            });
                        }

                        OnClientMessageReceived(client, message);
                    };
                });
            }
        }

        public void PublishObjectSpawnedToParent(GameObject gameObject)
        {
            OnObjectSpawned(gameObject);
        }

        public void PublishObjectSpawnedToChildren(GameObject gameObject)
        {
            OnObjectSpawned(gameObject);

            physicsWorld.PublishObjectSpawnedToChildren(gameObject);
        }

        public void PublishObjectDespawnedToParent(GameObject gameObject)
        {
            OnObjectDespawned(gameObject);
        }

        public void PublishObjectDespawnedToChildren(GameObject gameObject)
        {
            OnObjectDespawned(gameObject);

            physicsWorld.PublishObjectDespawnedToChildren(gameObject);
        }

        public void PublishObjectUpdatedToParent(GameObject gameObject)
        {
            OnObjectUpdated(gameObject);
        }

        public void PublishObjectUpdatedToChildren(GameObject gameObject)
        {
            OnObjectUpdated(gameObject);

            physicsWorld.PublishObjectUpdatedToChildren(gameObject);
        }

        public void Stop()
        {
            if (isRunning)
            {
                server.Dispose();

                lock (clients)
                {
                    clients.Clear();
                }

                server = null;
                physicsWorld = null;
                clients = null;
                isRunning = false;
            }
        }

        private void SendMessageToClient(WebSocketClient client, string message)
        {
            client.GetConnection().Send(message);

            //Console.WriteLine("Sent message to " + client.GetSocketAddress() + ": " + message);
        }

        private void OnObjectSpawned(GameObject gameObject)
        {
            foreach (WebSocketClient otherClient in clients)
            {
                if (otherClient.GetPlayerObject() != null)
                {
                    SendMessageToClient(otherClient, ParameterMap.Stringify("type", "3",
                       "id", gameObject.GetIdentifier(),
                       "x", gameObject.GetPosXAsString(),
                       "y", gameObject.GetPosYAsString(),
                       "size", gameObject.GetSizeAsString(),
                       "name", gameObject.GetName(),
                       "color", gameObject.GetColor()));
                }
            }
        }

        private void OnObjectDespawned(GameObject gameObject)
        {
            lock (clients)
            {
                foreach (WebSocketClient otherClient in clients)
                {
                    SendMessageToClient(otherClient, ParameterMap.Stringify("type", "5",
                        "id", gameObject.GetIdentifier()));
                }
            }
        }

        private void OnObjectUpdated(GameObject gameObject)
        {
            lock (clients)
            {
                foreach (WebSocketClient otherClient in clients)
                {
                    SendMessageToClient(otherClient, ParameterMap.Stringify("type", "4",
                        "id", gameObject.GetIdentifier(),
                        "x", gameObject.GetPosXAsString(),
                        "y", gameObject.GetPosYAsString(),
                        "size", gameObject.GetSizeAsString(),
                        "name", gameObject.GetName(),
                        "color", gameObject.GetColor()));
                }
            }
        }

        private void OnClientConnected(WebSocketClient client)
        {

        }

        private void OnClientDisconnected(WebSocketClient client)
        {
            if (client.GetPlayerObject() != null)
            {
                PublishObjectDespawnedToChildren(client.GetPlayerObject());
            }
        }

        private void OnClientMessageReceived(WebSocketClient client, string message)
        {
            Dictionary<string, string> parameterMap = ParameterMap.Parse(message);

            if (parameterMap.ContainsKey("type"))
            {
                OnClientValidMessageReceived(parameterMap, client);
            }
        }

        private void OnClientValidMessageReceived(Dictionary<string, string> parameterMap, WebSocketClient client)
        {
            int type;

            if (Int32.TryParse(parameterMap["type"], out type))
            {
                if (type != 1)
                {
                    Console.WriteLine("Received message from " + client.GetSocketAddress() + ": " + ParameterMap.Stringify(parameterMap));
                }

                switch (type)
                {
                    case 0: OnClientReady(client, parameterMap["name"], parameterMap["color"]); break;
                    case 1:
                        {
                            float x;
                            float y;

                            if (Single.TryParse(parameterMap["x"], NumberStyles.Float, CultureInfo.InvariantCulture, out x))
                            {
                                if (Single.TryParse(parameterMap["y"], NumberStyles.Float, CultureInfo.InvariantCulture, out y))
                                {
                                    OnClientPositionUpdateReceived(client, x, y);
                                }
                            }
                            break;
                        }
                }
            }
        }

        private void OnClientReady(WebSocketClient client, string name, string color)
        {
            GameObject playerObject = physicsWorld.CreateObject(name, 5, color);

            client.SetPlayerObject(playerObject);

            foreach (GameObject gameObject in physicsWorld.GetGameObject())
            {
                SendMessageToClient(client, ParameterMap.Stringify("type", "3",
                    "id", gameObject.GetIdentifier(),
                    "x", gameObject.GetPosXAsString(),
                    "y", gameObject.GetPosYAsString(),
                    "size", gameObject.GetSizeAsString(),
                    "name", gameObject.GetName(),
                    "color", gameObject.GetColor()));
            }

            if (client.GetPlayerObject() != null)
            {
                SendMessageToClient(client, ParameterMap.Stringify("type", "2",
                    "id", client.GetPlayerObject().GetIdentifier()));

                lock (clients)
                {
                    foreach (WebSocketClient otherClient in clients)
                    {
                        if (otherClient != client)
                        {
                            SendMessageToClient(otherClient, ParameterMap.Stringify("type", "3",
                                "id", client.GetPlayerObject().GetIdentifier(),
                                "x", client.GetPlayerObject().GetPosXAsString(),
                                "y", client.GetPlayerObject().GetPosYAsString(),
                                "size", client.GetPlayerObject().GetSizeAsString(),
                                "name", client.GetPlayerObject().GetName(),
                                "color", client.GetPlayerObject().GetColor()));
                        }
                    }
                }
            }
        }

        private void OnClientPositionUpdateReceived(WebSocketClient client, float x, float y)
        {
            if (client.GetPlayerObject() != null)
            {
                client.GetPlayerObject().SetPosX(x);
                client.GetPlayerObject().SetPosY(y);

                PublishObjectUpdatedToChildren(client.GetPlayerObject());
            }
        }
    }
}
