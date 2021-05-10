using System;
using System.Collections.Generic;
using System.Text;

namespace HaffmanLibrary
{
    class Translate
    {
        //Method for translate number to binary
        public static string TranslateToBinary(string stringNumber)
        {
            int j = 0;

            int count = 0; //Quantity digits in binary code
            int decNumbers = TranslateIntegerToDecimal(stringNumber); //Write digit in decimal system
            int bufDecNumbers = decNumbers; //Copy of digit in decimal system to not change decNumber

            //Calculation quantity of digits in system _basis2
            while (bufDecNumbers > 0)
            {
                bufDecNumbers /= 2;
                count++;
            }

            char[] arrNumbers = new char[count]; //Array of digits in system _basis2
            char[] resultString = new char[100]; //Result in binary
            int index = 0;

            //Translate digit in system _basis2
            while (decNumbers > 0)
            {
                arrNumbers[index] = IntToChar(decNumbers % 2);
                decNumbers /= 2;
                index++;
            }

            index = 0;
            //Writing digit in system _basis2 to correct view
            for (int i = 0; i < count; i++)
            {
                resultString[i] = arrNumbers[count - 1 - i];
                index++;
            }

            string res = null, buf = null;
            int countBit = 0;
            index = 0;
            while (resultString[index] != '\0')
            {
                res += resultString[index];
                countBit++;
                index++;
            }
            while (countBit < 8)
            {
                buf += "0";
                countBit++;
            }
            buf += res;
            res = buf;

                return res;
        }

        //Method for translate number to decimal system
        private static int TranslateIntegerToDecimal(string number)
        {
            int decimalNumber = 0, //Number in decimal system
                index = 1; //The degree to which the number should be raised
            int count = number.Length; //Length of the number

            for (int i = count - 1; i >= 0; i--)
            {
                int bufNumber = CharToInt(number[i]);
                decimalNumber += (bufNumber * index);
                index *= 10;
            }

            return decimalNumber;
        }

        //Method for translate char to int
        private static int CharToInt(char symbol)
        {
            if (symbol >= '0' && symbol <= '9') return symbol - '0';
            else
            {
                if (symbol >= 'A' && symbol <= 'Z') return symbol - 'A' + 10;
                else return 100000;
            }
        }
        //Method for translate int to char
        private static char IntToChar(int numbers)
        {
            if (numbers <= 9) return (char)(numbers + '0');
            else return (char)(numbers + 55);
        }
    }
}
