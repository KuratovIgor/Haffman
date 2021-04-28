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

            int count = 0; //Quantity digits in binary
            int decNumbers = TranslateIntegerToDecimal(stringNumber); //Записываем число в десятичной системе счисления
            int bufDecNumbers = decNumbers; //Переменная-буфер для хранения копии десятичной записи числа,
                                                //чтобы не изменять значение decNumbers

            //Вычисление количества цифр в записи числа в системе счисления _basis2
            while (bufDecNumbers > 0)
            {
                bufDecNumbers /= 2;
                count++;
            }

            char[] arrNumbers = new char[count]; //Массив цифр в системе счисления _basis2
            char[] resultString = new char[100]; //Результат в виде числа в системе счисления _basis2
            int index = 0;

            //Перевод числа в систему счисления _basis2
            while (decNumbers > 0)
            {
                arrNumbers[index] = IntToChar(decNumbers % 2);
                decNumbers /= 2;
                index++;
            }

            index = 0;
            //Запись числа в системе счисления _basis2 в корректный вид
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

        //Функция перевода целой части числа в десятичную систему счисления
        private static int TranslateIntegerToDecimal(string number)
        {
            int decimalNumber = 0, //Хранит число в десятичной системе счисления
                index = 1; //Хранит степень, в которую нужно возводить каждую цифру числа
            int count = number.Length; //Хранит длину целочисленной части числа

            for (int i = count - 1; i >= 0; i--)
            {
                int bufNumber = CharToInt(number[i]);
                decimalNumber += (bufNumber * index);
                index *= 10;
            }

            return decimalNumber;
        }

        //Функция перевода символьного значения числа в целочисленное
        private static int CharToInt(char symbol)
        {
            //symbol - Символьное значения числа
            if (symbol >= '0' && symbol <= '9') return symbol - '0';
            else
            {
                if (symbol >= 'A' && symbol <= 'Z') return symbol - 'A' + 10;
                else return 100000;
            }
        }
        //Функция перевода целочисленного значения числа в символьное
        private static char IntToChar(int numbers)
        {
            //numbers - целочисленное значение числа
            if (numbers <= 9) return (char)(numbers + '0');
            else return (char)(numbers + 55);
        }
    }
}
