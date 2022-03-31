// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace FantasyCritic.Web.Models.Identity;

public class ConsentViewModel : ConsentInputModel
{
    public ConsentViewModel(string returnUrl) : base(returnUrl)
    {
        IdentityScopes = new List<ScopeViewModel>();
        ApiScopes = new List<ScopeViewModel>();
    }

    public string? ClientName { get; set; }
    public string? ClientUrl { get; set; }
    public string? ClientLogoUrl { get; set; }
    public bool AllowRememberConsent { get; set; }

    public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }
    public IEnumerable<ScopeViewModel> ApiScopes { get; set; }
}
