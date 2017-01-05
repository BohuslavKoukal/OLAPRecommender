using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Common.Enums;

namespace Recommender.Common.Helpers
{
    public static class FileTypeExtension
    {
        public static FileType GetFileType(this string filePath)
        {
            return filePath.EndsWith(".csv") ? FileType.Csv : FileType.Ttl;
        }
    }
}
