using Com.DanLiris.Service.Purchasing.Lib.Models.BankDocumentNumber;
using Com.Moonlay.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Helpers
{
    public class BankDocumentNumberGenerator
    {
        private readonly DbSet<BankDocumentNumber> dbSet;
        private readonly PurchasingDbContext dbContext;
        private readonly string USER_AGENT = "document-number-generator";

        public BankDocumentNumberGenerator(PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<BankDocumentNumber>();
        }

        public async Task<string> GenerateDocumentNumber(string Type, string BankCode, string Username)
        {
            string result = "";
            BankDocumentNumber lastData = await dbSet.Where(w => w.BankCode.Equals(BankCode) && w.Type.Equals(Type)).FirstOrDefaultAsync();

            DateTime Now = DateTime.Now;

            if (lastData == null)
            {

                result = $"{Now.ToString("yy")}{Now.ToString("mm")}{BankCode}001";
                BankDocumentNumber bankDocumentNumber = new BankDocumentNumber()
                {
                    BankCode = BankCode,
                    Type = Type,
                    LastDocumentNumber = 1
                };
                EntityExtension.FlagForCreate(bankDocumentNumber, Username, USER_AGENT);

                dbSet.Add(bankDocumentNumber);
                await dbContext.SaveChangesAsync();
            }
            else
            {
                if (lastData.CreatedUtc.Month != Now.Month)
                {
                    result = $"{Now.ToString("yy")}{Now.ToString("mm")}{BankCode}001";

                    lastData.LastDocumentNumber = 1;
                }
                else
                {
                    lastData.LastDocumentNumber += 1;
                    result = $"{Now.ToString("yy")}{Now.ToString("mm")}{BankCode}{lastData.LastDocumentNumber.ToString().PadLeft(4, '0')}";
                }
                EntityExtension.FlagForUpdate(lastData, Username, USER_AGENT);
                dbContext.Entry(lastData).Property(x => x.LastDocumentNumber).IsModified = true;
                dbContext.Entry(lastData).Property(x => x.LastModifiedAgent).IsModified = true;
                dbContext.Entry(lastData).Property(x => x.LastModifiedBy).IsModified = true;
                dbContext.Entry(lastData).Property(x => x.LastModifiedUtc).IsModified = true;

                await dbContext.SaveChangesAsync();
            }

            return result;
        }
    }
}
