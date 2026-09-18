using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Save payload posted to /api/TeaAWR/SaveOrUpdate. T_AWR is denormalized
    // (see T_AWR.cs), so this is a flat line list -- no separate header object.
    public class T_AWR_PT_DATA
    {
        public List<T_AWR> T_AWR { get; set; } = new List<T_AWR>();
    }
}
