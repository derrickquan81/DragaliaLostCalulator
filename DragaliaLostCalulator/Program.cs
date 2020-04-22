using System;

namespace DragaliaLostCalulator
{
    class Program
    {
        static void Main(string[] args)
        {
            var worker = new Worker(new double[] { 0.005, 0.01, 0.015, 0.04 });

            //Supported values for desiredResult:
            // 0 : any rate up unit
            // 1 to 3 : looking for specific unit
            int desiredResult = 3;

            Console.Write("Single pulls only : ");
            worker.DoRuns(1000000, 1000, desiredResult);

            Console.Write("Ten pulls only : ");
            worker.DoRuns(1000000, 0, desiredResult);

            Console.Write("10 Single before Ten pulls : ");
            worker.DoRuns(1000000, 10, desiredResult);

            Console.Write("20 Single before Ten pulls : ");
            worker.DoRuns(1000000, 20, desiredResult);

            Console.Write("30 Single before Ten pulls : ");
            worker.DoRuns(1000000, 30, desiredResult);

            Console.Write("40 Single before Ten pulls : ");
            worker.DoRuns(1000000, 40, desiredResult);
        }
    }

    class Worker
    {
        private int rateLength;
        private double[][] rates;

        private Random random = new Random();

        public Worker(double[] baseRates)
        {
            rateLength = baseRates.Length;
            rates = new double[50][];
            rates[0] = baseRates;

            //Calculate pity rates based on base rates
            //Assumes no max pity rate
            for (var i = 1; i < 50; i++)
            {
                rates[i] = new double[rateLength];

                var inc = (rates[i - 1][rateLength - 1] + 0.005) / rates[i - 1][rateLength - 1];
                for (var j = 0; j < rateLength; j++)
                {
                    rates[i][j] = rates[i - 1][j] * inc;
                }
            }

        }

        public void DoRuns(int max, int maxTierCount, int desiredResult)
        {
            var fiveStartCount = 0;
            var totalCount = 0;
            for(var i = 0;i < max;i++)
            {
                var runResult = DoWork(maxTierCount, desiredResult);
                fiveStartCount += runResult.FiveStarCount;
                totalCount += runResult.Count;
            }

            Console.WriteLine(string.Format("{0} (average pulls), {1} (5 stars)", totalCount / (double)max, fiveStartCount / (double)max));
        }

        public RunResult DoWork(int maxSinglePullCount, int desiredResult)
        {
            var result = new RunResult(rateLength + 1);
            
            var pullCount = 0;
            //Array to keep track of which desired unit is obtained
            var got = new bool[desiredResult > 0 ? desiredResult : 1];
            var done = false;
            while (true)
            {
                //keep track of current pity rate, in case of 10 pull
                var tempTier = (int)(pullCount / 10.0);

                //check if we're doing 10 pulls or single
                var numRolls = (pullCount >= maxSinglePullCount) ? 10 : 1;

                var obtainedFiveStar = false;

                for (var j = 0; j < numRolls; j++)
                {
                    var r = DoOne(tempTier);

                    if (r < rateLength)
                    {
                        obtainedFiveStar = true;
                        result.FiveStarCount++;
                    }

                    pullCount++;
                    result.Details[r] += 1;
                    result.Count++;

                    //Check if desired outcome is achieved
                    if (desiredResult > 0)
                    {
                        if (r < got.Length)
                        {
                            got[r] = true;

                            done = true;
                            foreach (var c in got)
                            {
                                done = done && c;
                            }
                        }
                    }
                    else if (desiredResult == 0)
                    {
                        done = done || r < (rateLength - 1);
                    }
                }

                //Reset pity rate.
                if(obtainedFiveStar)
                {
                    pullCount = 0;
                }

                if (done)
                {
                    break;
                }
            }

            return result;
        }

        public int DoOne(int currentTier)
        {
            var odds = random.NextDouble();
            for (var i = 0; i < rateLength; i++)
            {
                if(odds < rates[currentTier][i])
                {
                    return i;
                }
            }
            return rateLength;
        }
    }

    public class RunResult
    {
        public RunResult(int length)
        {
            Details = new int[length];
        }
        public int[] Details { get; set; }
        public int Count { get; set; }
        public int FiveStarCount { get; set; }
    }
}
