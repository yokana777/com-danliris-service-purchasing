using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentBeacukaiModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentBeacukaiFacade
{
	public class GarmentBeacukaiFacade : IGarmentBeacukaiFacade
	{
		private readonly PurchasingDbContext dbContext;
		private readonly DbSet<GarmentBeacukai> dbSet;
		public readonly IServiceProvider serviceProvider;
		private readonly DbSet<GarmentDeliveryOrder> dbSetDeliveryOrder;
		private string USER_AGENT = "Facade";
		public GarmentBeacukaiFacade(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
		{
			this.dbContext = dbContext;
			this.dbSet = dbContext.Set<GarmentBeacukai>();
			this.dbSetDeliveryOrder = dbContext.Set<GarmentDeliveryOrder>();
			this.serviceProvider = serviceProvider;
		}

		public Tuple<List<GarmentBeacukai>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
		{
			IQueryable<GarmentBeacukai> Query = this.dbSet.Include(m => m.Items);

			List<string> searchAttributes = new List<string>()
			{
				"beacukaiNo", "suppliername","customsType","items.garmentdono"
			};

			Query = QueryHelper<GarmentBeacukai>.ConfigureSearch(Query, searchAttributes, Keyword);

			Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
			Query = QueryHelper<GarmentBeacukai>.ConfigureFilter(Query, FilterDictionary);

			Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
			Query = QueryHelper<GarmentBeacukai>.ConfigureOrder(Query, OrderDictionary);

			Pageable<GarmentBeacukai> pageable = new Pageable<GarmentBeacukai>(Query, Page - 1, Size);
			List<GarmentBeacukai> Data = pageable.Data.ToList();
			int TotalData = pageable.TotalCount;

			return Tuple.Create(Data, TotalData, OrderDictionary);
		}

		public GarmentBeacukai ReadById(int id)
		{
			var model = dbSet.Where(m => m.Id == id)
			 .Include(m => m.Items)
			 .FirstOrDefault();
			return model;
		}

		public string GenerateBillNo()
		{
			string BillNo = null;
			GarmentDeliveryOrder deliveryOrder = (from data in dbSetDeliveryOrder
									   orderby data.BillNo descending
								 select data).FirstOrDefault();
			string year = DateTime.Now.Year.ToString().Substring(2, 2);
			string month = DateTime.Now.Month.ToString("D2");
			string day = DateTime.Now.Day.ToString("D2");
			string formatDate = year + month + day;
			int counterId = 0;
			if (deliveryOrder.BillNo != null)
			{
				BillNo = deliveryOrder.PaymentBill;
				string days = BillNo.Substring(4, 2);
				string number = BillNo.Substring(8);
				if (month == DateTime.Now.Month.ToString("D2"))
				{
					counterId = Convert.ToInt32(number) +1;
				}
				else
				{
					counterId = 1;
				}
			}else
			{
				counterId = 1;
				
			}
			BillNo = "BP" + formatDate + counterId.ToString("D3");
			return BillNo;

		}

		public string GeneratePaymentBillNo()
		{
			string PaymentBill = null;
			GarmentDeliveryOrder deliveryOrder = (from data in dbSetDeliveryOrder
												  orderby data.PaymentBill descending
												  select data).FirstOrDefault();
			string year = DateTime.Now.Year.ToString().Substring(2, 2);
			string month = DateTime.Now.Month.ToString("D2");
			string day = DateTime.Now.Day.ToString("D2");
			string formatDate = year + month + day;
			int counterId = 0;
			if (deliveryOrder.BillNo != null)
			{
				PaymentBill = deliveryOrder.PaymentBill;
				string days = PaymentBill.Substring(4, 2);
				string number = PaymentBill.Substring(8);
				if (month == DateTime.Now.Month.ToString("D2"))
				{
					counterId = Convert.ToInt32(number) + 1;
				}
				else
				{
					counterId = 1;
				}
			}
			else
			{
				counterId = 1;
			}
			PaymentBill = "BB" + formatDate + counterId.ToString("D3");

			return PaymentBill;

		}
		public async Task<int> Create(GarmentBeacukai model, string username, int clientTimeZoneOffset = 7)
		{
			int Created = 0;

			using (var transaction = this.dbContext.Database.BeginTransaction())
			{
				try
				{

					EntityExtension.FlagForCreate(model, username, USER_AGENT);
					
					foreach (GarmentBeacukaiItem item in model.Items)
					{
						GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.Include(m => m.Items)
															.ThenInclude(i => i.Details).FirstOrDefault(s => s.Id == item.GarmentDOId);
						if (deliveryOrder != null)
						{
							deliveryOrder.BillNo = GenerateBillNo();
							deliveryOrder.PaymentBill = GeneratePaymentBillNo();
							deliveryOrder.CustomsId = model.Id;
							double qty = 0;
							foreach(var  deliveryOrderItem in deliveryOrder.Items)
							{
								foreach(var detail in deliveryOrderItem.Details)
								{
									qty += detail.DOQuantity;
								}
							}
							item.TotalAmount = Convert.ToDecimal(deliveryOrder.TotalAmount);
							item.TotalQty = qty;
							EntityExtension.FlagForCreate(item, username, USER_AGENT);
						}
					}

					this.dbSet.Add(model);
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
		public int Delete(int id, string username)
		{
			int Deleted = 0;

			using (var transaction = this.dbContext.Database.BeginTransaction())
			{
				try
				{
					var model = this.dbSet
						.Include(d => d.Items)
						.SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

					EntityExtension.FlagForDelete(model, username, USER_AGENT);

					foreach (var item in model.Items)
					{
						GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.GarmentDOId);
						if (deliveryOrder != null)
						{
							deliveryOrder.BillNo = null;
							deliveryOrder.PaymentBill = null;
							deliveryOrder.CustomsId = 0;
							EntityExtension.FlagForDelete(item, username, USER_AGENT);
						}
						

					}

					Deleted = dbContext.SaveChanges();
					transaction.Commit();
				}
				catch (Exception e)
				{
					transaction.Rollback();
					throw new Exception(e.Message);
				}
			}

			return Deleted;
		}

		public HashSet<long> GetGarmentBeacukaiId(long id)
		{
			return new HashSet<long>(dbContext.GarmentBeacukaiItems.Where(d => d.GarmentBeacukai.Id == id).Select(d => d.Id));
		}

		public async Task<int> Update(int id, GarmentBeacukai m, string user, int clientTimeZoneOffset = 7)
		{
			int Updated = 0;

			using (var transaction = this.dbContext.Database.BeginTransaction())
			{
				try
				{
					 
					EntityExtension.FlagForUpdate(m, user, USER_AGENT);
					this.dbSet.Update(m);
					Updated = await dbContext.SaveChangesAsync();
					transaction.Commit();

				}
				catch (Exception e)
				{
					transaction.Rollback();
					throw new Exception(e.Message);
				}
			}

			return Updated;
		}
	}
}