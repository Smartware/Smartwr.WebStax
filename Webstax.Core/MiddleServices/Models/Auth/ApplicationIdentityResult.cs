﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartwr.Webstax.Core.MiddleServices.Models.Auth
{
    public class ApplicationIdentityResult
    {
        public IEnumerable<string> Errors
        {
            get;
            private set;
        }

        public bool Succeeded
        {
            get;
            private set;
        }

        public ApplicationIdentityResult(IEnumerable<string> errors, bool succeeded)
        {
            Succeeded = succeeded;
            Errors = errors;
        }
    }
}
