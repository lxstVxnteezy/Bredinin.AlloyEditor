using Bredinin.AlloyEditor.Common.Desktop.Api.Alloys;
using System.Net.Http;
using Refit;

namespace Bredinin.AlloyEditor.Common.Desktop
{
    public static class DependenciesExtensions
    {
        public static IContainerRegistry AddApiServices(this IContainerRegistry container, string baseApiUrl)
        {
            container.Register<HttpClient>(() => new HttpClient()
            {
                BaseAddress = new Uri(baseApiUrl)
            });

            container.RegisterSingleton<IAlloyApiService>(provider =>
                RestService.For<IAlloyApiService>(
                    provider.Resolve<HttpClient>(),
                    new RefitSettings
                    {
                        ContentSerializer = new SystemTextJsonContentSerializer()
                    }
                ));

            return container;
        }

    }
}
