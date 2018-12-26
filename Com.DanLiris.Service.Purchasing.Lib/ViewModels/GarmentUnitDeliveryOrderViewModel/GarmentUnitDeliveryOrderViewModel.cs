using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitDeliveryOrderViewModel
{
    public class GarmentUnitDeliveryOrderViewModel : BaseViewModel//, IValidatableObject
    {
        public string UnitDOType { get; set; }
        public DateTimeOffset UnitDODate { get; set; }
        public string UnitDONo { get; set; }

        public UnitViewModel UnitRequest { get; set; }

        public UnitViewModel UnitSender { get; set; }


        public IntegrationViewModel.StorageViewModel Storage { get; set; }
        public string RONo { get; set; }
        public string Article { get; set; }
        public List<GarmentUnitDeliveryOrderItemViewModel> Items { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
