using Microsoft.AspNetCore.Components.Forms;
using RcTracking.Shared.Model;

namespace RcTracking.UI.Interface
{
    public interface IImageService
    {
        Dictionary<Guid, ImageModel> Images { get; }
        bool HasLoaded { get; }
        Task LoadImages();
        Task AddImage(Guid planeId, IBrowserFile image);
        Task UpdateImage(Guid id, IBrowserFile image);
        Task DeleteImage(Guid id);

    }
}
