using Utilities.Extensions;
using Utilities.Launcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sandbox.Launcher;

internal class Program
{
    private static void Main()
    {
        Runner runner = new ();

        while (true)
        {
            Console.Clear();

            Console.WriteLine($"Found {runner.RunnableTypes.Count} modules:\n");

            Console.WriteLine($"  0 -> [Exit]");

            runner.RunnableTypes.Indexed().ForEach(tuple =>
            {
                Console.WriteLine($" {tuple.Index + 1, 2:D} -> {tuple.Item.Name}");
            });

            Console.WriteLine("\nPlease enter the index of the module you would like to run.");

            Console.Write("> ");

            int index = -1;

            while (index < 0 || index >= runner.RunnableTypes.Count)
            {
                string input = Console.ReadLine();
                if (int.TryParse(input, out int potentialIndex))
                {
                    if (potentialIndex >= 0 && potentialIndex <= runner.RunnableTypes.Count)
                    {
                        index = potentialIndex;
                        break;
                    }
                }
                Console.WriteLine("Invalid index.");
                Console.Write("> ");
            }

            if (index == 0)
            {
                return;
            }

            Console.Clear();

            Console.WriteLine($"Launching module \"{runner.RunnableTypes[index - 1].Name}\"...\n");

            runner.Run(index);

            GC.Collect();

            Console.Clear();
        }
    }

    private class Runner
    {
        public IReadOnlyList<Type> RunnableTypes { get; init; }

        private readonly Type __iRunnable = typeof(IRunnable);

        public Runner()
        {
            ReflectionHelper.LoadSandboxAssembly();

            GLFWHelper.LoadSharedContext();

            RunnableTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => __iRunnable.IsAssignableFrom(type) 
                                && __iRunnable != type)
                .ToList();
        }

        public void Run(int index)
        {
            RunAction(RunnableTypes[index - 1]);
            //GetRunnerTask(RunnableTypes[index]).Start();
        }

        private Task GetRunnerTask(Type runnable)
        {
            return new Task(GetRunnerAction(runnable));//, TaskCreationOptions.LongRunning);
        }

        private Action GetRunnerAction(Type runnable)
        {
            return () => RunAction(runnable);
        }

        private void RunAction(Type runnable)
        {
            __iRunnable.GetMethod(nameof(IRunnable.Run), 1, Array.Empty<Type>())
                .MakeGenericMethod(runnable)
                .Invoke(null, null);
        }
    }

    
}
