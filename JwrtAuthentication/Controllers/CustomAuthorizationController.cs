using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JwrtAuthentication.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JwrtAuthentication.Controllers
{
    public class CustomAuthorizationController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IConfiguration _configuration;
        public CustomAuthorizationController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [Route("register")]
        [HttpPost]

        public async Task<ActionResult> InsertUser([FromBody] RegisterViewModel model)
        {
            var user = new IdentityUser
            {
                Email = model.Email,
                UserName = model.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
            }
            return Ok(new { Username = user.UserName });
        }

        [Route("login")]
        [HttpPost]
        public async Task<ActionResult> Login([FromBody] LoginViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var claim = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName)
                };
                var signinKey = new SymmetricSecurityKey(
                 Encoding.UTF8.GetBytes(_configuration["Jwt:SigninKey"])); 
                int expiryMinutes = Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"]);
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Site"],
                    audience: _configuration["Jwt:Site"],
                    expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                    signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(
                    new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });

            };
            return Unauthorized();

        }
    }
}

//First Step in Brouser, In Postman //https://localhost:44396/register You are Alredy registered

//Second Step In Postman https://localhost:44396/login you can see your token