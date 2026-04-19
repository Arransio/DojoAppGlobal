# Ejemplos de Prueba: Confirmación de Email

## Con cURL

### 1. Registro
```bash
curl -X POST http://localhost:5221/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "password123"
  }'
```

Respuesta esperada:
```json
{
  "message": "Usuario registrado exitosamente. Por favor, confirma tu email.",
  "userId": 1
}
```

### 2. Intentar Login sin Confirmar (Debe fallar)
```bash
curl -X POST http://localhost:5221/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "password123"
  }'
```

Respuesta esperada:
```json
"Por favor confirma tu email antes de iniciar sesión. Revisa tu bandeja de entrada."
```

### 3. Confirmar Email
```bash
# Obtener el token del email o de los logs
curl -X POST http://localhost:5221/api/auth/confirm-email \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "token": "COPIAR_TOKEN_DEL_EMAIL"
  }'
```

Respuesta esperada:
```json
{
  "message": "Email confirmado exitosamente. Ya puedes iniciar sesión."
}
```

### 4. Login Después de Confirmar (Debe funcionar)
```bash
curl -X POST http://localhost:5221/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "password123"
  }'
```

Respuesta esperada:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1
}
```

---

## Con Postman

### Configurar colección:

**1. Variable de entorno**
```
api_url = http://localhost:5221
token = (se llena después del login)
email = test@example.com
```

**2. Endpoints**

#### POST {{api_url}}/api/auth/register
Body (raw, JSON):
```json
{
  "username": "postman_user",
  "email": "{{email}}",
  "password": "pass1234"
}
```

#### POST {{api_url}}/api/auth/confirm-email
Body (raw, JSON):
```json
{
  "email": "{{email}}",
  "token": "COPIAR_DEL_EMAIL_O_LOGS"
}
```

#### POST {{api_url}}/api/auth/login
Body (raw, JSON):
```json
{
  "username": "postman_user",
  "password": "pass1234"
}
```

Post-script (guardar token):
```javascript
var jsonData = pm.response.json();
pm.environment.set("token", jsonData.token);
```

#### GET {{api_url}}/api/auth/profile
Headers:
```
Authorization: Bearer {{token}}
```

---

## Flujo Completo de Prueba Manual

### Paso 1: Registrar Usuario

**Entrada:**
- Username: `juan123`
- Email: `juan@example.com`
- Password: `miPassword456`

**Esperado:**
- Mensaje: "Usuario registrado exitosamente"
- Email recibido en `juan@example.com` con link de confirmación

### Paso 2: Probar Login Sin Confirmación

**Intentar loguear con:**
- Username: `juan123`
- Password: `miPassword456`

**Esperado:**
- Error: "Por favor confirma tu email antes de iniciar sesión"

### Paso 3: Confirmar Email

**Hacer clic en el link del email:**
```
http://localhost:5173/confirm-email?token=bXF0ZF...&email=juan@example.com
```

**Esperado:**
- Página muestra: "Email confirmado exitosamente"
- En logs: `[AuthController] Email confirmado para usuario: juan123`

### Paso 4: Login Exitoso

**Loguear con:**
- Username: `juan123`
- Password: `miPassword456`

**Esperado:**
- Token JWT recibido
- Mensaje de éxito
- Redirección a HomePage

---

## Pruebas de Validación

### Test: Email duplicado
```bash
# Primer registro - OK
curl -X POST http://localhost:5221/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user1",
    "email": "same@example.com",
    "password": "pass1234"
  }'

# Segundo registro con mismo email - DEBE FALLAR
curl -X POST http://localhost:5221/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user2",
    "email": "same@example.com",
    "password": "pass1234"
  }'
```

Esperado: `"El email ya está registrado"`

### Test: Email inválido
```bash
curl -X POST http://localhost:5221/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user",
    "email": "esto-no-es-email",
    "password": "pass1234"
  }'
```

Esperado: `"El email no es válido"`

### Test: Username corto
```bash
curl -X POST http://localhost:5221/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "ab",
    "email": "user@example.com",
    "password": "pass1234"
  }'
```

Esperado: `"Usuario debe tener mínimo 3 caracteres"`

### Test: Contraseña corta
```bash
curl -X POST http://localhost:5221/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user123",
    "email": "user@example.com",
    "password": "abc"
  }'
```

Esperado: `"Contraseña debe tener mínimo 4 caracteres"`

### Test: Token inválido
```bash
curl -X POST http://localhost:5221/api/auth/confirm-email \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "token": "token-incorrecto"
  }'
```

Esperado: `"Token inválido"`

### Test: Token expirado (simulado)
```bash
# Token que fue generado hace más de 24 horas
curl -X POST http://localhost:5221/api/auth/confirm-email \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "token": "token-expirado"
  }'
```

Esperado: `"El token ha expirado. Por favor, solicita un nuevo email de confirmación"`

---

## En Aplicación MAUI

### Prueba de Interfaz

1. **Iniciar la app MAUI**
2. **Clic en "¿No tienes cuenta? Registrarse"**
3. **Ingresar datos**:
   - Username: `mauiuser`
   - Email: `maui@test.com`
   - Password: `mauipass123`
4. **Esperar confirmación**: "Se ha enviado un email de confirmación a..."
5. **Abrir email y hacer clic en link**
6. **Confirmar**
7. **Volver a app e intentar login**
8. **Verificar que funciona**

---

## Notas de Debug

### Ver token de confirmación en logs:
```
[AuthController] Creando nuevo usuario: juan123
[AuthController] Usuario 'juan123' creado exitosamente con ID: 5
[EmailService] Email de confirmación enviado a: juan@example.com
```

### Obtener token del email:
Si usas Gmail con "App Password", verás el link en el email:
```
http://localhost:5173/confirm-email?token=bXF0ZF9jMTIz...&email=juan@example.com
```

### Extraer token para pruebas manuales:
```
token = bXF0ZF9jMTIz...
email = juan@example.com
```

Luego en cURL:
```bash
curl -X POST http://localhost:5221/api/auth/confirm-email \
  -H "Content-Type: application/json" \
  -d '{
    "email": "juan@example.com",
    "token": "bXF0ZF9jMTIz..."
  }'
```
