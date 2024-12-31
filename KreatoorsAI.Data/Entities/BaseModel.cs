using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KreatoorsAI.Data.Entities
{
    public class BaseModel
    {
        public int Id { get; set; }
        public bool active { get; set; }
        public DateTime addedOn { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; }
    }
}
