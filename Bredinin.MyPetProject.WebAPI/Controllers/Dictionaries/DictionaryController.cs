using Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Bredinin.MyPetProject.Handlers.Methods.Dictionaries.DictChemicalElements;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.WebAPI.Controllers.Dictionaries
{
    [Microsoft.AspNetCore.Components.Route("api/dictionary")]
    public class DictionaryController : BaseApiController
    {
        [HttpGet("chemical_elements")]
        public Task<DictChemicalElementResponse[]> GetChemicalElements(
            [FromServices] IInfoDictChemicalElementsHandler handler,
            CancellationToken ctn)
        {
            return handler.Handle(ctn);
        }
    }
}
