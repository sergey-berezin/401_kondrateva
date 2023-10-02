using NuGetBert;

namespace MyApp
{
    class Program
    {
        static CancellationTokenSource cts = new CancellationTokenSource();
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }
            string textPath = args[0];
            var model = new Bert(textPath);
            Console.WriteLine("session started");
            var question = "";
            while ((question = Console.ReadLine()) != "")
            {
                var answer = model.GetAnswer(question, cts.Token);
                Console.WriteLine($"{question}: {answer.Result}");
            }

        }
    }
}