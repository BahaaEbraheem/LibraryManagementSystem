using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.DAL.Data
{
    /// <summary>
    /// معلومات صحة قاعدة البيانات
    /// Database health information
    /// </summary>

    public class DatabaseHealthInfo
    {
        public bool IsHealthy { get; set; }
        public long ResponseTimeMs { get; set; }
        public string? ServerVersion { get; set; }
        public string? Database { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CheckTime { get; set; }
    }
}
