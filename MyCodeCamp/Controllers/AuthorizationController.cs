using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Attributes;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.DTOs;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    /// <summary>
    /// ***     THIS IS REALLY IF THE API WAS 
    /// ***     GOING TO BE USED WITH A WEB SIDE 
    /// ***     THAT USER MIGHT BE ACCESS WITH PASSWRD DURING LOGIN
    /// </summary>
    public class AuthorizationController : Controller
    {
        #region Fields
        private CampContext campContext;
        private ILogger<AuthorizationController> logger;
        private readonly SignInManager<CampUser> signInManager;
        private UserManager<CampUser> userManager;
        private IPasswordHasher<CampUser> passwordHasher;
        private IConfigurationRoot configuration;
        #endregion

        #region Constructor
        public AuthorizationController(CampContext campContext, SignInManager<CampUser> signInManager, ILogger<AuthorizationController> logger,
                                       UserManager<CampUser>  userManager, IPasswordHasher<CampUser> passwordHasher, IConfigurationRoot configuration)
        {
            this.campContext = campContext;
            this.signInManager = signInManager;
            this.logger = logger;
            this.userManager = userManager;
            this.passwordHasher = passwordHasher;
            this.configuration = configuration;
        }
        #endregion

        #region Methods
        [HttpPost("api/authorization/login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] CredentialDto credentialDto)
        {
            try
            {
                var result = await this.signInManager.PasswordSignInAsync(credentialDto.UserName, credentialDto.Password, false, false);
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"An exception was thrown while logging in: {ex}");
            }
            // *** Don't give any clues on how to hack in!!!! 
            //  ***  ie. Username, password info
            return BadRequest("Failed to Login.");
        }

        public async Task<IActionResult> CreateToken([FromBody] CredentialDto credentialDto)
        {
            try
            {
                var user = await this.userManager.FindByNameAsync(credentialDto.UserName);
                if (user != null)
                {
                    if (this.passwordHasher.VerifyHashedPassword(user, user.PasswordHash, credentialDto.Password) == PasswordVerificationResult.Success)
                    {
                        var userClaims = await this.userManager.GetClaimsAsync(user);
                        var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                            new Claim(JwtRegisteredClaimNames.Email, user.Email)
                        }.Union(userClaims);

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["Tokens:Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken
                        (
                            issuer: this.configuration["Tokens:Issuer"],
                            audience: this.configuration["Tokens:Audience"],
                            claims: claims,
                            expires: DateTime.Now.AddMinutes(15),
                            signingCredentials: credentials
                        );
                    }
                }

            }
            catch (Exception ex)
            {
                this.logger.LogError($"An exception was thrown while creating JWT: {ex}");
            }
            return BadRequest("Failed to generate token.");
        }
        #endregion
    }
}
