using System;
using System.IO;
using HaffmanLibrary;

namespace Haffman
{
    class Program
    {
        static void Main()
        {
            //C:\Users\kurat\source\repos\Haffman\Haffman\StartText.txt
            //C:\Users\kurat\source\repos\Haffman\Haffman\StartText.Arch.txt

            ShowMenu();

            int key = 0;

            HuffmanArchiver.Notify += ShowMessage;

            while (true)
            {
                try
                {
                    Console.WriteLine("Enter key: ");
                    key = Convert.ToInt32(Console.ReadLine());
                }
                catch (Exception)
                {
                    Console.Write("Please, enter digit! ");
                }        

                try
                {
                    PerformAction(key);
                }
                catch (Exception)
                {
                    Console.WriteLine("Maybe mistake in path to file.");
                }
            }
        }

        private static void PerformAction(int key)
        {
            if (key == 1)
            {
                Console.WriteLine("Enter path file with text: ");
                string pathStart = Console.ReadLine();
                HuffmanArchiver.Archive(pathStart);
            }

            if (key == 2)
            {
                Console.WriteLine("Enter path file with archive: ");
                string pathArchive = Console.ReadLine();
                HuffmanArchiver.UnArchive(pathArchive);
            }

            if (key == 3)
            {
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("Key isn't correct!");
            }
        }

        private static void ShowMenu()
        {
            Console.WriteLine("Menu:");
            Console.WriteLine("1 - Archive");
            Console.WriteLine("2 - Unarchive");
            Console.WriteLine("3 - Exit");
        }

        private static void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
