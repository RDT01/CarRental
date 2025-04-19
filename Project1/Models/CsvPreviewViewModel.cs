using System.Collections.Generic;

namespace Project1.Models
{
    public class CsvPreviewViewModel
    {
        public List<string> Headers { get; set; }
        public List<List<string>> PreviewRows { get; set; }
        
        // Path to the uploaded CSV file stored temporarily on the server
        public string TempFilePath { get; set; }

        // Mapped model properties from user selection
        public List<string> ColumnMappings { get; set; }
    }
}
