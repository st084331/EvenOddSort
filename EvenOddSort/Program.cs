using MPI;
using Environment = MPI.Environment;

namespace EvenOddSort;

public class Program
{

    public static void Main(string[] args)
    {
        using (var env = new Environment(ref args))
        {
            int[]? inputArray = null;
            var commun = Communicator.world;

            if (commun.Rank == 0)
            {
            //Console.WriteLine($"{string.Join(" ", args)}");

                if (args.Length < 2)
                {
                    return;
                }

                var inputFile = args[0];
                var streamReader = new StreamReader(inputFile);
                inputArray = streamReader.ReadToEnd().Split().Select(x => int.Parse(x)).ToArray();
                streamReader.Close();
                inputArray = commun.Scatter(Enumerable.Repeat(inputArray, commun.Size).ToArray(), 0);
            }
            else
            {
                inputArray = commun.Scatter<int[]>(0);
            }

            var sorted = Sort.OddEvenSort(inputArray);

            if (commun.Rank == 0)
            {
                //Console.WriteLine($"{string.Join(" ", sorted)}");
                var outputFile = args[1];
                var streamWriter = new StreamWriter(outputFile);
                foreach (var elem in sorted)
                {
                    streamWriter.Write($"{elem} ");
                }
                streamWriter.Write(System.Environment.NewLine);
                streamWriter.Close();
            }
        }
    }
}