using System.Globalization;

namespace Astrac.Assemblers
{
    internal class ASMv100 : ASMvBase
    {
        string _isa;
        string _cwd;
        FileStream _isaStream;
        StreamReader _isaReader;
        List<string> _isaLines;
        IsaSettings? _Settings;

        internal ASMv100(string isaFilePath, string cwd)
        {
            _isa = isaFilePath;
            _cwd = cwd;
            _isaStream = File.OpenRead(isaFilePath);
            _isaReader = new StreamReader(_isaStream);
            _isaLines = new List<string>();
        }

        override internal void run()
        {
            Utils.ResetConColors();
            if (AsmSettings())
            {
                Console.WriteLine("The ISA parser returned with errors, which will have printed above. Correct these issues and try again. Assembler cannot run!");
                Utils.EndProgram();
                return;
            }

            if (_Settings == null)
            {
                Utils.FatalEndProgram("Assembler came back with missing settings! This is not a user error, but the Assembler cannot run.");
                return;
            }

            Console.WriteLine("Looking for .asm files (note: we do not parse .txt files for various reasons, but .asm files should be just plain text, nothing special)...");

            string[] Files = Directory.GetFiles(_cwd, "*.asm", SearchOption.TopDirectoryOnly);
            int Successful = Files.Length;

            if (Files.Length > 0)
            {
                string FilesFound = Files.Length > 1 ? "Files" : "File";
                Console.WriteLine($"{Files.Length} {FilesFound} found... begin assembly...");

                int i = 0;
                foreach (string file in Files)
                {
                    List<ulong> bytecode = new List<ulong>();

                    bool Error = false;
                    i++;
                    Console.WriteLine($"Assembling file {i} of {Files.Length}: {Path.GetFileName(file)}");

                    if (File.Exists(file))
                    {
                        FileStream fs = File.OpenRead(Path.GetFullPath(file));
                        StreamReader sr = new StreamReader(fs);
                        List<string> fl = new List<string>();

                        Console.WriteLine("Reading file...");
                        bool MLC = false;
                        while (true)
                        {
                            string? line = sr.ReadLine();
                            if (line == null) break;
                            
                            if (MLC)
                            {
                                if (line.EndsWith("*;"))
                                {
                                    MLC = false;
                                }
                                continue;
                            } else
                            {
                                string[] SplitLine = line.Split(";*");
                                if (SplitLine.Length > 1)
                                {
                                    MLC = true;
                                }

                                if (SplitLine[0].StartsWith(";") || string.IsNullOrWhiteSpace(line)) continue;
                                fl.Add(SplitLine[0].Split(';')[0].Trim());
                            }
                        }
                        Console.WriteLine("File read, closing and assembling.");
                        sr.Close();
                        fs.Close();

                        int j = 0;
                        foreach (string l in fl)
                        {
                            j++;
                            string[] SplitL = l.Split(' ');

                            if (!_Settings.Instructions.ContainsKey(SplitL[0]))
                            {
                                Utils.WriteError($"Line {j}: Cannot resolve instruction '{SplitL[0]}'");
                                Error = true;
                            } else
                            {
                                Instruction inst = _Settings.Instructions[SplitL[0]];
                                Dictionary<string, string> Params = new Dictionary<string, string>();
                                Dictionary<string, ulong> ParsedParams = new Dictionary<string, ulong>();

                                for (int k = 1; k <= inst.Parameters.Count; k++)
                                {
                                    Params[inst.Parameters[k-1]] = SplitL[k].Trim();
                                }

                                foreach (var k in Params)
                                {
                                    if (_Settings.Registers.ContainsKey(k.Value))
                                    {
                                        ParsedParams[k.Key] = _Settings.Registers[k.Value];
                                    } else
                                    {
                                        ulong p = 0;
                                        if (ParseNumber(k.Value, out p))
                                        {
                                            ParsedParams[k.Key] = p;
                                        } else
                                        {
                                            Utils.WriteError($"Line {j}: Cannot resolve symbol '{k.Value}'");
                                            Error = true;
                                        }
                                    }
                                }

                                foreach (string b in inst.BitField)
                                {
                                    string Final = b;
                                    foreach (string p in ParsedParams.Keys)
                                    {
                                        Final = ReplaceParam(Final, p, ParsedParams[p]);
                                    }
                                    Error = !ulong.TryParse(Final, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong result);
                                    bytecode.Add(result);
                                }
                            }
                        }
                    }

                    if (!Error)
                    {
                        using (BinaryWriter bw = new BinaryWriter(File.Open(Path.GetFileNameWithoutExtension(file) + _Settings.OutType, FileMode.Create)))
                        {
                            int w = _Settings.InstWidth / 2;
                            foreach (ulong inst in bytecode)
                            {
                                byte[] bytes = BitConverter.GetBytes(inst);
                                for (int j = w - 1; j >= 0; j--)
                                {
                                    bw.Write(bytes[j]);
                                }
                            }
                        }

                        Console.WriteLine($"Assembly of file: {file} finished! Outputs written to {Path.GetFileNameWithoutExtension(file) + _Settings.OutType}.");
                    } else
                    {
                        Console.Beep();
                        Console.WriteLine($"Assembly of file: {file} returned with errors. Final result will not be written.");
                    }
                }
                Utils.EndProgram();
            } else
            {
                Utils.FatalEndProgram("Assembler could not find any .asm files! Assembler will not run.");
            }
        }

