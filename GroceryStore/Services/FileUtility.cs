using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryStore.Services
{
    public class FileUtility
    {
        private const string IMAGE_CONTENT_TYPE = "image";
        private const char IMAGE_CONTENT_TYPE_DIVIDER = '/';

        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        private static readonly HashSet<string> supportedImageTypes = new HashSet<string>(2) { "jpeg", "png" };  // image types supported by SixLabors ImageSharp plugin and common browsers

        public FileUtility(IConfiguration configuration, IHostingEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public static long MaxImageSizeInBinaryBytes => 9437184;
        public static decimal MaxImageSizeInBinaryMegabytes => MaxImageSizeInBinaryBytes / 1024m / 1024m; 

        public static string GetSupportedImageTypesAsString()
        {
            HashSet<string> imageTypes = supportedImageTypes;
            string result = string.Empty;

            for (int i = 0; i < imageTypes.Count; i++)
            {
                result += imageTypes.ElementAt(i);

                if (i < imageTypes.Count - 1)
                {
                    result += ", ";
                }
            }

            return result;
        }

        public static bool IsImageSupported(IFormFile file)
        {
            string[] imageTypeParts = file.ContentType.Split(IMAGE_CONTENT_TYPE_DIVIDER);

            return file != null && imageTypeParts.First() == IMAGE_CONTENT_TYPE && supportedImageTypes.Contains(imageTypeParts.Last());
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null)
            {
                throw new NullReferenceException("Image file cannot be null.");
            }

            if (!IsImageSupported(file))
            {
                throw new FormatException("The specified file is not an image.");
            }

            if (file.Length == 0)
            {
                throw new ApplicationException("The specified file is empty. Only files with content inside can be uploaded.");
            }
            else if (file.Length > MaxImageSizeInBinaryBytes)
            {
                throw new ApplicationException("The specified file is too large.");
            }

            string fileName = $"{file.Length.ToString()}.{file.FileName.Split('.').Last()}";
            string filePath = Path.Combine(_env.ContentRootPath, _configuration.GetSection("UploadLocation").Value, fileName);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            Image<Rgba32> image = Image.Load(filePath);

            int imageWidth = Convert.ToInt32(_configuration.GetSection("ImageWidth").Value);
            int imageHeight = Convert.ToInt32(_configuration.GetSection("ImageHeight").Value);

            image.Mutate(i => i.AutoOrient().Resize(imageWidth, imageHeight));
            image.Save(filePath);

            return $"{_configuration.GetSection("UploadLocation").Value}/{fileName}"; // the path relative to the host
        }
    }
}
