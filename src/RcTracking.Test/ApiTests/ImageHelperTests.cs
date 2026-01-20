

using RcTracking.ApiFunction.Interface;
using RcTracking.ApiFunction.Service;
using System.IO;
using System.Drawing;

namespace RcTracking.Test
{
    [TestFixture]
    public class ImageHelperTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ToBase64String_ReturnsBase64_FromJpgFile()
        {
            // Arrange - locate the test image file that is included in the test project
            var fileName = "PXL_20220901_144727125.jpg";
            var filePath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "TestFiles", fileName));
            Assert.That(File.Exists(filePath), Is.True, $"Test file not found: {filePath}");

            using var image = Image.FromFile(filePath);

            // Act
            var base64 = ImageHelper.ToBase64String(image);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(base64, Is.Not.Null.And.Not.Empty);

                // Validate that the base64 decodes to an image byte array
                var bytes = Convert.FromBase64String(base64);
                Assert.That(bytes.Length, Is.GreaterThan(0));

                using var ms = new MemoryStream(bytes);
                using var decoded = Image.FromStream(ms);
                Assert.That(decoded, Is.Not.Null);
                // Basic sanity check: decoded should have positive dimensions
                Assert.That(decoded.Width, Is.GreaterThan(0));
                Assert.That(decoded.Height, Is.GreaterThan(0));
            }
        }

        [Test]
        public void ResizeImage_ReturnsThumbnail_FromJpgFile()
        {
            // Arrange - locate the test image file that is included in the test project
            var fileName = "PXL_20220901_144727125.jpg";
            var filePath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "TestFiles", fileName));
            Assert.That(File.Exists(filePath), Is.True, $"Test file not found: {filePath}");

            var image = Image.FromFile(filePath);

            // Act
            var result = ImageHelper.ResizeImage(image);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(300, Is.EqualTo(result.Width), "Thumbnail width should be 300");
                Assert.That(300, Is.EqualTo(result.Height), "Thumbnail height should be 300");
            }
        }
    }
}
