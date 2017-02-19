﻿using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using SelfHost.Logging;
using System.Threading.Tasks;

namespace SelfHost.Extensions
{
    class CustomGrantValidator : ICustomGrantValidator
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IUserService _users;

        public CustomGrantValidator(IUserService users)
        {
            _users = users;
        }

        public async Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            var legacyAccountStoreType = request.Raw.Get("account_store");
            var id = request.Raw.Get("legacy_id");
            var secret = request.Raw.Get("legacy_secret");

            if (string.IsNullOrWhiteSpace(legacyAccountStoreType) ||
                string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(secret))
            {
                Logger.Error("malformed request");
                return null;
            }

            var message = new SignInMessage { Tenant = legacyAccountStoreType };
            var context = new LocalAuthenticationContext
            {
                UserName = id, Password = secret,
                SignInMessage = message
            };
            await _users.AuthenticateLocalAsync(context);

            var result = context.AuthenticateResult;
            if (result.IsError)
            {
                Logger.Error("authentication failed");
                return new CustomGrantValidationResult("Authentication failed.");
            }

            return new CustomGrantValidationResult(
                result.User.GetSubjectId(),
                "custom",
                result.User.Claims);
        }

        public string GrantType
        {
            get { return "legacy_account_store"; }
        }
    }
}