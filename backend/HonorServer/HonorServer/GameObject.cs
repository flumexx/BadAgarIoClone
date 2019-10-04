using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace HonorServer
{
    class GameObject
    {
        private string identifier;
        private float posX;
        private float posY;
        private string name;
        private float size;
        private string color;
        private int score;
        private bool isPlayerObject;

        private GameObject(string identifier, float posX, float posY, string name, float size, string color)
        {
            this.identifier = identifier;
            this.posX = posX;
            this.posY = posY;
            this.name = name;
            this.size = size;
            this.color = color;
            this.score = 20;
            this.isPlayerObject = false;
        }

        public static GameObject Create(string name, float size, string color)
        {
            Random random = new Random();

            string identifier = Guid.NewGuid().ToString();
            float posX = random.Next(10, 91);
            float posY = random.Next(10, 91);

            return new GameObject(identifier, posX, posY, name, size, color);
        }

        public string GetIdentifier()
        {
            return identifier;
        }

        public float GetPosX()
        {
            return posX;
        }

        public string GetPosXAsString()
        {
            return posX.ToString(CultureInfo.InvariantCulture);
        }

        public void SetPosX(float posX)
        {
            this.posX = posX;
        }

        public float GetPosY()
        {
            return posY;
        }

        public string GetPosYAsString()
        {
            return posY.ToString(CultureInfo.InvariantCulture);
        }

        public void SetPosY(float posY)
        {
            this.posY = posY;
        }

        public string GetName()
        {
            return name;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public float GetSize()
        {
            return size;
        }

        public string GetSizeAsString()
        {
            return size.ToString(CultureInfo.InvariantCulture);
        }

        public void SetSize(float size)
        {
            this.size = size;
        }

        public string GetColor()
        {
            return color;
        }

        public void SetColor(string color)
        {
            this.color = color;
        }

        public int GetScore()
        {
            return score;
        }

        public void SetScore(int score)
        {
            this.score = score;
        }

        public bool GetIsPlayerObject()
        {
            return this.isPlayerObject;
        }

        public void SetIsPlayerObject(bool isPlayerObject)
        {
            this.isPlayerObject = isPlayerObject;
        }
    }
}
