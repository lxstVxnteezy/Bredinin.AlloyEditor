using Bredinin.AlloyEditor.Contracts.Common;
using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;

public interface ISearchAlloyGradeHandler : IHandler
{
    Task<PagedResponse<AlloyGradeResponse>> Handle(
        string? search = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ctn = default);
}

internal class SearchAlloyGradeHandler(ServiceDbContext context)
    : ISearchAlloyGradeHandler
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public async Task<PagedResponse<AlloyGradeResponse>> Handle(
        string? search = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ctn = default)
    {
        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = context.AlloyGrades
            .AsNoTracking()
            .Include(x => x.ChemicalCompositions)
            .Include(x => x.HeatTreatments)
                .ThenInclude(ht => ht.HeatTreatmentType)
            .Include(x => x.MechanicalProperties)
                .ThenInclude(mp => mp.PropertyType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.Contains(search) ||
                (x.Description != null && x.Description.Contains(search))
            );
        }

        var totalCount = await query.CountAsync(ctn);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .Select(x => new AlloyGradeResponse(
                x.Id,
                x.Name,
                x.Description,
                x.AlloySystemId,
                x.ChemicalCompositions.Select(cc => new ChemicalCompositionsDto(
                    cc.Id,
                    cc.MinValue,
                    cc.MaxValue,
                    cc.ExactValue,
                    cc.ChemicalElementId
                )).ToArray(),
                x.HeatTreatments.Select(ht => new HeatTreatmentDto(
                    ht.Id,
                    ht.HeatTreatmentTypeId,
                    ht.HeatTreatmentType.Name,
                    ht.TemperatureMin,
                    ht.TemperatureMax,
                    ht.TemperatureExact,
                    ht.HoldingTimeMin,
                    ht.HoldingTimeMax,
                    ht.HoldingTimeExact,
                    ht.CoolingMedium,
                    ht.Description,
                    ht.StepOrder,
                    ht.IsDefault
                )).ToArray(),
                x.MechanicalProperties.Select(mp => new MechanicalPropertyDto(
                    mp.Id,
                    mp.PropertyTypeId,
                    mp.PropertyType.Name,
                    mp.PropertyType.Unit,
                    mp.PropertyType.Symbol,
                    mp.ValueMin,
                    mp.ValueMax,
                    mp.ValueExact,
                    mp.Condition,
                    mp.Source,
                    mp.HeatTreatmentId
                )).ToArray()
            ))
            .ToArrayAsync(ctn);

        return new PagedResponse<AlloyGradeResponse>(
            Items: items,
            TotalCount: totalCount,
            Page: validPage,
            PageSize: validPageSize,
            TotalPages: (int)Math.Ceiling(totalCount / (double)validPageSize)
        );
    }
}