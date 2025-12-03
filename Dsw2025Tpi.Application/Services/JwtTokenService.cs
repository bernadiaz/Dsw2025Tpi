using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Dsw2025Tpi.Application.Services;
public class JwtTokenService
{
    private readonly IConfiguration _config;
    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string username, string role, Guid Id)
    {
        var jwtConfig = _config.GetSection("Jwt");
        var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("Jwt Key");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //especifica q algoritmo debe usarse para firmar la clave

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Role, role.ToUpper()) ,//si se quisiera agregar un rol al token, no olvidar agregarlo tmb en generateToken(username, role) y en el endpoint de creación del token
	    new Claim(ClaimTypes.NameIdentifier, Id.ToString())
        //definimos claims, q el token usa. Se pueden llegar a definir más
        };

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(double.Parse(jwtConfig["ExpireInMinutes"] ?? "60")),
            signingCredentials: creds
            );
        //creación del token
        return new JwtSecurityTokenHandler().WriteToken(token);
        //generación del token
    }

}
