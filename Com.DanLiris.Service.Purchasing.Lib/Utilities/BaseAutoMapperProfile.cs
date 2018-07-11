using AutoMapper;

namespace Com.DanLiris.Service.Purchasing.Lib.Utilities
{
    public class BaseAutoMapperProfile : Profile
    {
        public BaseAutoMapperProfile()
        {
            //RecognizePrefixes("_");
            RecognizeAlias("_id", "Id");
        }
    }
}
