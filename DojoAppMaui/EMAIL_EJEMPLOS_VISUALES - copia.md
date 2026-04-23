# Email de Confirmación: Ejemplos Visuales

## 📧 Plantilla HTML del Email

La plantilla que se envía es:

```html
<html>
  <body style='font-family: Arial, sans-serif;'>
    <h2>¡Bienvenido a Dojo App!</h2>
    <p>Por favor, confirma tu email haciendo clic en el siguiente link:</p>
    <p>
      <a href='http://localhost:5173/confirm-email?token=XXXXXX&email=user@example.com' 
         style='background-color: #4CAF50; color: white; padding: 10px 20px; 
                 text-decoration: none; border-radius: 4px; display: inline-block;'>
        Confirmar Email
      </a>
    </p>
    <p>O copia y pega este link en tu navegador:</p>
    <p>http://localhost:5173/confirm-email?token=XXXXXX&email=user@example.com</p>
    <p>Este link expirará en 24 horas.</p>
    <p>Si no creaste esta cuenta, ignora este email.</p>
  </body>
</html>
```

---

## 🎨 Cómo se ve en diferentes clientes:

### Gmail
```
┌─────────────────────────────────────────┐
│ ¡Bienvenido a Dojo App!                │
│                                         │
│ Por favor, confirma tu email haciendo   │
│ clic en el siguiente link:              │
│                                         │
│ [Confirmar Email] ← Botón verde        │
│                                         │
│ O copia y pega este link en tu          │
│ navegador:                              │
│ http://localhost:5173/confirm-email    │
│ ?token=XXX&email=user@example.com      │
│                                         │
│ Este link expirará en 24 horas.        │
│ Si no creaste esta cuenta, ignora.     │
└─────────────────────────────────────────┘
```

### Outlook
```
┌─────────────────────────────────────────┐
│ ¡Bienvenido a Dojo App!                │
│                                         │
│ Por favor, confirma tu email...         │
│                                         │
│ [Confirmar Email]                       │
│                                         │
│ Enlace alternativo: http://...          │
│ (24h validez)                           │
└─────────────────────────────────────────┘
```

---

## 📊 Estructura del Link de Confirmación

```
http://localhost:5173/confirm-email?token=XXXXXX&email=user@example.com
└──────────────┬──────────────────┘ └───┬───┘ └────────┬────────────────┘
       URL base                   path    query params
```

**Parámetros:**
- `token`: Token criptográfico único (43 caracteres base64)
- `email`: Email del usuario (URL-encoded)

**Ejemplo real:**
```
http://localhost:5173/confirm-email?token=bXF0ZF9jMTIz&email=juan%40example.com
```

---

## 🔐 Token de Confirmación

### Generación:
```csharp
// 32 bytes aleatorios
byte[] tokenData = new byte[32];
rng.GetBytes(tokenData);

// Convertido a base64 seguro para URL
string token = Convert.ToBase64String(tokenData)
  .Replace("/", "_")
  .Replace("+", "-");

// Ejemplo: "bXF0ZF9jMTIz..."
```

### Características:
- ✅ 32 bytes = 256 bits de entropía
- ✅ Criptográficamente seguro
- ✅ Único por usuario
- ✅ No predecible
- ✅ Válido por 24 horas

---

## 📝 Respuesta JSON de Registro

**Request:**
```json
POST /api/auth/register
{
  "username": "juan123",
  "email": "juan@example.com",
  "password": "myPassword123"
}
```

**Response:**
```json
200 OK
{
  "message": "Usuario registrado exitosamente. Por favor, confirma tu email.",
  "userId": 5
}
```

---

## 📝 Respuesta JSON de Confirmación

**Request:**
```json
POST /api/auth/confirm-email
{
  "email": "juan@example.com",
  "token": "bXF0ZF9jMTIz..."
}
```

**Response:**
```json
200 OK
{
  "message": "Email confirmado exitosamente. Ya puedes iniciar sesión."
}
```

---

## 🌐 Frontend - Página de Confirmación

La URL que el usuario cliquea:
```
http://localhost:5173/confirm-email?token=XXX&email=user@example.com
```

Debería redirigir a una página que:

