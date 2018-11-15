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
			throw new NotImplementedException();
		}

		public GarmentBeacukai ReadById(int id)
		{
			throw new NotImplementedException();
		}

		public async Task<int> Create(GarmentBeacukai model, string username, int clientTimeZoneOffset = 7)
		{
			int Created = 0;

			using (var transaction = this.dbContext.Database.BeginTransaction())
			{
				try
				{

					EntityExtension.FlagForCreate(model, username, USER_AGENT);

					foreach (var item in model.Items)
					{
						GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.GarmentDOId);
						if (deliveryOrder != null)
							deliveryOrder.IsInvoice = true;
						EntityExtension.FlagForCreate(item, username, USER_AGENT);
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
						GarmentDeliveryOrder garmentDeliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.GarmentDOId);
						if (garmentDeliveryOrder != null)
							garmentDeliveryOrder.IsInvoice = false;
						EntityExtension.FlagForDelete(item, username, USER_AGENT);

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

		public Task<int> Update(int id, GarmentBeacukai m, string user, int clientTimeZoneOffset = 7)
		{
			throw new NotImplementedException();
		}
	}
}