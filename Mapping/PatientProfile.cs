using AutoMapper;
using HospitalApi.Dtos;
using HospitalApi.Models;

namespace HospitalApi.Mapping
{
    public class PatientProfile : Profile
    {
        public PatientProfile()
        {
            CreateMap<PatientCreateDto, Patient>();
            CreateMap<Patient, PatientDto>();
            CreateMap<PatientUpdateDto, Patient>();
        }
    }
}
