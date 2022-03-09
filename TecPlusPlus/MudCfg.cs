using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TecPlusPlus
{
    public class MudCfg
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Encode { get; set; }
        public enum EnumColorRule
        {
            CR_EC,
            CR_ANSI
        };
        public EnumColorRule ColorRule {get;set;}
    }
}
