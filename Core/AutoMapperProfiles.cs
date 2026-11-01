using AutoMapper;
using BodegaApp.Domain.Entities;
using BodegaApp.DTOs.Supplier;

namespace BodegaApp.Core;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Supplier, SupplierDTO>();
        CreateMap<CreateSupplierDTO, Supplier>();
        CreateMap<UpdateSupplierDTO, Supplier>()
            .ForMember(destination => destination.Id, options => options.Ignore());
    }
}
