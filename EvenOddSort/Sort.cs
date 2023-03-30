using MPI;

namespace EvenOddSort;

public static class Sort
{
    private static int[] Merge(int[] arr1, int[] arr2)
    {
        int i = 0, j = 0, k = 0;
        var merged = new int[arr1.Length + arr2.Length];

        while (i < arr1.Length && j < arr2.Length)
            if (arr1[i].CompareTo(arr2[j]) < 0)
                merged[k++] = arr1[i++];
            else
                merged[k++] = arr2[j++];

        while (i < arr1.Length) merged[k++] = arr1[i++];

        while (j < arr2.Length) merged[k++] = arr2[j++];

        return merged;
    }

    public static int[] OddEvenSort(int[] list)
    {
        var commun = Communicator.world;
        var communRank = commun.Rank;
        var communSize = commun.Size;

        var subListLength = list.Length / communSize + (list.Length % communSize > 0 ? 1 : 0);
        var subList = new int[subListLength];

        if (communRank == 0)
            subList = commun.Scatter(list.Chunk(subListLength).ToArray(), 0);
        else
            subList = commun.Scatter<int[]>(0);

        Array.Sort(subList);

        for (var phase = 0; phase < communSize; phase++)
        {
            if (phase % 2 == communRank % 2 && communRank < communSize - 1)
            {
                commun.Send<int[]>(subList, communRank + 1, 0);
                subList = commun.Receive<int[]>(communRank + 1, 0);
            }
            else if (phase % 2 != communRank % 2 && communRank > 0)
            {
                var received = commun.Receive<int[]>(communRank - 1, 0);
                var merged = Merge(received, subList);

                commun.Send<int[]>(merged.Take(subListLength).ToArray(), communRank - 1, 0);
                subList = merged.Skip(subListLength).ToArray();
            }
        }

        if (communRank == 0)
            list = commun.Gather(subList, 0).SelectMany(x => x).ToArray();
        else
            commun.Gather(subList, 0);

        return list;
    }
}