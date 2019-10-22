using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNoteViewModel
{
    public class UnitNoteSpbViewModel : BaseViewModel
    {
        public string UrnNo { get; set; }
        public DateTimeOffset ReceiptDate {get; set;}
        public string SupplierCode{get; set;}
        public string SupplierName{get; set;}
        public string DONo{get; set;}
        public DateTimeOffset DODate {get; set;}
        public string EPONo{get; set;}
        public DateTimeOffset OrderDate{get; set;}
        public string PaymentDueDays{get; set;}
        public string PRNo{get; set;}
        public string BudgetName { get; set; }
        public string UnitName{get; set;}
        public string CategoryName{get; set;}
        public string ProductCode{get; set;}
        public string ProductName{get; set;}
        public double ReceiptQuantity{get; set;}
        public string Uom{get; set;}
        public string createdBy {get; set;}
        public string IsPaid{get; set;}
        public string UPONo{get; set;}


}
}
