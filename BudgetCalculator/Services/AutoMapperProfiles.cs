using AutoMapper;
using BudgetCalculator.Models;

namespace BudgetCalculator.Services
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Acount, CreateAcountViewModel>();
            CreateMap<UpdateTransactionViewModel, Transaction>().ReverseMap();
        }
    }
}
