using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MyAuthorizationDemo.Domain.Constants;
using MyAuthorizationDemo.Domain.Entities;

namespace MyAuthorizationDemo.Application.Auth.Commands.AuthenticateCommand;

public class AuthenticateCommand : IRequest<AccessToken>
{
    /// <summary>
    ///     รหัสผู้ใช้
    /// </summary>
    /// <example>apiuser</example>
    public string? Username { get; set; }

    /// <summary>
    ///     รหัสผ่าน
    /// </summary>
    /// <example>password</example>
    public string? Password { get; set; }

    public class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, AccessToken>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthenticateCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<AccessToken> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
        {
            if (request.Username != null)
            {
                ApplicationUser? appUser = await _userManager.FindByEmailAsync(request.Username);
                JwtSecurityTokenHandler tokenHandler = new();
                byte[] secretKey = Encoding.UTF8.GetBytes("2bb80d537b1da3e38bd30361aa855686bde0eacd7162fef6a25fe97bf527a25b");

                if (appUser?.UserName != null)
                {
                    List<Claim> identityProperty = new()
                    {
                        new Claim(ClaimTypes.NameIdentifier, appUser.Id),
                        new Claim(ClaimTypes.Name, appUser.UserName),
                        new Claim(ClaimTypes.Email, appUser.Email),
                    };
                    identityProperty.AddRange(await _userManager.GetClaimsAsync(appUser));

                    // TODO ADD MORE POLICIES OR ROLES
                    // foreach (var policy in Policies.Claims)
                    // {
                    //     identityProperty.Add(new Claim(policy.Key, "true"));
                    // }
                    
                    SecurityTokenDescriptor tokenDescription = new()
                    {
                        Subject = new ClaimsIdentity(identityProperty),
                        Expires = DateTime.Now.AddHours(1),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey),
                            SecurityAlgorithms.HmacSha256Signature)
                    };
                    SecurityToken? token = tokenHandler.CreateToken(tokenDescription);
                    return new AccessToken { Token = tokenHandler.WriteToken(token) };
                }
            }

            return new AccessToken();
        }
    }
}
