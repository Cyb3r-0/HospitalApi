using AutoMapper;
using HospitalApi.Dtos;
using HospitalApi.Models;

namespace HospitalApi.Mapping
{
    public class PatientProfile : Profile
    {
        public PatientProfile()
        {
            CreateMap<Patient, PatientDto>();

            CreateMap<PatientCreateDto, Patient>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<PatientUpdateDto, Patient>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}
