namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public class UOMDto
    {
        public UOMDto(int uomId, string uomUnit)
        {
            Id = uomId;
            Unit = uomUnit;
        }

        public UOMDto(string dealUomId, string dealUomUnit)
        {
        }

        public int Id { get; private set; }
        public string Unit { get; private set; }
    }
}