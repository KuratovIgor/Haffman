using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HaffmanLibrary
{
    public static class HuffmanArchiver
    {
        public delegate void ShowMessage(string message); 
        public static event ShowMessage Notify; 

        private static readonly Dictionary<char, int> _countChars = new Dictionary<char, int> { }; //Dictionary for storage quantity symbols in text
        private static readonly Dictionary<char, string> _binaryCodes = new Dictionary<char, string> { }; //Dictionary for storage new binary codes of symbols
        private static readonly Dictionary<string, char> _codesToSymbols = new Dictionary<string, char> { }; //Dictionary for translating binary codes to symbols

        private static readonly List<List> _listOfTree = new List<List> { }; 

        private static string _code = null; //Binary code
        private static string _pathSourceFile = null;
        private static string _pathArchiveFile = null; 
        private static string _pathResultFile = null; 

        public static void Archive(string pathSourceFile)
        {
            _pathSourceFile = pathSourceFile;
              
            int countExtraCode = 0;   

            CountCharacters(); //Counting symbols in text
            CreateTree(); 
            CreateBinaryCodes(_listOfTree[0]);

            //Creating archive file in directory with source file
            FileInfo file = new FileInfo(_pathSourceFile);
            string nameArchiveFile = "\\" + file.Name[0..^3] + "Arch.txt";
            CreateArchiveFile(nameArchiveFile); 

            using (StreamReader streamRead = File.OpenText(_pathSourceFile)) 
            {
                using FileStream streamWrite = new FileStream(_pathArchiveFile, FileMode.Create); 
                
                WriteTreeToFile(_listOfTree[0], streamWrite, ref countExtraCode); 

                //Counting and writing to file extra binary codes
                countExtraCode = 8 - (countExtraCode % 8);
                byte[] array = System.Text.Encoding.UTF8.GetBytes(Convert.ToString(countExtraCode));
                streamWrite.Write(array, 0, array.Length); //Writing extra code to file

                GenerateArchive(streamRead, streamWrite, countExtraCode);              
            }

            _binaryCodes.Clear(); 
            _codesToSymbols.Clear(); 

            Notify?.Invoke("File was archived!");
        }

        private static void GenerateArchive(StreamReader source, FileStream archive, int countExtraCode)
        {
            char symbolFromFile; 
            string byteCode = "", 
                extraByte; //Extra bits from byte

            while (byteCode.Length != countExtraCode)
                byteCode += '0';

            while (source.Peek() != -1)
            {
                symbolFromFile = Convert.ToChar(source.Read()); 

                byteCode += _binaryCodes[symbolFromFile];                  

                if (byteCode != null && byteCode.Length == 8)
                {
                    WriteByteToFile(archive, byteCode);

                    byteCode = null;
                } 

                if (byteCode != null && byteCode.Length > 8) 
                {
                    extraByte = byteCode[8..];
                    byteCode = byteCode.Substring(0, byteCode.Length - extraByte.Length);

                    WriteByteToFile(archive, byteCode);

                    byteCode = extraByte;
                }
            }

            //Write last code if it exist
            if (byteCode != null)
                WriteByteToFile(archive, byteCode);
        }

        private static void WriteByteToFile(FileStream archive, string code)
        {
            List<Byte> byteList = new List<Byte>()
            {
                Convert.ToByte(code, 2) //Convert string of bits to byte
            };

            archive.Write(byteList.ToArray()); 
        }

        public static void UnArchive(string pathArchive)
        {
            _pathArchiveFile = pathArchive;
            try
            {
                CreateResultFile("\\Result.txt"); 

                Notify?.Invoke("Please, wait...");

                using (BinaryReader readBinary = new BinaryReader(File.Open(pathArchive, FileMode.Open))) 
                {
                    using StreamWriter streamWrite = new StreamWriter(_pathResultFile, false, System.Text.Encoding.UTF8); 
                   
                    _listOfTree.Add(new List()); //Create first element in list for save head of tree
                    _listOfTree[0] = RecoverTree(_listOfTree[0], readBinary); 
                    CreateBinaryCodes(_listOfTree[0]);

                    UnarchiveFile(readBinary, streamWrite);
                }

                Notify?.Invoke("File was unarchived!");
            }
            catch (Exception)
            {
                Notify?.Invoke("Error when reading data from a file!");
            }
        }

        private static void UnarchiveFile(BinaryReader source, StreamWriter result)
        {
            StringBuilder readerCodes = new StringBuilder(); //For translate binary codes to symbols

            char charFromFile = source.ReadChar(); //Reading count extra code from file
            int countExtraCode = CharToInt(charFromFile);

            byte byteFromFile = source.ReadByte(); //Read byte from archive

            string stringBits = AppendToByte(byteFromFile);

            stringBits = stringBits[countExtraCode..];

            while (true)
            {
                for (int i = 0; i < stringBits.Length; i++)
                {
                    if (_codesToSymbols.ContainsKey(readerCodes.ToString()))
                    {
                        result.Write(_codesToSymbols[readerCodes.ToString()]); 
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
                        result.Write(_codesToSymbols[readerCodes.ToString()]); //Write symbol to result file
                        break;
                    }
                    catch (Exception)
                    {
                        break; 
                    }
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
            List newLeftBranch = new List();
            List newRightBranch = new List(); 

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
        private static void CountCharacters()
        {
            char symbol = '\0'; //Symbol from text

            try
            {
                using StreamReader streamRead = File.OpenText(_pathSourceFile);
                
                while (streamRead.Peek() != -1) 
                {
                    symbol = Convert.ToChar(streamRead.Read());

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
            catch (Exception) 
            {
                Notify?.Invoke("File isn't found!");
            }
        }

        private static void CreateTree()
        {
            //Convert all symbols in lists of tree and push in list
            foreach (char symbol in _countChars.Keys)
            {
                _listOfTree.Add(new List(symbol, _countChars[symbol]));
            }

            while (_listOfTree.Count != 1)
            {
                _listOfTree.Sort(); //Sort elments of tree by quantity

                //Create new list from first and second elements
                List newList = new List(_listOfTree[0].Weight + _listOfTree[1].Weight)
                {
                    Left = _listOfTree[0],
                    Right = _listOfTree[1]
                };

                _listOfTree.RemoveAt(0); //Delete first element 
                _listOfTree.RemoveAt(0); //Delete second element
                _listOfTree.Add(newList); //Add new element
            }
        }

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

        private static void CreateArchiveFile(string nameFile)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(_pathSourceFile); //Get directory where the start file is located

            string pathBuf = Convert.ToString(dirInfo.Parent) + nameFile;
            FileInfo file = new FileInfo(pathBuf); //Create new file in this directory
            _pathArchiveFile = Convert.ToString(file.FullName);
        }

        private static void CreateResultFile(string nameFile)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(_pathArchiveFile); //Get directory where the archive file is located;

            string pathBuf = Convert.ToString(dirInfo.Parent) + nameFile;
            FileInfo file = new FileInfo(pathBuf); //Create new file in this directory
            _pathResultFile = Convert.ToString(file.FullName);
        } 

        public static int CharToInt(char symbol)
        {
            if (symbol >= '0' && symbol <= '9') 
                return symbol - '0';

            if (symbol >= 'A' && symbol <= 'Z') 
                return symbol - 'A' + 10;
               
            else return 100000;
        }
    }
}

