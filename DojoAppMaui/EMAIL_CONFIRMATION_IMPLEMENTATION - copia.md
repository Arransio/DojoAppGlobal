# Implementación: Confirmación de Email en Registro

## Resumen de Cambios

Se ha implementado un sistema completo de confirmación de email para el registro de nuevos usuarios. Ahora, cuando un usuario se registra:

1. **Se solicita el email** durante el registro
2. **Se envía un correo de confirmación** con un link único
3. **El usuario NO puede iniciar sesión** hasta confirmar su email
4. **Después de confirmar el email**, el usuario puede usar sus credenciales normalmente

---

## Cambios en la API (.NET 8)

### 1. Modelo de Usuario Actualizado
**Archivo**: `..\Dojo-App\Api_Dojo_App\Models\User.cs`

Se añadieron 3 campos para manejar la confirmación:
```csharp
public bool IsEmailConfirmed { get; set; } = false;
public string? EmailConfirmationToken { get; set; }
public DateTime? EmailConfirmationTokenExpiry { get; set; }
```

### 2. Servicio de Email
**Archivo**: `..\Dojo-App\Api_Dojo_App\Services\EmailService.cs` (NUEVO)

Implementa:
- Interface `IEmailService` para inyección de dependencias
- Método `SendConfirmationEmailAsync()` que:
  - Se conecta a un servidor SMTP (Gmail por defecto)
  - Envía un email HTML con un link de confirmación
  - El link incluye el token y email como parámetros
  - El email expira en 24 horas

### 3. Controlador de Autenticación Actualizado
**Archivo**: `..\Dojo-App\Api_Dojo_App\Controllers\AuthController.cs`

**Cambios en `Register`**:
- Ahora requiere email además de username y password
- Crea el usuario con `IsEmailConfirmed = false`
- Genera un token único de confirmación
- Envía el email de confirmación
- NO genera token JWT hasta que se confirme el email

**Nuevo endpoint `POST /api/auth/confirm-email`**:
- Recibe email y token de confirmación
- Valida que el token no haya expirado (24 horas)
- Marca el usuario como confirmado
- Retorna mensaje de éxito

**Cambio en `Login`**:
- Verifica que `IsEmailConfirmed == true`
- Si no está confirmado, rechaza el login con mensaje claro

### 4. Models para Solicitudes
- `RegisterRequest.cs` (NUEVO) - Para registro con email
- `ConfirmEmailRequest.cs` (NUEVO) - Para confirmar email

### 5. Configuración
**Archivo**: `..\Dojo-App\Api_Dojo_App\appsettings.json`

Nuevas secciones:
```json
"AppSettings": {
  "FrontendUrl": "http://localhost:5173"
},
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": "587",
  "SenderEmail": "your-email@gmail.com",
  "SenderPassword": "your-app-password"
}
```

### 6. Program.cs
- Se registró el servicio `IEmailService` en la inyección de dependencias

### 7. Migración de Base de Datos
**Archivo**: `..\Dojo-App\Api_Dojo_App\Migrations\20260416_AddEmailConfirmationToUser.cs` (NUEVO)

Añade las 3 columnas al modelo User en la BD

---

## Cambios en Cliente MAUI

### 1. AuthService Actualizado
**Archivo**: `Services\AuthService.cs`

**`RegisterAsync(string username, string email, string password)`**:
- Ahora requiere los 3 parámetros
- Retorna `RegisterResponse` (sin token)
- Informa que se envió un email de confirmación

**`ConfirmEmailAsync(string email, string token)`** (NUEVO):
- Realiza la llamada POST a `/api/auth/confirm-email`
- Retorna `true` si fue exitoso
- Lanza excepción con mensaje descriptivo si falla

### 2. Modelos de Respuesta MAUI
**Archivo**: `Models\RegisterResponse.cs` (NUEVO)
```csharp
public class RegisterResponse
{
    public string Message { get; set; }
    public int UserId { get; set; }
}
```

### 3. LoginViewModel Actualizado
**Archivo**: `ViewModels\LoginViewModel.cs`

**Nuevas propiedades**:
- `Email` - Para almacenar el email del registro

**Método `OnRegisterClicked()`**:
- Ahora pide al usuario: Username → Email → Contraseña
- Valida que el email sea válido
- Llama a `AttemptRegister()` con los 3 parámetros

**Método `AttemptRegister()`**:
- Acepta username, email y password
- Muestra un mensaje informando que se envió el email
- NO intenta guardar token (porque el usuario no está confirmado aún)
- Limpia los campos

**Método helper `IsValidEmail()`**:
- Valida el formato del email localmente

---

## Flujo Completo del Usuario

### Registro:
1. Usuario hace clic en "¿No tienes cuenta? Registrarse"
2. Se le pide: usuario, email, contraseña
3. Se envía datos a servidor
4. Servidor:
   - Crea el usuario con `IsEmailConfirmed = false`
   - Genera token de confirmación
   - Envía email con link: `http://localhost:5173/confirm-email?token=XXX&email=user@example.com`
5. Aplicación muestra: "Se envió un email de confirmación"

### Confirmación:
1. Usuario abre el link en el email
2. El navegador/app procesa: `confirm-email?token=XXX&email=user@example.com`
3. Llamada a `POST /api/auth/confirm-email`
4. Servidor valida y marca como confirmado
5. Usuario puede iniciar sesión normalmente

### Login:
1. Usuario intenta iniciar sesión
2. Si `IsEmailConfirmed == false`: error "Por favor confirma tu email"
3. Si `IsEmailConfirmed == true`: funcionamiento normal

---

## Configuración Requerida

### Para producción con Gmail:
1. Habilitar "Contraseñas de app" en tu cuenta Google
2. En `appsettings.json`:
   ```json
   "SenderEmail": "tu-email@gmail.com",
   "SenderPassword": "tu-contraseña-de-app"
   ```

### Para pruebas locales:
- Actualizar `"FrontendUrl"` en `appsettings.json` según dónde esté alojado el frontend
- Configurar credenciales de SMTP

### Ejecutar migración:
```bash
dotnet ef database update
```

---

## Endpoints de la API

### POST /api/auth/register
```json
{
  "username": "usuario123",
  "email": "usuario@example.com",
  "password": "contrasena123"
}
```
Respuesta:
```json
{
  "message": "Usuario registrado exitosamente. Por favor, confirma tu email.",
  "userId": 1
}
```

### POST /api/auth/confirm-email
```json
{
  "email": "usuario@example.com",
  "token": "token_generado"
}
```
Respuesta:
```json
{
  "message": "Email confirmado exitosamente. Ya puedes iniciar sesión."
}
```

### POST /api/auth/login
```json
{
  "username": "usuario123",
  "password": "contrasena123"
}
```
Respuesta (solo si email está confirmado):
```json
{
  "token": "jwt_token",
  "userId": 1
}
```

---

## Mejoras Futuras

1. **Reenviar email de confirmación**: Endpoint para reenviar el email si expiró
2. **Resend después de 24h**: Si token expira, permitir solicitar uno nuevo
3. **Confirmación de cambio de email**: Cuando usuario actualice su email
4. **Dos factores**: Después de confirmar email, pedir 2FA
5. **Deep linking**: En MAUI, capturar deep links del email para confirmar automáticamente