        internal bool AsmSettings()
        {
            _Settings = new IsaSettings();

            Utils.ResetConColors();
            Console.WriteLine("Reading ISA settings...");

            while (true)
            {
                string? line = _isaReader.ReadLine();
                if (line == null) break;
                _isaLines.Add(line);
            }
            Console.WriteLine("ISA lines read, closing file...");
            _isaReader.Close();
            _isaStream.Close();

            // - These variables below need to persist between each iteration of the foreach loop.
            Instruction Inst = new Instruction();
            bool AddBitfield = false;

            bool NameTag = false;
            bool DescTag = false;
            bool Reg_Tag = false;
            bool InstTag = false;

            bool NT_present = false;
            bool DT_present = false;
            bool RT_present = false;
            bool OT_present = false;

            bool Error = false;

            int i = 0;
            foreach (string l in _isaLines)
            {
                i++;
                // - If we are on the first or second lines, then fill in the assembler version and what file type we want the final machine code to be in.
                if (i == 1)
                {
                    if (l.StartsWith("$ASTRAC_V"))
                    {
                        _Settings.Version = l.Split('=')[1].Trim();
                        continue;
                    }
                } else if (i == 2)
                {
                    if (l.StartsWith("$ASTRAC_OUT_TYPE"))
                    {
                        _Settings.OutType = l.Split('=')[1].Trim();
                        OT_present = true;

                        if (string.IsNullOrWhiteSpace(_Settings.OutType))
                        {
                            Utils.WriteError("No output file type is defined! Ensure the second line is '$ASTRAC_OUT_TYPE =' followed by a file type!");
                            Error = true;
                        }
                        if (!_Settings.OutType.StartsWith("."))
                        {
                            Utils.WriteError("Defined output type is invalid! Ensure file type starts with a '.'");
                            Error = true;
                        }
                        continue;
                    }
                }

                // - If the line starts with a comment, is empty, or has only whitespace, skip the line.
                if (l.StartsWith(";") || string.IsNullOrWhiteSpace(l))
                    continue;

                // - Check for the various tags.
                if (_Settings.Name == "")
                {
                    if (l.StartsWith("[Name]"))
                    {
                        NameTag = true;
                        NT_present = true;
                        continue;
                    }
                }

                if (_Settings.Description == "")
                {
                    if (l.StartsWith("[Description]"))
                    {
                        DescTag = true;
                        DT_present = true;
                        continue;
                    }
                }

                if (!RT_present)
                {
                    if (l.StartsWith("[Registers]"))
                    {
                        Reg_Tag = true;
                        RT_present = true;
                        continue;
                    }
                }

                if (!InstTag && !RT_present)
                {
                    if (l.StartsWith("[Instructions]"))
                    {
                        Utils.WriteError("Invalid ISA: [Instructions] should only appear after [Registers]");
                        Error = true;
                        continue;
                    }
                }

                if (NameTag)
                {
                    if (string.IsNullOrWhiteSpace(l))
                    {
                        NT_present = false;
                        NameTag = false;
                        continue;
                    }
                    _Settings.Name = l.Split(";")[0].Trim();
                    NameTag = false;
                }

                if (DescTag)
                {
                    if (string.IsNullOrWhiteSpace(l))
                    {
                        DT_present = false;
                        DescTag = false;
                        continue;
                    }
                    _Settings.Description = l.Split(";")[0].Trim();
                    DescTag = false;
                }

                if (Reg_Tag)
                {
                    if (l.StartsWith("[Instructions]"))
                    {
                        Reg_Tag = false;
                        InstTag = true;
                    }

                    string[] parts = l.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;
                    if (parts[0].StartsWith(";") || parts[1].StartsWith(";")) continue;

                    string regID = parts[0];
                    byte   regVV = byte.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);
                    _Settings.Registers[regID] = regVV;
                }

                if (InstTag)
                {
                    if (string.IsNullOrWhiteSpace(l)) continue;
                    string[] parts = l.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts[0].StartsWith(";")) continue;

                    if (!AddBitfield)
                    {
                        AddBitfield = true;
                        Inst = new Instruction();
                        Inst.Name = parts[0];

                        for (int j = 1; j < parts.Length; j++)
                        {
                            if (parts[j].StartsWith(";")) continue;
                            if (parts[j].StartsWith("%")) Inst.Parameters.Add(parts[j]);
                        }
                    } else
                    {
                        Inst.BitField = l.Split(' ').ToList();
                        _Settings.Instructions[Inst.Name] = Inst;
                        AddBitfield = false;
                    }
                }
            }

