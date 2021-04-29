using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;


namespace Calculator
{

    public static class YieldCalculation
    {
        // Read the csv file into a list of lists of strings
        public static List<List<string>> ReadCSV(string filePath)
        {
            List<List<string>> listOfLists = new List<List<string>>();

            if (File.Exists(filePath))
            {
                StreamReader reader = new StreamReader(File.OpenRead(filePath));
                // Read first line and do nothing with it, i.e. skip header
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var row = line.Split(',');
                    List<string> listOfStrings = new List<string>();
                    foreach (var column in row)
                    {
                        if (!string.IsNullOrEmpty(column))
                        {
                            listOfStrings.Add(column);
                        }
                    }

                    if (listOfStrings != null && listOfStrings.Count > 0)
                    {
                        listOfLists.Add(listOfStrings);
                    }

                }
            }
            else
            {
                Console.WriteLine("Input csv file doesn't exist");
                Environment.Exit(0);
            }
            return listOfLists;
        }

        // Calculate the months between two dates, we'll assume that the day of the month does not matter 
        public static int GetMonthDifference(string date1, string date2)
        {
            var date1DateTime = DateTime.Parse(date1);
            var date2DateTime = DateTime.Parse(date2);

            return (date2DateTime.Month - (((date1DateTime.Year - date2DateTime.Year) * 12) + date1DateTime.Month));
        }

        // Calculate yield using newton's method
        public static double CalculateYield(double initialGuess, double[] cashFlow, double[] yearFraction, double bondPrice)
        {
            double error = 0.0000000001;
            double x_i = initialGuess - 1.0;
            double x_i_next = initialGuess;

            double numerator;
            double denominator;

            int maxIterations = 10000;
            int iterationCount = 0;

            while (Math.Abs(x_i_next - x_i) > error)
            {
                x_i = x_i_next;
                // returns SUM( cashFlow(i) * exp(-yearFraction(i) * x_i) )
                numerator = cashFlow.Zip(yearFraction, (x, y) => (x * Math.Exp(y * -1 * x_i))).Sum();

                // returns SUM( yearFraction(i) * cashFlow(i) * exp(-yearFraction(i) * x_i) )
                denominator = yearFraction.Zip(cashFlow, (x, y) => (x * y * Math.Exp(x * -1 * x_i))).Sum();

                x_i_next = x_i + (numerator - bondPrice) / denominator;

                // in case of non-convergence we'll stop after max iterations
                iterationCount++;
                if (iterationCount > maxIterations)
                    Console.WriteLine("Exceeded maximum iterations of " + maxIterations);
                    break;
            }
            return x_i_next;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Get bond price from user via console input
            Console.WriteLine("Enter a bond price:");
            double bondPrice;
            bool bondPriceParsed;
            bondPriceParsed = Double.TryParse(Console.ReadLine(), out bondPrice);
            while (!bondPriceParsed || bondPrice < 0.0)
            {
                Console.WriteLine("Invalid bond price, try again:");
                bondPriceParsed = Double.TryParse(Console.ReadLine(), out bondPrice);
            }

            // Get pricing date from user input
            Console.WriteLine("Enter a pricing date in format: dd/mm/yyyy");
            string pricingDate;
            pricingDate = Console.ReadLine();

            // try to parse date string into DateTime, if fail then try again
            DateTime tempDateTime;
            while (!DateTime.TryParse(pricingDate, out tempDateTime))
            {
                Console.WriteLine("Invalid datetime, please enter in format dd/mm/yyyy:");
                pricingDate = Console.ReadLine();
            }

            // Read cashflow data from provided csv file
            List<List<string>> cashFlowData;
            cashFlowData = YieldCalculation.ReadCSV(@"C:\Users\tho\Desktop\YTMCalculation\YTMCalculation\FixedIncomeCashflows.csv");

            // Read from cashFlowData, do some manipulation and set to two lists
            List<int> paymentDateList = new List<int>();
            List<double> cashFlowList = new List<double>();

            foreach (var row in cashFlowData)
            {
                    // Get the months between pricing date and payment date and add to list
                    paymentDateList.Add(YieldCalculation.GetMonthDifference(pricingDate, row[0]));
                    // Find total cash flow at each payment date and add to list
                    cashFlowList.Add(Convert.ToDouble(row[1]) + Convert.ToDouble(row[2]));
            }
              
            double[] cashFlow = cashFlowList.ToArray();

            // Months from pricing date of payment then convert that into fraction years 
            double[] yearFraction = paymentDateList.ToArray().Select(i => i / 12.0).ToArray();

            double initialGuess = 0.1;

            Console.WriteLine(YieldCalculation.CalculateYield(initialGuess, cashFlow, yearFraction, bondPrice));
        }
    }
}