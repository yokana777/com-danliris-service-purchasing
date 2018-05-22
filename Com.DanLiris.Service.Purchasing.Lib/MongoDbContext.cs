using MongoDB.Bson;
using MongoDB.Driver;

namespace Com.DanLiris.Service.Purchasing.Lib
{
    public class MongoDbContext
    {
        public static string connectionString;

        private readonly IMongoDatabase database;

        public MongoDbContext()
        {
            MongoUrl mongoUrl = new MongoUrl(connectionString);

            MongoClientSettings mongoClientSettings = new MongoClientSettings()
            {
                Server = mongoUrl.Server,
                Credential = MongoCredential.CreateCredential(mongoUrl.DatabaseName, mongoUrl.Username, mongoUrl.Password),
                UseSsl = mongoUrl.UseSsl,
                VerifySslCertificate = false
            };

            MongoClient mongoClient = new MongoClient(mongoClientSettings);

            database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
        }

        public IMongoCollection<BsonDocument> UnitReceiptNote => database.GetCollection<BsonDocument>("unit-receipt-notes");
        public IMongoCollection<BsonDocument> UnitPaymentOrder => database.GetCollection<BsonDocument>("unit-payment-orders");
    }
}