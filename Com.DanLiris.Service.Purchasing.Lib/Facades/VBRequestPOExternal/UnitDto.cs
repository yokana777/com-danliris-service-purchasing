namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public class UnitDto
    {
        public UnitDto()
        {
        }

        public UnitDto(string unitId, string unitCode, string unitName)
        {
        }

        public int Id { get; private set; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public DivisionDto Division { get; private set; }
    }
}