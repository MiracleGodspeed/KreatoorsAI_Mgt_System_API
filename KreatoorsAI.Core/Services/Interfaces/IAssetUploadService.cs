﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Core.Services.Interfaces
{
    public interface IAssetUploadService
    {
        Task<string> GetFileUploadLink(IFormFile file, string filePath, string directory, string givenFileName);
    }
}
