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
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ABCCompanyService.Controllers
{
    /// <summary>
    /// AbsenceController
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class AbsenceController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _context;
        private readonly ILogger _logger;
        private IConfiguration _configuration;

        /// <summary>
        /// Controller for managing absences 
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="roleManager"></param>
        /// <param name="dbContext"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        public AbsenceController(
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


        CalendarService GetcalendarService(ExternalLoginInfo info)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json");


            var userId = info.Principal.FindFirstValue(ClaimTypes.Email);
            var token2 = info.AuthenticationTokens.Where(u => u.Name == "access_token").FirstOrDefault().Value.ToString();

            var initializer2 = new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _configuration["ClientId"],
                    ClientSecret = _configuration["ClientSecret"],

                },
                Scopes = new[] { "openid", "email", CalendarService.Scope.Calendar, }
            };

            var flow = new GoogleAuthorizationCodeFlow(initializer2);

            UserCredential cred = new UserCredential(flow, userId, new Google.Apis.Auth.OAuth2.Responses.TokenResponse()
            {
                Issued = DateTime.Now,
                TokenType = "Bearer",
                AccessToken = token2,
                ExpiresInSeconds = 3600,
            });

            var initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = cred,
                ApplicationName = "ABCCalendarApplication",
            };

            var service = new CalendarService(initializer);

            return service;
        }

        /// <summary>
        /// Requests a new Absence
        /// </summary>
        /// <param name="absenceEvent"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult<NewAbsenceResponse>> Absence([FromBody] AbsenceEvent absenceEvent)
        {
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                var userId = info.Principal.FindFirstValue(ClaimTypes.Email);
                var token2 = info.AuthenticationTokens.Where(u => u.Name == "access_token").FirstOrDefault().Value.ToString();

                var result = _context.AbsenceRequests.Add(new AbsenceRequest()
                {
                    CreatedDateTime = DateTime.Now,
                    Description = absenceEvent.description,
                    StartEventDateTime = absenceEvent.startEventDateTime,
                    EndEventDateTime = absenceEvent.endEventDateTime,
                    Status = AbsenceRequestStatus.Pending,
                    StatusDateTime = DateTime.Now,
                    UserId = userId
                });
                await _context.SaveChangesAsync();

                var groupId = _configuration["GroupId"];
                // Create access rule with associated scope
                AclRule rule = new AclRule();
                rule.Role = "writer";
                rule.Scope = new AclRule.ScopeData() { Type = "group", Value = groupId };

                var service = GetcalendarService(info);

                var temprule = await service.Acl.List(userId).ExecuteAsync();

                bool flag = false;
                foreach (var item in temprule.Items)
                {
                    if (item.Id.Contains(groupId))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    // Insert new access rule
                    AclRule res = await service.Acl.Insert(rule, userId).ExecuteAsync();
                }

                return new ApiResult<NewAbsenceResponse>()
                {
                    Result = new NewAbsenceResponse()
                    {
                        absenceRequestId = result.Entity.AbsenceRequestId
                    },
                    Message = Models.Api.ApiResult<NewAbsenceResponse>.SuccessMessage

                };
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                _logger.LogError(ex.Message, ex);
                return new ApiResult<NewAbsenceResponse>()
                {
                    Message = Models.Api.ApiResult<NewAbsenceResponse>.ErrorMessage
                };
            }
        }

        /// <summary>
        /// List Absences by status(pending,aproved,declined) and date 
        /// </summary>
        /// <param name="status"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        [HttpGet("status/{status}/from/{from}/to/{to}", Name = "Absences")]
        public async Task<object> GetAbsences(AbsenceRequestStatus status, DateTime from, DateTime to)
        {
            try
            {
                if (from == null)
                    throw new ArgumentNullException(nameof(from));

                if (from == null)
                    throw new ArgumentNullException(nameof(to));

                var info = await _signInManager.GetExternalLoginInfoAsync();

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                this.User.ToString();
                var u1 = _userManager.GetUserId(this.User);
                var u2 = await _userManager.FindByIdAsync(u1);
                var validateUser = await _userManager.GetUserAsync(this.User);
                var roles = await _userManager.GetRolesAsync(validateUser);

                this.User.Claims.Append(new Claim("role", "Admin"));

                await _userManager.AddClaimAsync(validateUser, new System.Security.Claims.Claim("role", "Admin"));

                var result = _context.AbsenceRequests.Where(u => u.UserId == email && u.Status == status && (u.CreatedDateTime >= from && u.CreatedDateTime <= to)).ToList();

                return new ApiResult<AbsenceRequests>()
                {
                    Result = new AbsenceRequests()
                    {
                        absenceRequests = result
                    },
                    Message = Models.Api.ApiResult<AbsenceRequests>.SuccessMessage
                };
            }
            catch (ArgumentException ex)
            {
                Response.StatusCode = 500;
                _logger.LogError(ex.Message, ex);
                return new ApiResult<AbsenceRequests>()
                {
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                _logger.LogError(ex.Message, ex);
                return new ApiResult<AbsenceRequests>()
                {
                    Message = Models.Api.ApiResult<AbsenceRequests>.ErrorMessage
                };
            }
        }

        /// <summary>
        /// Pending Absences, only users with role Admin can view
        /// </summary>
        /// <returns></returns>
        [HttpGet("PendingAbsences")]
        [Authorize(Roles = "Admin")]
        public ApiResult<AbsenceRequests> PendingAbsences()
        {
            try
            {
                var result = _context.AbsenceRequests.Where(u => u.Status == AbsenceRequestStatus.Pending).ToList();
                return new ApiResult<AbsenceRequests>()
                {
                    Result = new AbsenceRequests()
                    {
                        absenceRequests = result
                    },
                    Message = Models.Api.ApiResult<AbsenceRequests>.SuccessMessage
                };
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                _logger.LogError(ex.Message, ex);
                return new ApiResult<AbsenceRequests>()
                {
                    Message = Models.Api.ApiResult<AbsenceRequests>.ErrorMessage
                };
            }
        }

        /// <summary>
        ///  Aproves an absence, only users with role Admin can aprove
        /// </summary>
        /// <param name="absenceRequestId"></param>
        /// <returns></returns>
        [HttpPut("AproveAbsence")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResult<AproveAbsenceResponse>> AproveAbsence([FromBody] int absenceRequestId)
        {
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                var userid=   info.Principal.FindFirstValue(ClaimTypes.Email);
                var result = _context.AbsenceRequests.Where(u => u.AbsenceRequestId == absenceRequestId).FirstOrDefault();
                if (result != null)
                {
                    if (result.Status != AbsenceRequestStatus.Pending)
                    {
                        var token2 = info.AuthenticationTokens.Where(u => u.Name == "access_token").FirstOrDefault().Value.ToString();

                        var userId = result.UserId;
                        var calendarId = result.UserId;

                        var service = GetcalendarService(info);

                        var c = await service.Calendars.Get(calendarId).ExecuteAsync();

                        if (result.StartEventDateTime.DayOfWeek == DayOfWeek.Saturday)
                            result.StartEventDateTime.AddDays(2);

                        if (result.StartEventDateTime.DayOfWeek == DayOfWeek.Sunday)
                            result.StartEventDateTime.AddDays(1);

                        Event myEvent = new Event
                        {
                            Summary = "Absence",
                            Description = result.Description,
                            Reminders = new Event.RemindersData() { UseDefault = false },
                            //Location = "Somewhere",
                            Locked = true,
                            Start = new EventDateTime()
                            {
                                DateTime = new DateTime(result.StartEventDateTime.Year, result.StartEventDateTime.Month, result.StartEventDateTime.Day, 9, 0, 0, 0),
                                TimeZone = c.TimeZone
                            },
                            End = new EventDateTime()
                            {
                                DateTime = new DateTime(result.StartEventDateTime.Year, result.StartEventDateTime.Month, result.StartEventDateTime.Day, 18, 0, 0, 0),
                                TimeZone = c.TimeZone
                            },
                            Recurrence = new String[] { string.Format("RRULE:FREQ=DAILY;UNTIL={0}{1}{2}T000000Z;INTERVAL=1;BYDAY=MO,TU,WE,TH,FR;WKST=SU;", result.EndEventDateTime.Year, result.EndEventDateTime.Month.ToString().PadLeft(2, '0'), result.EndEventDateTime.Day.ToString().PadLeft(2, '0')) },
                        };

                        Event recurringEvent = await service.Events.Insert(myEvent, calendarId).ExecuteAsync();

                        result.GoogleEventId = recurringEvent.Id;
                        result.Status = AbsenceRequestStatus.Aproved;
                        result.StatusDateTime = DateTime.Now;
                        result.StatusChangedBy = userid;

                        _context.Update(result);
                        await _context.SaveChangesAsync();

                        return new ApiResult<AproveAbsenceResponse>()
                        {
                            Result = new AproveAbsenceResponse()
                            {
                                googleEventId = recurringEvent.Id
                            },
                            Message = Models.Api.ApiResult<AproveAbsenceResponse>.SuccessMessage

                        };
                    }
                    else
                    {
                        Response.StatusCode = 500;
                        return new ApiResult<AproveAbsenceResponse>()
                        {
                            Result = new AproveAbsenceResponse()
                            {
                            },
                            Message = "Absence Resquest status can´t be changed"
                        };
                    }
                }
                else
                {
                    Response.StatusCode = 500;
                    return new ApiResult<AproveAbsenceResponse>()
                    {
                        Message = Models.Api.ApiResult<AproveAbsenceResponse>.ErrorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                _logger.LogError(ex.Message, ex);
                return new ApiResult<AproveAbsenceResponse>()
                {
                    Message = Models.Api.ApiResult<AproveAbsenceResponse>.ErrorMessage
                };
            }
        }

        /// <summary>
        ///  Aproves an absence, only users with role Admin can aprove
        /// </summary>
        /// <param name="absenceRequestId"></param>
        /// <returns></returns>
        [HttpPut("DeclineAbsence")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResult<AproveAbsenceResponse>> DeclineAbsence([FromBody] int absenceRequestId)
        {
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                var userid = info.Principal.FindFirstValue(ClaimTypes.Email);
                var result = _context.AbsenceRequests.Where(u => u.AbsenceRequestId == absenceRequestId).FirstOrDefault();
                if (result != null)
                {
                    if (result.Status != AbsenceRequestStatus.Pending)
                    {

                        result.Status = AbsenceRequestStatus.Aproved;
                        result.StatusDateTime = DateTime.Now;
                        result.StatusChangedBy = userid;

                        _context.Update(result);
                        await _context.SaveChangesAsync();

                        return new ApiResult<AproveAbsenceResponse>()
                        {
                            Result = new AproveAbsenceResponse()
                            {
                            },
                            Message = Models.Api.ApiResult<AproveAbsenceResponse>.SuccessMessage
                        };
                    }
                    else
                    {
                        Response.StatusCode = 500;
                        return new ApiResult<AproveAbsenceResponse>()
                        {
                            Result = new AproveAbsenceResponse()
                            {
                            },
                            Message = "Absence Resquest status can´t be changed"
                        };
                    }
                }
                else
                {
                    Response.StatusCode = 500;
                    return new ApiResult<AproveAbsenceResponse>()
                    {
                        Message = Models.Api.ApiResult<AproveAbsenceResponse>.ErrorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                _logger.LogError(ex.Message, ex);
                return new ApiResult<AproveAbsenceResponse>()
                {
                    Message = Models.Api.ApiResult<AproveAbsenceResponse>.ErrorMessage
                };
            }
        }
    }
}
