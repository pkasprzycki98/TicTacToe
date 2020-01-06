using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TicTacToe.Data;
using TicTacToe.Managers;
using TicTacToe.Models;

namespace TicTacToe.Services
{
    public class UserService : IUserService
    {
        private ILogger<UserService> _logger;
        private ApplicationUserManager _userManager;
        private SignInManager<UserModel> _signInManager;
        public UserService(RoleManager<RoleModel> roleManager, ApplicationUserManager userManager, ILogger<UserService> logger, SignInManager<UserModel> signInManager)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;

            var emailTokenProvider = new EmailTokenProvider<UserModel>();
            _userManager.RegisterTokenProvider("Default", emailTokenProvider);

            if (!roleManager.RoleExistsAsync("Player").Result)
                roleManager.CreateAsync(new RoleModel { Name = "Player" }).Wait();

            if (!roleManager.RoleExistsAsync("Administrator").Result)
                roleManager.CreateAsync(new RoleModel { Name = "Administrator" }).Wait();
        }

        public async Task<bool> ConfirmEmail(string email, string code)
        {
            var start = DateTime.Now;
            _logger.LogTrace($"Potwierdzenie adresu e-mail {email}");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                    return false;

                var result = await _userManager.ConfirmEmailAsync(user, code);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Nie można potwierdzić adresu e-mail {email} - {ex}");
                return false;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogTrace($"Powierdzenie adresu e-mail użytkownika ukończono w ciągu {stopwatch.Elapsed}");
            }
        }


        public async Task<string> GetEmailConfirmationCode(UserModel user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<bool> RegisterUser(UserModel userModel, bool isOnline = false)
        {
            var start = DateTime.Now;
            _logger.LogTrace($"Rozpoczęcie rejestracji użytkownika {userModel.Email} - {start}");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                userModel.UserName = userModel.Email;
                var result = await _userManager.CreateAsync(userModel, userModel.Password);
                if (result == IdentityResult.Success)
                {
                    if (userModel.FirstName == "Jason")
                        await _userManager.AddToRoleAsync(userModel, "Administrator");
                    else
                        await _userManager.AddToRoleAsync(userModel, "Player");

                    if (isOnline)
                    {
                        HttpContext httpContext = new HttpContextAccessor().HttpContext;
                        await SignIn(httpContext, userModel);
                    }
                }

                return result == IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Nie można zarejestrować użytkownika {userModel.Email} - {ex}");
                return false;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogTrace($"Rozpoczęcie rejestracji użytkownika {userModel.Email} zakończono o {DateTime.Now} - upłynęło(y){stopwatch.Elapsed.TotalSeconds} sekund(y)");
            }
        }

        public async Task<UserModel> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> IsUserExisting(string email)
        {
            return (await _userManager.FindByEmailAsync(email)) != null;
        }

        public async Task<IEnumerable<UserModel>> GetTopUsers(int numberOfUsers)
        {
            return await _userManager.Users.OrderByDescending(x => x.Score).ToListAsync();
        }

        public async Task UpdateUser(UserModel userModel)
        {
            await _userManager.UpdateAsync(userModel);
        }

        public async Task<SignInResult> SignInUser(LoginModel loginModel, HttpContext httpContext)
        {
            var start = DateTime.Now;
            _logger.LogTrace($"Logowanie użytkownika {loginModel.UserName}");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var user = await _userManager.FindByNameAsync(loginModel.UserName);
                var isValid = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, true);

                if (!isValid.Succeeded)
                {
                    return SignInResult.Failed;
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    return SignInResult.NotAllowed;
                }

                if (await _userManager.GetTwoFactorEnabledAsync(user))
                {
                    return SignInResult.TwoFactorRequired;
                }

                await SignIn(httpContext, user);

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Nie można zalogować użytkownika {loginModel.UserName} - {ex}");
                throw ex;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogTrace($"Logowanie użytkownika {loginModel.UserName} ukończono w czasie {stopwatch.Elapsed}");
            }
        }

        private async Task SignIn(HttpContext httpContext, UserModel user)
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
            identity.AddClaim(new Claim("displayName", $"{user.FirstName} {user.LastName}"));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                identity.AddClaim(new Claim(ClaimTypes.HomePhone, user.PhoneNumber));
            }
            identity.AddClaim(new Claim("Score", user.Score.ToString()));

            var roles = await _userManager.GetRolesAsync(user);
            identity.AddClaims(roles?.Select(r => new Claim(ClaimTypes.Role, r)));

            if (user.FirstName == "Jason")
                identity.AddClaim(new Claim("AccessLevel", "Administrator"));

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });
        }

        public async Task SignOutUser(HttpContext httpContext)
        {
            await _signInManager.SignOutAsync();
            await httpContext.SignOutAsync(new AuthenticationProperties { IsPersistent = false });
            return;
        }

        public async Task<AuthenticationProperties> GetExternalAuthenticationProperties(string provider, string redirectUrl)
        {
            return await Task.FromResult(_signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl));
        }

        public async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            return await _signInManager.GetExternalLoginInfoAsync();
        }

        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent)
        {
            _logger.LogInformation($"Logowanie użytkownika przez zewnętrzny serwis {loginProvider} - {providerKey}");
            return await _signInManager.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent);
        }

        public async Task<IdentityResult> EnableTwoFactor(string name, bool enabled)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(name);
                user.TwoFactorEnabled = true;
                await _userManager.SetTwoFactorEnabledAsync(user, enabled);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> GetTwoFactorCode(string userName, string tokenProvider)
        {
            var user = await GetUserByEmail(userName);
            return await _userManager.GenerateTwoFactorTokenAsync(user, tokenProvider);
        }

        public async Task<bool> ValidateTwoFactor(string userName, string tokenProvider, string token, HttpContext httpContext)
        {
            var user = await GetUserByEmail(userName);
            if (await _userManager.VerifyTwoFactorTokenAsync(user, tokenProvider, token))
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
                identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
                identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
                identity.AddClaim(new Claim("displayName", $"{user.FirstName} {user.LastName}"));

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    identity.AddClaim(new Claim(ClaimTypes.HomePhone, user.PhoneNumber));
                }

                identity.AddClaim(new Claim("Score", user.Score.ToString()));
                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });

                return true;
            }
            return false;
        }

        public async Task<string> GetResetPasswordCode(UserModel user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPassword(string userName, string password, string token)
        {
            var start = DateTime.Now;
            _logger.LogTrace($"Resetowanie hasła użytkownika {userName}");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                var result = await _userManager.ResetPasswordAsync(user, token, password);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Nie można zresetować hasła użytkownika {userName} - {ex}");
                throw ex;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogTrace($"Resetowanie hasła użytkownika {userName} ukończono w ciągu {stopwatch.Elapsed}");
            }
        }
    }
}
