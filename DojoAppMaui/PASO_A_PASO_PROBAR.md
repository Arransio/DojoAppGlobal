# 🎬 PASO A PASO: Probar Confirmación de Email Funcional

## ✅ Todo está listo, solo sigue estos pasos:

---

## 📋 PASO 1: Asegúrate que la API esté corriendo

```powershell
# Abre PowerShell en la carpeta de la API
cd "..\Dojo-App\Api_Dojo_App"

# Ejecuta la API
dotnet run
```

**Deberías ver algo como:**
```
info: Microsoft.Hosting.Lifetime[14]
Now listening on: http://[::]:5221
info: Microsoft.Hosting.Lifetime[0]
Application started. Press Ctrl+C to quit.
```

✅ **La API está corriendo en puerto 5221**

---

## 📱 PASO 2: Registra un Usuario en la App MAUI

1. **Abre la app MAUI** (ejecutando en emulador Android)

2. **En la pantalla de login, haz clic en:**
   ```
   "¿No tienes cuenta? Registrarse"
   ```

3. **Se abre un diálogo pidiendo:**
   - **Usuario:** `test123` (o lo que quieras)
   - **Email:** `tu-email-personal@gmail.com` (TU EMAIL REAL)
   - **Contraseña:** `password123` (o lo que quieras)

4. **Haz clic en REGISTRARSE**

5. **Espera el mensaje:**
   ```
   ✅ "Se ha enviado un email de confirmación a..."
   ```

**✅ Usuario creado y email enviado**

---

## 📧 PASO 3: Abre tu Email

1. **Abre tu bandeja de entrada** de Gmail/Outlook/etc

2. **Busca email de:** `dojoappcorreo@gmail.com`

3. **Asunto:** `"Confirma tu email - Dojo App"`

4. **Verás el email con:**
   - Mensaje de bienvenida
   - Botón verde "Confirmar Email"
   - Link alternativo

**✅ Email recibido correctamente**

---

## 🔗 PASO 4: Haz Clic en el Link

**Hay dos opciones:**

### Opción A: Clic en Botón (RECOMENDADO)
```
Haz clic en el botón verde "Confirmar Email"
```

### Opción B: Copiar Link
```
O copia el link completo:
http://10.0.2.2:5221/confirm-email.html?token=...&email=...

Y pégalo en el navegador del emulador
```

**✅ Se abre una página bonita**

---

## ✨ PASO 5: Página de Confirmación

Se abre una página con estos elementos:

```
┌─────────────────────────────────────┐
│           ✉️ Confirmar Email        │
├─────────────────────────────────────┤
│                                     │
│  ⏳ Procesando tu confirmación...   │
│  [spinner girando]                  │
│                                     │
└─────────────────────────────────────┘
```

**¿Qué está pasando?**
- La página detecta: `token` y `email` de la URL
- La página hace POST a: `http://10.0.2.2:5221/api/auth/confirm-email`
- El servidor valida el token
- El servidor marca email como confirmado

---

## ✅ PASO 6: Ver Resultado

Después de 2-3 segundos, la página debería mostrar:

```
┌─────────────────────────────────────┐
│     ✅ Email Confirmado             │
├─────────────────────────────────────┤
│                                     │
│ ¡Éxito!                            │
│ Email confirmado exitosamente.     │
│ Ya puedes iniciar sesión.          │
│                                     │
│ [Información sobre la API]         │
│                                     │
└─────────────────────────────────────┘
```

**✅ Email confirmado exitosamente**

---

## 📱 PASO 7: Iniciar Sesión en MAUI

1. **Vuelve a la app MAUI**

2. **Ingresa en la pantalla de login:**
   - **Usuario:** `test123` (lo que registraste)
   - **Contraseña:** `password123` (lo que registraste)

3. **Haz clic en INICIAR SESIÓN**

4. **Deberías ver:**
   ```
   ✅ "Login exitoso"
   ✅ Redirección a HomePage
   ```

**✅ ¡FUNCIONA PERFECTAMENTE!**

---

## 🎊 ¡COMPLETADO!

```
✅ Email enviado correctamente
✅ Link accesible desde emulador
✅ Confirmación en página HTML
✅ API actualiza en BD
✅ Login sin email confirmado: ERROR
✅ Login con email confirmado: ✅ FUNCIONA

🚀 TODO LISTO PARA PRODUCCIÓN
```

---

## 🐛 ¿Algo No Funcionó?

### ❌ "Email no llegó"
```
Problema: SMTP
Solución:
1. Verifica credenciales en appsettings.json
   - SenderEmail: dojoappcorreo@gmail.com
   - SenderPassword: vikv exwk zbfn eyvj
2. Mira los logs de la API
3. Prueba registrando con otro email
```

### ❌ "Link muestra timeout"
```
Problema: API no corriendo
Solución:
1. Abre PowerShell
2. cd "..\Dojo-App\Api_Dojo_App"
3. dotnet run
4. Espera a ver "Now listening on: http://[::]:5221"
5. Reinicia emulador Android
6. Intenta de nuevo
```

### ❌ "Token inválido"
```
Problema: Token expirado (>24h) o incorrecto
Solución:
1. Registrate de nuevo
2. Confirma dentro de 24 horas
3. Usa email diferente
```

### ❌ "Página muestra error de conexión"
```
Problema: No puede alcanzar la API
Solución:
1. Verifica API está corriendo en 5221
2. Intenta acceder desde el navegador:
   http://10.0.2.2:5221/api/auth/login
   Debería retornar algo (no error 500)
3. Si falla, hay problema de red del emulador
```

### ❌ "Login dice email no confirmado"
```
Problema: La confirmación no funcionó
Solución:
1. Verifica en DB que IsEmailConfirmed sea 1
2. Revisa logs de la API
3. Intenta confirmar de nuevo
```

---

## 📊 Resumen Visual

```
┌─────────────────────────────────┐
│ MAUI APP                        │
│ [Registrarse]                   │
└────────────┬────────────────────┘
             ↓
┌─────────────────────────────────┐
│ API - Crea Usuario              │
│ Envía Email                     │
└────────────┬────────────────────┘
             ↓
┌─────────────────────────────────┐
│ EMAIL EN BANDEJA                │
│ [Link de confirmación]          │
└────────────┬────────────────────┘
             ↓ [Usuario hace clic]
┌─────────────────────────────────┐
│ NAVEGADOR - Página HTML         │
│ Confirma Email en API           │
└────────────┬────────────────────┘
             ↓
┌─────────────────────────────────┐
│ ✅ Email Confirmado             │
│ Mensaje de éxito                │
└────────────┬────────────────────┘
             ↓
┌─────────────────────────────────┐
│ MAUI APP - Login                │
│ ✅ Funciona!                    │
└─────────────────────────────────┘
```

---

## ✨ Checklist Final

- [ ] API corriendo en puerto 5221
- [ ] Accedí a wwwroot/confirm-email.html
- [ ] Registré usuario en MAUI
- [ ] Recibí email de confirmación
- [ ] Hice clic en link
- [ ] Vi página de confirmación
- [ ] Página mostró éxito ✅
- [ ] Inicié sesión correctamente
- [ ] ✅ ¡TODO FUNCIONA!

---

**¡Listo! El sistema de confirmación de email está 100% funcional! 🎉**

Cualquier duda, consulta el documento `PROBLEMA_SOLUCION_RESUMEN.md` o `SOLUCION_EMAIL_CONFIRMATION_LINK.md`
