namespace PacketTea.Models.PT
{
    // Save payload posted to /api/ProductionEntry/SaveOrUpdate. Unlike the
    // head+details or flat-list shapes used elsewhere, this wraps a single
    // T_PROD -- Production Entry has no line grid, matching the VB6 form.
    public class T_PROD_DATA
    {
        public T_PROD T_PROD { get; set; } = new T_PROD();
    }
}
