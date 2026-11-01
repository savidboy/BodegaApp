using AutoMapper;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Core;
using BodegaApp.Core.Pagination;
using BodegaApp.Data;
using BodegaApp.Domain.Entities;
using BodegaApp.DTOs.Supplier;
using BodegaApp.Services.Abstractions;

namespace BodegaApp.Services.Implementations;

public class SuppliersService : CustomQueryableOperationsService, ISuppliersService
{
    private readonly BodegaDbContext _context;

    public SuppliersService(
        BodegaDbContext context,
        IMapper mapper,
        ILogger<CustomQueryableOperationsService> logger) : base(context, mapper, logger)
    {
        _context = context;
    }

    public async Task<Response<PaginationResponse<SupplierDTO>>> GetPaginationAsync(SupplierPaginationRequest request)
    {
        IQueryable<Supplier> query = Visible(request.IncludeInactive).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            string filter = request.Filter.Trim().ToLower();

            query = query.Where(s => s.Name.ToLower().Contains(filter)
                || (s.Contact != null && s.Contact.ToLower().Contains(filter))
                || (s.Email != null && s.Email.ToLower().Contains(filter)));
        }

        Response<PaginationResponse<SupplierDTO>> response =
            await GetPagedListAsync<SupplierDTO, Supplier>(request, query.OrderBy(s => s.Name));

        if (response.Result is not null)
        {
            response.Result.RouteValues = new Dictionary<string, string>
            {
                ["filter"] = request.Filter?.Trim() ?? string.Empty,
                ["includeInactive"] = request.IncludeInactive ? "true" : "false",
                ["recordsPerPage"] = request.RecordsPerPage.ToString()
            };
        }

        return response;
    }

    public async Task<Response<SupplierDTO>> GetOneAsync(int id, bool includeInactive)
    {
        return await GetOneAsync<SupplierDTO, Supplier>(id, Visible(includeInactive));
    }

    public async Task<Response<CreateSupplierDTO>> CreateAsync(CreateSupplierDTO dto)
    {
        return await CreateAsync<CreateSupplierDTO, Supplier>(dto);
    }

    public async Task<Response<SupplierDTO>> UpdateAsync(UpdateSupplierDTO dto, bool includeInactive)
    {
        try
        {
            Supplier? supplier = await Visible(includeInactive).FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (supplier is null)
            {
                return Response<SupplierDTO>.Failure($"No existe el proveedor con id {dto.Id}.");
            }

            Mapper.Map(dto, supplier);
            await _context.SaveChangesAsync();

            return Response<SupplierDTO>.Success(
                Mapper.Map<SupplierDTO>(supplier),
                "Proveedor actualizado correctamente.");
        }
        catch (Exception ex)
        {
            return Fail<SupplierDTO>("No se pudo actualizar el proveedor.", ex);
        }
    }

    public async Task<Response> DeactivateAsync(int id, bool includeInactive)
    {
        try
        {
            Supplier? supplier = await Visible(includeInactive).FirstOrDefaultAsync(s => s.Id == id);

            if (supplier is null)
            {
                return Response.Failure($"No existe el proveedor con id {id}.");
            }

            if (!supplier.IsActive)
            {
                return Response.Failure("El proveedor ya está inactivo.");
            }

            supplier.IsActive = false;
            await _context.SaveChangesAsync();

            return Response.Success("Proveedor desactivado correctamente.");
        }
        catch (Exception ex)
        {
            return Fail("No se pudo desactivar el proveedor.", ex);
        }
    }

    public async Task<Response> DeleteAsync(int id)
    {
        return await DeleteAsync<Supplier>(id);
    }

    private IQueryable<Supplier> Visible(bool includeInactive)
    {
        return includeInactive
            ? _context.Suppliers
            : _context.Suppliers.Where(s => s.IsActive);
    }
}