            if (!(NT_present && DT_present && RT_present && OT_present && InstTag) || Error)
            {
                if (!(NT_present && DT_present && RT_present && OT_present && InstTag))
                {
                    Utils.WriteError("Some tags are missing or in the wrong order! Ensure the following are present, in this order:\n  1. $ASTRAC_OUT_TYPE = [enter file type here]\n  2. [Name]\n  3. [Description]\n  4. [Registers]\n  5. [Instructions]");
                }
                return true;
            }
            _Settings.SetWidth();
            return false;
        }

        internal static bool ParseNumber(string input, out ulong number)
        {
            string CleanInput = input;
            bool hex = false; bool binary = false;

            if (input.StartsWith("$") || input.StartsWith("0x"))
            {
                hex = true;
                CleanInput = input.TrimStart('$').TrimStart(new char[] {'0','x'});
            } else if (input.StartsWith("%") || input.StartsWith("0b")) {
                binary = true;
                CleanInput = input.TrimStart('%').TrimStart(new char[] {'0','b'});
            }

            if (hex)
            {
                return ulong.TryParse(CleanInput, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out number);
            } else if (binary)
            {
                return ulong.TryParse(CleanInput, NumberStyles.BinaryNumber, CultureInfo.InvariantCulture, out number);
            } else
            {
                return ulong.TryParse(CleanInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
            }
        }
        internal static string ReplaceParam(string bitfield, string param, ulong value)
        {
            char p = param.TrimStart('%')[0];
            int Nibbles = bitfield.Count(c => c == p);
            if (Nibbles == 0) return bitfield;

            ulong mask = (1UL << (Nibbles * 4)) - 1;
            ulong MaskedValue = value & mask;

            string hex = MaskedValue.ToString($"X{Nibbles}");

            return bitfield.Replace(new string(p, Nibbles), hex);
        }
    }
}
