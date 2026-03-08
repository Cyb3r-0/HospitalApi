using AutoMapper;
using HospitalApi.Dtos;
using HospitalApi.Models;

namespace HospitalApi.Mapping
{
    public class BillProfile : Profile
    {
        public BillProfile()
        {
            CreateMap<BillCreateDto, Bill>();
            CreateMap<BillUpdateDto, Bill>();

            CreateMap<Bill, BillDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.Name))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.Name));
        }
    }
}