using System;
using System.Collections.Generic;
using System.Text;
using Fleck;

namespace HonorServer
{
    class WebSocketClient
    {
        private IWebSocketConnection connection;
        private GameObject playerObject;

        public WebSocketClient(IWebSocketConnection connection)
        {
            this.connection = connection;
        }

        public IWebSocketConnection GetConnection()
        {
            return connection;
        }

        public string GetIpAddress()
        {
            return connection.ConnectionInfo.ClientIpAddress;
        }

        public int GetPort()
        {
            return connection.ConnectionInfo.ClientPort;
        }

        public string GetSocketAddress()
        {
            return GetIpAddress() + ":" + GetPort();
        }

        public GameObject GetPlayerObject()
        {
            return playerObject;
        }

        public void SetPlayerObject(GameObject playerObject)
        {
            this.playerObject = playerObject;
        }
    }
}
