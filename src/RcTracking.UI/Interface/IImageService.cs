using Microsoft.AspNetCore.Components.Forms;
using RcTracking.Shared.Model;

namespace RcTracking.UI.Interface
{
    public interface IImageService
    {
        Dictionary<Guid, ImageModel> Images { get; }
        bool HasLoaded { get; }
        Task LoadImages();
        Task AddImage(Guid planeId, byte[] image, string fileName);
        Task DeleteImage(Guid id);

    }
}
