using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;


namespace Calculator
{
    public static class YTM
    {
        public static List<string> ReadCSV(string filePath)
        {
            List<string> listA = new List<string>();
            StreamReader reader = null;
            if (File.Exists(filePath))
            {
                reader = new StreamReader(File.OpenRead(filePath));
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    foreach (var item in values)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            listA.Add(item);
                        }

                    }
                }
            }
            else
            {
                Console.WriteLine("File doesn't exist");
            }
            return listA;
        }

        public static double CalculateYield(double initialGuess, double[] cashflows, double[] yearFrac, double B)
        {
            double error = 0.000000001;
            double x_i = initialGuess - 1.0;
            double x_i_next = initialGuess;

            double numerator;
            double denominator;

            while (Math.Abs(x_i_next - x_i) > error)
            {
                x_i = x_i_next;
                numerator = cashflows.Zip(yearFrac, (x, y) => (x * Math.Exp(y * -1 * x_i))).Sum();
                denominator = yearFrac.Zip(cashflows, (x, y) => (x * y * Math.Exp(x * -1 * x_i))).Sum();
                x_i_next = x_i + (numerator - B) / denominator;
            }
            return x_i_next;
        }

        public static int GetMonths(string date1, string date2)
        {
            var date1_dt = DateTime.Parse(date1);
            var date2_dt = DateTime.Parse(date2);

            return (date2_dt.Month - (((date1_dt.Year - date2_dt.Year) * 12) + date1_dt.Month));
        }


    }

    class Program
    {
        static void Main(string[] args)
        {
            List<string> cashFlowData;
            cashFlowData = YTM.ReadCSV(@"C:\Users\tho\Desktop\YTMCalculation\YTMCalculation\FixedIncomeCashflows.csv");
            List<int> PaymentDateList = new List<int>();
            List<double> CashFlowList = new List<double>();

            int count = 0;
            int col = 0;
            int number;
            double cashFlowTemp=0;

            bool isParsable;

            foreach (var column in cashFlowData)
            {
                count += 1;
                if(count>3)
                {
                    col += 1;
                    switch (col)
                    {
                        case 1:
                            PaymentDateList.Add(YTM.GetMonths("20/05/2024", column));
                            break;
                        case 2:
                            isParsable = Int32.TryParse(column, out number);
                            if (isParsable)
                                cashFlowTemp += number;
                            break;
                        case 3:
                            isParsable = Int32.TryParse(column, out number);
                            if (isParsable)
                            {
                                cashFlowTemp += number;
                                CashFlowList.Add(cashFlowTemp);
                            }
                            cashFlowTemp = 0;
                            col = 0;
                            break;
                    }
                    
                }
            }

            foreach (var i in PaymentDateList)
            {
                Console.WriteLine(i);
            }
            foreach (var i in CashFlowList)
            {
                Console.WriteLine(i);
            }

            // use some numbers to illustrate the point
            // maturities in months
            int[] maturities = PaymentDateList.ToArray();
            double[] yearFrac = maturities.Select(i => i / 12.0).ToArray();
            // bond price
            double B = 108;

            // calculate future cashflows
            double[] cashflows = CashFlowList.ToArray();
            double initialGuess = 0.1;
            Console.WriteLine(YTM.CalculateYield(initialGuess, cashflows, yearFrac, B));
        }
    }
}