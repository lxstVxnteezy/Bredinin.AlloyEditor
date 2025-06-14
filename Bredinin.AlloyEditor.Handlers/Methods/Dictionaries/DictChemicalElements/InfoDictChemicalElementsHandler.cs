﻿using Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.Domain.Dictionaries;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Dictionaries.DictChemicalElements
{
    public interface IInfoDictChemicalElementsHandler : IHandler
    {
        public Task<DictChemicalElementResponse[]> Handle(CancellationToken ctn = default);
    }

    internal class InfoDictChemicalElementsHandler(IRepository<DictChemicalElement> dictChemicalElementRepository)
        : IInfoDictChemicalElementsHandler
    {

        public async Task<DictChemicalElementResponse[]> Handle(CancellationToken ctn = default)
        {
            var chemicalElements = await dictChemicalElementRepository.Query.ToArrayAsync(ctn);

            return chemicalElements.Select(MapToResponse).ToArray();
        }

        private static DictChemicalElementResponse MapToResponse(DictChemicalElement chemicalElement)
        {
            return new DictChemicalElementResponse(
                Id: chemicalElement.Id,
                Name: chemicalElement.Name,
                Symbol: chemicalElement.Symbol,
                Description: chemicalElement.Description,
                IsBaseForAlloySystem: chemicalElement.IsBaseForAlloySystem,
                AtomicNumber: chemicalElement.AtomicNumber,
                AtomicWeight: chemicalElement.AtomicWeight,
                Group: chemicalElement.Group,
                Period: chemicalElement.Period,
                Density: chemicalElement.Density
            );
        }
    }
}
