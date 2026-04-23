# 📝 Implementación de Registro de Usuario con Throttling

## ✅ Lo que se implementó

He añadido un sistema completo de registro de usuario con las siguientes características:

### **1. Backend - Nuevo Endpoint `/api/auth/register`**

```csharp
[HttpPost("register")]
public IActionResult Register([FromBody] LoginRequest request)
{
    // Validaciones:
    // - Usuario mínimo 3 caracteres
    // - Contraseña mínimo 4 caracteres
    // - Genera JWT token automáticamente
    // - Retorna token + UserId + Message
}
```

**Reglas de validación:**
- ✅ Usuario debe tener mínimo 3 caracteres
- ✅ Contraseña debe tener mínimo 4 caracteres
- ✅ Los datos no pueden estar vacíos

### **2. Frontend - Nueva Interfaz de Registro**

#### **Flujo de Registro:**

```
┌─────────────────────────────────────────┐
│  USUARIO CLICA "¿No tienes cuenta?"     │
└────────────┬────────────────────────────┘
             │
             ▼ (Si no está en throttle)
┌─────────────────────────────────────────┐
│  MUESTRA DIÁLOGO: "Crear nueva cuenta"  │
│  Opciones: [Continuar] [Cancelar]       │
└────────────┬────────────────────────────┘
             │
             ▼ (Si elige Continuar)
┌─────────────────────────────────────────┐
│  PIDE NUEVO USUARIO (mín. 3 caracteres) │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│  PIDE NUEVA CONTRASEÑA (mín. 4 caract.) │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│  ENVÍA POST /api/auth/register          │
│  { username, password }                 │
└────────────┬────────────────────────────┘
       OK    │     ERROR
        │    ▼     ▼
        │  MOSTRAR ERROR
        │   (validación)
        │
        ▼
    GUARDAR TOKEN
    NAVEGAR A HOMEPAGE
    INICIA THROTTLE (60 segundos)
```

### **3. Throttling (Límite de 1 Minuto)**

El botón de registro tiene un sistema de throttling que:

- ✅ Restringe clicks a máximo 1 vez por minuto
- ✅ Muestra un contador regresivo: "Espera... (60s)"
- ✅ Desactiva el botón durante el tiempo de espera
- ✅ Restaura el estado después de 60 segundos

```csharp
// Cuando el usuario clica en menos de 60 segundos:
var timeSinceLastClick = DateTime.Now - _lastRegisterClickTime;

if (timeSinceLastClick.TotalSeconds < 60)
{
    var secondsRemaining = 60 - (int)timeSinceLastClick.TotalSeconds;
    // Mostrar: "Puedes volver a registrarte en {secondsRemaining} segundos"
}
```

---

## 📝 Archivos Modificados

### **1. Backend:**
- ✅ `Controllers/AuthController.cs` - Nuevo endpoint `[HttpPost("register")]`
- ✅ `Models/LoginResponse.cs` - Añadido campo `Message`

### **2. Frontend MAUI:**
- ✅ `Models/LoginResponse.cs` - Añadido campo `Message`
- ✅ `Services/AuthService.cs` - Nuevo método `RegisterAsync()`
- ✅ `ViewModels/LoginViewModel.cs` - Lógica de registro + throttling
- ✅ `Views/LoginPage.xaml` - Nuevo botón de registro

---

## 🧪 Casos de Prueba

### **Caso 1: Registro Exitoso**
```
1. Clica "¿No tienes cuenta?"
2. Elige "Continuar"
3. Ingresa usuario: "testuser" (mínimo 3 caracteres)
4. Ingresa contraseña: "test1234" (mínimo 4 caracteres)
5. Espera respuesta del servidor

Resultado Esperado:
✅ Alert: "Usuario registrado y autenticado"
✅ Navega a HomePage automáticamente
✅ Token se guarda
✅ Botón desactivado por 60 segundos
✅ Contador: "Espera... (60s)" → "Espera... (59s)" → ...
```

### **Caso 2: Usuario Muy Corto**
```
1. Clica "¿No tienes cuenta?"
2. Elige "Continuar"
3. Ingresa usuario: "ab" (menos de 3 caracteres)

Resultado Esperado:
❌ Alert: "El usuario debe tener mínimo 3 caracteres"
❌ No envía petición al servidor
✅ El botón sigue disponible (no aplica throttle)
```

### **Caso 3: Contraseña Muy Corta**
```
1. Clica "¿No tienes cuenta?"
2. Elige "Continuar"
3. Ingresa usuario: "testuser"
4. Ingresa contraseña: "123" (menos de 4 caracteres)

Resultado Esperado:
❌ Alert: "La contraseña debe tener mínimo 4 caracteres"
❌ No envía petición al servidor
✅ El botón sigue disponible
```

