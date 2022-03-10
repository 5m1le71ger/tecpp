using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace TecPlusPlus
{
    class AnsiColor
    {
        public AnsiColor()
        {
            Ground = EnumGround.Forground;
            ColorValue = Colors.Black;
        }
        public AnsiColor(EnumGround g,Color c)
        {
            Ground = g;
            ColorValue = c;
        }
        public enum EnumGround
        {
            Forground,
            Background,
        };
        public EnumGround Ground { get; set; }
        public Color ColorValue;

        static readonly Dictionary<string, AnsiColor> table = new Dictionary<string, AnsiColor>()
        {
            { "[30m", new AnsiColor(EnumGround.Forground, Colors.Black) },
            { "[31m", new AnsiColor(EnumGround.Forground, Colors.DarkRed) },
            { "[32m", new AnsiColor(EnumGround.Forground, Colors.DarkGreen) },
            { "[33m", new AnsiColor(EnumGround.Forground, Colors.Gold) },
            { "[34m", new AnsiColor(EnumGround.Forground, Colors.DarkBlue) },
            { "[35m", new AnsiColor(EnumGround.Forground, Colors.DarkMagenta) },
            { "[36m", new AnsiColor(EnumGround.Forground, Colors.DarkCyan) },
            { "[37m", new AnsiColor(EnumGround.Forground, Colors.GhostWhite) },
            { "[1;31m", new AnsiColor(EnumGround.Forground, Colors.Red) },
            { "[1;32m", new AnsiColor(EnumGround.Forground, Colors.Green) },
            { "[1;33m", new AnsiColor(EnumGround.Forground, Colors.Yellow) },
            { "[1;34m", new AnsiColor(EnumGround.Forground, Colors.Blue) },
            { "[1;35m", new AnsiColor(EnumGround.Forground, Colors.Magenta) },
            { "[1;36m", new AnsiColor(EnumGround.Forground, Colors.Cyan) },
            { "[1;37m", new AnsiColor(EnumGround.Forground, Colors.White) },
            { "[41;1m", new AnsiColor(EnumGround.Background, Colors.White) },
            { "[42;1m", new AnsiColor(EnumGround.Background, Colors.Green) },
            { "[43;1m", new AnsiColor(EnumGround.Background, Colors.Yellow) },
            { "[44;1m", new AnsiColor(EnumGround.Background, Colors.Blue) },
            { "[45;1m", new AnsiColor(EnumGround.Background, Colors.Magenta) },
            { "[46;1m", new AnsiColor(EnumGround.Background, Colors.Cyan) },
            { "[47;1m", new AnsiColor(EnumGround.Background, Colors.White) },
            { "[40m", new AnsiColor(EnumGround.Background, Colors.Black) },
            { "[41m", new AnsiColor(EnumGround.Background, Colors.DarkRed) },
            { "[42m", new AnsiColor(EnumGround.Background, Colors.DarkGreen) },
            { "[43m", new AnsiColor(EnumGround.Background, Colors.Gold) },
            { "[44m", new AnsiColor(EnumGround.Background, Colors.DarkBlue) },
            { "[45m", new AnsiColor(EnumGround.Background, Colors.DarkMagenta) },
            { "[46m", new AnsiColor(EnumGround.Background, Colors.DarkCyan) },
            { "[47m", new AnsiColor(EnumGround.Background, Colors.GhostWhite) },
            { "[2;37;0m", new AnsiColor(EnumGround.Forground, Colors.White) },
            { "[256D", new AnsiColor(EnumGround.Forground, Colors.White) },
            { "[K", new AnsiColor(EnumGround.Forground, Colors.White) },

        };

        public static int Parse(string text, ref AnsiColor c)
        {
            int rtn = 0;
            foreach (KeyValuePair<string, AnsiColor> kv in table)
            {
                if(0 == text.IndexOf(kv.Key))
                {
                    c = kv.Value;
                    rtn = kv.Key.Length;
                    break;
                }
            }
            return rtn;
        }

    }
}
