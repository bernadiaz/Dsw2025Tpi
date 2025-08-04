using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;//son servicio definidos 
    private readonly SignInManager<IdentityUser> _signInManager;//como manager
    private readonly JwtTokenService _jwtTokenService;
    private readonly IAuthService _authService;

    public AuthenticationController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
        JwtTokenService jwtTokenService, IAuthService authService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            return Unauthorized("Usuario o contraseña incorrectos");
        }
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized("Usuario o contraseña incorrectos");
        }
        var rol = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.GenerateToken(user.UserName, rol.FirstOrDefault());
        return Ok(new {token});
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel request)
    {
        var (isValid, errors) = await _authService.ValidateRegistrationAsync(request);
        if (!isValid)
        {
            return BadRequest(new { errors });
        }
        // Crear nuevo usuario
        var user = new IdentityUser { UserName = request.Username, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        //si se quisiera agregar un rol al usuario, se haría aquí
        var role = string.IsNullOrWhiteSpace(request.role) ? "cliente" : request.role;
        // Verificar si el rol existe, y crearlo si no
        //if (!await _roleManager.RoleExistsAsync(role))
        //{
        //    await _roleManager.CreateAsync(new IdentityRole(role));
        //}

        // Asignar el rol al usuario
        await _userManager.AddToRoleAsync(user, role);
        //tmb opcionalmente se podria agregar un mail de confirmacion
        return Ok("Usuario registrado exitosamente");
    }
}
