using AutoMapper;
using HospitalApi.Dtos;
using HospitalApi.Models;

namespace HospitalApi.Mapping
{
    public class PaymentEventProfile : Profile
    {
        public PaymentEventProfile()
        {
            CreateMap<PaymentEvent, PaymentEventDto>();
        }
    }
}