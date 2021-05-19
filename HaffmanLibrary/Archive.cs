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
              
            int countExtraCode = 0; //Count bits in archive text  

            ToCountChars(); //Counting symbols in text
            CreateTree(); //Create binary tree from symbols
            CreateBinaryCodes(_listOfTree[0]); //Create new binary codes for symbols

            FileInfo file = new FileInfo(_pathStart);
            string nameFile = "\\" + file.Name.Substring(0, file.Name.Length - 3) + "Arch.txt";
            CreateFile(ref _pathArchive, nameFile); //Creating file with codes

            using (StreamReader streamRead = File.OpenText(_pathStart)) //Read source text from file
            {
                using (FileStream streamWrite = new FileStream(_pathArchive, FileMode.Create)) //Write arhive text to buf file
                {
                    WriteTreeToFile(_listOfTree[0], streamWrite, ref countExtraCode); //Save binary tree in file "Archive.txt"

                    countExtraCode = 8 - (countExtraCode % 8);
                    byte[] array = System.Text.Encoding.UTF8.GetBytes(Convert.ToString(countExtraCode));
                    streamWrite.Write(array, 0, array.Length); //Writing extra code to file

                    CreateAndWriteArchive(streamRead, streamWrite, countExtraCode); //Archiving and writing archive
                }
            }

            _binaryCodes.Clear(); //Clear dictionary with new binary codes
            _codesToSymbols.Clear(); //Clear dictionary with codes and symbols

            Notify?.Invoke("File was archived!");
        }

        //Method for archiving file
        private static void CreateAndWriteArchive (StreamReader source, FileStream archive, int countExtraCode)
        {
            char symbol = '\0'; //Symbol from file
            string code = "", //Binary code for archive
                extraCode = null; //Extra bits from byte
            bool firstByte = true;

            while (source.Peek() != -1)
            {
                if (firstByte == true)
                {
                    while (code.Length != countExtraCode)
                        code += '0';

                    firstByte = false;
                }

                symbol = Convert.ToChar(source.Read()); //Read symbol from file

                code += _binaryCodes[symbol]; //Creating new binary code for archive                 

                if (code != null &&  code.Length == 8) //If bits make up byte, translate it to symbol and write to arhcive
                {
                    ArchiveSymbol(archive, code);

                    code = null;
                } 

                if (code != null && code.Length > 8) //Extra bits relocate to next byte
                {
                    extraCode = code.Substring(8);
                    code = code.Substring(0, code.Length - extraCode.Length);

                    ArchiveSymbol(archive, code);

                    code = extraCode;
                }
            }

            //Write last code if it exist
            if (code != null)
                ArchiveSymbol(archive, code);
        }

        private static void ArchiveSymbol(FileStream archive, string code)
        {
            List<Byte> byteList = new List<Byte>(); //List of bytes

            byteList.Add(Convert.ToByte(code, 2)); //Convert string of bits to byte

            archive.Write(byteList.ToArray()); //Write new symbol to file
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

                        UnarchiveFile(readBinary, streamWrite);
                    }
                }

                Notify?.Invoke("File was unarchived!");
            }
            catch (Exception)
            {
                Notify?.Invoke("Error when reading data from a file!");
            }
        }

        private static void UnarchiveFile(BinaryReader source, StreamWriter unarchive)
        {
            StringBuilder readerCodes = new StringBuilder(); //For translate binary codes to symbols
            byte byteFromFile ; //For read symbols as a byte
            int countExtraCode;
            char charFromFile;

            charFromFile = source.ReadChar(); //Reading count extra code from file
            countExtraCode = CharToInt(charFromFile);

            byteFromFile = source.ReadByte(); //Read byte from archive

            string stringBits = AppendToByte(byteFromFile);

            stringBits = stringBits.Substring(countExtraCode);

            while (true)
            {
                for (int i = 0; i < stringBits.Length; i++)
                {
                    if (_codesToSymbols.ContainsKey(readerCodes.ToString()))
                    {
                        unarchive.Write(_codesToSymbols[readerCodes.ToString()]); //Write unarchiving symbol to result file
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
                    byteFromFile = source.ReadByte(); //Read byte from archive
                }
                catch (Exception)
                {
                    try
                    {
                        unarchive.Write(_codesToSymbols[readerCodes.ToString()]); //Write symbol to result file
                        break;
                    }
                    catch (Exception) { break; }
                }

                stringBits = AppendToByte(byteFromFile);
            }
        }

        private static string AppendToByte(byte byteFromFile)
        {
            StringBuilder newBinaryCode = new StringBuilder("00000000");

            string binaryCode = Convert.ToString(byteFromFile, 2);
            int index = newBinaryCode.Length - binaryCode.Length;
            newBinaryCode.Remove(index, binaryCode.Length);
            newBinaryCode.Insert(index, binaryCode);

            return newBinaryCode.ToString();
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

