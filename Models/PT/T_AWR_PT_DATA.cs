using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Save payload posted to /api/TeaAWR/SaveOrUpdate. T_AWR is denormalized
    // (see T_AWR.cs), so this is a flat line list -- no separate header object.
    public class T_AWR_PT_DATA
    {
        public List<T_AWR> T_AWR { get; set; } = new List<T_AWR>();

        // Set by the client from the entry page's #isEdit flag (IsNew: !IS_EDIT) -- lets the
        // controller's AEDV back-date check (Aday vs Eday) tell an Add from an Edit. Only read
        // by this front end's Save action; harmless if it rides along in the JSON forwarded
        // to the API (the API DTO ignores unknown members).
        public bool? IsNew { get; set; }
    }
}
