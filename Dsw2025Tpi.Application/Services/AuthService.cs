using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Dsw2025Tpi.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;

    public AuthService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> ValidateRegistrationAsync(RegisterModel model)
    {
        var errors = new List<string>();

        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(model.Username))
            errors.Add("El nombre de usuario es obligatorio");

        if (string.IsNullOrWhiteSpace(model.Email) || !IsValidEmail(model.Email))
            errors.Add("El correo electrónico no es válido");

        if (errors.Any())
            return (false, errors);

        // Verificar duplicados
        if (await _userManager.FindByNameAsync(model.Username) != null)
            errors.Add("El nombre de usuario ya está en uso");

        if (await _userManager.FindByEmailAsync(model.Email) != null)
            errors.Add("El correo electrónico ya está registrado");

        return (errors.Count == 0, errors);
    }

    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
