using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentCorrectionNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentCorrectionNoteDataUtils
{
    public class GarmentCorrectionNoteDataUtil
    {
        private readonly GarmentCorrectionNotePriceFacade garmentCorrectionNoteFacade;
        private readonly GarmentDeliveryOrderDataUtil garmentDeliveryOrderDataUtil;

        public GarmentCorrectionNoteDataUtil(GarmentCorrectionNotePriceFacade garmentCorrectionNoteFacade, GarmentDeliveryOrderDataUtil garmentDeliveryOrderDataUtil)
        {
            this.garmentCorrectionNoteFacade = garmentCorrectionNoteFacade;
            this.garmentDeliveryOrderDataUtil = garmentDeliveryOrderDataUtil;
        }

        public (GarmentCorrectionNote GarmentCorrectionNote, GarmentDeliveryOrder GarmentDeliveryOrder) GetNewData()
        {
            var garmentDeliveryOrder = Task.Run(() => garmentDeliveryOrderDataUtil.GetTestData()).Result;

            GarmentCorrectionNote garmentCorrectionNote = new GarmentCorrectionNote
            {
                CorrectionDate = DateTimeOffset.Now,
                DOId = garmentDeliveryOrder.Id,
                DONo = garmentDeliveryOrder.DONo,
                SupplierId = garmentDeliveryOrder.SupplierId,
                SupplierCode = garmentDeliveryOrder.SupplierCode,
                SupplierName = garmentDeliveryOrder.SupplierName,
                Remark = "Remark",
                Items = new List<GarmentCorrectionNoteItem>()
            };

            foreach (var item in garmentDeliveryOrder.Items)
            {
                foreach (var detail in item.Details)
                {
                    garmentCorrectionNote.Items.Add(
                        new GarmentCorrectionNoteItem
                        {
                            DODetailId = detail.Id,
                            EPOId = item.EPOId,
                            EPONo = item.EPONo,
                            PRId = detail.PRId,
                            PRNo = detail.PRNo,
                            POId = detail.POId,
                            POSerialNumber = detail.POSerialNumber,
                            RONo = detail.RONo,
                            ProductId = detail.ProductId,
                            ProductCode = detail.ProductCode,
                            ProductName = detail.ProductName,
                            Quantity = (decimal)detail.QuantityCorrection,
                            UomId = Convert.ToInt32(detail.UomId),
                            UomIUnit = detail.UomUnit,
                        });
                }
            }

            return (garmentCorrectionNote, garmentDeliveryOrder);
        }

        public GarmentCorrectionNote GetNewDataKoreksiHargaSatuan()
        {
            var data = GetNewData();

            data.GarmentCorrectionNote.CorrectionType = "Harga Satuan";

            foreach (var item in data.GarmentDeliveryOrder.Items)
            {
                foreach (var detail in item.Details)
                {
                    var garmentCorrectionNoteItem = data.GarmentCorrectionNote.Items.First(i => i.DODetailId == detail.Id);
                    garmentCorrectionNoteItem.PricePerDealUnitBefore = (decimal)detail.PricePerDealUnitCorrection;
                    garmentCorrectionNoteItem.PricePerDealUnitAfter = (decimal)detail.PricePerDealUnitCorrection + 1;
                    garmentCorrectionNoteItem.PriceTotalBefore = (decimal)detail.PriceTotalCorrection;
                    garmentCorrectionNoteItem.PriceTotalAfter = (decimal)detail.QuantityCorrection * garmentCorrectionNoteItem.PricePerDealUnitAfter;
                }
            }

            return data.GarmentCorrectionNote;
        }

        public GarmentCorrectionNote GetNewDataKoreksiHargaTotal()
        {
            var data = GetNewData();

            data.GarmentCorrectionNote.CorrectionType = "Harga Total";

            foreach (var item in data.GarmentDeliveryOrder.Items)
            {
                foreach (var detail in item.Details)
                {
                    var garmentCorrectionNoteItem = data.GarmentCorrectionNote.Items.First(i => i.DODetailId == detail.Id);
                    garmentCorrectionNoteItem.PricePerDealUnitBefore = (decimal)detail.PricePerDealUnitCorrection;
                    garmentCorrectionNoteItem.PricePerDealUnitAfter = (decimal)detail.PricePerDealUnitCorrection;
                    garmentCorrectionNoteItem.PriceTotalBefore = (decimal)detail.PriceTotalCorrection;
                    garmentCorrectionNoteItem.PriceTotalAfter = (decimal)detail.PriceTotalCorrection + 1;
                }
            }

            return data.GarmentCorrectionNote;
        }

        public async Task<GarmentCorrectionNote> GetTestDataKoreksiHargaSatuan(string user)
        {
            var data = GetNewDataKoreksiHargaSatuan();
            await garmentCorrectionNoteFacade.Create(data, user);
            return data;
        }

        public async Task<GarmentCorrectionNote> GetTestDataKoreksiHargaTotal(string user)
        {
            var data = GetNewDataKoreksiHargaTotal();
            await garmentCorrectionNoteFacade.Create(data, user);
            return data;
        }
    }
}
