using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentPurchaseRequestViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades
{
	public class GarmentPurchaseRequestFacade : IGarmentPurchaseRequestFacade
	{
		private string USER_AGENT = "Facade";

		private readonly PurchasingDbContext dbContext;
		private readonly DbSet<GarmentPurchaseRequest> dbSet;

		public GarmentPurchaseRequestFacade(PurchasingDbContext dbContext)
		{
			this.dbContext = dbContext;
			this.dbSet = dbContext.Set<GarmentPurchaseRequest>();
		}

		public Tuple<List<GarmentPurchaseRequest>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
		{
			IQueryable<GarmentPurchaseRequest> Query = this.dbSet;

			Query = Query.Select(s => new GarmentPurchaseRequest
			{
				Id = s.Id,
				UId = s.UId,
				RONo = s.RONo,
				PRNo = s.PRNo,
				Article = s.Article,
				Date = s.Date,
				ExpectedDeliveryDate = s.ExpectedDeliveryDate,
				ShipmentDate = s.ShipmentDate,
				BuyerId = s.BuyerId,
				BuyerCode = s.BuyerCode,
				BuyerName = s.BuyerName,
				UnitId = s.UnitId,
				UnitCode = s.UnitCode,
				UnitName = s.UnitName,
				IsPosted = s.IsPosted,
				CreatedBy = s.CreatedBy,
				LastModifiedUtc = s.LastModifiedUtc
			});

			List<string> searchAttributes = new List<string>()
			{
				"PRNo", "RONo", "BuyerCode", "BuyerName", "UnitName", "Article"
			};

			Query = QueryHelper<GarmentPurchaseRequest>.ConfigureSearch(Query, searchAttributes, Keyword);

			Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
			Query = QueryHelper<GarmentPurchaseRequest>.ConfigureFilter(Query, FilterDictionary);

			Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
			Query = QueryHelper<GarmentPurchaseRequest>.ConfigureOrder(Query, OrderDictionary);

			Pageable<GarmentPurchaseRequest> pageable = new Pageable<GarmentPurchaseRequest>(Query, Page - 1, Size);
			List<GarmentPurchaseRequest> Data = pageable.Data.ToList<GarmentPurchaseRequest>();
			int TotalData = pageable.TotalCount;

			return Tuple.Create(Data, TotalData, OrderDictionary);
		}

		public GarmentPurchaseRequest ReadById(int id)
		{
			var a = this.dbSet.Where(p => p.Id == id)
				.Include(p => p.Items)
				.FirstOrDefault();
			return a;
		}

		public GarmentPurchaseRequest ReadByRONo(string rono)
		{
			var a = this.dbSet.Where(p => p.RONo.Equals(rono))
				.Include(p => p.Items)
				.FirstOrDefault();
			return a;
		}

		public async Task<int> Create(GarmentPurchaseRequest m, string user, int clientTimeZoneOffset = 7)
		{
			int Created = 0;

			using (var transaction = this.dbContext.Database.BeginTransaction())
			{
				try
				{
					EntityExtension.FlagForCreate(m, user, USER_AGENT);

					m.PRNo = $"PR{m.RONo}";
					m.IsPosted = true;
					m.IsUsed = false;

					foreach (var item in m.Items)
					{
						EntityExtension.FlagForCreate(item, user, USER_AGENT);

						item.Status = "Belum diterima Pembelian";
					}

					this.dbSet.Add(m);

					Created = await dbContext.SaveChangesAsync();
					transaction.Commit();
				}
				catch (Exception e)
				{
					transaction.Rollback();
					throw new Exception(e.Message);
				}
			}

			return Created;
		}

		public async Task<int> Update(int id, GarmentPurchaseRequest m, string user, int clientTimeZoneOffset = 7)
		{
			int Updated = 0;

			using (var transaction = dbContext.Database.BeginTransaction())
			{
				try
				{
					var oldM = this.dbSet.AsNoTracking()
						.Include(d => d.Items)
						.SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

					if (oldM != null && oldM.Id == id)
					{
						EntityExtension.FlagForUpdate(m, user, USER_AGENT);

						foreach (var item in m.Items)
						{
							if (item.Id == 0)
							{
								EntityExtension.FlagForCreate(item, user, USER_AGENT);
								item.Status = "Belum diterima Pembelian";
							}
							else
							{
								EntityExtension.FlagForUpdate(item, user, USER_AGENT);
							}
						}

						dbSet.Update(m);

						foreach (var oldItem in oldM.Items)
						{
							var newItem = oldM.Items.FirstOrDefault(i => i.Id.Equals(oldItem.Id));
							if (newItem == null)
							{
								EntityExtension.FlagForDelete(oldItem, user, USER_AGENT);
								dbContext.GarmentPurchaseRequestItems.Update(oldItem);
							}
						}

						Updated = await dbContext.SaveChangesAsync();
						transaction.Commit();
					}
					else
					{
						throw new Exception("Invalid Id");
					}
				}
				catch (Exception e)
				{
					transaction.Rollback();
					throw new Exception(e.Message);
				}
			}

			return Updated;
		}

		public List<GarmentInternalPurchaseOrder> ReadByTags(string tags, DateTimeOffset shipmentDateFrom, DateTimeOffset shipmentDateTo)
		{
			IQueryable<GarmentPurchaseRequest> Models = this.dbSet.AsQueryable();

			if (shipmentDateFrom != DateTimeOffset.MinValue && shipmentDateTo != DateTimeOffset.MinValue)
			{
				Models = Models.Where(m => m.ShipmentDate >= shipmentDateFrom && m.ShipmentDate <= shipmentDateTo);
			}

			string[] stringKeywords = new string[3];

			if (tags != null)
			{
				List<string> Keywords = new List<string>();

				if (tags.Contains("#"))
				{
					Keywords = tags.Split("#").ToList();
					Keywords.RemoveAt(0);
					Keywords = Keywords.Take(stringKeywords.Length).ToList();
				}
				else
				{
					Keywords.Add(tags);
				}

				for (int n = 0; n < Keywords.Count; n++)
				{
					stringKeywords[n] = Keywords[n].Trim().ToLower();
				}
			}

			Models = Models
				.Where(m =>
					(string.IsNullOrWhiteSpace(stringKeywords[0]) || m.UnitName.ToLower().Contains(stringKeywords[0])) &&
					(string.IsNullOrWhiteSpace(stringKeywords[1]) || m.BuyerName.ToLower().Contains(stringKeywords[1])) &&
					m.Items.Any(i => i.IsUsed == false) &&
					m.IsUsed == false
					)
				.Select(m => new GarmentPurchaseRequest
				{
					Id = m.Id,
					Date = m.Date,
					PRNo = m.PRNo,
					RONo = m.RONo,
					BuyerId = m.BuyerId,
					BuyerCode = m.BuyerCode,
					BuyerName = m.BuyerName,
					Article = m.Article,
					ExpectedDeliveryDate = m.ExpectedDeliveryDate,
					ShipmentDate = m.ShipmentDate,
					UnitId = m.UnitId,
					UnitCode = m.UnitCode,
					UnitName = m.UnitName,
					Items = m.Items
						.Where(i =>
							i.IsUsed == false &&
							(string.IsNullOrWhiteSpace(stringKeywords[2]) || i.CategoryName.ToLower().Contains(stringKeywords[2]))
							)
						.ToList()
				})
				.Where(m => m.Items.Count > 0);

			var IPOModels = new List<GarmentInternalPurchaseOrder>();

			foreach (var model in Models)
			{
				foreach (var item in model.Items)
				{
					var IPOModel = new GarmentInternalPurchaseOrder
					{
						PRId = model.Id,
						PRDate = model.Date,
						PRNo = model.PRNo,
						RONo = model.RONo,
						BuyerId = model.BuyerId,
						BuyerCode = model.BuyerCode,
						BuyerName = model.BuyerName,
						Article = model.Article,
						ExpectedDeliveryDate = model.ExpectedDeliveryDate,
						ShipmentDate = model.ShipmentDate,
						UnitId = model.UnitId,
						UnitCode = model.UnitCode,
						UnitName = model.UnitName,
						//IsPosted = false,
						//IsClosed = false,
						//Remark = "",
						Items = new List<GarmentInternalPurchaseOrderItem>
						{
							new GarmentInternalPurchaseOrderItem
							{
								GPRItemId = item.Id,
								PO_SerialNumber = item.PO_SerialNumber,
								ProductId = item.ProductId,
								ProductCode = item.ProductCode,
								ProductName = item.ProductName,
								Quantity = item.Quantity,
								BudgetPrice = item.BudgetPrice,
								UomId = item.UomId,
								UomUnit = item.UomUnit,
								CategoryId = item.CategoryId,
								CategoryName = item.CategoryName,
								ProductRemark = item.ProductRemark,
                                //Status = "PO Internal belum diorder"
                            }
						}
					};
					IPOModels.Add(IPOModel);
				}
			}

			return IPOModels;
		}
		#region monitoringpurchasealluser
		public List<GarmentPurchaseRequest> ReadName(string Keyword = null, string Filter = "{}")
		{
			IQueryable<GarmentPurchaseRequest> Query = this.dbSet;

			List<string> searchAttributes = new List<string>()
			{
				"CreatedBy",
			};

			Query = QueryHelper<GarmentPurchaseRequest>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dengan .Where setelahnya maka case sensitive, kalo tanpa .Where tidak masalah

			Query = Query
				.Where(m => m.IsPosted == true && m.IsDeleted == false && m.CreatedBy.Contains(Keyword))
				.Select(s => new GarmentPurchaseRequest
				{
					CreatedBy = s.CreatedBy
				}).Distinct();

			Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
			Query = QueryHelper<GarmentPurchaseRequest>.ConfigureFilter(Query, FilterDictionary);

			return Query.ToList();
		}

		public IQueryable<MonitoringPurchaseAllUserViewModel> GetMonitoringPurchaseAllReportQuery(string epono, string unit, string roNo, string article, string poSerialNumber, string username, string doNo, string ipoStatus, string supplier, string status, DateTime? dateFrom, DateTime? dateTo, int offset)
		{


			DateTime d1 = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
			DateTime d2 = dateTo == null ? DateTime.Now : (DateTime)dateTo;
			offset = 7;

			List<MonitoringPurchaseAllUserViewModel> listEPO = new List<MonitoringPurchaseAllUserViewModel>();
			var Query = (from
						  a in dbContext.GarmentPurchaseRequests
						 join b in dbContext.GarmentPurchaseRequestItems on a.Id equals b.GarmentPRId
						 //internalPO
						 join l in dbContext.GarmentInternalPurchaseOrderItems on b.Id equals l.GPRItemId into ll
						 from ipoitem in ll.DefaultIfEmpty()
						 join c in dbContext.GarmentInternalPurchaseOrders on ipoitem.GPOId equals c.Id into d
						 from ipo in d.DefaultIfEmpty()

							 //eksternalpo
						 join e in dbContext.GarmentExternalPurchaseOrderItems on ipo.Id equals e.POId into f
						 from epo in f.DefaultIfEmpty()

						 join g in dbContext.GarmentExternalPurchaseOrders on epo.GarmentEPOId equals g.Id into gg
						 from epos in gg.DefaultIfEmpty()
							 //do
						 join h in dbContext.GarmentDeliveryOrderDetails on epo.Id equals h.EPOItemId into ii
						 from dodetail in ii.DefaultIfEmpty()

						 join j in dbContext.GarmentDeliveryOrderItems on dodetail.GarmentDOItemId equals j.Id into hh
						 from doitem in hh.DefaultIfEmpty()

						 join k in dbContext.GarmentDeliveryOrders on doitem.GarmentDOId equals k.Id into kk
						 from dos in kk.DefaultIfEmpty()


							 //bc
						 join bcs in dbContext.GarmentBeacukaiItems on dos.Id equals bcs.GarmentDOId into bb
						 from beacukai in bb.DefaultIfEmpty()
						 join m in dbContext.GarmentBeacukais on beacukai.BeacukaiId equals m.Id into n
						 from bc in n.DefaultIfEmpty()
							 //urn
						 join q in dbContext.GarmentUnitReceiptNoteItems on dodetail.Id equals q.DODetailId into qq
						 from unititem in qq.DefaultIfEmpty()

						 join o in dbContext.GarmentUnitReceiptNotes on unititem.URNId equals o.Id into p
						 from receipt in p.DefaultIfEmpty()

							 //inv
						 join invd in dbContext.GarmentInvoiceDetails on dodetail.Id equals invd.DODetailId into rr
						 from invoicedetail in rr.DefaultIfEmpty()
						 join inv in dbContext.GarmentInvoiceItems on invoicedetail.InvoiceItemId equals inv.Id into r
						 from invoiceitem in r.DefaultIfEmpty()
						 join s in dbContext.GarmentInvoices on invoiceitem.InvoiceId equals s.Id into ss
						 from inv in ss.DefaultIfEmpty()
							 //intern
						 join w in dbContext.GarmentInternNoteDetails on invoicedetail.Id equals w.InvoiceDetailId into ww
						 from internotedetail in ww.DefaultIfEmpty()
						 join t in dbContext.GarmentInternNoteItems on internotedetail.GarmentItemINId equals t.Id into u
						 from intern in u.DefaultIfEmpty()
						 join v in dbContext.GarmentInternNotes on intern.GarmentINId equals v.Id into vv
						 from internnote in vv.DefaultIfEmpty()

							 //corr
						 join y in dbContext.GarmentCorrectionNoteItems on dodetail.Id equals y.DODetailId into oo
						 from corrItem in oo.DefaultIfEmpty()

						 join x in dbContext.GarmentCorrectionNotes on corrItem.GCorrectionId equals x.Id into cor
						 from correction in cor.DefaultIfEmpty()
						
						 where
						 ipoitem.IsDeleted == false && ipo.IsDeleted == false && epo.IsDeleted == false && epos.IsDeleted == false && intern.IsDeleted == false && internnote.IsDeleted == false && internotedetail.IsDeleted == false
						 && inv.IsDeleted == false && invoiceitem.IsDeleted == false && receipt.IsDeleted == false && unititem.IsDeleted == false && bc.IsDeleted == false

						  && a.IsDeleted == false && b.IsDeleted == false && ipo.IsDeleted == false && ipoitem.IsDeleted == false
						
						 && a.UnitId == (string.IsNullOrWhiteSpace(unit) ? a.UnitId : unit)
						  && a.Article == (string.IsNullOrWhiteSpace(article) ? a.Article : article) &&
						 epos.EPONo == (string.IsNullOrWhiteSpace(epono) ? epos.EPONo : epono)
						 && b.PO_SerialNumber == (string.IsNullOrWhiteSpace(poSerialNumber) ? b.PO_SerialNumber : poSerialNumber)
						 && dos.DONo == (string.IsNullOrWhiteSpace(doNo) ? dos.DONo : doNo)
						 && epos.SupplierId.ToString() == (string.IsNullOrWhiteSpace(supplier) ? epos.SupplierId.ToString() : supplier)
						 && a.RONo == (string.IsNullOrWhiteSpace(roNo) ? a.RONo : roNo)
						 && ipo.CreatedBy == (string.IsNullOrWhiteSpace(username) ? ipo.CreatedBy : username)
						 && b.IsUsed == (ipoStatus == "BELUM" ? false : ipoStatus == "SUDAH" ? true : b.IsUsed)
						 && ipoitem.Status == (string.IsNullOrWhiteSpace(status) ? ipoitem.Status : status)
						 && ((d1 != new DateTime(1970, 1, 1)) ? (a.Date.Date >= d1 && a.Date.Date <= d2) : true)

						 select new MonitoringPurchaseAllUserViewModel
						 {

							 poextNo = epos != null ? epos.EPONo : "",
							 poExtDate = epos != null ? epos.OrderDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 deliveryDate = epos != null ? epos.DeliveryDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 supplierCode = epos != null ? epos.SupplierCode : "",
							 supplierName = epos != null ? epos.SupplierName : "",
							 prNo = a.PRNo,
							 poSerialNumber = b.PO_SerialNumber,
							 prDate = a.Date.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
							 PrDate = a.Date,
							 unitName = a.UnitName,
							 buyerCode = a.BuyerCode,
							 buyerName = a.BuyerName,
							 ro = a.RONo,
							 article = a.Article,
							 shipmentDate = a.ShipmentDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
							 productCode = b.ProductCode,
							 productName = b.ProductName,
							 prProductRemark = b.ProductRemark,
							 poProductRemark = epo != null ? epo.Remark : "",
							 poDealQty = epo != null ? epo.DealQuantity : 0,
							 poDealUomUnit = epo != null ? epo.DealUomUnit : "",
							 prBudgetPrice = b.BudgetPrice,
							 poPricePerDealUnit = epo != null ? epo.PricePerDealUnit : 0,
							 incomeTaxRate = epos != null ? (epos.IncomeTaxRate).ToString() : "",
							 totalNominalPO = epo != null ? String.Format("{0:N2}", (epo.DealQuantity * epo.PricePerDealUnit)) : "",
							 poCurrencyCode = epos != null ? epos.CurrencyCode : "",
							 poCurrencyRate = epos != null ? epos.CurrencyRate : 0,
							 totalNominalRp = epo != null && epos != null ? String.Format("{0:N2}", (epo.DealQuantity * epo.PricePerDealUnit * epos.CurrencyRate)).ToString() : "",
							 ipoDate = ipo != null ? ipo.CreatedUtc.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 username = ipo != null ? ipo.CreatedBy : "",
							 useIncomeTax = epos != null ? epos.IsIncomeTax.Equals(true) ? "YA" : "TIDAK" : "",
							 useVat = epos != null ? epos.IsUseVat.Equals(true) ? "YA" : "TIDAK" : "",
							 useInternalPO = b.IsUsed == true ? "SUDAH" : "BELUM",
							 status = ipoitem != null ? ipoitem.Status : "",
							 doNo = dos != null ? dos.DONo : "",
							 doDate = dos != null ? dos.DODate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 arrivalDate = dos != null ? dos.ArrivalDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 doQty = dodetail != null ? dodetail.DOQuantity : 0,
							 doUomUnit = dodetail != null ? dodetail.UomUnit : "",
							 remainingDOQty = dodetail != null ? dodetail.DealQuantity - dodetail.DOQuantity : 0,
							 bcNo = bc != null ? bc.BeacukaiNo : "",
							 bcDate = bc != null ? bc.BeacukaiDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 receiptNo = receipt != null ? receipt.URNNo : "",
							 receiptDate = receipt != null ? receipt.ReceiptDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 receiptQty = unititem != null ? String.Format("{0:N2}", unititem.ReceiptQuantity) : "",
							 receiptUomUnit = unititem != null ? unititem.UomUnit : "",
							 invoiceNo = inv != null ? inv.InvoiceNo : "",
							 invoiceDate = inv != null ? inv.InvoiceDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 incomeTaxDate = inv != null ? inv.IncomeTaxDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 incomeTaxNo = inv != null ? inv.IncomeTaxNo : "",
							 incomeTaxType = inv != null ? inv.IncomeTaxName : "",
							 incomeTaxtRate = inv != null ? (inv.IncomeTaxRate).ToString() : "",
							 incomeTaxtValue = inv != null ? String.Format("{0:N2}", dodetail.DOQuantity * dodetail.PricePerDealUnit * inv.IncomeTaxRate / 100) : "",
							 vatNo = inv != null ? inv.VatNo : "",
							 vatDate = inv != null ? inv.VatDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 vatValue = inv != null ? String.Format("{0:N2}", dodetail.DOQuantity * dodetail.PricePerDealUnit * 10 / 100) : "",
							 internNo = internnote != null ? internnote.INNo : "",
							 internDate = internnote != null ? internnote.INDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 maturityDate = internotedetail != null ? internotedetail.PaymentDueDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 internTotal = internotedetail != null ? String.Format("{0:N2}", internotedetail.Quantity * internotedetail.PricePerDealUnit) : "",
							 dodetailId = corrItem != null ? corrItem.DODetailId : 0,
							 correctionNoteNo = correction != null ? correction.CorrectionNo : "",
							 correctionDate = correction != null ? correction.CorrectionDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 correctionRemark = correction != null ? correction.CorrectionType : "",
							 correctionTotal = correction == null ? 0 : correction.CorrectionType == "Harga Total" ? corrItem.PriceTotalAfter - corrItem.PriceTotalBefore : correction.CorrectionType == "Harga Satuan" ? (corrItem.PricePerDealUnitAfter - corrItem.PricePerDealUnitBefore) * corrItem.Quantity : correction.CorrectionType == "Jumlah" ? corrItem.PriceTotalAfter : 0,


						 }).Distinct().OrderBy(s => s.PrDate);
			int i = 1;
			foreach (var item in Query.Distinct())
			{
				listEPO.Add(
					new MonitoringPurchaseAllUserViewModel
					{
						index = i,
						poextNo = item.poextNo,
						poExtDate = item.poExtDate,
						deliveryDate = item.deliveryDate,
						supplierCode = item.supplierCode,
						supplierName = item.supplierName,
						prNo = item.prNo,
						poSerialNumber = item.poSerialNumber,
						prDate = item.prDate,
						PrDate = item.PrDate,
						unitName = item.unitName,
						buyerCode = item.buyerCode,
						buyerName = item.buyerName,
						ro = item.ro,
						article = item.article,
						shipmentDate = item.shipmentDate,
						productCode = item.productCode,
						productName = item.productName,
						prProductRemark = item.prProductRemark,
						poProductRemark = item.poProductRemark,
						poDealQty = item.poDealQty,
						poDealUomUnit = item.poDealUomUnit,
						prBudgetPrice = item.prBudgetPrice,
						poPricePerDealUnit = item.poPricePerDealUnit,
						totalNominalPO = item.totalNominalPO,
						poCurrencyCode = item.poCurrencyCode,
						poCurrencyRate = item.poCurrencyRate,
						totalNominalRp = item.totalNominalRp,
						incomeTaxRate = item.incomeTaxRate,
						ipoDate = item.ipoDate,
						username = item.username,
						useIncomeTax = item.useIncomeTax,
						useVat = item.useVat,
						useInternalPO = item.useInternalPO,
						incomeTaxtRate = item.incomeTaxtRate,
						status = item.status,
						doNo = item.doNo,
						doDate = item.doDate,
						arrivalDate = item.arrivalDate,
						doQty = item.doQty,
						doUomUnit = item.doUomUnit,
						remainingDOQty = item.remainingDOQty,
						bcNo = item.bcNo,
						bcDate = item.bcDate,
						receiptNo = item.receiptNo,
						receiptDate = item.receiptDate,
						receiptQty = item.receiptQty,
						receiptUomUnit = item.receiptUomUnit,
						invoiceNo = item.invoiceNo,
						invoiceDate = item.invoiceDate,
						incomeTaxDate = item.incomeTaxDate,
						incomeTaxNo = item.incomeTaxNo,
						incomeTaxType = item.incomeTaxType,
						incomeTaxtValue = item.incomeTaxtValue,
						vatNo = item.vatNo,
						vatDate = item.vatDate,
						vatValue = item.vatValue,
						internNo = item.internNo,
						internDate = item.internDate,
						internTotal = item.internTotal,
						maturityDate = item.maturityDate,
						dodetailId = item.dodetailId,
						correctionNoteNo = item.correctionNoteNo,
						correctionDate = item.correctionDate,
						correctionRemark = item.correctionRemark,
						correctionTotal = item.correctionTotal,

					}
					);



				i++;
			}
			Dictionary<long, string> qry = new Dictionary<long, string>();
			Dictionary<long, string> qryDate = new Dictionary<long, string>();
			Dictionary<long, string> qryQty = new Dictionary<long, string>();
			Dictionary<long, string> qryType = new Dictionary<long, string>();
			List<MonitoringPurchaseAllUserViewModel> listData = new List<MonitoringPurchaseAllUserViewModel>();

			var index = 0;
			List<string> corNo = new List<string>();
			foreach (MonitoringPurchaseAllUserViewModel data in listEPO.OrderByDescending(s => s.prDate))
			{
				string value;
				if (data.dodetailId != 0)
				{
					//string correctionDate = data.correctionDate == new DateTime(1970, 1, 1) ? "-" : data.correctionDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
					if (data.correctionNoteNo != null)
					{
						if (qry.TryGetValue(data.dodetailId, out value))
						{
							var isexist = (from a in corNo
										   where a == data.correctionNoteNo
										   select a).FirstOrDefault();
							if (isexist == null)
							{
								qry[data.dodetailId] += (index).ToString() + ". " + data.correctionNoteNo + "\n";
								qryType[data.dodetailId] += (index).ToString() + ". " + data.correctionRemark + "\n";
								qryDate[data.dodetailId] += (index).ToString() + ". " + data.correctionDate + "\n";
								qryQty[data.dodetailId] += (index).ToString() + ". " + String.Format("{0:N2}", data.correctionTotal) + "\n";
								index++;
								corNo.Add(data.correctionNoteNo);
							}

						}
						else
						{
							index = 1;
							qry[data.dodetailId] = (index).ToString() + ". " + data.correctionNoteNo + "\n";
							qryType[data.dodetailId] = (index).ToString() + ". " + data.correctionRemark + "\n";
							qryDate[data.dodetailId] = (index).ToString() + ". " + data.correctionDate + "\n";
							qryQty[data.dodetailId] = (index).ToString() + ". " + String.Format("{0:N2}", data.correctionTotal) + "\n";
							corNo.Add(data.correctionNoteNo);
							index++;
						}
					}
				}
				else
				{
					listData.Add(data);
				}

			}
			foreach (var corrections in qry.Distinct())
			{
				foreach (MonitoringPurchaseAllUserViewModel data in Query.ToList())
				{
					if (corrections.Key == data.dodetailId)
					{
						data.correctionNoteNo = qry[data.dodetailId];
						data.correctionRemark = qryType[data.dodetailId];
						data.valueCorrection = (qryQty[data.dodetailId]);
						data.correctionDate = qryDate[data.dodetailId];
						listData.Add(data);
						break;
					}
				}
			}

			//var op = qry;
			return listData.AsQueryable().OrderBy(s => s.PrDate);
			//return listEPO.AsQueryable();

		}


		public Tuple<List<MonitoringPurchaseAllUserViewModel>, int> GetMonitoringPurchaseReport(string epono, string unit, string roNo, string article, string poSerialNumber, string username, string doNo, string ipoStatus, string supplier, string status, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
		{
			var Query = GetMonitoringPurchaseAllReportQuery(epono, unit, roNo, article, poSerialNumber, username, doNo, ipoStatus, supplier, status, dateFrom, dateTo, offset);

			Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
			//if (OrderDictionary.Count.Equals(0))
			//{
			//	Query = Query.OrderByDescending(b => b.prDate);
			//}

			Pageable<MonitoringPurchaseAllUserViewModel> pageable = new Pageable<MonitoringPurchaseAllUserViewModel>(Query, page - 1, size);
			List<MonitoringPurchaseAllUserViewModel> Data = pageable.Data.ToList<MonitoringPurchaseAllUserViewModel>();
			int TotalData = pageable.TotalCount;

			return Tuple.Create(Data, TotalData);
		}
		public MemoryStream GenerateExcelPurchase(string epono, string unit, string roNo, string article, string poSerialNumber, string username, string doNo, string ipoStatus, string supplier, string status, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
		{
			var Query = GetMonitoringPurchaseAllReportQuery(epono, unit, roNo, article, poSerialNumber, username, doNo, ipoStatus, supplier, status, dateFrom, dateTo, offset);
			Query = Query.OrderBy(b => b.PrDate);
			DataTable result = new DataTable();

			result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nomor Purchase Request", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Purchase Request", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "No Ref.PO", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Dibuat PO Internal", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO RO", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Artikel", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kode Buyer", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nama Buyer", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Shipment GMT", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nomor PO Eksternal", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl PO Eksternal", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Target Datang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kena PPN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kena PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = " PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kode Supplier", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nama Supplier", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Status", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Keterangan Barang (PR)", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Keterangan Barang (PO EKS)", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Jumlah", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Satuan Barang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Harga Budget", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Harga Beli", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "MT UANG", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kurs", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Total RP", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Terima PO Intern", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl SJ", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Datang Barang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Qty Datang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Satuan SJ", DataType = typeof(String) });
			//result.Columns.Add(new DataColumn() { ColumnName = "Qty Sisa", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "No Beacukai", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Beacukai", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nomor Bon Terima", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Terima Unit", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Qty Terima Unit", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Satuan Terima Unit", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nomor Invoice", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Invoice", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO PPN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl PPN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nilai PPN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Jenis PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Rate PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "No PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nilai PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nomor Nota Intern", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Nota Intern", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nilai Intern", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Jatuh Tempo", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "No Nota Koreksi", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Nota Koreksi", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nilai Koreksi", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Staff Pembelian", DataType = typeof(String) });


			if (Query.ToArray().Count() == 0)
				result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
			else
			{
				int index = 0;
				foreach (var item in Query)
				{
					index++;

					result.Rows.Add(index, item.prNo, item.prDate, item.unitName, item.poSerialNumber, item.useInternalPO, item.ro, item.article, item.buyerCode, item.buyerName, item.shipmentDate, item.poextNo, item.poExtDate, item.deliveryDate, item.useVat, item.useIncomeTax, item.incomeTaxRate, item.supplierCode, item.supplierName, item.status, item.productCode, item.productName, item.prProductRemark, item.poProductRemark, item.poDealQty,
						item.poDealUomUnit, item.prBudgetPrice, item.poPricePerDealUnit, item.totalNominalPO, item.poCurrencyCode, item.poCurrencyRate, item.totalNominalRp, item.ipoDate, item.doNo,
						item.doDate, item.arrivalDate, item.doQty, item.doUomUnit, item.bcNo, item.bcDate, item.receiptNo, item.receiptDate, item.receiptQty, item.receiptUomUnit,
						item.invoiceNo, item.invoiceDate, item.vatNo, item.vatDate, item.vatValue, item.incomeTaxType, item.incomeTaxtRate, item.incomeTaxNo, item.incomeTaxDate, item.incomeTaxtValue,
						item.internNo, item.internDate, item.internTotal, item.maturityDate, item.correctionNoteNo, item.correctionDate, item.valueCorrection, item.correctionRemark, item.username);
				}
			}

			return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
		}

		#endregion
		#region monitoring purchase
		public IQueryable<MonitoringPurchaseAllUserViewModel> GetMonitoringPurchaseByUserReportQuery(string epono, string unit, string roNo, string article, string poSerialNumber, string username, string doNo, string ipoStatus, string supplier, string status, DateTime? dateFrom, DateTime? dateTo, int offset)
		{


			DateTime d1 = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
			DateTime d2 = dateTo == null ? DateTime.Now : (DateTime)dateTo;
			offset = 7;

			List<MonitoringPurchaseAllUserViewModel> listEPO = new List<MonitoringPurchaseAllUserViewModel>();
			var Query = (from
						  a in dbContext.GarmentPurchaseRequests
						 join b in dbContext.GarmentPurchaseRequestItems on a.Id equals b.GarmentPRId
						 //internalPO
						 join l in dbContext.GarmentInternalPurchaseOrderItems on b.Id equals l.GPRItemId into ll
						 from ipoitem in ll.DefaultIfEmpty()
						 join c in dbContext.GarmentInternalPurchaseOrders on ipoitem.GPOId equals c.Id into d
						 from ipo in d.DefaultIfEmpty()

							 //eksternalpo
						 join e in dbContext.GarmentExternalPurchaseOrderItems on ipo.Id equals e.POId into f
						 from epo in f.DefaultIfEmpty()

						 join g in dbContext.GarmentExternalPurchaseOrders on epo.GarmentEPOId equals g.Id into gg
						 from epos in gg.DefaultIfEmpty()
							 //do
						 join h in dbContext.GarmentDeliveryOrderDetails on epo.Id equals h.EPOItemId into ii
						 from dodetail in ii.DefaultIfEmpty()

						 join j in dbContext.GarmentDeliveryOrderItems on dodetail.GarmentDOItemId equals j.Id into hh
						 from doitem in hh.DefaultIfEmpty()

						 join k in dbContext.GarmentDeliveryOrders on doitem.GarmentDOId equals k.Id into kk
						 from dos in kk.DefaultIfEmpty()


							 //bc
						 join bcs in dbContext.GarmentBeacukaiItems on dos.Id equals bcs.GarmentDOId into bb
						 from beacukai in bb.DefaultIfEmpty()
						 join m in dbContext.GarmentBeacukais on beacukai.BeacukaiId equals m.Id into n
						 from bc in n.DefaultIfEmpty()
							 //urn
						 join q in dbContext.GarmentUnitReceiptNoteItems on dodetail.Id equals q.DODetailId into qq
						 from unititem in qq.DefaultIfEmpty()

						 join o in dbContext.GarmentUnitReceiptNotes on unititem.URNId equals o.Id into p
						 from receipt in p.DefaultIfEmpty()

							 //inv
						 join invd in dbContext.GarmentInvoiceDetails on dodetail.Id equals invd.DODetailId into rr
						 from invoicedetail in rr.DefaultIfEmpty()
						 join inv in dbContext.GarmentInvoiceItems on invoicedetail.InvoiceItemId equals inv.Id into r
						 from invoiceitem in r.DefaultIfEmpty()
						 join s in dbContext.GarmentInvoices on invoiceitem.InvoiceId equals s.Id into ss
						 from inv in ss.DefaultIfEmpty()
							 //intern
						 join w in dbContext.GarmentInternNoteDetails on invoicedetail.Id equals w.InvoiceDetailId into ww
						 from internotedetail in ww.DefaultIfEmpty()
						 join t in dbContext.GarmentInternNoteItems on internotedetail.GarmentItemINId equals t.Id into u
						 from intern in u.DefaultIfEmpty()
						 join v in dbContext.GarmentInternNotes on intern.GarmentINId equals v.Id into vv
						 from internnote in vv.DefaultIfEmpty()

							 //corr
						 join y in dbContext.GarmentCorrectionNoteItems on dodetail.Id equals y.DODetailId into oo
						 from corrItem in oo.DefaultIfEmpty()

						 join x in dbContext.GarmentCorrectionNotes on corrItem.GCorrectionId equals x.Id into cor
						 from correction in cor.DefaultIfEmpty()

						 where
						 ipoitem.IsDeleted == false && ipo.IsDeleted == false && epo.IsDeleted == false && epos.IsDeleted == false && intern.IsDeleted == false && internnote.IsDeleted == false && internotedetail.IsDeleted == false
						 && inv.IsDeleted == false && invoiceitem.IsDeleted == false && receipt.IsDeleted == false && unititem.IsDeleted == false && bc.IsDeleted == false

						  && a.IsDeleted == false && b.IsDeleted == false && ipo.IsDeleted == false && ipoitem.IsDeleted == false

						 && a.UnitId == (string.IsNullOrWhiteSpace(unit) ? a.UnitId : unit)
						  && a.Article == (string.IsNullOrWhiteSpace(article) ? a.Article : article) &&
						 epos.EPONo == (string.IsNullOrWhiteSpace(epono) ? epos.EPONo : epono)
						 && b.PO_SerialNumber == (string.IsNullOrWhiteSpace(poSerialNumber) ? b.PO_SerialNumber : poSerialNumber)
						 && dos.DONo == (string.IsNullOrWhiteSpace(doNo) ? dos.DONo : doNo)
						 && epos.SupplierId.ToString() == (string.IsNullOrWhiteSpace(supplier) ? epos.SupplierId.ToString() : supplier)
						 && a.RONo == (string.IsNullOrWhiteSpace(roNo) ? a.RONo : roNo)
						 && ipo.CreatedBy == (string.IsNullOrWhiteSpace(username) ? ipo.CreatedBy : username)
						 && b.IsUsed == (ipoStatus == "BELUM" ? false : ipoStatus == "SUDAH" ? true : b.IsUsed)
						 && ipoitem.Status == (string.IsNullOrWhiteSpace(status) ? ipoitem.Status : status)
						 && ((d1 != new DateTime(1970, 1, 1)) ? (a.Date.Date >= d1 && a.Date.Date <= d2) : true)

						 select new MonitoringPurchaseAllUserViewModel
						 {

							 poextNo = epos != null ? epos.EPONo : "",
							 poExtDate = epos != null ? epos.OrderDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 deliveryDate = epos != null ? epos.DeliveryDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 supplierCode = epos != null ? epos.SupplierCode : "",
							 supplierName = epos != null ? epos.SupplierName : "",
							 prNo = a.PRNo,
							 poSerialNumber = b.PO_SerialNumber,
							 prDate = a.Date.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
							 PrDate = a.Date,
							 unitName = a.UnitName,
							 buyerCode = a.BuyerCode,
							 buyerName = a.BuyerName,
							 ro = a.RONo,
							 article = a.Article,
							 shipmentDate = a.ShipmentDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
							 productCode = b.ProductCode,
							 productName = b.ProductName,
							 prProductRemark = b.ProductRemark,
							 poProductRemark = epo != null ? epo.Remark : "",
							 poDealQty = epo != null ? epo.DealQuantity : 0,
							 poDealUomUnit = epo != null ? epo.DealUomUnit : "",
							 prBudgetPrice = b.BudgetPrice,
							 poPricePerDealUnit = epo != null ? epo.PricePerDealUnit : 0,
							 incomeTaxRate = epos != null ? (epos.IncomeTaxRate).ToString() : "",
							 totalNominalPO = epo != null ? String.Format("{0:N2}", (epo.DealQuantity * epo.PricePerDealUnit)) : "",
							 poCurrencyCode = epos != null ? epos.CurrencyCode : "",
							 poCurrencyRate = epos != null ? epos.CurrencyRate : 0,
							 totalNominalRp = epo != null && epos != null ? String.Format("{0:N2}", (epo.DealQuantity * epo.PricePerDealUnit * epos.CurrencyRate)).ToString() : "",
							 ipoDate = ipo != null ? ipo.CreatedUtc.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 username = ipo != null ? ipo.CreatedBy : "",
							 useIncomeTax = epos != null ? epos.IsIncomeTax.Equals(true) ? "YA" : "TIDAK" : "",
							 useVat = epos != null ? epos.IsUseVat.Equals(true) ? "YA" : "TIDAK" : "",
							 useInternalPO = b.IsUsed == true ? "SUDAH" : "BELUM",
							 status = ipoitem != null ? ipoitem.Status : "",
							 doNo = dos != null ? dos.DONo : "",
							 doDate = dos != null ? dos.DODate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 arrivalDate = dos != null ? dos.ArrivalDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 doQty = dodetail != null ? dodetail.DOQuantity : 0,
							 doUomUnit = dodetail != null ? dodetail.UomUnit : "",
							 remainingDOQty = dodetail != null ? dodetail.DealQuantity - dodetail.DOQuantity : 0,
							 bcNo = bc != null ? bc.BeacukaiNo : "",
							 bcDate = bc != null ? bc.BeacukaiDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 receiptNo = receipt != null ? receipt.URNNo : "",
							 receiptDate = receipt != null ? receipt.ReceiptDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 receiptQty = unititem != null ? String.Format("{0:N2}", unititem.ReceiptQuantity) : "",
							 receiptUomUnit = unititem != null ? unititem.UomUnit : "",
							 invoiceNo = inv != null ? inv.InvoiceNo : "",
							 invoiceDate = inv != null ? inv.InvoiceDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 incomeTaxDate = inv != null ? inv.IncomeTaxDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 incomeTaxNo = inv != null ? inv.IncomeTaxNo : "",
							 incomeTaxType = inv != null ? inv.IncomeTaxName : "",
							 incomeTaxtRate = inv != null ? (inv.IncomeTaxRate).ToString() : "",
							 incomeTaxtValue = inv != null ? String.Format("{0:N2}", dodetail.DOQuantity * dodetail.PricePerDealUnit * inv.IncomeTaxRate / 100) : "",
							 vatNo = inv != null ? inv.VatNo : "",
							 vatDate = inv != null ? inv.VatDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 vatValue = inv != null ? String.Format("{0:N2}", dodetail.DOQuantity * dodetail.PricePerDealUnit * 10 / 100) : "",
							 internNo = internnote != null ? internnote.INNo : "",
							 internDate = internnote != null ? internnote.INDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 maturityDate = internotedetail != null ? internotedetail.PaymentDueDate.AddHours(offset).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 internTotal = internotedetail != null ? String.Format("{0:N2}", internotedetail.Quantity * internotedetail.PricePerDealUnit) : "",
							 dodetailId = corrItem != null ? corrItem.DODetailId : 0,
							 correctionNoteNo = correction != null ? correction.CorrectionNo : "",
							 correctionDate = correction != null ? correction.CorrectionDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "",
							 correctionRemark = correction != null ? correction.CorrectionType : "",
							 correctionTotal = correction == null ? 0 : correction.CorrectionType == "Harga Total" ? corrItem.PriceTotalAfter - corrItem.PriceTotalBefore : correction.CorrectionType == "Harga Satuan" ? (corrItem.PricePerDealUnitAfter - corrItem.PricePerDealUnitBefore) * corrItem.Quantity : correction.CorrectionType == "Jumlah" ? corrItem.PriceTotalAfter : 0,


						 }).Distinct().OrderBy(s => s.PrDate);
			int i = 1;
			foreach (var item in Query.Distinct())
			{
				listEPO.Add(
					new MonitoringPurchaseAllUserViewModel
					{
						index = i,
						poextNo = item.poextNo,
						poExtDate = item.poExtDate,
						deliveryDate = item.deliveryDate,
						supplierCode = item.supplierCode,
						supplierName = item.supplierName,
						prNo = item.prNo,
						poSerialNumber = item.poSerialNumber,
						prDate = item.prDate,
						PrDate = item.PrDate,
						unitName = item.unitName,
						buyerCode = item.buyerCode,
						buyerName = item.buyerName,
						ro = item.ro,
						article = item.article,
						shipmentDate = item.shipmentDate,
						productCode = item.productCode,
						productName = item.productName,
						prProductRemark = item.prProductRemark,
						poProductRemark = item.poProductRemark,
						poDealQty = item.poDealQty,
						poDealUomUnit = item.poDealUomUnit,
						prBudgetPrice = item.prBudgetPrice,
						poPricePerDealUnit = item.poPricePerDealUnit,
						totalNominalPO = item.totalNominalPO,
						poCurrencyCode = item.poCurrencyCode,
						poCurrencyRate = item.poCurrencyRate,
						totalNominalRp = item.totalNominalRp,
						incomeTaxRate = item.incomeTaxRate,
						ipoDate = item.ipoDate,
						username = item.username,
						useIncomeTax = item.useIncomeTax,
						useVat = item.useVat,
						useInternalPO = item.useInternalPO,
						incomeTaxtRate = item.incomeTaxtRate,
						status = item.status,
						doNo = item.doNo,
						doDate = item.doDate,
						arrivalDate = item.arrivalDate,
						doQty = item.doQty,
						doUomUnit = item.doUomUnit,
						remainingDOQty = item.remainingDOQty,
						bcNo = item.bcNo,
						bcDate = item.bcDate,
						receiptNo = item.receiptNo,
						receiptDate = item.receiptDate,
						receiptQty = item.receiptQty,
						receiptUomUnit = item.receiptUomUnit,
						invoiceNo = item.invoiceNo,
						invoiceDate = item.invoiceDate,
						incomeTaxDate = item.incomeTaxDate,
						incomeTaxNo = item.incomeTaxNo,
						incomeTaxType = item.incomeTaxType,
						incomeTaxtValue = item.incomeTaxtValue,
						vatNo = item.vatNo,
						vatDate = item.vatDate,
						vatValue = item.vatValue,
						internNo = item.internNo,
						internDate = item.internDate,
						internTotal = item.internTotal,
						maturityDate = item.maturityDate,
						dodetailId = item.dodetailId,
						correctionNoteNo = item.correctionNoteNo,
						correctionDate = item.correctionDate,
						correctionRemark = item.correctionRemark,
						correctionTotal = item.correctionTotal,

					}
					);



				i++;
			}
			Dictionary<long, string> qry = new Dictionary<long, string>();
			Dictionary<long, string> qryDate = new Dictionary<long, string>();
			Dictionary<long, string> qryQty = new Dictionary<long, string>();
			Dictionary<long, string> qryType = new Dictionary<long, string>();
			List<MonitoringPurchaseAllUserViewModel> listData = new List<MonitoringPurchaseAllUserViewModel>();

			var index = 0;
			List<string> corNo = new List<string>();
			foreach (MonitoringPurchaseAllUserViewModel data in listEPO.OrderByDescending(s => s.prDate))
			{
				string value;
				if (data.dodetailId != 0)
				{
					//string correctionDate = data.correctionDate == new DateTime(1970, 1, 1) ? "-" : data.correctionDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
					if (data.correctionNoteNo != null)
					{
						if (qry.TryGetValue(data.dodetailId, out value))
						{
							var isexist = (from a in corNo
										   where a == data.correctionNoteNo
										   select a).FirstOrDefault();
							if (isexist == null)
							{
								qry[data.dodetailId] += (index).ToString() + ". " + data.correctionNoteNo + "\n";
								qryType[data.dodetailId] += (index).ToString() + ". " + data.correctionRemark + "\n";
								qryDate[data.dodetailId] += (index).ToString() + ". " + data.correctionDate + "\n";
								qryQty[data.dodetailId] += (index).ToString() + ". " + String.Format("{0:N2}", data.correctionTotal) + "\n";
								index++;
								corNo.Add(data.correctionNoteNo);
							}

						}
						else
						{
							index = 1;
							qry[data.dodetailId] = (index).ToString() + ". " + data.correctionNoteNo + "\n";
							qryType[data.dodetailId] = (index).ToString() + ". " + data.correctionRemark + "\n";
							qryDate[data.dodetailId] = (index).ToString() + ". " + data.correctionDate + "\n";
							qryQty[data.dodetailId] = (index).ToString() + ". " + String.Format("{0:N2}", data.correctionTotal) + "\n";
							corNo.Add(data.correctionNoteNo);
							index++;
						}
					}
				}
				else
				{
					listData.Add(data);
				}

			}
			foreach (var corrections in qry.Distinct())
			{
				foreach (MonitoringPurchaseAllUserViewModel data in Query.ToList())
				{
					if (corrections.Key == data.dodetailId)
					{
						data.correctionNoteNo = qry[data.dodetailId];
						data.correctionRemark = qryType[data.dodetailId];
						data.valueCorrection = (qryQty[data.dodetailId]);
						data.correctionDate = qryDate[data.dodetailId];
						listData.Add(data);
						break;
					}
				}
			}

			//var op = qry;
			return listData.AsQueryable().OrderBy(s => s.PrDate);
			//return listEPO.AsQueryable();

		}


		public Tuple<List<MonitoringPurchaseAllUserViewModel>, int> GetMonitoringPurchaseByUserReport(string epono, string unit, string roNo, string article, string poSerialNumber, string username, string doNo, string ipoStatus, string supplier, string status, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
		{
			var Query = GetMonitoringPurchaseByUserReportQuery(epono, unit, roNo, article, poSerialNumber, username, doNo, ipoStatus, supplier, status, dateFrom, dateTo, offset);

			Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
			//if (OrderDictionary.Count.Equals(0))
			//{
			//	Query = Query.OrderByDescending(b => b.prDate);
			//}

			Pageable<MonitoringPurchaseAllUserViewModel> pageable = new Pageable<MonitoringPurchaseAllUserViewModel>(Query, page - 1, size);
			List<MonitoringPurchaseAllUserViewModel> Data = pageable.Data.ToList<MonitoringPurchaseAllUserViewModel>();
			int TotalData = pageable.TotalCount;

			return Tuple.Create(Data, TotalData);
		}

		public MemoryStream GenerateExcelByUserPurchase(string epono, string unit, string roNo, string article, string poSerialNumber, string username, string doNo, string ipoStatus, string supplier, string status, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
		{
			var Query = GetMonitoringPurchaseAllReportQuery(epono, unit, roNo, article, poSerialNumber, username, doNo, ipoStatus, supplier, status, dateFrom, dateTo, offset);
			Query = Query.OrderBy(b => b.PrDate);
			DataTable result = new DataTable();

			result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nomor Purchase Request", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Purchase Request", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "No Ref.PO", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Dibuat PO Internal", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO RO", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Artikel", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kode Buyer", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nama Buyer", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Shipment GMT", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nomor PO Eksternal", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl PO Eksternal", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Target Datang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kena PPN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kena PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = " PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kode Supplier", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nama Supplier", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Status", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Keterangan Barang (PR)", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Keterangan Barang (PO EKS)", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Jumlah", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Satuan Barang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Harga Budget", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Harga Beli", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "MT UANG", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Kurs", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Total RP", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Terima PO Intern", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl SJ", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Datang Barang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Qty Datang", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Satuan SJ", DataType = typeof(String) });
			//result.Columns.Add(new DataColumn() { ColumnName = "Qty Sisa", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "No Beacukai", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Beacukai", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nomor Bon Terima", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Terima Unit", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Qty Terima Unit", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Satuan Terima Unit", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nomor Invoice", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Invoice", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "NO PPN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl PPN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nilai PPN", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Jenis PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Rate PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "No PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nilai PPH", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nomor Nota Intern", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Nota Intern", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nilai Intern", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Jatuh Tempo", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "No Nota Koreksi", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Tgl Nota Koreksi", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Nilai Koreksi", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(String) });
			result.Columns.Add(new DataColumn() { ColumnName = "Staff Pembelian", DataType = typeof(String) });


			if (Query.ToArray().Count() == 0)
				result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
			else
			{
				int index = 0;
				foreach (var item in Query)
				{
					index++;

					result.Rows.Add(index, item.prNo, item.prDate, item.unitName, item.poSerialNumber, item.useInternalPO, item.ro, item.article, item.buyerCode, item.buyerName, item.shipmentDate, item.poextNo, item.poExtDate, item.deliveryDate, item.useVat, item.useIncomeTax, item.incomeTaxRate, item.supplierCode, item.supplierName, item.status, item.productCode, item.productName, item.prProductRemark, item.poProductRemark, item.poDealQty,
						item.poDealUomUnit, item.prBudgetPrice, item.poPricePerDealUnit, item.totalNominalPO, item.poCurrencyCode, item.poCurrencyRate, item.totalNominalRp, item.ipoDate, item.doNo,
						item.doDate, item.arrivalDate, item.doQty, item.doUomUnit, item.bcNo, item.bcDate, item.receiptNo, item.receiptDate, item.receiptQty, item.receiptUomUnit,
						item.invoiceNo, item.invoiceDate, item.vatNo, item.vatDate, item.vatValue, item.incomeTaxType, item.incomeTaxtRate, item.incomeTaxNo, item.incomeTaxDate, item.incomeTaxtValue,
						item.internNo, item.internDate, item.internTotal, item.maturityDate, item.correctionNoteNo, item.correctionDate, item.valueCorrection, item.correctionRemark, item.username);
				}
			}

			return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
		}
		#endregion
	}
}
