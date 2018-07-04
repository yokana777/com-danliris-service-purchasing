using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Helpers
{
    public class BankDocumentNumberGeneratorTest
    {
        private const string ENTITY = "BankDocumentNumberGenerator";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return string.Concat(sf.GetMethod().Name, "_", ENTITY);
        }

        private PurchasingDbContext _dbContext(string testName)
        {
            DbContextOptionsBuilder<PurchasingDbContext> optionsBuilder = new DbContextOptionsBuilder<PurchasingDbContext>();
            optionsBuilder
                .UseInMemoryDatabase(testName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            PurchasingDbContext dbContext = new PurchasingDbContext(optionsBuilder.Options);

            return dbContext;
        }

        [Fact]
        public async void Should_Success_Get_Data()
        {
            BankDocumentNumberGenerator generator = new BankDocumentNumberGenerator(_dbContext(GetCurrentMethod()));
            string result = await generator.GenerateDocumentNumber("test", "test", "test");
            Assert.NotNull(result);
        }
    }
}
