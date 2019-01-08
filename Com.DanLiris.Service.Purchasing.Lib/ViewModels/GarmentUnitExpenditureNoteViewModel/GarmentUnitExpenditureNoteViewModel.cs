using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitExpenditureNoteViewModel
{
    public class GarmentUnitExpenditureNoteViewModel : BaseViewModel
    {
        public string UENNo { get; set; }
        public DateTimeOffset ExpenditureDate { get; set; }
        public string ExpenditureType { get; set; }
        public string ExpenditureTo { get; set; }
        public long UnitDOId { get; set; }
        public string UnitDONo { get; set; }


        public UnitViewModel UnitRequest { get; set; }

        public UnitViewModel UnitSender { get; set; }
        public IntegrationViewModel.StorageViewModel Storage { get; set; }
        public IntegrationViewModel.StorageViewModel StorageRequest { get; set; }

        public List<GarmentUnitExpenditureNoteItemViewModel> Items { get; set; }
    }
}
