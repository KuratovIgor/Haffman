using System;
using System.Collections.Generic;
using System.IO;

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

        private static string _code = null; //
        private static string _pathArchive = @"ARCHIVE.txt"; //Path archive file
        private static string _pathCodes = @"CODES.txt"; //Path file with new binary codes

        //Method for archiving file
        public static void ToArchive(string pathStart)
        {
            //pathStart - path file with source text
            char symbol = '\0'; //Symbol from file
            string code = null, //Binary code for archive
                extraCode = null; //Extra bits from byte
            int countBit = 0; //Quantity of bits

            ToCountChars(pathStart); //Counting symbols in text
            CreateTree(); //Create binary tree from symbols
            CreateBinaryCodes(_listOfTree[0]); //Create new binary codes for symbols
            WriteCodesToFile(); //Save all new binary codes in file "ARCHIVE.txt"

            using (StreamReader streamRead = File.OpenText(pathStart)) //Read source text from file
            {
                using (FileStream streamWrite = new FileStream(_pathArchive, FileMode.Create)) //Write arhive text to new file
                {
                    byte[] array; //Array of bytes
                    while (streamRead.Peek() != -1)
                    {
                        symbol = Convert.ToChar(streamRead.Read()); //Read symbol from file

                        code += _binaryCodes[symbol]; //Creating new binary code for archive

                        countBit = code.Length;

                        if (countBit == 8) //If bits make up byte, translate it to symbol and write to arhcive
                        {
                            List<Byte> byteList = new List<Byte>(); //List of bytes

                            for (int i = 0; i < code.Length; i += 8)
                            {
                                byteList.Add(Convert.ToByte(code.Substring(i, 8), 2)); //Convert string of bits to byte
                            }

                            streamWrite.Write(byteList.ToArray()); //Write new symbol to file

                            code = null;
                            countBit = 0;
                        }
                        else if (countBit > 8) //Extra bits relocate to next byte
                        {
                            extraCode = code.Substring(8);
                            code = code.Substring(0, code.Length - extraCode.Length);

                            List<Byte> byteList = new List<Byte>();

                            for (int i = 0; i < code.Length; i += 8)
                            {
                                byteList.Add(Convert.ToByte(code.Substring(i, 8), 2));
                            }

                            streamWrite.Write(byteList.ToArray());

                            code = extraCode;
                            countBit = extraCode.Length;
                        }
                    }

                    //Write last code if it exist
                    if (code != null) 
                    {
                        while (code.Length != 8)
                            code += "0";

                        List<Byte> byteList = new List<Byte>();

                        for (int i = 0; i < code.Length; i += 8)
                        {
                            byteList.Add(Convert.ToByte(code.Substring(i, 8), 2));
                        }

                        streamWrite.Write(byteList.ToArray());
                    }
                }
            }

            _binaryCodes.Clear(); //Clear dictionary with new binary codes
            _codesToSymbols.Clear(); //Clear dictionary with codes and symbols

            Notify?.Invoke("File was archived!");
        }

        //Method for unarchiving file
        public static void UnArchive (string pathResult)
        {
            try
            {
                RecoverCodes(); //Read binary code for symbols from file

                Notify?.Invoke("Please, wait...");

                using (BinaryReader readBinary = new BinaryReader(File.Open(_pathArchive, FileMode.Open))) //Read archive
                {
                    using (StreamWriter streamWrite = new StreamWriter(pathResult, false)) //Write unarchiving text to result file
                    {
                        string readerCodes = null; //For translate binary codes to symbols
                        while (true)
                        {
                            byte byteFromFile = 0; //For read symbols as a byte
                            try
                            {
                                byteFromFile = readBinary.ReadByte(); //Read byte from archive
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    streamWrite.Write(_codesToSymbols[readerCodes]); //Write in unarchiving symbol to result file
                                    break;
                                }
                                catch (Exception) { break; }   
                            }
                            
                            string stringBits = Translate.TranslateToBinary(byteFromFile.ToString()); //String with bits of symbol
                                                                                                     //Translate byte to bits

                            for (int i = 0; i < stringBits.Length; i++)
                            {
                                try
                                {
                                    streamWrite.Write(_codesToSymbols[readerCodes]); //Write in unarchiving symbol to result file
                                    readerCodes = Convert.ToString(stringBits[i]); //Write in readerCodes first element of next code
                                }
                                catch (Exception)
                                {
                                    readerCodes += stringBits[i]; //Push next bit to readerCode
                                }
                            }
                        }
                    }                 
                }

                Notify?.Invoke("File was unarchived!");
            }
            catch (Exception)
            {
                Notify?.Invoke("Error when reading data to a file!"); 
            }
        }

        //Method for reading binary code for symbols from file
        private static void RecoverCodes ()
        {
            string infoAboutCodes = null, //For symbols and codes from file
                codeForDictionary = null; //Binary code for pushing in dictionary
            int index = 2; //Code start index in file

            using (StreamReader streamRead = File.OpenText(_pathCodes))
            {
                while (streamRead.Peek() != -1)
                {
                    infoAboutCodes = streamRead.ReadLine(); //Read symbols and codes from file

                    while (index < infoAboutCodes.Length)
                    {
                        codeForDictionary += infoAboutCodes[index]; //Recovering dictionary by info from file
                        index++;
                    }

                    index = 2;
                    _binaryCodes.Add(infoAboutCodes[0], codeForDictionary); //Push binary code in dictionary
                    _codesToSymbols.Add(codeForDictionary, infoAboutCodes[0]); //Push symbols and codes in dictionary
                    codeForDictionary = null;
                }
            }
        }

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

        //Method for writing all new binary codes to file CODES.txt
        private static void WriteCodesToFile()
        {
            try
            {
                using (StreamWriter streamWrite = new StreamWriter(_pathCodes, false))
                { 
                    foreach(char ch in _binaryCodes.Keys)
                    {
                        streamWrite.WriteLine($"{ch} {_binaryCodes[ch]}");
                    }

                    Notify?.Invoke("New binary codes was writed in file CODES.txt!");
                }
            }
            catch (Exception)
            {
                Notify?.Invoke("Error when writing data to a file!");
            }
            
        }
    }
}

