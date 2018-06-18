// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;

namespace identity
{
    public class LoginViewModel : LoginInputModel
    {
        public bool AllowRememberLogin { get; set; }
        public bool EnableLocalLogin { get; set; }

        public IEnumerable<ExternalProvider> IdentityProviders { get; set; }
        public IEnumerable<ExternalProvider> VisibleIdentityProviders => Enumerable.Where(IdentityProviders, x => !String.IsNullOrWhiteSpace(x.DisplayName));

        public bool IsExternalLoginOnly => EnableLocalLogin == false && IdentityProviders?.Count() == 1;
        public string ExternalLoginScheme => IdentityProviders?.SingleOrDefault()?.AuthenticationScheme;
    }
}