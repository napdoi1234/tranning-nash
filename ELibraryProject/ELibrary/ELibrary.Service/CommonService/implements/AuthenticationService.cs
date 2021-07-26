using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ELibrary.Data.Entities;
using ELibrary.Utilities.Constants.User;
using ELibrary.Utilities.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ELibrary.Service.CommonService.implements
{
  public class AuthenticationService : IAuthenticationService
  {
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _config;
    private readonly SignInManager<User> _signInManager;
    public AuthenticationService(UserManager<User> userManager, IConfiguration config, SignInManager<User> signInManager)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _config = config;
    }

    public async Task<string> Authenticate(LoginRequestDTO requestDTO)
    {
      var user = await _userManager.FindByNameAsync(requestDTO.UserName);
      if (user == null)
      {
        return UserConstant.NotFoundUser;
      };

      var result = await _signInManager.PasswordSignInAsync(user, requestDTO.Password, false, false);
      if (!result.Succeeded)
      {
        return UserConstant.WrongPassword;
      }

      var userRoles = await _userManager.GetRolesAsync(user);

      var authClaims = new[]
      {
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Role, string.Join(";",userRoles)),
      };

      var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));

      var token = new JwtSecurityToken(
          issuer: _config["Tokens:Issuer"],
          audience: _config["Tokens:Issuer"],
          claims: authClaims,
          signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
          );

      return (new JwtSecurityTokenHandler().WriteToken(token));
    }
  }
}