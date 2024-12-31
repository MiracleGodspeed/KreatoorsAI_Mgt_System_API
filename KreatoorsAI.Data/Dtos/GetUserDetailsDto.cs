using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Data.Dtos
{
#pragma warning disable
    public class GetUserDetailsDto
    {
        public string emailaddress { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string profileImage { get; set; }
    }

    public class UpdateProfileDto
    {
        public string emailaddress { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public IFormFile? ImageFile { get; set; }


    }
}
