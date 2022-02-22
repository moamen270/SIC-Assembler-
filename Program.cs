using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SP
{
    public class Program
    {
        public static string[] ReadFile(string Filename)
        {
            File.WriteAllLines(Filename, File.ReadAllLines(Filename).Where(arg => !string.IsNullOrWhiteSpace(arg)));
            return File.ReadAllLines(Filename);
        }

        public static List<Line> Prepare(string[] lines)
        {

            List<Line> Lines = new List<Line>();
            for (int i = 0; i < lines.Length; i++)
            {
                string[] Parts = lines[i].Split(' ');

                if (Parts[0] == ".")
                    continue;

                List<string> line = new List<string>();
                if (string.IsNullOrWhiteSpace(Parts[0]))
                    line.Add(Parts[0]);

                foreach (var item in Parts)
                    if (!string.IsNullOrWhiteSpace(item))
                        line.Add(item);

                if (string.IsNullOrWhiteSpace(Parts[2]))
                    line.Add(Parts[2]);

                Lines.Add(new Line(line[0], line[1], line[2]));

            }
            return Lines;
        }

        public static string AddHex(string Num1, int Num2)
        {
            int result = Convert.ToInt32(Num1, 16) + Num2;
            return result.ToString("X");
        }

        public static int IncreaseAmount(string Part2, string Part3)
        {
            if (Part2 == "RESW")
            {
                int value = int.Parse(Part3);

                return value * 3;
            }
            else
             if (Part2 == "RESB")
            {
                int value = int.Parse(Part3);

                return value;
            }
            else
            if (Part2 == "BYTE")
            {
                string item = Part3;
                int value = 0;
                if (item[0] == 'C')
                {
                    value = (item.Length - 3);
                }
                if (item[0] == 'X')
                {
                    double test = (item.Length - 3) / 2;
                    value = (int)test;
                    if (test - value != 0)
                        value++;
                }
                return value;
            }
            else
            {
                return 3;
            }
        }

        public static string InsertBits(string word, int num)
        {
            string bits = "";
            for (int i = word.Length; i < num; i++)
            {
                bits += "0";
            }
            return bits + word;

        }

        public static string AddEight(char hex)
        {
            string p = hex.ToString();
            if (int.Parse(p) > 7)
                return p;
            return AddHex(p, 8);
        }
        public static void ObjectCodeProcess(List<Line> Lines, Dictionary<string, string> OpCode)
        {

            for (int i = 0; i < Lines.Count - 2; i++)
            {
                if (Lines[i + 1].Part2 == "RESB" || Lines[i + 1].Part2 == "RESW")
                    Lines[i + 1].ObjectCode = "      ";
                else
                if (Lines[i + 1].Part2 == "WORD")
                {
                    Lines[i + 1].ObjectCode += InsertBits(Lines[i + 1].Part3, 6);
                }
                else
                    if (Lines[i + 1].Part2 == "BYTE")
                    if (Lines[i + 1].Part3[0] == 'X')
                        Lines[i + 1].ObjectCode += Lines[i + 1].Part3.Substring(2, Lines[i + 1].Part3.Length - 3);
                    else
                        foreach (var item in Lines[i + 1].Part3.Substring(2, Lines[i + 1].Part3.Length - 3))
                        {
                            int ascii = (int)item;
                            Lines[i + 1].ObjectCode += ascii.ToString("X");
                        }
                else
                {
                    Lines[i + 1].ObjectCode = OpCode[Lines[i + 1].Part2];
                    string test = Lines[i + 1].Part3;
                    if (test.Length > 0 && test[test.Length - 1] == 'X')
                    {
                        string Value = test.Substring(0, test.Length - 2);
                        string address = "";
                        foreach (var item in Lines)
                        {
                            if (item.Part1 == Value)
                                address = item.Location;
                        }
                        string test2 = "";
                        test2 += AddEight(address[0]);
                        test2 += address.Substring(1, address.Length - 1);
                        Lines[i + 1].ObjectCode += test2.Trim();
                    }
                    else
                    {
                        string address = "";
                        foreach (var item in Lines)
                        {
                            if (item.Part1 == test)
                                address = item.Location;
                        }
                        Lines[i + 1].ObjectCode += address;
                    }
                }

            }

        }
        public static string[] ObjectProgramProcess(List<Line> Lines)
        {
            string Name = Lines[0].Part1;
            for (int i = Name.Length; i < 6; i++)
                Name += " ";

            string ProgramSize = AddHex(Lines[Lines.Count - 2].Location, -Convert.ToInt32(Lines[1].Location, 16) + 1);
            string Head = "H," + Name + "," + InsertBits(Lines[1].Location, 6) + "," + InsertBits(ProgramSize, 6);
            string End = "E," + InsertBits(Lines[1].Location, 6);

            List<string> Text = new List<string>();
            int Size = 0, C = 0;
            while (C < Lines.Count - 2)
            {
                string StartAddress = Lines[C + 1].Location;
                string Record = "";
                for (int i = C; i < C + 10; i++)
                {
                    if (i >= Lines.Count - 2)
                        break;
                    if (Lines[i + 1].Part2 == "BYTE")
                        Size += IncreaseAmount(Lines[i + 1].Part2, Lines[i + 1].Part3);
                    else
                    if (Lines[i + 1].ObjectCode != "      ")
                        Size += 3;
                }
                for (int i = C; i < C + 10; i++)
                {
                    if (i >= Lines.Count - 2)
                        break;
                    if (Lines[i + 1].ObjectCode != "      ")
                        Record += "," + Lines[i + 1].ObjectCode.Trim();
                }
                string HexSize = InsertBits(Size.ToString("X"), 2);
                Text.Add("T," + InsertBits(StartAddress, 6) + "," + HexSize + Record);
                C += 10;
                Size = 0;
            }
            // Object Program Display
            Console.WriteLine("\n\n" + Head);
            foreach (var item in Text)
                Console.WriteLine(item);
            Console.WriteLine(End);
            // Store Object Program
            string[] ObjectProgram = new string[Text.Count + 2];
            ObjectProgram[0] = Head;
            ObjectProgram[ObjectProgram.Length - 1] = End;
            for (int i = 1; i < ObjectProgram.Length - 1; i++)
                ObjectProgram[i] = Text[i - 1];
            return ObjectProgram;
        }

        public static void Create_WriteFile(List<Line> Lines, string[] ObjectProgram)
        {
            string fileName1 = @"C:\moamen";
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(fileName1, "Answer.txt")))
            {
                foreach (var line in Lines)
                    outputFile.WriteLine((line.Location + "\t" + line.Part1 + "\t" + line.Part2 + "\t" + line.Part3.Trim() + "\t" + line.ObjectCode));
                outputFile.WriteLine("\n \n");
                foreach (string line in ObjectProgram)
                    outputFile.WriteLine(line);
            }
        }



        static void Main(string[] args)
        {
            // OPCODE
            Dictionary<string, string> OpCode = new Dictionary<string, string>
            {
                { "ADD", "18" },
                { "ADDF", "58" },
                { "ADDR", "90" },
                { "AND", "40" },
                { "CLEAR", "B4" },
                { "COMP", "28" },
                { "COMPF", "88" },
                { "COMPR", "A0" },
                { "DIV", "24" },
                { "DIVF", "64" },
                { "DIVR", "9C" },
                { "FIX", "C4" },
                { "FLOAT", "C0" },
                { "HIO", "F4" },
                { "J", "3C" },
                { "JEQ", "30" },
                { "JGT", "34" },
                { "JLT", "38" },
                { "JSUB", "48" },
                { "LDA", "00" },
                { "LDB", "68" },
                { "LDCH", "50" },
                { "LDF", "70" },
                { "LDL", "08" },
                { "LDS", "6C" },
                { "LDT", "74" },
                { "LDX", "04" },
                { "LPS", "D0" },
                { "MUL", "20" },
                { "MULF", "60" },
                { "MULR", "98" },
                { "NORM", "C8" },
                { "OR", "44" },
                { "RD", "D8" },
                { "RMO", "AC" },
                { "RSUB", "4C0000" },
                { "SHIFTL", "A4" },
                { "SHIFTR", "A8" },
                { "SIO", "F0" },
                { "SSK", "EC" },
                { "STA", "0C" },
                { "STB", "78" },
                { "STCH", "54" },
                { "STF", "80" },
                { "STI", "D4" },
                { "STL", "14" },
                { "STS", "7C" },
                { "STSW", "E8" },
                { "STT", "84" },
                { "STX", "10" },
                { "SUB", "1C" },
                { "SUBF", "5C" },
                { "SUBR", "94" },
                { "SVC", "B0" },
                { "TD", "E0" },
                { "TIO", "F8" },
                { "TIX", "2C" },
                { "TIXR", "B8" },
                { "WD", "DC" }
            };

            // ReadFile                      
            string[] lines = ReadFile(@"C:\moamen\Q1.txt");

            // Prepare            
            List<Line> Lines = Prepare(lines);

            // Loccation  
            string LocProcess = Lines[0].Part3;
            string[] Locations = new string[Lines.Count - 2];
            Locations[0] = LocProcess;
            Lines[1].Location = Locations[0];
            for (int i = 1; i < Locations.Length; i++)
            {
                Locations[i] = AddHex(LocProcess, IncreaseAmount(Lines[i].Part2, Lines[i].Part3));
                Lines[i + 1].Location = Locations[i];
                LocProcess = Locations[i];
            }

            // ObjectCode                      
            ObjectCodeProcess(Lines, OpCode);
            foreach (var item in Lines)
            {
                Console.WriteLine(item.Location + "\t" + item.Part1 + "\t" + item.Part2 + "\t" + item.Part3.Trim() + "\t" + item.ObjectCode);
            }

            // Create File            
            Create_WriteFile(Lines, ObjectProgramProcess(Lines));
        }
    }
}
