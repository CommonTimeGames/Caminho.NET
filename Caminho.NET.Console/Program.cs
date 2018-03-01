using System;
using Caminho;

namespace Caminho
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var engine = new CaminhoEngine();
            engine.Initialize();

            engine.Start("test");

            Console.WriteLine(engine.Current.Text);
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();

            engine.Continue();

            Console.WriteLine(engine.Current.Text);
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
        }
    }
}
