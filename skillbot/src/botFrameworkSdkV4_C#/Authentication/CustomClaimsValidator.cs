using Microsoft.Bot.Connector.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReneeSampleSkillBot.Authentication
{
    public class CustomClaimsValidator : ICredentialProvider
    {
        public Task<string> GetAppPasswordAsync(string appId)
        {
            return Task.FromResult("jBg~H8Om-z345.VLx~90R_mcX72~hn_IX-");
        }

        public Task<bool> IsAuthenticationDisabledAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsValidAppIdAsync(string appId)
        {
            return Task.FromResult(true);
        }

    }
}
