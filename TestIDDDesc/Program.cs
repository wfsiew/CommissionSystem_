using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TestIDDDesc
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = "IDD_12-March06-STD15-MOB16(TF)CBD(20sec)-KL/Mel/Ns";
            Regex rx = new Regex("std\\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex rn = new Regex("\\d+");
            var x = rx.Match(s);
            var y = rn.Match(x.Value);

            Console.WriteLine(y.Value);

            Console.ReadKey();
        }
    }
}
