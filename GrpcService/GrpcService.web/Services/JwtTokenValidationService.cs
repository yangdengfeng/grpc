using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace GrpcService.web.Services
{
    public class JwtTokenValidationService
    {
        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<TokenModel> GenerateTokenAsync(UserModel model)
        {
            if (model.UserName == "admin" && model.Password == "123")
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "test@126.com"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, "admin"),
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ja0a89fnq3423ripomcx"));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    "localhost", "localhost", 
                    claims, 
                    expires: DateTime.Now.AddMinutes(10), 
                    signingCredentials: credentials);

                return new TokenModel
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expriation = token.ValidTo,
                    Success = true
                };
            }

            return new TokenModel
            {
                Success = false
            };
        }
    }

    public class UserModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class TokenModel
    {
        public string Token { get; set; }

        public DateTime Expriation { get; set; }

        public bool Success { get; set; }
    }
}
