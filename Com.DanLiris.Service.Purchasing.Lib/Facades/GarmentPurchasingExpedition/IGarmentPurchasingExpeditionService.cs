using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingExpedition
{
    public interface IGarmentPurchasingExpeditionService
    {
        List<GarmentInternalNoteDto> GetGarmentInternalNotes(string keyword, GarmentInternalNoteFilterDto filter);
        int UpdateInternNotePosition(UpdatePositionFormDto form);
    }
}
