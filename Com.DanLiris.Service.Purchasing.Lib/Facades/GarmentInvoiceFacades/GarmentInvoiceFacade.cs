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

		public async Task<int> Update(int id, GarmentInvoice model, string user)
		{
			int Updated = 0;

			using (var transaction = this.dbContext.Database.BeginTransaction())
			{
				try
				{
					var oldM = this.dbSet.AsNoTracking()
						  .Include(d => d.Items)
						  .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

					var existingModel = this.dbSet.AsNoTracking()
						.Include(d => d.Items)
							.ThenInclude(d => d.Details)
						.SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

					if (existingModel != null && id == model.Id)
					{
						EntityExtension.FlagForUpdate(model, user, USER_AGENT);

						foreach (var item in model.Items.ToList())
						{
							var existingItem = existingModel.Items.SingleOrDefault(m => m.Id == item.Id);
							EntityExtension.FlagForCreate(item, user, USER_AGENT); 
						}
						foreach (var oldItem in oldM.Items)
						{
							var newItem = model.Items.FirstOrDefault(i => i.Id.Equals(oldItem.Id));
							if (newItem == null)
							{
								//GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(oldItem.POId));
								//internalPurchaseOrder.IsPosted = false;

								GarmentDeliveryOrder garmentDeliveryOrder = this.dbContext.GarmentDeliveryOrders.FirstOrDefault(a => a.Id.Equals(oldItem.DeliveryOrderId));
								garmentDeliveryOrder.IsInvoice = false;
								EntityExtension.FlagForDelete(oldItem, user, USER_AGENT);
								dbContext.GarmentInvoiceItems.Update(oldItem);
							}
						}

						this.dbContext.Update(model);
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

		public Task<int> Update(int id, GarmentInvoice m, string user, int clientTimeZoneOffset = 7)
		{
			throw new NotImplementedException();
		}

	
	}
}
