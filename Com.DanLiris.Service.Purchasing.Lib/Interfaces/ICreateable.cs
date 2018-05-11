using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface ICreateable
    {
        Task<int> Create(object model);
    }
}
