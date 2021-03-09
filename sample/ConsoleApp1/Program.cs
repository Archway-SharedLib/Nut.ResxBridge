using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Resources.Strings.Key1);
            Console.WriteLine(Resources.Strings.Key_2);
            Console.WriteLine(Resources.Strings.Key_3);
            Console.WriteLine(Resources.Strings.Key_4);
            Console.WriteLine(Resources.Strings.Val1);
            Console.WriteLine(Resources.Strings.Method1("P1", "P2", "P3"));

            Console.WriteLine(Resources.PublicStrings.Public_String);

        }
    }
}
