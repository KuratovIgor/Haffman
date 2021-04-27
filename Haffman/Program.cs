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
            //G:\Куратов И. А. 907б2\Haffman\Haffman\StartText.txt


            Console.WriteLine("Enter path file with text: ");
            string pathStart = Console.ReadLine();

            Archive.ToArchive(pathStart);
        }
    }
}
