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

            ShowMenu();

            int key = 0;

            Archive.Notify += ShowMessage;

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
                    switch (key)
                    {
                        case 1:
                            {
                                Console.WriteLine("Enter path file with text: ");
                                string pathStart = Console.ReadLine();
                                Archive.ToArchive(pathStart);
                            }
                            break;
                        case 2:
                            {
                                Console.WriteLine("Enter path file with archive: ");
                                string pathArchive = Console.ReadLine();
                                Archive.UnArchive(pathArchive);
                            }
                            break;
                        case 3:
                            {
                                Environment.Exit(1);
                            }
                            break;
                        default:
                            {
                                Console.WriteLine("Key isn't correct!");
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Maybe mistake in path to file.");
                }
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
