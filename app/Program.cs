using NuGetBert;

namespace MyApp
{
    class Program
    {
        static CancellationTokenSource cts = new CancellationTokenSource();
        static async Task PrintAnswer(string question, Bert model, CancellationTokenSource cts)
        {
            var answer = await model.GetAnswer(question, cts.Token);
            Console.WriteLine($"{question}: {answer}");
        }
        static async  Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }
            string textPath = args[0];
            var model = new Bert(textPath);
            Console.WriteLine("session started");
            var tasks = new List<Task>();
            var question = "";
            while ((question = Console.ReadLine()) != "")
            {
                tasks.Add(PrintAnswer(question, model, cts));
            }
            await Task.WhenAll(tasks);

        }
    }
}