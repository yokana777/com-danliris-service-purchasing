using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingBookReport
{
    public class ReportDto
    {
        public ReportDto(List<ReportIndexDto> data, List<ReportCategoryDto> categories, List<ReportCurrencyDto> currencies)
        {
            Data = data;
            Categories = categories;
            Currencies = currencies;
        }

        public List<ReportIndexDto> Data { get; private set; }
        public List<ReportCategoryDto> Categories { get; private set; }
        public List<ReportCurrencyDto> Currencies { get; private set; }
    }
}