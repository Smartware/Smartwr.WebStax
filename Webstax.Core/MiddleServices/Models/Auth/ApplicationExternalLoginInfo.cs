using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Smartwr.Webstax.Core.MiddleServices.Models.Auth
{
    public class ApplicationExternalLoginInfo
    {
        public string DefaultUserName
        {
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public ClaimsIdentity ExternalIdentity
        {
            get;
            set;
        }

        public ApplicationUserLoginInfo Login
        {
            get;
            set;
        }
    }
}
