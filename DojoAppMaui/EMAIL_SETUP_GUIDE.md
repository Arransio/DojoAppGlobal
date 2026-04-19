# Guía de Configuración: Confirmación de Email

## 1. Configurar Servidor SMTP

### Opción A: Gmail (Recomendado para desarrollo)

1. **Habilitar "Contraseñas de app"**:
   - Ve a: https://myaccount.google.com/security
   - Activa "Verificación en dos pasos"
   - Ve a "Contraseñas de app"
   - Selecciona "Mail" y "Windows"
   - Copia la contraseña generada

2. **Actualizar `appsettings.json`**:
   ```json
   "EmailSettings": {
     "SmtpServer": "smtp.gmail.com",
     "SmtpPort": "587",
     "SenderEmail": "tu-email@gmail.com",
     "SenderPassword": "contraseña-de-app-generada"
   }
   ```

### Opción B: Otro servidor SMTP

```json
"EmailSettings": {
  "SmtpServer": "tu-servidor-smtp.com",
  "SmtpPort": "587",
  "SenderEmail": "tu-email@dominio.com",
  "SenderPassword": "tu-contraseña"
}
```

---

## 2. Configurar URL del Frontend

En `appsettings.json`, actualiza la URL según dónde esté tu cliente:

```json
"AppSettings": {
  "FrontendUrl": "http://localhost:5173"  // Para desarrollo local
}
```

Para producción:
```json
"AppSettings": {
  "FrontendUrl": "https://tudominio.com"
}
```

---

## 3. Aplicar Migración de Base de Datos

```bash
# En el directorio del proyecto API
dotnet ef database update
```

Esto añadirá las columnas necesarias a la tabla `Users`:
- `IsEmailConfirmed` (bool)
- `EmailConfirmationToken` (string)
- `EmailConfirmationTokenExpiry` (datetime)

---

## 4. Probar el Flujo

### Terminal 1: Ejecutar la API
```bash
cd ..\Dojo-App\Api_Dojo_App
dotnet run
# La API estará en: http://localhost:5221
```

### Terminal 2: Ejecutar el Cliente MAUI (o navegador web)
```bash
# Para MAUI
dotnet build -t:Run -f net8.0-android

# Para web (si existe)
npm run dev
```

### En la aplicación:

1. **Registro**:
   - Clic en "¿No tienes cuenta? Registrarse"
   - Ingresa usuario, email y contraseña
   - Verifica que veas el mensaje: "Se ha enviado un email de confirmación"

2. **Confirmación**:
   - Abre tu email
   - Haz clic en el link de confirmación
   - Deberías ver: "Email confirmado exitosamente"

3. **Login**:
   - Ahora puedes iniciar sesión con el usuario y contraseña
   - Sin confirmar el email, verás: "Por favor confirma tu email"

---

## 5. Debugging

### Ver logs en la API:
```
[AuthController] Creando nuevo usuario: username
[EmailService] Email de confirmación enviado a: email@example.com
[AuthController] Email confirmado para usuario: username
```

### Si el email no llega:

1. **Revisar credenciales SMTP** en `appsettings.json`
2. **Ver logs** de la API para errores
3. **Revisar spam/basura** del email
4. **Probar con otro proveedor** si Gmail no funciona

### Si el token expira:

- El token dura 24 horas
- Si expira, el usuario necesitará registrarse de nuevo
- En el futuro, implementar "Reenviar email de confirmación"

---

## 6. Datos de Prueba

Para pruebas rápidas (sin enviar emails reales), puedes modificar `EmailService.cs`:

```csharp
// Comentar este bloque para no enviar realmente
// using (var smtpClient = new SmtpClient(...)) { ... }

// Y retornar true sin enviar:
Debug.WriteLine($"[EmailService] EMAIL SIMULADO a: {email}, Token: {confirmationLink}");
return true;
```

---

## 7. Notas Importantes

- ⚠️ **Nunca guardes credenciales en control de versiones**
- ⚠️ **En producción, usar variables de entorno** para credenciales
- ✅ **El token de confirmación es único y criptográficamente seguro**
- ✅ **Los tokens expiran en 24 horas**
- ✅ **El usuario NO obtiene JWT hasta confirmar email**

---

## 8. Estructura de la Base de Datos Actualizada

```sql
-- Columnas añadidas a tabla Users:
IsEmailConfirmed BIT DEFAULT 0
EmailConfirmationToken NVARCHAR(MAX) NULL
EmailConfirmationTokenExpiry DATETIME2 NULL
```

Usuario no confirmado:
```
IsEmailConfirmed: 0
EmailConfirmationToken: "bXF0ZF...base64..."
EmailConfirmationTokenExpiry: 2026-04-17 10:30:00
```

Usuario confirmado:
```
IsEmailConfirmed: 1
EmailConfirmationToken: NULL
EmailConfirmationTokenExpiry: NULL
```
