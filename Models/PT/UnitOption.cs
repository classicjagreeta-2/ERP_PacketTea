namespace PacketTea.Models.PT
{
    // One entry of the current user's User.UnitList (see JwtMiddleware.cs) for the
    // PacketTea module -- backs the "New" inline row's Unit picker on the Master
    // Blend Entry list page (ClassicERPCoreAPI's TeaBlendController.GetUnitsForUser).
    public class UnitOption
    {
        public string CODE { get; set; }
        public string NAME { get; set; }
        public string STATE { get; set; }   // GST state code (Sales UNIT master); only some screens fill it
    }
}
