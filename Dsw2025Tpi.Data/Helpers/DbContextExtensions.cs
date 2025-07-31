using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Data.Helpers;

public static class DbContextExtensions
{
    public static void SeedWork<T>(this Dsw2025TpiContext context, string dataSource) where T : class
    {
        if (context.Set<T>().Any()) return;
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, dataSource));
        var entities = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });
        if(entities == null || entities.Count == 0) return;
        try
        {
            context.Set<T>().AddRange(entities);
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al guardar los cambios: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Excepción interna: {ex.InnerException.Message}");
            }
            throw;
        }
    }
}
