namespace PacketTea.Models.PT
{
    // Save payload posted to /api/ProductionEntry/SaveOrUpdate. Unlike the
    // head+details or flat-list shapes used elsewhere, this wraps a single
    // T_PROD -- Production Entry has no line grid, matching the VB6 form.
    public class T_PROD_DATA
    {
        public T_PROD T_PROD { get; set; } = new T_PROD();

        // Set by the client from the entry page's #isEdit flag (IsNew: !IS_EDIT) -- lets Save
        // apply the AEDV Add-vs-Edit + back-date check. Only read by the MVC Save action; the
        // API is sent T_PROD alone.
        public bool? IsNew { get; set; }
    }
}
