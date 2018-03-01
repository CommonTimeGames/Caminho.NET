using System;
using Caminho;

namespace Caminho
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("---Caminho.NET Dialogue Runner---");

            var dialogueName = "test.lua";

            if (args.Length == 0)
            {
                Console.WriteLine("Enter a dialogue to run (test.lua): ");
                var input = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(input))
                {
                    dialogueName = input;
                }
            }
            else
            {
                dialogueName = args[0];
            }

            var engine = new CaminhoEngine();
            engine.Initialize();

            var packageName = args.Length > 1 ? args[1] : "default";

            Console.WriteLine("Loaded dialogue '{0}', package '{1}'",
                              dialogueName, packageName);


            engine.Start(dialogueName, packageName);

            while (engine.Status == CaminhoStatus.Active)
            {
                bool enterToContinue = false;

                switch (engine.Current.Type)
                {
                    case CaminhoNodeType.Text:
                        Console.WriteLine("[Text] {0}", engine.Current.Text);
                        Console.WriteLine("Press ENTER to continue...");
                        enterToContinue = true;
                        break;

                    case CaminhoNodeType.Choice:
                        Console.WriteLine("[Choice] {0}", engine.Current.Text);
                        enterToContinue = true;
                        break;

                    case CaminhoNodeType.Function:
                        Console.WriteLine("[Function] {0}",
                                          engine.Current.FunctionName);
                        break;

                    case CaminhoNodeType.Wait:
                        Console.WriteLine("[Wait] {0}",
                                          engine.Current.WaitTime);
                        break;

                    case CaminhoNodeType.Event:
                        Console.WriteLine("[Event] {0}",
                                          engine.Current.Event);
                        break;

                    case CaminhoNodeType.Set:
                        Console.WriteLine("[Set] '{0}' to '{1}'",
                                          engine.Current.ContextVariable,
                                          engine.Current.ContextValue);
                        break;

                    case CaminhoNodeType.Increment:
                        Console.WriteLine("[Increment] '{0}'",
                                          engine.Current.ContextVariable);
                        break;

                    case CaminhoNodeType.Decrement:
                        Console.WriteLine("[Decrement] '{0}'",
                                          engine.Current.ContextVariable);
                        break;

                    case CaminhoNodeType.Error:
                        Console.WriteLine("[Error] '{0}'",
                                          engine.Current.ErrorMessage);
                        break;

                }

                int choice = 0;

                if (enterToContinue)
                {
                    var input = Console.ReadLine();
                    int.TryParse(input, out choice);
                }

                engine.Continue(choice);

            }

            Console.WriteLine("End of dialogue!");
        }
    }
}
