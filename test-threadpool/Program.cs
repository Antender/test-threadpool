using System;
using FixedThreadPool;

internal class Task : ITask
{
    readonly int _itemNumber;

    public Task(int itemNumber)
    {
        _itemNumber = itemNumber;
    }

    public void Execute()
    {
        System.Threading.Thread.Sleep(1000);
        Console.WriteLine(" Task" + _itemNumber);
    }
}

internal class Program
{
    private static void Main()
    {
        var tp = new ThreadPool(2);

        Console.WriteLine("Enqueuing 10 items...");

        for (var i = 0; i < 10; i++)
        {
            var t = new Task(i);
            switch (i%3)
            {
                case 0:
                    tp.Execute(t, Priority.High);
                    break;
                case 1:
                    tp.Execute(t, Priority.Normal);
                    break;
                case 2:
                    tp.Execute(t, Priority.Low);
                    break;
            }
        }

        tp.Stop();
        Console.WriteLine();
        Console.WriteLine("Workers complete!");
        Console.ReadLine();
    }
}
