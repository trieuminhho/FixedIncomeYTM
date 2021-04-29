using System.IO;
using System.Collections.Generic;
using System;

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
                        if(!string.IsNullOrEmpty(item))
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
	}

    class Program
    {
        static void Main(string[] args)
        {
            List<string> cashFlowData;
            cashFlowData = YTM.ReadCSV("FixedIncomeCashflows.csv");

            foreach (var coloumn1 in cashFlowData)
            {
                Console.WriteLine(coloumn1);
            }
            Console.ReadLine();
        }
    }
}