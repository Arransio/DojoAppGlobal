# 🔍 Análisis de Errores en Conexión con API y JWT

## ❌ Problemas Identificados y Corregidos

### **1. CRÍTICO: Mismatch en nombres de propiedades (LoginResponse)**

**El Problema:**
- El cliente MAUI tenía `LoginResponse` con propiedad `token` (minúscula)
- El servidor devuelve `Token` (mayúscula)
- El código intentaba acceder a `result.token` que no existía

**Línea afectada:**
```csharp
// ANTES - LoginViewModel.cs línea 54
Debug.WriteLine("TOKEN LOGIN: " + result.token);  // ❌ token no existe
await TokenStorage.SaveToken(result.token);       // ❌ token no existe
```

**Solución:**
```csharp
// DESPUÉS - LoginViewModel.cs línea 54
Debug.WriteLine("TOKEN LOGIN: " + result.Token);  // ✅ Token (mayúscula)
await TokenStorage.SaveToken(result.Token);       // ✅ Token (mayúscula)
```

- ✅ Actualizado `Models/LoginResponse.cs` - cambié `token` → `Token`
- ✅ Actualizado `ViewModels/LoginViewModel.cs` - cambié `result.token` → `result.Token`
- ✅ Actualizado `Services/AuthService.cs` - cambié `result.token` → `result.Token`

---

### **2. Token no se envía en peticiones autorizadas (Falta de [Authorize] support)**

**El Problema:**
- `ApiService` no agregaba el token a las peticiones HTTP
- Peticiones que requieren `[Authorize]` en el servidor fallaban
- No había un mecanismo consistente para inyectar el Bearer token

**Solución:**
```csharp
// Añadido a ApiService.cs
private async Task EnsureAuthorization()
{
    var token = await TokenStorage.GetToken();
    if (!string.IsNullOrEmpty(token))
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        Debug.WriteLine($"[ApiService] Token añadido: {token.Substring(0, 20)}...");
    }
}

// Cada método ahora llama EnsureAuthorization() primero
public async Task<Campaign> GetActiveCampaignAsync()
{
    await EnsureAuthorization();  // ✅ Asegura token antes de llamar
    return await _httpClient.GetFromJsonAsync<Campaign>("api/campaigns/active");
}
```

---

### **3. AuthService: Logging mejorado y manejo de errores**

**Cambios en `AuthService.cs`:**

✅ **Mejor manejo de errores en LoginAsync:**
```csharp
if (!response.IsSuccessStatusCode) 
{
    var error = await response.Content.ReadAsStringAsync();
    Debug.WriteLine($"[AuthService] Login error: {response.StatusCode} - {error}");
    throw new Exception($"Login failed: {response.StatusCode} - {error}");
}
```

✅ **Logging detallado del token recibido:**
```csharp
if (result?.Token != null)
{
    Debug.WriteLine($"[AuthService] Token recibido: {result.Token.Substring(0, 20)}...");
}
else
{
    Debug.WriteLine("[AuthService] ⚠️ Token es null en respuesta");
}
```

✅ **TestAuth mejorado con validaciones:**
```csharp
public async Task<string> TestAuth()
{
    var token = await TokenStorage.GetToken();
    
    if (string.IsNullOrEmpty(token))
    {
        Debug.WriteLine("[AuthService] ⚠️ NO HAY TOKEN");
        throw new Exception("No token available");
    }
    
    // ... resto del código
    
    if (!response.IsSuccessStatusCode)
    {
        Debug.WriteLine($"[AuthService] ⚠️ Status: {response.StatusCode}");
    }
}
```

---

## 🔧 Configuración en Backend (Verificado)

El backend `Program.cs` está configurado correctamente:

```csharp
// ✅ JWT configurado correctamente
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],           // "dojoapp"
        ValidAudience = builder.Configuration["Jwt:Audience"],       // "dojoapp_users"
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// ✅ Middlewares en orden correcto
app.UseAuthentication();  // Primero autenticar
app.UseAuthorization();   // Luego autorizar
```

**appsettings.json verificado:**
```json
{
  "Jwt": {
    "Key": "7fA9k2LmQ8xP3vR6ZtY1bN5uHcW4eJ0sDgK",
    "Issuer": "dojoapp",
    "Audience": "dojoapp_users",
    "ExpireMinutes": 60
  }
}
```

---

## ✅ Archivos Modificados

1. **Models/LoginResponse.cs**
   - Cambió `token` → `Token`

2. **ViewModels/LoginViewModel.cs**
   - Cambió `result.token` → `result.Token` (2 instancias)

3. **Services/AuthService.cs**
   - Mejorado `LoginAsync()` con logging detallado
   - Mejorado `TestAuth()` con validaciones
   - Cambió `result.token` → `result.Token`

4. **Services/ApiService.cs**
   - Añadido método `EnsureAuthorization()`
   - Actualizado `GetActiveCampaignAsync()` para enviar token
   - Añadido método `GetAsync<T>()` para peticiones autorizadas

---

## 🧪 Cómo Probar

### Test 1: Login básico
```csharp
// En LoginPage, ingresa:
Username: admin
Password: 1234
```

**Esperado en Output:**
```
[AuthService] Response: {"token":"eyJ0eXAiOiJKV1QiLCJhbGc...","userId":1}
[AuthService] Token recibido correctamente: eyJ0eXAiOiJKV1QiLCJhbGc...
[AuthService] TOKEN TEST: eyJ0eXAiOiJKV1QiLCJhbGc...
[AuthService] STATUS: OK
[AuthService] CONTENT: {"Message":"Acceso autorizado","User":"admin"}
```

### Test 2: Petición con token
Cuando llames a una petición que requiera `[Authorize]`:

**Esperado:**
```
[ApiService] Token añadido a headers: eyJ0eXAiOiJKV1QiLCJhbGc...
```

---

## 🚨 Si Aún tienes problemas:

### Revisa en Debug Output:

1. **Token es null o vacío:**
   - Revisa que `TokenStorage.SaveToken()` se ejecutó exitosamente
   - Verifica que `SecureStorage` funcione (usa fallback a `Preferences` en emulador)

2. **Status 401 Unauthorized:**
   - El token no se está enviando correctamente
   - Verifica que `Authorization` header esté presente: `Bearer <token>`

3. **Status 403 Forbidden:**
   - El token es válido pero el usuario no tiene permisos
   - Revisa el endpoint y si realmente tiene `[Authorize]`

4. **Status 500 del servidor:**
   - Hay error en el servidor al procesar el token
   - Revisa los logs del backend

---

## 📋 Checklist Final

- [x] LoginResponse.Token (mayúscula) en ambos clientes
- [x] AuthService llama TokenStorage.SaveToken(result.Token)
- [x] ApiService.EnsureAuthorization() antes de peticiones
- [x] Token se envía en header: `Authorization: Bearer <token>`
- [x] Backend JWT configurado correctamente
- [x] app.UseAuthentication() antes de app.UseAuthorization()
- [x] Logging detallado para debugging

---

**Compilación:** ✅ Correcta (sin errores)
