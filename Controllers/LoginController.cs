using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ABCCompanyService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Calendar.v3.Data;
using ABCCompanyService.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using ABCCompanyService.Models.Api;
using Microsoft.Extensions.Configuration;

namespace ABCCompanyService.Controllers
{
    /// <summary>
    /// Controller for managing the account login
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _context;
        private readonly ILogger _logger;
        private IConfiguration _configuration;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="roleManager"></param>
        /// <param name="dbContext"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        public LoginController(
           UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           RoleManager<IdentityRole> roleManager,
           ApplicationDbContext dbContext,
           IConfiguration configuration,

           ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<AbsenceController>();
            _context = dbContext;
        }

        /// <summary>
        /// Login Callback
        /// </summary>
        /// <returns></returns>
        [HttpGet("ExternalLogin")]
        // [ValidateAntiForgeryToken]
        public async Task<ApiResult<LoginCallBackResponse>> ExternalLoginGet()
        {
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info != null)
                {
                    var validateUser = await _userManager.GetUserAsync(HttpContext.User);
                    if (validateUser == null)
                    {
                        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                        var id = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                        var user = new ApplicationUser { Id = id, UserName = email, Email = email };

                        var result = await _userManager.CreateAsync(user);
                        if (result.Succeeded)
                        {
                            await _context.SaveChangesAsync();
                            await _userManager.AddLoginAsync(user, info);
                            await _userManager.AddClaimsAsync(user, info.Principal.Claims);
                            await _userManager.AddToRoleAsync(user, "User");
                            if (_configuration["AdminUser"] == email)
                            {
                                await _userManager.AddToRoleAsync(user, "Admin");
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                if (!_signInManager.IsSignedIn(HttpContext.User))
                {
                    var validateUser2 = await _userManager.GetUserAsync(info.Principal);
                    await _signInManager.SignInAsync(validateUser2, isPersistent: false);
                }

                return new ApiResult<LoginCallBackResponse>()
                {
                    Result = new LoginCallBackResponse()
                    {
                        token_type = info.AuthenticationTokens.Where(u => u.Name == "token_type").FirstOrDefault().Value.ToString(),
                        access_token = info.AuthenticationTokens.Where(u => u.Name == "access_token").FirstOrDefault().Value.ToString(),
                        expires_at = info.AuthenticationTokens.Where(u => u.Name == "expires_at").FirstOrDefault().Value.ToString(),
                    },
                    Message = Models.Api.ApiResult<LoginCallBackResponse>.SuccessMessage
                };
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                _logger.LogError(ex.Message, ex);
                return new ApiResult<LoginCallBackResponse>()
                {
                    Message = Models.Api.ApiResult<LoginCallBackResponse>.ErrorMessage
                };
            }
        }

        /// <summary>
        /// External Login 
        /// </summary>
        /// <returns></returns>
        [HttpPost("ExternalLogin")]
        [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        public async Task<ApiResult<LoginResponse>> ExternalLogin()
        {
            try
            {
                var provider = "Google";
                var returnUrl = "";

                // Request a redirect to the external login provider.   
                var redirectUrl = Url.Action("ExternalLoginCallback", "AbsenceController", new { ReturnUrl = returnUrl });
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

                HttpContext.Items["OnRedirectToAuthorizationEndpointRequest"] = true;
                await HttpContext.Authentication.ChallengeAsync(provider, properties);

                if (HttpContext.Items.ContainsKey("OnRedirectToAuthorizationEndpoint"))
                {
                    var redirectContext = (OAuthRedirectToAuthorizationContext)HttpContext.Items["OnRedirectToAuthorizationEndpoint"];
                    return new ApiResult<LoginResponse>()
                    {
                        Result = new LoginResponse()
                        {
                            redirectUri = redirectContext.RedirectUri
                        },
                        Message = Models.Api.ApiResult<LoginResponse>.SuccessMessage
                    };
                }
                else
                {
                    await _signInManager.SignOutAsync();
                    _logger.LogInformation(4, "User logged out.");
                    return new ApiResult<LoginResponse>()
                    {
                        Result = new LoginResponse()
                        {
                        },
                        Message = "User logged out."
                    };
                }

            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                _logger.LogError(ex.Message, ex);
                return new ApiResult<LoginResponse>()
                {
                    Message = Models.Api.ApiResult<LoginResponse>.ErrorMessage
                };
            }
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        [HttpPost("Logout")]
        [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        public async Task<ApiResult<LoginResponse>> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation(4, "User logged out.");
                return new ApiResult<LoginResponse>()
                {
                    Result = new LoginResponse()
                    {
                    },
                    Message = "User logged out."
                };
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                _logger.LogError(ex.Message, ex);
                return new ApiResult<LoginResponse>()
                {
                    Message = Models.Api.ApiResult<LoginResponse>.ErrorMessage
                };
            }
        }
    }
}
