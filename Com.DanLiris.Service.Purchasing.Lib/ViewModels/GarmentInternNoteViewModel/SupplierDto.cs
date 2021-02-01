namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel
{
    public class SupplierDto
    {
        public SupplierDto(long? supplierId, string supplierName)
        {
            Id = (int)supplierId.GetValueOrDefault();
            Name = supplierName;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsImport { get; set; }
    }
}