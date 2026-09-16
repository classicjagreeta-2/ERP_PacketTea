using Finance.Models.PT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.PT
{
    public class PT_ALL_LIST
    {
        public T_TEA_PURCHASE T_TEA_HEAD { get; set; }
        public List<T_TEA_PURCHASE> T_TEA_DETAIL { get; set; }
    }
}