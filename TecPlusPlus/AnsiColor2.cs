using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace TecPlusPlus
{
    class AnsiColor2
    {
        public Color Foreground { get; set; }
        public Color Background { get; set; }

        private static readonly Hashtable ColorTable = new Hashtable()
        {
            { "30", Colors.Black },
            { "31", Colors.Red },
            { "32", Colors.Green },
            { "33", Colors.Yellow },
            { "34", Colors.Blue },
            { "35", Colors.Magenta },
            { "36", Colors.Cyan },
            { "37", Colors.White },
            { "40", Colors.Black },
            { "41", Colors.Red },
            { "42", Colors.Green },
            { "43", Colors.Yellow },
            { "44", Colors.Blue },
            { "45", Colors.Magenta },
            { "46", Colors.Cyan },
            { "47", Colors.White }
        };
        public static int Parse(string text, ref AnsiColor c)
        {
            if (text.IndexOf('[') != 0)
            {
                return 0;
            }
            int pos2 = text.IndexOf('m');
            if(pos2 == -1)
            {
                return 0;
            }
            string str = text.Substring(1, pos2 - 1);
            string[] ss = str.Split(';');
            foreach (string s in ss)
            {
                if (ColorTable.Contains(s))
                {
                    if(int.Parse(s) >=30 && int.Parse(s) <= 37 )
                    Color c = (Color)ColorTable[s];
                }
            }

        }

    }
}
