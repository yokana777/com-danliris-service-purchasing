using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using System;
using System.Collections.Generic;
using System.Text; 


namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentInvoice 
    {
        Tuple<List<GarmentInvoice>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentInvoice ReadById(int id);
    }
}
