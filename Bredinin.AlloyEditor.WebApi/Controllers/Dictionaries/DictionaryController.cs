using Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.Handlers.Methods.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.WebAPI.Controllers.Dictionaries
{
    [Route("api/dictionary")]
    public class DictionaryController : BaseApiController
    {
        [HttpGet("/chemical-elements")]
        public Task<DictChemicalElementResponse[]?> GetChemicalElements(
            [FromServices] IInfoDictChemicalElementsHandler handler,
            CancellationToken ctn)
        {
            return handler.Handle(ctn);
        }
    }
}
