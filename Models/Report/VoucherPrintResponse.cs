using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.Report
{
    public class VoucherPrintResponse
    {
        public VoucherPrintValue value { get; set; }
    }

    public class VoucherPrintValue
    {
        public List<VOU_PRINT_V2> results { get; set; }
        public string vfooter { get; set; }
        public string pdto { get; set; }
        public string totalAmountWords { get; set; }
    }
}