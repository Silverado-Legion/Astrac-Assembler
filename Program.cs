using Astrac.Assemblers;

namespace Astrac
{
    internal class Program
    {
        internal static byte vMAJOR = 1;
        internal static byte vMINOR = 0;
        internal static byte vPATCH = 0;

        internal static string vVER = $"{vMAJOR}.{vMINOR}.{vPATCH}";
        internal static string vGREET = $" Astrac Assembler:  v{vVER} ";

        internal static string CWD = Directory.GetCurrentDirectory();
        internal static string ISA = "";

        static void Main(string[] args)
        {
            string Divider = "";
            for (int i = 0; i < vGREET.Length; i++)
            {
                if (i % 2 == 1)
                    Divider += "=";
                else 
                    Divider += "-";
            }

            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"|{Divider}|\n|{vGREET}|\n|{Divider}|\n");

            HandleParams(args);
            FindISA();
        }

        static void RunAssembler(string cwd, string isa)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"Selected ISA file: {Path.GetFileName(isa)}");

            string ver = VersionCheck.GetAssemblerVersionString(isa);
            Console.WriteLine($"ISA version: v{ver}, init Assembler");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            var ASM = VersionCheck.InitAsmByVersion(ver, isa, cwd);
            ASM.run();
        }

        static void HandleParams(string[] args)
        {
            int NewCWD = Array.IndexOf(args, "--Dir") | Array.IndexOf(args, "-d");
            if (NewCWD != -1)
            {
                if (!string.IsNullOrWhiteSpace(args[NewCWD + 1]))
                {
                    if (Directory.Exists(args[NewCWD + 1]))
                    {
                        Environment.CurrentDirectory = args[NewCWD + 1];
                        CWD = args[NewCWD + 1];
                    } else
                    {
                        Utils.WriteError($"Directory in parameter: '{args[NewCWD]}' was invalid... resorting to current directory...");
                    }
                } else
                {
                    Utils.WriteError($"Parameter: '{args[NewCWD]}' was left blank... resorting to current directory...");
                }
            }

            int NewISA = Array.IndexOf(args, "--Isa") | Array.IndexOf(args, "-i");
            if (NewISA != -1)
            {
                if (!string.IsNullOrWhiteSpace(args[NewISA + 1]))
                {
                    if (Path.Exists(args[NewISA + 1]))
                    {
                        ISA = args[NewISA + 1];
                    }
                    else
                    {
                        Utils.WriteError($"Directory in parameter: '{args[NewISA]}' was invalid... resorting to current directory...");
                    }
                } else
                {
                    Utils.WriteError($"Parameter: '{args[NewISA]}' was left blank... resorting to current directory...");
                }
            }
        }

        static void FindISA_InDir(string path)
        {
            string[] ISA_Files = Directory.GetFiles(path, "*.isa", SearchOption.TopDirectoryOnly);
            if (ISA_Files.Length > 1)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Astrac does not currently support loading more than one ISA file! Only the first found will be used.");
            }
            else if (ISA_Files.Length <= 0)
            {
                Utils.FatalEndProgram("Astrac could not find any ISA files!");
            }
            else
            {
                RunAssembler(CWD, ISA_Files[0]);
            }
        }

        static void FindISA()
        {
            if (string.IsNullOrWhiteSpace(ISA))
            {
                FindISA_InDir(CWD);
            } else if (File.Exists(Path.GetFullPath(ISA)))
            {
                RunAssembler(CWD, ISA);
            } else if (Directory.Exists(Path.GetFullPath(ISA)))
            {
                FindISA_InDir(ISA);
            } else
            {
                Utils.FatalEndProgram("Astrac was given an invalid ISA location! This is not a user error, but the Assembler won't run.");
            }
        }
    }
}
