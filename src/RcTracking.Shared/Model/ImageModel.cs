using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;

namespace RcTracking.Shared.Model
{
    public class ImageModel(Guid Id, Guid PlaneId, string ImageString)
    {
        [Required]
        public Guid Id { get; init; } = Id;
        [Required]
        public Guid PlaneId { get; init; } = PlaneId;
        [Required]
        public string ImageString { get; set; } = ImageString;

        public static ImageModel CreateDbRec(Guid planeId, string imageString) => new(Guid.NewGuid(), planeId, imageString);

        public void UpdateFrom(ImageModel imageModel)
        {
            ImageString = imageModel.ImageString;
        }

        public void UpdateFrom(string imageString)
        {
            ImageString = imageString;
        }
    }
}