### **Caso 4: Cancelar Registro**
```
1. Clica "¿No tienes cuenta?"
2. Clica "Cancelar"

Resultado Esperado:
✅ Se cierra el diálogo
✅ Regresa a LoginPage
✅ El botón de registro sigue habilitado (no aplica throttle)
```

### **Caso 5: Throttling Activo**
```
1. Clica "¿No tienes cuenta?" (y completa el registro)
2. Espera 5 segundos
3. Intenta clicando de nuevo

Resultado Esperado:
❌ Alert: "Puedes volver a registrarte en 55 segundos"
❌ No abre el diálogo de registro
✅ Continúa mostrando el contador
```

### **Caso 6: Después del Throttling**
```
1. Clica "¿No tienes cuenta?"
2. Espera 60 segundos (o cierra la app)
3. Intenta clicando de nuevo

Resultado Esperado:
✅ Se abre nuevamente el diálogo
✅ Funciona normalmente (se reinicia el throttle)
```

---

## 🔍 Debug Output Esperado

### **Registro Exitoso:**
```
[LoginViewModel] Iniciando registro...
[AuthService] Register Response: {"token":"eyJ0eXAiOiJKV1QiLCJhbGc...","userId":1,"message":"Usuario registrado exitosamente"}
[AuthService] Registro exitoso, token recibido: eyJ0eXAiOiJKV1QiLCJhbGc...
[LoginViewModel] Token guardado correctamente
[LoginViewModel] Login exitoso, navegando a HomePage
```

### **Registro Fallido (Validación):**
```
[LoginViewModel] Iniciando registro...
[AuthService] Register error: 400 - Usuario debe tener mínimo 3 caracteres y contraseña mínimo 4
[LoginViewModel] Error en registro: Usuario debe tener mínimo 3 caracteres y contraseña mínimo 4
```

---

## 🛠️ Cómo Funciona el Throttling

```csharp
// Al hacer clic en el botón de registro:

1. Se calcula el tiempo desde el último click
2. Si pasaron menos de 60 segundos:
   - Muestra alerta: "Espera X segundos"
   - No permite continuar

3. Si pasaron 60+ segundos o es el primer click:
   - Abre el diálogo de registro
   - Guarda el tiempo actual: _lastRegisterClickTime = DateTime.Now
   - Inicia el timer: StartThrottleTimer()

4. El timer cuenta hacia atrás durante 60 segundos:
   - Desactiva el botón: IsRegisterButtonEnabled = false
   - Cambia el texto: "Espera... (60s)" → "Espera... (59s)" → ...
   - Después de 60s, restaura el estado inicial
```

---

## 📱 UI/UX Changes

### **Antes:**
```
┌─────────────────────────────┐
│   LOGO                      │
├─────────────────────────────┤
│ [Usuario____________________]│
│ [Contraseña________________]│
│ [Iniciar sesión]            │
├─────────────────────────────┤
│ ☐ Recordar usuario          │
│ ¿Olvidaste tu contraseña?   │
└─────────────────────────────┘
```

### **Después:**
```
┌─────────────────────────────┐
│   LOGO                      │
├─────────────────────────────┤
│ [Usuario____________________]│
│ [Contraseña________________]│
│ [Iniciar sesión]            │
│ ¿No tienes cuenta?          │ ← NUEVO
│  Registrarse                │
├─────────────────────────────┤
│ ☐ Recordar usuario          │
│ ¿Olvidaste tu contraseña?   │
└─────────────────────────────┘
```

---

## 🔐 Seguridad & Validaciones

✅ **Cliente:**
- Usuario mínimo 3 caracteres
- Contraseña mínimo 4 caracteres
- Campos no vacíos

✅ **Servidor:**
- Validación de entrada duplicada
- Longitudes máximas (50 caracteres)
- Manejo de errores HTTP 400/500

✅ **Throttling:**
- Evita spam de requests
- Limita a 1 registro por minuto
- Se reinicia con cada app restart

---

## 🚀 Próximas Mejoras Sugeridas

1. **Verificación de email:** Validar que el usuario no exista previamente
2. **Confirmación de email:** Enviar correo de confirmación
3. **Reset de throttling:** Añadir opción manual en config
4. **Persistencia:** Guardar último username (opcional)
5. **Biometría:** Autenticación con huella dactilar
6. **OAuth:** Integración con redes sociales

---

**Compilación:** ✅ Correcta (sin errores)
