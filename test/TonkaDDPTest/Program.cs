﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TonkaACMTest
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            int ret = Xunit.ConsoleClient.Program.Main(new string[] {
                Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length),
                //"/noshadow",
                // "/trait", "THIS=ONE",
            });
            Console.In.ReadLine();
            return ret;
        }
    }
}
