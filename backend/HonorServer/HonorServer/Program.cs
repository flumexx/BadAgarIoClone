using System;

namespace HonorServer
{
    class Program
    {
        static void Main(string[] args)
        {
            GameServer gameServer = new GameServer();

            gameServer.Start(7777);

            bool keepRunning = true;

            while (keepRunning)
            {
                string command = Console.ReadLine();

                if (command == "stop" || command == "quit" || command == "end")
                {
                    gameServer.Stop();
                    keepRunning = false;
                }
            }
        }
    }
}
