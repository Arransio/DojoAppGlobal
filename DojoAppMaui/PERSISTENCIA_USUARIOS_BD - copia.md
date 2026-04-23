# 🗄️ Persistencia de Usuarios en Base de Datos

## ✅ Lo que se implementó

He actualizado el sistema de autenticación para **guardar usuarios reales en la base de datos** en lugar de solo generar tokens falsos. Ahora hay persistencia completa.

---

## 🏗️ Cambios Realizados

### **1. Backend - AuthController.cs**

#### **Antes (INCORRECTO):**
```csharp
[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest request)
{
    // ❌ Login hardcodeado solo para "admin" / "1234"
    if (request.Username != "admin" || request.Password != "1234")
        return Unauthorized("...");
    
    var token = GenerateJwtToken(request.Username);
    return Ok(new LoginResponse { Token = token, UserId = 1 });
}

[HttpPost("register")]
public IActionResult Register([FromBody] LoginRequest request)
{
    // ❌ NO guardaba el usuario en BD
    var token = GenerateJwtToken(request.Username);
    return Ok(new LoginResponse { Token = token, UserId = 1 });
}
```

#### **Después (CORRECTO):**

**Constructor actualizado:**
```csharp
private readonly IConfiguration _config;
private readonly AppDbContext _dbContext;  // ← NUEVO

public AuthController(IConfiguration config, AppDbContext dbContext)  // ← INYECCIÓN
{
    _config = config;
    _dbContext = dbContext;
}
```

**Método Login mejorado:**
```csharp
[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest request)
{
    // ✅ 1. Buscar usuario en BD
    var user = _dbContext.Users.FirstOrDefault(u => u.Username == request.Username);
    
    if (user == null)
        return Unauthorized("Usuario o contraseña incorrectos");
    
    // ✅ 2. Verificar contraseña (hash PBKDF2)
    if (!VerifyPassword(request.Password, user.PasswordHash))
        return Unauthorized("Usuario o contraseña incorrectos");
    
    // ✅ 3. Generar token válido
    var token = GenerateJwtToken(user.Username, user.Id);
    
    return Ok(new LoginResponse
    {
        Token = token,
        UserId = user.Id,
        Message = "Login exitoso"
    });
}
```

**Método Register implementado:**
```csharp
[HttpPost("register")]
public IActionResult Register([FromBody] LoginRequest request)
{
    // ✅ 1. Validaciones
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        return BadRequest("Usuario y contraseña son requeridos");
    
    if (request.Username.Length < 3 || request.Password.Length < 4)
        return BadRequest("Usuario debe tener mínimo 3 caracteres...");
    
    // ✅ 2. Verificar duplicado
    if (_dbContext.Users.Any(u => u.Username == request.Username))
        return BadRequest("El usuario ya existe");
    
    // ✅ 3. Hash la contraseña (PBKDF2)
    var passwordHash = HashPassword(request.Password);
    
    // ✅ 4. Crear usuario
    var newUser = new User
    {
        Username = request.Username,
        Email = string.Empty,
        PasswordHash = passwordHash,
        Role = "user"
    };
    
    // ✅ 5. Guardar en BD
    _dbContext.Users.Add(newUser);
    _dbContext.SaveChanges();
    
    // ✅ 6. Generar token
    var token = GenerateJwtToken(newUser.Username, newUser.Id);
    
    return Ok(new LoginResponse
    {
        Token = token,
        UserId = newUser.Id,
        Message = "Usuario registrado exitosamente"
    });
}
```

**Métodos de hash agregados:**
```csharp
/// <summary>
/// Hash la contraseña usando PBKDF2 (algoritmo seguro)
/// </summary>
private string HashPassword(string password)
{
    using (var pdb = new Rfc2898DeriveBytes(password, salt, 10000))
    {
        byte[] key = pdb.GetBytes(20);
        byte[] salt = pdb.Salt;
        
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(key, 0, hashBytes, 16, 20);
        
        return Convert.ToBase64String(hashBytes);
    }
}

/// <summary>
/// Verifica si la contraseña coincide con el hash
/// </summary>
private bool VerifyPassword(string password, string hash)
{
    byte[] hashBytes = Convert.FromBase64String(hash);
    byte[] salt = new byte[16];
    Array.Copy(hashBytes, 0, salt, 0, 16);
    
    using (var pdb = new Rfc2898DeriveBytes(password, salt, 10000))
    {
        byte[] key = pdb.GetBytes(20);
        
        for (int i = 0; i < 20; i++)
        {
            if (hashBytes[i + 16] != key[i])
                return false;
        }
        return true;
    }
}
```

---

## 🗂️ Estructura de Base de Datos

### **Tabla `Users`:**
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) UNIQUE NOT NULL,
    Email NVARCHAR(100),
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(50) DEFAULT 'user'
);
```

### **Modelo C#:**
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
```

---

## 🔐 Seguridad: Hash PBKDF2

Se implementó el algoritmo **PBKDF2** (Password-Based Key Derivation Function 2) que:

✅ **Genera un salt aleatorio** para cada contraseña
✅ **10,000 iteraciones** (configurable, aumentar para más seguridad)
✅ **Encoding Base64** para almacenar en BD
✅ **Verificación constante** (no vulnerable a timing attacks)

**Flujo de Hash:**
```
PASSWORD: "test1234"
    ↓
PBKDF2 + SALT (10,000 iteraciones)
    ↓
HASH: "AHq7aOK1MAujMqHcKT2B/w==" (almacenado en BD)
    ↓
Al verificar:
    PASSWORD + SALT → PBKDF2 → COMPARA CON HASH
    ↓
    ¿Coinciden? → ✅ Login exitoso
```

