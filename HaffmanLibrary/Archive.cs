using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HaffmanLibrary
{
    public static class Archive
    {
        public delegate void ShowMessage(string message); //Delegate for show message about error
        public static event ShowMessage Notify; //Event for show message about error

        private static Dictionary<char, int> _countChars = new Dictionary<char, int> { }; //Dictionary for storage quantity symbols in text
        private static Dictionary<char, string> _binaryCodes = new Dictionary<char, string> { }; //Dictionary for storage new binary codes of symbols
        private static Dictionary<string, char> _codesToSymbols = new Dictionary<string, char> { }; //Dictionary for translating binary codes to symbols

        private static List<List> _listOfTree = new List<List> { }; //List for creating binary tree

        private static string _code = null;
        private static string _pathArchive = @"ARCHIVE.txt";

        //Method for archiving file
        public static void ToArchive(string pathStart)
        {
            char symbol = '\0';
            string code = null, extraCode = null;
            int countBit = 0;

            ToCountChars(pathStart); //Counting symbols in text
            CreateTree(); //Create binary tree from symbols
            CreateBinaryCodes(_listOfTree[0]); //Create new binary codes for symbols
            WriteCodesToFile(); //Save all new binary codes in file "ARCHIVE.txt"

            using (StreamReader streamRead = File.OpenText(pathStart))
            {
                while (streamRead.Peek() != -1)
                {
                    symbol = Convert.ToChar(streamRead.Read());

                    code += _binaryCodes[symbol];

                    countBit = code.Length;

                    if (countBit == 8)
                    {
                        using (StreamWriter streamWrite = new StreamWriter(_pathArchive, true))
                        {
                           // streamWrite.Write($"{code} ");

                            List<Byte> byteList = new List<Byte>();

                            for (int i = 0; i < code.Length; i += 8)
                            {
                                byteList.Add(Convert.ToByte(code.Substring(i, 8), 2));
                            }

                            streamWrite.Write($"{Encoding.ASCII.GetString(byteList.ToArray())}");

                            code = null;
                            countBit = 0;
                        }
                    }
                    else if (countBit > 8)
                    {
                        extraCode = code.Substring(8);
                        code = code.Substring(0, code.Length - extraCode.Length);

                        using (StreamWriter streamWrite = new StreamWriter(_pathArchive, true))
                        {
                          //  streamWrite.Write($"{code} ");

                            List<Byte> byteList = new List<Byte>();

                            for (int i = 0; i < code.Length; i += 8)
                            {
                                byteList.Add(Convert.ToByte(code.Substring(i, 8), 2));
                            }

                            streamWrite.Write($"{Encoding.ASCII.GetString(byteList.ToArray())}");

                            code = extraCode;
                            countBit = extraCode.Length;
                        }
                    }
                }
                using (StreamWriter streamWrite = new StreamWriter(_pathArchive, true))
                {
                    while (extraCode.Length != 8)
                        extraCode += "0";
                    
                    //streamWrite.Write($"{extraCode} ");

                    List<Byte> byteList = new List<Byte>();

                    for (int i = 0; i < extraCode.Length; i += 8)
                    {
                        byteList.Add(Convert.ToByte(extraCode.Substring(i, 8), 2));
                    }

                    streamWrite.Write($"{Encoding.ASCII.GetString(byteList.ToArray())}");               
                }

                
                //byte[] strBytes = System.Text.Encoding.Unicode.GetBytes(symbol);
                //Console.WriteLine(strBytes);
            }

            _binaryCodes.Clear();
            _codesToSymbols.Clear();
        }
///////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void UnArchive (string pathResult)
        {
            try
            {
                string infoAboutCodes = null, codeForDictionary = null;
                int index = 2;

                using (StreamReader streamRead = File.OpenText(_pathArchive))
                {
                    while ((infoAboutCodes = streamRead.ReadLine()) != "=====================================================")
                    {
                        while (index < infoAboutCodes.Length)
                        {
                            codeForDictionary += infoAboutCodes[index];
                            index++;
                        }

                        index = 0;
                        _binaryCodes.Add(infoAboutCodes[0], codeForDictionary);
                        _codesToSymbols.Add(codeForDictionary, infoAboutCodes[0]);
                    }

                    while (streamRead.Peek() != -1)
                    {

                    }
                }
            }
            catch (Exception)
            {
                Notify?.Invoke("Error when reading data from a file!");
            }
        
        }
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Method for counting symbols in text
        private static void ToCountChars(string pathStart)
        {
            //pathStart - path of file with text
            char symbol = '\0'; //Symbol from text

            try
            {
                using (StreamReader streamRead = File.OpenText(pathStart))
                {
                    while (streamRead.Peek() != -1) //Until file don't over
                    {
                        symbol = Convert.ToChar(streamRead.Read()); //Reading symbol from file

                        try
                        {
                            _countChars[symbol]++; //If symbol is exist in dictionary count it
                        }
                        catch (Exception)
                        {
                            _countChars.Add(symbol, 1); //Else push symbol in dictionary
                        }
                    }
                }
            }
            catch (Exception) //If file not found show error
            {
                Notify?.Invoke("File isn't found!");
            }
        }

        //Method for create binary tree for creating codes
        private static void CreateTree()
        {
            //Convert all symbols in lists of tree and push in list
            foreach (char symbol in _countChars.Keys)
            {
                _listOfTree.Add(new List(symbol, _countChars[symbol]));
            }

            //Create binary tree
            while (_listOfTree.Count != 1)
            {
                _listOfTree.Sort(); //Sort elments of tree by quantity

                //Create new list from first and second elements
                List newList = new List(_listOfTree[0].Weight + _listOfTree[1].Weight);
                newList.Left = _listOfTree[0];
                newList.Right = _listOfTree[1];

                _listOfTree.RemoveAt(0); //Delete first element 
                _listOfTree.RemoveAt(0); //Delete second element
                _listOfTree.Add(newList); //Add new element
            }
        }

        //Methos for create binary codes for symbols from text
        private static void CreateBinaryCodes(List list)
        {
            //If go left, 0 is added to the code
            if (list.Left != null)
            {
                _code += "0";
                CreateBinaryCodes(list.Left);
            }

            //If go right, 1 is added to the code
            if (list.Right != null)
            {
                _code += "1";
                CreateBinaryCodes(list.Right);
            }

            //if the code is created, add it to the libraries
            if (list.Left == null && list.Right == null)
            {
                _binaryCodes.Add(list.Value, _code);
                _codesToSymbols.Add(_code, list.Value);
            }

            list.Code = _code; //Save new binary code
            _code = null;

            //Step back up the tree
            if (list.Code != null)
                for (int i = 0; i < list.Code.Length - 1; i++)
                {
                    _code += list.Code[i];
                }
        }

        //Method for writing all new binary codes to file ARCHIVE.txt
        private static void WriteCodesToFile()
        {
            try
            {
                using (StreamWriter streamWrite = new StreamWriter(_pathArchive, false))
                { 
                    foreach(char ch in _binaryCodes.Keys)
                    {
                        streamWrite.WriteLine($"{ch} {_binaryCodes[ch]}");
                    }

                    streamWrite.WriteLine("=====================================================");

                    Notify?.Invoke("New binary codes was writed in file ARCHIVE.txt!");
                }
            }
            catch (Exception)
            {
                Notify?.Invoke("Error when writing data to a file!");
            }
            
        }
    }
}

