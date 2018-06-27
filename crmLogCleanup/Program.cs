using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crmLogCleanup
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputArgs = "crmLogCleanup";

            if (args == null)
            {
                Console.WriteLine("No Customer info found");
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string argument = args[i];
                    inputArgs = args[0];
                }
            }

            string[] cmdArgs = inputArgs.Split('/');

            foreach(string incident in cmdArgs)
            {
                Console.WriteLine(incident);
            }

            //Console.WriteLine(args[0]);
            Console.Read();

        }
    }
}
