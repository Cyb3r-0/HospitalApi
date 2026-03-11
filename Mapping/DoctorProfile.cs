using AutoMapper;
using HospitalApi.Dtos;
using HospitalApi.Models;

namespace HospitalApi.Mapping
{
    public class DoctorProfile : Profile
    {
        public DoctorProfile()
        {
            CreateMap<DoctorCreateDto, Doctor>();
            CreateMap<Doctor, DoctorDto>();
            CreateMap<DoctorUpdateDto, Doctor>();
        }
    }
}
