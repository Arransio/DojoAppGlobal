# 🔒 Configuración Segura de Credenciales

## ⚠️ IMPORTANTE: Nunca commits credenciales

Si accidentalmente commiteaste `appsettings.json` con credenciales, sigue estos pasos:

### 1. Crear `appsettings.local.json` (Ignorado por Git)

En `..\Dojo-App\Api_Dojo_App\` crear:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "SenderEmail": "tu-email@gmail.com",
    "SenderPassword": "tu-contraseña-de-app"
  }
}
```

### 2. Agregar a `.gitignore`

En la raíz del proyecto:

```
appsettings.local.json
appsettings.*.json
*.env
.env
.env.local
```

### 3. Usar en Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Cargar configuración
var env = builder.Environment.EnvironmentName;
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true);

// En desarrollo, también cargar .local
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
}
```

---

## 🌍 Variables de Entorno (Recomendado para Producción)

### Windows (PowerShell)

```powershell
$env:SMTP_SERVER = "smtp.gmail.com"
$env:SMTP_PORT = "587"
$env:SENDER_EMAIL = "tu-email@gmail.com"
$env:SENDER_PASSWORD = "tu-contraseña-de-app"
$env:FRONTEND_URL = "https://tudominio.com"
```

### Linux/Mac

```bash
export SMTP_SERVER="smtp.gmail.com"
export SMTP_PORT="587"
export SENDER_EMAIL="tu-email@gmail.com"
export SENDER_PASSWORD="tu-contraseña-de-app"
export FRONTEND_URL="https://tudominio.com"
```

### En Program.cs

```csharp
var smtpServer = builder.Configuration["EmailSettings:SmtpServer"] 
    ?? Environment.GetEnvironmentVariable("SMTP_SERVER");
```

---

## 🔐 Obtener Contraseña de App en Gmail

1. Ve a https://myaccount.google.com/
2. Clic en "Seguridad" (izquierda)
3. Busca "Contraseñas de app"
4. Selecciona "Mail" y "Windows Computer"
5. Copia la contraseña de 16 caracteres
6. Pégala en `appsettings.json` o variable de entorno

---

## ✅ Checklist de Seguridad

- [ ] No commitar `appsettings.json` con credenciales
- [ ] Crear `appsettings.local.json` para desarrollo
- [ ] Agregar a `.gitignore`
- [ ] Usar variables de entorno en producción
- [ ] Revisar commits anteriores si es necesario
- [ ] Cambiar contraseñas si fueron expuestas

---

## 🚀 En Azure / Cloud

Si despliegas en Azure:

```
Azure Portal → App Service → Configuration → Add new setting

EMAILSETTINGS__SMTPSERVER = smtp.gmail.com
EMAILSETTINGS__SMTPPORT = 587
EMAILSETTINGS__SENDEREMAIL = tu-email@gmail.com
EMAILSETTINGS__SENDERPASSWORD = tu-contraseña-de-app
```

---

## 📝 Ejemplo Completo Seguro

### Estructura recomendada:

```
..\Dojo-App\Api_Dojo_App\
├── appsettings.json ← Valores por defecto (SIN credenciales)
│   {
│     "EmailSettings": {
│       "SmtpServer": "${SMTP_SERVER}",
│       "SmtpPort": "${SMTP_PORT}",
│       "SenderEmail": "${SENDER_EMAIL}",
│       "SenderPassword": "${SENDER_PASSWORD}"
│     }
│   }
│
├── appsettings.local.json ← Desarrollo (EN .gitignore)
│   {
│     "EmailSettings": {
│       "SmtpServer": "smtp.gmail.com",
│       "SmtpPort": "587",
│       "SenderEmail": "mi-email@gmail.com",
│       "SenderPassword": "xyzt 1234 5678 91ab"
│     }
│   }
│
└── .gitignore (incluir appsettings.local.json)
```

---

## 🛡️ Best Practices

✅ **DO** - Hacer esto
- Usar variables de entorno en producción
- Crear appsettings.local.json para desarrollo
- Agregar a .gitignore archivos sensibles
- Cambiar contraseñas regularmente
- Usar "App Passwords" de Gmail, no contraseña cuenta

❌ **DON'T** - No hacer esto
- Commitear credenciales en código
- Usar contraseña de tu cuenta Gmail directamente
- Guardar tokens en código
- Compartir credenciales por email
- Hacer hardcode de URLs o emails

---

## 🔄 Si Ya Commiteaste Credenciales

### Opción 1: Cambiar Contraseña (Rápido)
1. Cambiar contraseña en Gmail
2. Actualizar en `appsettings.local.json`
3. Commit vacío para marcar que se revirtió

### Opción 2: Reescribir Historial (Avanzado)
```bash
# Usar git filter-branch o BFG Repo-Cleaner
# CUIDADO: modifica historio de commits
```

---

## 📚 Recursos Recomendados

- https://docs.microsoft.com/en-us/dotnet/architecture/cloud-native/security
- https://github.com/github/gitignore
- https://git-scm.com/docs/gitignore
- https://docs.microsoft.com/en-us/azure/app-service/configure-common

---

**Protege tus credenciales siempre! 🔒**
