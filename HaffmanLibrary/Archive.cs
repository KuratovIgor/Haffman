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

        private static string _code = null; //Binary code
        private static string _pathStart = null; //Path start file
        private static string _pathArchive = null; //Path archive file
        private static string _pathResult = null; //Path result file

        //Method for archiving file
        public static void ToArchive(string pathStart)
        {
            //pathStart - path file with source text
            _pathStart = pathStart;
            char symbol = '\0'; //Symbol from file
            string code = "", //Binary code for archive
                extraCode = null; //Extra bits from byte
            int countExtraCode = 0; //Count bits in archive text
            bool firstByte = true;

            ToCountChars(); //Counting symbols in text
            CreateTree(); //Create binary tree from symbols
            CreateBinaryCodes(_listOfTree[0]); //Create new binary codes for symbols
            CreateFile(ref _pathArchive, "\\Archive.txt"); //Creating file with codes

            using (StreamReader streamRead = File.OpenText(_pathStart)) //Read source text from file
            {
                using (FileStream streamWrite = new FileStream(_pathArchive, FileMode.Create)) //Write arhive text to buf file
                {
                    WriteTreeToFile(_listOfTree[0], streamWrite, ref countExtraCode); //Save binary tree in file "Archive.txt"
                    countExtraCode = 8 - (countExtraCode % 8);
                    byte[] array = System.Text.Encoding.UTF8.GetBytes(Convert.ToString(countExtraCode));
                    streamWrite.Write(array, 0, array.Length); //Writing extra code to file

                    while (streamRead.Peek() != -1)
                    {
                        if (firstByte == true)
                        {
                            while (code.Length != countExtraCode)
                                code += '0';
                            firstByte = false;
                        }

                        symbol = Convert.ToChar(streamRead.Read()); //Read symbol from file

                        code += _binaryCodes[symbol]; //Creating new binary code for archive                 

                        if (code.Length == 8) //If bits make up byte, translate it to symbol and write to arhcive
                        {
                            List<Byte> byteList = new List<Byte>(); //List of bytes

                            byteList.Add(Convert.ToByte(code, 2)); //Convert string of bits to byte

                            streamWrite.Write(byteList.ToArray()); //Write new symbol to file

                            code = null;
                        }
                        else if (code.Length > 8) //Extra bits relocate to next byte
                        {
                            extraCode = code.Substring(8);
                            code = code.Substring(0, code.Length - extraCode.Length);

                            List<Byte> byteList = new List<Byte>();

                            byteList.Add(Convert.ToByte(code, 2));

                            streamWrite.Write(byteList.ToArray());

                            code = extraCode;
                        }
                    }

                    //Write last code if it exist
                    if (code != null)
                    {
                        List<Byte> byteList = new List<Byte>();

                        byteList.Add(Convert.ToByte(code, 2));

                        streamWrite.Write(byteList.ToArray());
                    }
                }
            }

            _binaryCodes.Clear(); //Clear dictionary with new binary codes
            _codesToSymbols.Clear(); //Clear dictionary with codes and symbols

            Notify?.Invoke("File was archived!");
        }

        //Method for unarchiving file
        public static void UnArchive(string pathArchive)
        {
            _pathArchive = pathArchive;
            try
            {
                CreateFile(ref _pathResult, "\\Result.txt"); //Creating result file

                Notify?.Invoke("Please, wait...");

                using (BinaryReader readBinary = new BinaryReader(File.Open(pathArchive, FileMode.Open))) //Read archive
                {
                    using (StreamWriter streamWrite = new StreamWriter(_pathResult, false, System.Text.Encoding.UTF8)) //Write unarchiving text to result file
                    {
                        _listOfTree.Add(new List()); //Create first element in list for save head of tree
                        _listOfTree[0] = RecoverTree(_listOfTree[0], readBinary); //Read binary tree from file
                        CreateBinaryCodes(_listOfTree[0]); //Create codes from tree

                        StringBuilder readerCodes = new StringBuilder(); //For translate binary codes to symbols
                        byte byteFromFile = 0; //For read symbols as a byte
                        int countExtraCode = 0; 

                        char charFromFile = '\0';

                        charFromFile = readBinary.ReadChar(); //Reading count extra code from file
                        countExtraCode = CharToInt(charFromFile);

                        byteFromFile = readBinary.ReadByte(); //Read byte from archive

                        StringBuilder newString = new StringBuilder("00000000"); //String for recover code to byte

                        string binaryCode = Convert.ToString(byteFromFile, 2);
                        int index = newString.Length - binaryCode.Length;
                        newString.Remove(index, binaryCode.Length);
                        newString.Insert(index, binaryCode);

                        string stringBits = newString.ToString();

                        stringBits = stringBits.Substring(countExtraCode);

                        while (true)
                        {
                            for (int i = 0; i < stringBits.Length; i++)
                            {
                                if (_codesToSymbols.ContainsKey(readerCodes.ToString()))
                                {
                                    streamWrite.Write(_codesToSymbols[readerCodes.ToString()]); //Write unarchiving symbol to result file
                                    readerCodes.Remove(0, readerCodes.Length);
                                    readerCodes.Insert(0, Convert.ToString(stringBits[i])); //Write in readerCodes first element of next code
                                }
                                else
                                {
                                    readerCodes.Append(stringBits[i]); //Push next bit to readerCode
                                }
                            }

                            try
                            {
                                byteFromFile = readBinary.ReadByte(); //Read byte from archive
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    streamWrite.Write(_codesToSymbols[readerCodes.ToString()]); //Write symbol to result file
                                    break;
                                }
                                catch (Exception) { break; }
                            }

                            newString = new StringBuilder("00000000");

                            binaryCode = Convert.ToString(byteFromFile, 2);
                            index = newString.Length - binaryCode.Length;
                            newString.Remove(index, binaryCode.Length);
                            newString.Insert(index, binaryCode);

                            stringBits = newString.ToString();
                        }
                    }
                }

                Notify?.Invoke("File was unarchived!");
            }
            catch (Exception)
            {
                Notify?.Invoke("Error when reading data from a file!");
            }
        }

        //Method for reading binary tree from file
        private static List RecoverTree(List head, BinaryReader streamRead)
        {
            List newLeftBranch = new List(); //For add element to left branch
            List newRightBranch = new List(); //For add element to right branch

            if ((Convert.ToChar(streamRead.Read())) == '0') //0 - not list
            {
                head.Left = newLeftBranch;
                RecoverTree(head.Left, streamRead);

                head.Right = newRightBranch;
                RecoverTree(head.Right, streamRead);
            }
            else //1 - list
            {
                head.Value = Convert.ToChar(streamRead.ReadChar()); //writing value in list
            }

            return head;
        }

        //Method for counting symbols in text
        private static void ToCountChars()
        {
            char symbol = '\0'; //Symbol from text

            try
            {
                using (StreamReader streamRead = File.OpenText(_pathStart))
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

        //Method for writing all new binary codes to file Codes.txt
        private static void WriteTreeToFile(List list, FileStream streamWrite, ref int countBits)
        {
            byte[] array;

            //If go left, 0 is writed to the file
            if (list.Left != null || list.Right != null)
            {
                array = System.Text.Encoding.UTF8.GetBytes("0");
                streamWrite.Write(array, 0, array.Length);
                WriteTreeToFile(list.Left, streamWrite, ref countBits);
                WriteTreeToFile(list.Right, streamWrite, ref countBits);
            }

            //if position on the list, write 1 and symbol to the file
            if (list.Left == null && list.Right == null)
            {
                array = System.Text.Encoding.UTF8.GetBytes("1");
                streamWrite.Write(array, 0, array.Length);
                array = System.Text.Encoding.Default.GetBytes(Convert.ToString(list.Value));
                streamWrite.Write(array);
                countBits += list.Weight * list.Code.Length;
            }
        }

        //Method for create files in needed directory
        private static void CreateFile(ref string pathToFile, string nameFile)
        {
            DirectoryInfo dirInfo;
            try
            {
                dirInfo = new DirectoryInfo(_pathStart); //Get directory where the start file is located
            }
            catch (Exception)
            {
                dirInfo = new DirectoryInfo(_pathArchive); //Get directory where the archive file is located
            }
            string pathBuf = Convert.ToString(dirInfo.Parent) + nameFile;
            FileInfo file = new FileInfo(pathBuf); //Create new file in this directory
            pathToFile = Convert.ToString(file.FullName);
        }

        //Method for translate char to int
        public static int CharToInt(char symbol)
        {
            if (symbol >= '0' && symbol <= '9') return symbol - '0';
            else
            {
                if (symbol >= 'A' && symbol <= 'Z') return symbol - 'A' + 10;
                else return 100000;
            }

        }
    }
}

