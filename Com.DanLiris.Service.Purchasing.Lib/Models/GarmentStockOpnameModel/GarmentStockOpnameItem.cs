using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.Moonlay.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentStockOpnameModel
{
    public class GarmentStockOpnameItem : StandardEntity<int>
    {
        public int GarmentStockOpnameId { get; set; }
        [ForeignKey("GarmentStockOpnameId")]
        public virtual GarmentStockOpname GarmentStockOpname { get; set; }

        public long DOItemId { get; set; }
        [ForeignKey("DOItemId")]
        public virtual GarmentDOItems DOItem { get; set; }

        public decimal BeforeQuantity { get; set; }
        public decimal Quantity { get; set; }
    }
}
