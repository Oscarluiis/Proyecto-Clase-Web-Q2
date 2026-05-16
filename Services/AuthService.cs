using Google.Cloud.Firestore;
using ProyectoClaseQ2.DTOs;
using ProyectoClaseQ2.Models;
using System.Security.Cryptography;
using System.Text;


namespace ProyectoClaseQ2.Services;

public class AuthService
{
    // Maneja lo relacionado a registro e inicio de sesion
    private readonly FirebaseService _firebaseService;
    private readonly IConfiguration _configuration;

    public AuthService(FirebaseService firebaseService, IConfiguration configuration)
    {
        _firebaseService = firebaseService;
        _configuration = configuration;
    }

    public async Task<User> Register(RegisterDto dto)
    {
        // Primero verificamos que no existe un usuario con ese correo
        var collection = _firebaseService.GetCollection("users");
        var existing = await collection
            .WhereEqualTo("Email", dto.Email)
            .GetSnapshotAsync();

        if (existing.Count > 0)
            throw new Exception("Ya existe un usuario con ese correo");
        
        // Creamos el objeto con la contraseña hasheada

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = HashPasword(dto.Password),
            Role = "user",
            CreatedAt =  DateTime.UtcNow
        };
        
        // Guardamos En FS usando el Id como nombre del documento
        await collection.Document(user.Id).SetAsync(new Dictionary<string, object>
        {
            { "Id", user.Id },
            { "FullName", user.FullName },
            { "Email", user.Email },
            { "PasswordHash", user.PasswordHash },
            { "Role", user.Role },
            { "CreatedAt", user.CreatedAt }
        });
        return user;
    }

    // Para encriptar la contraseña
    private string HashPasword(string password)
    {
        // SHA256 - tipo de encriptacion
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}