1. **Lee los parámetros** de la URL
2. **Llama a** `POST /api/auth/confirm-email`
3. **Muestra uno de estos mensajes**:
   - ✅ "¡Email confirmado! Ahora puedes iniciar sesión"
   - ❌ "El token es inválido o ha expirado"
   - ❌ "Error al confirmar tu email"

---

## 🔄 Flujo Paso a Paso

### 1️⃣ Usuario hace clic en "Registrarse"
```
App MAUI
    ↓
Solicita: Username, Email, Contraseña
```

### 2️⃣ App envía datos
```
POST /api/auth/register
{
  "username": "juan123",
  "email": "juan@example.com",
  "password": "pass123"
}
```

### 3️⃣ Servidor procesa
```
API
├─ Valida datos ✓
├─ Crea usuario con IsEmailConfirmed=false
├─ Genera token único: "bXF0ZF9jMTIz..."
├─ Set expiry: ahora + 24 horas
└─ Envía email
```

### 4️⃣ Email llega a usuario
```
From: dojo-app@gmail.com
To: juan@example.com
Subject: Confirma tu email - Dojo App

¡Bienvenido a Dojo App!
Por favor, confirma tu email:
[Confirmar Email] ← CLIC AQUÍ
```

### 5️⃣ Usuario hace clic en "Confirmar Email"
```
Navegador abre:
http://localhost:5173/confirm-email?token=bXF0ZF9jMTIz...&email=juan@example.com
```

### 6️⃣ Frontend (React/MAUI) procesa
```
1. Lee parámetros de URL
2. Llamada POST /api/auth/confirm-email
3. Espera respuesta
4. Muestra: "¡Email confirmado!"
```

### 7️⃣ Servidor confirma
```
API
├─ Encuentra usuario por email
├─ Valida token
├─ Valida que no haya expirado
├─ Sets IsEmailConfirmed=true
├─ Limpia token y expiry
└─ Guarda cambios en BD
```

### 8️⃣ Usuario intenta login
```
App MAUI
├─ Usuario: juan123
├─ Contraseña: pass123
└─ Envía a POST /api/auth/login
```

### 9️⃣ Servidor valida
```
API
├─ Encuentra usuario
├─ Verifica contraseña ✓
├─ Verifica IsEmailConfirmed=true ✓
├─ Genera JWT token
└─ Retorna token
```

### 🔟 Usuario autenticado ✅
```
App guarda token y navega a HomePage
```

---

## ⚠️ Casos de Error

### Token inválido
```
POST /api/auth/confirm-email
{
  "email": "juan@example.com",
  "token": "token-incorrecto"
}

Response:
400 Bad Request
"Token inválido"
```

### Token expirado
```
POST /api/auth/confirm-email
{
  "email": "juan@example.com",
  "token": "token-valido-pero-viejo"
}

Response:
400 Bad Request
"El token ha expirado. Por favor, solicita un nuevo email de confirmación"
```

### Usuario no existe
```
POST /api/auth/confirm-email
{
  "email": "noexiste@example.com",
  "token": "algún-token"
}

Response:
400 Bad Request
"Usuario no encontrado"
```

### Email ya confirmado
```
POST /api/auth/confirm-email
{
  "email": "juan@example.com",
  "token": "token-valido"
}

Response:
400 Bad Request
"El email ya ha sido confirmado"
```

---

## 📋 Checklist de Configuración

Para que todo funcione:

- [ ] SMTP configurado en `appsettings.json`
- [ ] Email sender y password válidos
- [ ] Puerto SMTP correcto (587 para Gmail)
- [ ] `FrontendUrl` apuntando a tu cliente
- [ ] Migración ejecutada: `dotnet ef database update`
- [ ] Token SMTP genera emails correctamente
- [ ] Link de confirmación accesible
- [ ] Frontend captura parámetros de URL
- [ ] Frontend llama a `/api/auth/confirm-email`

---

## 🎯 Testing Completo

```
1. Registrarse ✓ → Email enviado
2. Abrir email ✓ → Link correcto
3. Clic en link ✓ → Confirmación exitosa
4. Intentar login ✓ → Token recibido
5. Usar token ✓ → Acceso a perfil
```

Si todo esto funciona, ¡la implementación está completa! 🎉
