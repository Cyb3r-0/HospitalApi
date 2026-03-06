using AutoMapper;
using HospitalApi.Dtos;
using HospitalApi.Models;

namespace HospitalApi.Mapping
{
    public class AppointmentProfile : Profile
    {
        public AppointmentProfile()
        {
            CreateMap<AppointmentCreateDto, Appointment>();
            CreateMap<AppointmentUpdateDto, Appointment>();

            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.Name))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.Name))
                .ForMember(dest => dest.DoctorSpecialization, opt => opt.MapFrom(src => src.Doctor.Specialization));
        }
    }
}