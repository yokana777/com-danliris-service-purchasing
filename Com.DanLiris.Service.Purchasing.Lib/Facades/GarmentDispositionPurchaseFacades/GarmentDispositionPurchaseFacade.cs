using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDispositionPurchaseModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDispositionPurchase;
using Com.Moonlay.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDispositionPurchaseFacades
{
    public class GarmentDispositionPurchaseFacade : IGarmentDispositionPurchaseFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentDispositionPurchase> dbSet;
        public readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;
        private readonly IMapper mapper;


        public GarmentDispositionPurchaseFacade(PurchasingDbContext dbContext, IServiceProvider serviceProvider, IdentityService identityService,IMapper mapper)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.GarmentDispositionPurchases;
            this.serviceProvider = serviceProvider;
            this.identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
            this.mapper = mapper;
        }

        public async Task<int> Post(FormDto model)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    GarmentDispositionPurchase dataModel = mapper.Map<FormDto, GarmentDispositionPurchase>(model);
                    EntityExtension.FlagForCreate(dataModel, identityService.Username, USER_AGENT);

                    var afterAddModel = dbContext.GarmentDispositionPurchases.Add(dataModel);
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

        public async Task<int> Delete(int id)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var dataModel = dbSet.FirstOrDefault(s => s.Id == id); 
                    
                    EntityExtension.FlagForDelete(dataModel, identityService.Username, USER_AGENT);

                    var afterDeletedModel = dbContext.GarmentDispositionPurchases.Update(dataModel);
                    Deleted = await dbContext.SaveChangesAsync();
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

        public async Task<FormDto> GetFormById(int id)
        {
            var dataModel = await dbSet
                .AsNoTracking()
                    .Include(p => p.GarmentDispositionPurchaseItems)
                        .ThenInclude(p => p.GarmentDispositionPurchaseDetails)
                .Where(d => d.Id.Equals(id))
                .FirstOrDefaultAsync();

            var model = mapper.Map<GarmentDispositionPurchase, FormDto>(dataModel);
            return model;
        }

        public async Task<DispositionPurchaseIndexDto> GetAll(string keyword, int page, int size)
        {
            var dataModel = dbSet
                .AsNoTracking()
                    .Include(p => p.GarmentDispositionPurchaseItems)
                        .ThenInclude(p => p.GarmentDispositionPurchaseDetails).AsQueryable();

            if (keyword != null)
                dataModel = dataModel.Where(s => s.DispositionNo.Contains(keyword) || s.SupplierName.Contains(keyword));

            var countData = dataModel.Count();

            var dataList = await dataModel.ToListAsync();

            var model = mapper.Map<List<GarmentDispositionPurchase>, List<DispositionPurchaseTableDto>>(dataList);

            var indexModel = new DispositionPurchaseIndexDto(model, page, countData);
            return indexModel;
        }

        public async Task<List<FormDto>> ReadByDispositionNo(string dispositionNo, int page, int size)
        {
            var dataModel = dbSet
                .AsNoTracking()
                    .Include(p => p.GarmentDispositionPurchaseItems)
                        .ThenInclude(p => p.GarmentDispositionPurchaseDetails).AsQueryable();

            if (dispositionNo != null)
                dataModel = dataModel.Where(s => s.DispositionNo.Contains(dispositionNo));


            var dataList = await dataModel.OrderBy(s=> s.DispositionNo).ToListAsync();

            var model = mapper.Map<List<GarmentDispositionPurchase>, List<FormDto>>(dataList);
            return model;
        }

        public async Task<int> Update(FormDto model)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    //validation
                    var dataExist = dbSet.FirstOrDefault(s => s.Id == model.Id);
                    if(dataExist== null)
                    {
                        throw new Exception("Data Not Found");
                    } 
                    GarmentDispositionPurchase dataModel = mapper.Map<FormDto, GarmentDispositionPurchase>(model);
                    EntityExtension.FlagForUpdate(dataModel, identityService.Username, USER_AGENT);

                    var afterUpdateModel = dbContext.GarmentDispositionPurchases.Update(dataModel);
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
