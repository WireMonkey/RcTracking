using RcTracking.Shared.Model;
using System.Drawing;

namespace RcTracking.ApiFunction.Interface
{
    public interface IImageService
    {
        public Task<ImageModel> AddImageAsync(Guid planeId, Image image);
        public Task<ImageModel> UpdateImageAsync(Guid id, Image image);
        public Task DeleteImageAsync(Guid id);
        public Task<ImageModel> GetImageAsync(Guid id);
        public Task<IEnumerable<ImageModel>> GetImagesAsync();
    }
}
