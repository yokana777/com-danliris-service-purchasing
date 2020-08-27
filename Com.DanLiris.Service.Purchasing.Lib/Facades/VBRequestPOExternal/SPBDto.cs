using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using iTextSharp.text;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public class SPBDto
    {
        public SPBDto(GarmentInternNote element)
        {
            Id = element.Id;
            No = element.INNo;
            Date = element.INDate;

            Items = element.Items.Select(item => new SPBDtoItem(element, item)).ToList();
        }

        public SPBDto(UnitPaymentOrder element)
        {
            Id = element.Id;
            No = element.UPONo;
            Date = element.Date;

            Items = element.Items.Select(item => new SPBDtoItem(element, item)).ToList();
        }

        public long Id { get; private set; }
        public string No { get; private set; }
        public DateTimeOffset Date { get; private set; }
        public List<SPBDtoItem> Items { get; private set; }
    }
}