using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemCParser;

namespace SystemCParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\Henry\repos\tonkalib\designs\SystemC_Examples\results\gvkbzzga\local_ccled\local_ccled.h";
            Uncomment nocomment = new Uncomment(filePath);
            Console.WriteLine("Done:\n{0}", nocomment.result);

            // ToDo: parse the system C info from the "nocomment.result" string.
            ScParse parsed = new ScParse(nocomment.result);
        }
    }
}
