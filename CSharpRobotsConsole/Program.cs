using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSharpRobotsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Arena.Arena arena = new Arena.Arena();
            arena.StartSingleMatch(typeof(Robots.Rook), typeof(Robots.Target));
            System.Threading.Thread.Sleep(2000);
            arena.StopMatch();
            System.Threading.Thread.Sleep(2000);
        }
    }
}
