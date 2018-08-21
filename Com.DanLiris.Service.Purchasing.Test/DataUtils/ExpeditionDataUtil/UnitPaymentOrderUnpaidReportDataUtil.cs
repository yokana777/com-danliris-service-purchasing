using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using MongoDB.Bson;
using System;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.ExpeditionDataUtil
{
    public class UnitPaymentOrderUnpaidReportDataUtil
    {
        private readonly UnitPaymentOrderUnpaidReportFacade Facade;

        public UnitPaymentOrderUnpaidReportDataUtil(UnitPaymentOrderUnpaidReportFacade Facade)
        {
            this.Facade = Facade;
        }

        BsonDocument GetNewData()
        {
            return new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "_deleted", false },
                { "no", "123456789" },
                { "date", new BsonDateTime (new DateTime(2018, 8, 1)) },
                { "currency", new BsonDocument
                    {
                        { "code", "IDR" }
                    }
                },
                { "supplier", new BsonDocument
                    {
                        { "name", "suplie name" },
                        { "code", "S1" },
                    }
                },
                { "invoceNo", "123456" },
                { "dueDate", new BsonDateTime (new DateTime(2018, 8, 1)) },
                { "items", new BsonArray
                    {
                        new BsonDocument
                        {
                            { "unitReceiptNote", new BsonDocument
                                {
                                    { "unit", new BsonDocument
                                        {
                                            { "code", "U1" },
                                            { "name", "Unit1" }
                                        }
                                    },
                                    { "items", new BsonArray
                                        {
                                            new BsonDocument
                                            {
                                                { "product", new BsonDocument
                                                    {
                                                        { "code", "P1" },
                                                        { "name", "Produk1" }
                                                    }
                                                },
                                                { "deliveredQuantity", 200 },
                                            }
                                        }
                                    }
                                }
                            },
                        }
                    }
                }
            };
        }

        public BsonDocument GetTestData()
        {
            BsonDocument data = GetNewData();
            this.Facade.DeleteDataMongoByNo(data["no"].AsString);
            this.Facade.InsertToMongo(data);

            return data;
        }
    }
}
