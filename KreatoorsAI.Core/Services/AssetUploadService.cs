using KreatoorsAI.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Core.Services
{
    public class AssetUploadService : IAssetUploadService
    {
        public async Task<string> GetFileUploadLink(IFormFile file, string filePath, string directory, string givenFileName)
        {

            var noteUrl = string.Empty;
            //Define allowed property of the uploaded file

            var validFileSize = 5 * 1024 * 1024; // 5 MB in bytes

            List<string> validFileExtension = new List<string>();
            validFileExtension.Add(".jpeg");
            validFileExtension.Add(".jpg");
            validFileExtension.Add(".png");
            if (file.Length > 0)
            {

                var extType = Path.GetExtension(file.FileName);
                var fileSize = file.Length;
                if (fileSize <= validFileSize)
                {

                    if (validFileExtension.Contains(extType))
                    {
                        string fileName = string.Format("{0}{1}", givenFileName.Replace("/", "_") + "_" + DateTime.Now.Millisecond, extType);
                        //create file path if it doesnt exist
                        if (!Directory.Exists(filePath))
                        {
                            Directory.CreateDirectory(filePath);
                        }
                        var fullPath = Path.Combine(filePath, fileName);
                        noteUrl = Path.Combine(directory, fileName);
                        //Delete if file exist
                        FileInfo fileExists = new FileInfo(fullPath);
                        if (fileExists.Exists)
                        {
                            fileExists.Delete();
                        }

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        return noteUrl = noteUrl.Replace('\\', '/');
                    }
                    else
                    {
                        throw new Exception("File format is not supported");
                    }
                }
                else
                    throw new Exception("File size is above the allowed size(1MB)");
            }
            return noteUrl;
        }

    }

}
