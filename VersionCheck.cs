using Astrac.Assemblers;

namespace Astrac
{
    internal static class VersionCheck
    {
        internal static string GetAssemblerVersionString(string isaFilePath)
        {
            using (FileStream fs = File.OpenRead(isaFilePath))
            using (StreamReader reader = new StreamReader(fs))
            {
                string? firstLine = reader.ReadLine();
                if (firstLine != null && firstLine.StartsWith("$ASTRAC_V"))
                {
                    return firstLine.Split('=')[1].Trim();
                }
                throw new Exception("Invalid .isa file: No $ASTRAC_V found on first line.");
            }
        }

        internal static int[] GetVersion(string isaFilePath)
        {
            string ver = GetAssemblerVersionString(isaFilePath);
            int[] Mmp = new int[3];

            var split = ver.Split('.');

            byte i = 0;
            foreach (var s in split)
            {
                if (i <= 2)
                    Mmp[i] = int.Parse(s);
                else break;
                i++;
            }

            return Mmp;
        }

        internal static int[] GetVersionFromString(string ver)
        {
            int[] Mmp = new int[3];

            var split = ver.Split('.');

            byte i = 0;
            foreach (var s in split)
            {
                if (i <= 2)
                    Mmp[i] = int.Parse(s);
                else break;
                i++;
            }

            return Mmp;
        }

        internal static ASMvBase InitAsmByVersion(string ver, string isaFilePath, string cwd)
        {
            int[] Mmp = GetVersionFromString(ver);

            if (ver == Program.vVER)
            {
                return new ASMv100(isaFilePath, cwd);
            }

            if (Mmp[0] > Program.vMAJOR)
                throw new Exception($".isa file requires version v{ver}, but Assembler Max Version is v{Program.vVER}");
            else if (Mmp[0] == Program.vMAJOR)
            {
                if (Mmp[1] > Program.vMINOR || (Mmp[1] == Program.vMINOR && Mmp[2] > Program.vPATCH)) throw new Exception($".isa file requires version v{ver}, but Assembler Max Version is v{Program.vVER}");
            }
            throw new Exception("Invalid .isa file: Version string is invalid!");
        }
    }
}
