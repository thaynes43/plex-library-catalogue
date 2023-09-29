
namespace PlexLibraryCatalogue.Uploaders
{
    using PlexLibraryCatalogue.DataTransferObjects;

    internal interface IUploader
    {
        Task Upload(List<CatalogueFiles> files, CancellationToken cancellationToken);
    }
}
