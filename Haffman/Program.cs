using System;
using System.IO;
using HaffmanLibrary;

namespace Haffman
{
    class Program
    {
        static void Main(string[] args)
        {
            //C:\Users\kurat\source\repos\Разработка ПМ\Haffman\Haffman\StartText.txt
            //C:\Users\kurat\source\repos\Разработка ПМ\Haffman\Haffman\Archive.txt

            Console.WriteLine("Enter path file with text: ");
            string pathStart = Console.ReadLine();

            Archive.Notify += ShowMessage;
            Archive.ToArchive(pathStart);

            Console.WriteLine("Enter path file with archive: ");
            string pathArchive = Console.ReadLine();
            Archive.UnArchive(pathArchive);
        }

        private static void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
