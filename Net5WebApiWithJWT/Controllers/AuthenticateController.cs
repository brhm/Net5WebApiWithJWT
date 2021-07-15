using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Net5WebApiWithJWT.IdentityAuth;
using Net5WebApiWithJWT.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Net5WebApiWithJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var existUser = await _userManager.FindByNameAsync(model.Username);

            if(existUser!=null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response() { Message = "User already exists!", Status = "Error" });
            }
            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Username,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result =await _userManager.CreateAsync(user, model.Password);
            if(!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response() { Status = "Error", Message = "User creation failed! Please check user detail and try again!" });
            }
            
            return Ok(new Response() { Status="Success",Message="User Created successfully!"});
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var existUser = await _userManager.FindByNameAsync(model.Username);

            if (existUser != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response() { Message = "User already exists!", Status = "Error" });
            }
            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Username,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response() { Status = "Error", Message = "User creation failed! Please check user detail and try again!" });
            }

            if(!await _roleManager.RoleExistsAsync(UserRoles.Admin.ToString()))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin.ToString()));
            }
            if (!await _roleManager.RoleExistsAsync(UserRoles.User.ToString()))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User.ToString()));
            }

            if(await _roleManager.RoleExistsAsync(UserRoles.Admin.ToString()))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin.ToString());
            }

            return Ok(new Response() { Status = "Success", Message = "User Created successfully!" });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if(user!=null && await _userManager.CheckPasswordAsync(user,model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };

                foreach (var item in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, item));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(5),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

    }
}
