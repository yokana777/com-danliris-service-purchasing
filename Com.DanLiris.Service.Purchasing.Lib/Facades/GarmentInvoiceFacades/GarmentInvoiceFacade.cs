using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInvoiceFacades
{
    public class GarmentInvoiceFacade : IGarmentInvoice
    {
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentInvoice> dbSet;
		private readonly DbSet<GarmentDeliveryOrder> dbSetDeliveryOrder;
		public readonly IServiceProvider serviceProvider;

        private string USER_AGENT = "Facade";
        public GarmentInvoiceFacade(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentInvoice>();
			this.dbSetDeliveryOrder = dbContext.Set<GarmentDeliveryOrder>();
            this.serviceProvider = serviceProvider;
        }
		public Tuple<List<GarmentInvoice>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
		{
			IQueryable<GarmentInvoice> Query = this.dbSet.Include(m => m.Items).ThenInclude(i => i.Details);

			List<string> searchAttributes = new List<string>()
			{
				"InvoiceNo", "InvoiceDate", "Suppliers.Name"
			};

			Query = QueryHelper<GarmentInvoice>.ConfigureSearch(Query, searchAttributes, Keyword);

			Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
			Query = QueryHelper<GarmentInvoice>.ConfigureFilter(Query, FilterDictionary);

			Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
			Query = QueryHelper<GarmentInvoice>.ConfigureOrder(Query, OrderDictionary);

			Pageable<GarmentInvoice> pageable = new Pageable<GarmentInvoice>(Query, Page - 1, Size);
			List<GarmentInvoice> Data = pageable.Data.ToList();
			int TotalData = pageable.TotalCount;

			return Tuple.Create(Data, TotalData, OrderDictionary);
		}

		public GarmentInvoice ReadById(int id)
		{
			var model = dbSet.Where(m => m.Id == id)
				 .Include(m => m.Items)
					 .ThenInclude(i => i.Details)
				 .FirstOrDefault();
			return model;
		}

		public async Task<int> Create(GarmentInvoice model, string username, int clientTimeZoneOffset = 7)
		{
			int Created = 0;

			using (var transaction = this.dbContext.Database.BeginTransaction())
			{
				try
				{
					double _total = 0;
					EntityExtension.FlagForCreate(model, username, USER_AGENT);

					foreach (var item in model.Items)
					{
						_total += item.TotalAmount;
						GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.DeliveryOrderId );
						if(deliveryOrder!=null)
						deliveryOrder.IsInvoice = true;
						EntityExtension.FlagForCreate(item, username, USER_AGENT);

						foreach (var detail in item.Details)
						{
							EntityExtension.FlagForCreate(detail, username, USER_AGENT);
						}
					}
					model.TotalAmount = _total;
					
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
							.ThenInclude(d => d.Details)
						.SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);
					
					EntityExtension.FlagForDelete(model, username, USER_AGENT);

					foreach (var item in model.Items)
					{
						GarmentDeliveryOrder garmentDeliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.DeliveryOrderId);
						garmentDeliveryOrder.IsInvoice = false;
						EntityExtension.FlagForDelete(item, username, USER_AGENT);
						foreach (var detail in item.Details)
						{
							EntityExtension.FlagForDelete(detail, username, USER_AGENT);
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

		public HashSet<long> GetGarmentInvoiceId(long id)
		{
			return new HashSet<long>(dbContext.GarmentInvoiceItems.Where(d => d.GarmentInvoice.Id == id).Select(d => d.Id));
		}
		public async Task<int> Update(int id, GarmentInvoice model, string user, int clientTimeZoneOffset = 7)
		{
			int Updated = 0;

			using (var transaction = this.dbContext.Database.BeginTransaction())
			{
				try
				{
					if (model.Items != null)
					{
						double total = 0;
						HashSet<long> detailIds = GetGarmentInvoiceId(id);
						foreach (var itemId in detailIds)
						{
							GarmentInvoiceItem data = model.Items.FirstOrDefault(prop => prop.Id.Equals(itemId));
							if (data == null)
							{
								GarmentInvoiceItem dataItem = dbContext.GarmentInvoiceItems.FirstOrDefault(prop => prop.Id.Equals(itemId));
								EntityExtension.FlagForDelete(dataItem, user, USER_AGENT);
							 
							}
							else
							{
								EntityExtension.FlagForUpdate(data, user, USER_AGENT);
							}

							foreach (GarmentInvoiceItem item in model.Items)
							{
								total += item.TotalAmount;
								if (item.Id <= 0)
								{
									GarmentDeliveryOrder garmentDeliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.DeliveryOrderId);
									if(garmentDeliveryOrder!=null)
									garmentDeliveryOrder.IsInvoice = true ;
									EntityExtension.FlagForCreate(item, user, USER_AGENT);
								}
								else
									EntityExtension.FlagForUpdate(item, user, USER_AGENT);

								foreach (GarmentInvoiceDetail detail in item.Details)
								{
									if (item.Id <= 0)
									{
									EntityExtension.FlagForCreate(detail, user, USER_AGENT);
									}
									else
										EntityExtension.FlagForUpdate(detail, user, USER_AGENT);
								}
							}
						}
					}
					EntityExtension.FlagForUpdate(model, user, USER_AGENT);
					this.dbSet.Update(model);
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
