using MongoDB.Bson;
using System;

namespace Com.DanLiris.Service.Purchasing.Mongo.Lib.MongoModels
{
    public class MongoBaseModel
    {
        public ObjectId _id { get; set; }
        public string _stamp { get; set; }
        public string _type { get; set; }
        public string _version { get; set; }
        public bool _active { get; set; }
        public bool _deleted { get; set; }
        public string _createdBy { get; set; }
        public DateTime _createdDate { get; set; }
        public string _createAgent { get; set; }
        public string _updatedBy { get; set; }
        public DateTime _updatedDate { get; set; }
        public string _updateAgent { get; set; }
    }
}
