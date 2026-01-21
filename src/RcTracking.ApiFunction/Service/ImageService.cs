using Microsoft.EntityFrameworkCore;
using RcTracking.ApiFunction.Context;
using RcTracking.ApiFunction.Interface;
using RcTracking.Shared.Model;
using System.Drawing;

namespace RcTracking.ApiFunction.Service
{
    public class ImageService : IImageService
    {

        private readonly ImageContext _imageContext;

        public ImageService(ImageContext imageContext)
        {
            _imageContext = imageContext;
        }

        public async Task<ImageModel> AddImageAsync(Guid planeId, Image image, bool isTest)
        {
            var imageString = ResizeImage(image);

            var dbRec = ImageModel.CreateDbRec(planeId, imageString, isTest);
            await _imageContext.Images.AddAsync(dbRec);
            await _imageContext.SaveChangesAsync();
            return dbRec;
        }

        public async Task DeleteImageAsync(Guid id)
        {
            var dbRec = await _imageContext.FindAsync<ImageModel>(id);
            if (dbRec != null)
            {
                _imageContext.Images.Remove(dbRec);
                await _imageContext.SaveChangesAsync();
            }
        }

        public async Task<ImageModel> GetImageAsync(Guid id)
        {
            var dbRec = await _imageContext.FindAsync<ImageModel>(id);
            return dbRec;
        }

        public async Task<IEnumerable<ImageModel>> GetImagesAsync()
        {
            return await _imageContext.Images
                .AsNoTracking()
                .ToListAsync();
        }

        private string ResizeImage(Image image)
        {
            var thumbnail = ImageHelper.ResizeImage(image);
            return ImageHelper.ToBase64String(thumbnail);

        }

        public async Task<ImageModel> UpdateImageAsync(Guid id, Image image)
        {
            var imageString = ResizeImage(image);

            var dbRec = await _imageContext.FindAsync<ImageModel>(id) ?? throw new KeyNotFoundException($"Image with id {id} not found");
            dbRec.UpdateFrom(imageString);
            await _imageContext.SaveChangesAsync();
            return dbRec;
        }
    }
}
