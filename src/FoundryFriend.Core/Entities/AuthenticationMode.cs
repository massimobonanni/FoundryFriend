using System;
using System.Collections.Generic;
using System.Text;

namespace FoundryFriend.Core.Entities;

/// <summary>
/// Specifies the authentication mode used to connect to an Azure AI Foundry resource.
/// </summary>
public enum AuthenticationMode
{
    /// <summary>
    /// Authentication is performed using a managed identity or service principal
    /// (Microsoft Entra ID / Azure Active Directory).
    /// </summary>
    Identity,

    /// <summary>
    /// Authentication is performed using an API key associated with the resource.
    /// </summary>
    Key
}
