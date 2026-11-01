using AutoMapper;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Core;
using BodegaApp.Core.Pagination;
using BodegaApp.Data;
using BodegaApp.Data.Abstractions;

namespace BodegaApp.Services;

public class CustomQueryableOperationsService
{
    protected readonly BodegaDbContext Context;
    protected readonly IMapper Mapper;
    private readonly ILogger<CustomQueryableOperationsService> _logger;

    public CustomQueryableOperationsService(
        BodegaDbContext context,
        IMapper mapper,
        ILogger<CustomQueryableOperationsService> logger)
    {
        Context = context;
        Mapper = mapper;
        _logger = logger;
    }

    protected async Task<Response<TDto>> CreateAsync<TDto, TEntity>(TDto dto) where TEntity : class, IId
    {
        try
        {
            TEntity entity = Mapper.Map<TEntity>(dto);

            Context.Set<TEntity>().Add(entity);
            await Context.SaveChangesAsync();

            return Response<TDto>.Success(dto, "Registro creado correctamente.");
        }
        catch (Exception ex)
        {
            return Fail<TDto>("No se pudo crear el registro.", ex);
        }
    }

    protected async Task<Response> DeleteAsync<TEntity>(int id) where TEntity : class, IId
    {
        try
        {
            TEntity? entity = await Context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
            {
                return Response.Failure($"No existe el registro con id {id}.");
            }

            Context.Set<TEntity>().Remove(entity);
            await Context.SaveChangesAsync();

            return Response.Success("Registro eliminado correctamente.");
        }
        catch (Exception ex)
        {
            return Fail("No se pudo eliminar el registro.", ex);
        }
    }

    protected async Task<Response<TDto>> GetOneAsync<TDto, TEntity>(int id, IQueryable<TEntity>? query = null)
        where TEntity : class, IId
    {
        try
        {
            query ??= Context.Set<TEntity>();

            TEntity? entity = await query.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
            {
                return Response<TDto>.Failure($"No existe el registro con id {id}.");
            }

            return Response<TDto>.Success(Mapper.Map<TDto>(entity));
        }
        catch (Exception ex)
        {
            return Fail<TDto>("No se pudo obtener el registro.", ex);
        }
    }

    protected async Task<Response<PaginationResponse<TDto>>> GetPagedListAsync<TDto, TEntity>(
        PaginationRequest request,
        IQueryable<TEntity>? query = null)
        where TEntity : class
        where TDto : class
    {
        try
        {
            query ??= Context.Set<TEntity>().AsNoTracking();

            PagedList<TEntity> page = await PagedList<TEntity>.ToPagedListAsync(query, request);

            PaginationResponse<TDto> response = new()
            {
                CurrentPage = page.CurrentPage,
                TotalPages = page.TotalPages,
                RecordsPerPage = page.RecordsPerPage,
                TotalCount = page.TotalCount,
                Filter = request.Filter,
                List = new PagedList<TDto>(
                    Mapper.Map<List<TDto>>(page),
                    page.TotalCount,
                    page.CurrentPage,
                    page.RecordsPerPage)
            };

            return Response<PaginationResponse<TDto>>.Success(response);
        }
        catch (Exception ex)
        {
            return Fail<PaginationResponse<TDto>>("No se pudo obtener la lista.", ex);
        }
    }

    protected Response<T> Fail<T>(string message, Exception exception)
    {
        _logger.LogError(exception, "{Message}", message);
        return Response<T>.Failure(message);
    }

    protected Response Fail(string message, Exception exception)
    {
        _logger.LogError(exception, "{Message}", message);
        return Response.Failure(message);
    }
}