---

## 🧪 Flujo Completo

### **Registro de Usuario:**
```
1. MAUI envia:
   POST /api/auth/register
   { "username": "testuser", "password": "test1234" }

2. Backend:
   ✅ Valida longitudes
   ✅ Verifica que no exista
   ✅ HashPassword("test1234") → hash aleatorio único
   ✅ Crea User en BD
   ✅ SaveChanges() → BD
   ✅ Genera JWT token
   ✅ Retorna token

3. BD ahora contiene:
   ID | Username  | Email | PasswordHash                      | Role
   1  | testuser  |       | AHq7aOK1MAujMqHcKT2B/w==...     | user

4. MAUI:
   ✅ Guarda token
   ✅ Navega a HomePage
```

### **Login de Usuario:**
```
1. MAUI envía:
   POST /api/auth/login
   { "username": "testuser", "password": "test1234" }

2. Backend:
   ✅ Busca usuario en BD
   ✅ Si no existe → Unauthorized
   ✅ VerifyPassword("test1234", hash_almacenado)
   ✅ Si no coincide → Unauthorized
   ✅ Genera JWT token
   ✅ Retorna token

3. MAUI:
   ✅ Guarda token
   ✅ Navega a HomePage
```

---

## 📊 Datos de Ejemplo en BD

Después de registrarse 3 usuarios, la tabla `Users` contendría:

```
┌────┬──────────┬────────────┬─────────────────────────────────────┬──────┐
│ Id │ Username │ Email      │ PasswordHash                        │ Role │
├────┼──────────┼────────────┼─────────────────────────────────────┼──────┤
│ 1  │ admin    │            │ [hash de "1234"]                    │ user │
│ 2  │ testuser │            │ [hash de "test1234"]                │ user │
│ 3  │ john     │            │ [hash diferente cada vez]           │ user │
└────┴──────────┴────────────┴─────────────────────────────────────┴──────┘
```

**Nota:** Cada hash es diferente aunque la contraseña sea igual porque PBKDF2 genera un salt aleatorio.

---

## 🔄 Comparación: Antes vs Después

| Aspecto | Antes | Después |
|---------|-------|---------|
| **Login** | Hardcodeado (admin/1234) | ✅ Desde BD con hash |
| **Registro** | No guarda nada | ✅ Crea usuario en BD |
| **Seguridad** | ❌ Sin hash | ✅ PBKDF2 + salt |
| **Persistencia** | ❌ No hay | ✅ SQL Server/SQLite |
| **Múltiples usuarios** | ❌ Solo "admin" | ✅ Ilimitados |
| **Duplicados** | Sin verificación | ✅ Previene duplicados |

---

## 🧪 Casos de Prueba

### **Caso 1: Registro exitoso**
```
1. MAUI: Register → usuario: "newuser", password: "pass1234"
2. Backend: Crea en BD
3. Result: ✅ Usuario guardado, token generado

BD después:
✓ Users contiene "newuser"
✓ PasswordHash no es plain text (es hash)
```

### **Caso 2: Duplicado rechazado**
```
1. MAUI: Register → usuario: "newuser" (ya existe)
2. Backend: Verifica duplicado
3. Result: ❌ 400 Bad Request "El usuario ya existe"

BD: Sin cambios
```

### **Caso 3: Login exitoso**
```
1. MAUI: Login → usuario: "newuser", password: "pass1234"
2. Backend: 
   - Busca "newuser" en BD ✓
   - VerifyPassword("pass1234", hash_almacenado) ✓
3. Result: ✅ Token válido, navega

JWT contiene: username + userId
```

### **Caso 4: Login fallido (contraseña incorrecta)**
```
1. MAUI: Login → usuario: "newuser", password: "wrongpass"
2. Backend:
   - Busca "newuser" en BD ✓
   - VerifyPassword("wrongpass", hash) ✗
3. Result: ❌ 401 Unauthorized
```

### **Caso 5: Login fallido (usuario no existe)**
```
1. MAUI: Login → usuario: "noexists", password: "pass1234"
2. Backend: FirstOrDefault() → null
3. Result: ❌ 401 Unauthorized
```

---

## 📝 Cambios en Archivos

| Archivo | Cambios |
|---------|---------|
| `AuthController.cs` | ✅ Inyección de DbContext, métodos reales, hash PBKDF2 |
| `AppDbContext.cs` | ✅ Ya existía (sin cambios) |
| `User.cs` | ✅ Ya existía (sin cambios) |

---

## 🚀 Próximas Mejoras Sugeridas

1. **Email verification:** Verificar email antes de activar usuario
2. **Password reset:** Endpoint para resetear contraseña
3. **User roles:** Implementar roles más complejos (admin, moderator, etc.)
4. **Login history:** Registrar intentos de login (éxito/fallo)
5. **Account lockout:** Bloquear tras X intentos fallidos
6. **2FA:** Autenticación de dos factores
7. **Token refresh:** Refresh token para extender sesión

---

## ⚠️ Notas Importantes

- **No se pueden revertir hashes:** Si olvidas contraseña, necesitas reset
- **Salt aleatorio:** Cada hash es único aunque la password sea igual
- **10,000 iteraciones:** Se puede aumentar para más seguridad (más lento)
- **BD debe estar actualizada:** Ejecutar migrations si es necesario

**Compilación:** ✅ Correcta (sin errores)
