
using PlexLibraryCatalogue.DataTransferObjects;

namespace PlexLibraryCatalogue.Collectors
{
    internal interface IDataCollector
    {
        Task<List<CatalogueFiles>> CollectData(CancellationToken cancellationToken);
    }
}
