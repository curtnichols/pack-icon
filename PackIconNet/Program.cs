using System;
using System.IO;
using System.Linq;

namespace PackIconNet
{
    class Program
    {
        static void Main(string[] args)
        {
            var output = args.Where(
                    arg => String.Compare(".ico", Path.GetExtension(arg), ignoreCase: true) == 0)
                    .ToArray();
            var inputs = args.Where(
                    arg => String.Compare(".png", Path.GetExtension(arg), ignoreCase: true) == 0)
                    .ToArray();

            if (inputs.Length == 0)
            {
                Console.Error.WriteLine("No .png files given");
                Environment.Exit(1);
            }

            if (output.Length != 1)
            {
                if (output.Length == 0)
                {
                    Console.Error.WriteLine("No .ico path given");
                    Environment.Exit(2);
                }
                else
                {
                    Console.Error.WriteLine("Too many .ico paths given");
                    Environment.Exit(3);
                }
            }

            Icon.PackIcon(output[0], inputs);
        }
    }
}
