using System;
using CommsLayer;
using DataLayer;
using MiddlemanLayer;

namespace TelegramBot
{
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            CommsController obj = new CommsController();
            obj.EnableComms();

            //BehaviourManager mng = new BehaviourManager();
            //mng.LoadReactions();
        }
    }
}
