using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace Recommender.Common.Constants
{
    public static class Constants
    {
        public static class String
        {
            public const string Value = "Value_";
            public const string Id = "Id";
            public const string FactTable = "FactTable";
            public const string View = "View";
        }
    }

    public static class Roles
    {
        public const string RoleUser = "User";
    }
}
