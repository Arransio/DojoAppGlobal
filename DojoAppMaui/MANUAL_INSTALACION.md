# 📱 Manual de Instalación - DojoApp Global

**Para personas que ya tienen la carpeta del proyecto descargada**

⏱️ Tiempo estimado: **20 minutos**

---

## 📋 Índice

1. [Verificar Requisitos](#1-verificar-requisitos)
2. [Restaurar Dependencias](#2-restaurar-dependencias)
3. [Configurar Base de Datos](#3-configurar-base-de-datos)
4. [Ejecutar el Proyecto](#4-ejecutar-el-proyecto)
5. [Solucionar Problemas](#5-solucionar-problemas)

---

## 1. Verificar Requisitos

Antes de empezar, asegúrate de tener instalado:

### ✅ .NET 8 SDK

Abre **PowerShell** y verifica:

```powershell
dotnet --version
```

Debes ver `8.0.x` o superior. Si no lo tienes, descárgalo de: https://dotnet.microsoft.com/download/dotnet/8.0

### ✅ Visual Studio 2022/2026

Debe tener instaladas las cargas de trabajo:
- **ASP.NET y desarrollo web**
- **Desarrollo móvil con .NET**

### ✅ SQL Server LocalDB

Viene incluido con Visual Studio. Para verificar:

```powershell
sqllocaldb info
```

---

## 2. Restaurar Dependencias

### 2.1 Backend (API)

Abre **PowerShell** en:

```powershell
cd "C:\Users\User\Desktop\DojoAppGlobal\Dojo-App\Api_Dojo_App"
```

Luego:

```powershell
dotnet restore
```

⏳ Espera 1-2 minutos a que termine.

### 2.2 Frontend (MAUI)

Abre otra **PowerShell** en:

```powershell
cd "C:\Users\User\Desktop\DojoAppGlobal\DojoAppMaui"
```

Luego:

```powershell
dotnet restore
```

⏳ Espera 2-3 minutos a que termine.

---

## 3. Configurar Base de Datos

### 3.1 Instalar Entity Framework CLI

En cualquier **PowerShell**, ejecuta (una sola vez):

```powershell
dotnet tool install --global dotnet-ef
```

### 3.2 Crear la Base de Datos

En **PowerShell**, navega al backend:

```powershell
cd "C:\Users\User\Desktop\DojoAppGlobal\Dojo-App\Api_Dojo_App"
```

Luego aplica las migraciones:

```powershell
dotnet ef database update
```

✅ Si ves **"Done. Success!"** la BD está lista.

### 3.3 Verificar la Base de Datos (Opcional)

En Visual Studio:

1. Ve a **View** → **SQL Server Object Explorer**
2. Busca `(localdb)\mssqllocaldb`
3. Expande **Databases**
4. Deberías ver `DojoAppDB`

---

## 4. Ejecutar el Proyecto

### Opción A: Desde Visual Studio (Recomendado ⭐)

#### Paso 1: Abrir la Solución

En Visual Studio:
1. **File** → **Open** → **Project/Solution**
2. Ve a: `C:\Users\User\Desktop\DojoAppGlobal\`
3. Abre `DojoAppGlobal.sln`

#### Paso 2: Ejecutar el Backend

1. En **Solution Explorer** (izquierda), haz clic derecho en `Api_Dojo_App`
2. Selecciona **Set as Startup Project**
3. Presiona `F5` o **Debug** → **Start Debugging**

💡 Verás una consola con los logs. Si ves `Now listening on: https://localhost:7088/` está funcionando.

#### Paso 3: Ejecutar el Frontend

1. En **Solution Explorer**, haz clic derecho en `DojoAppMaui`
2. Selecciona **Set as Startup Project**
3. En el dropdown superior, selecciona tu plataforma:
   - `net8.0-windows10.0.19041.0` (Windows)
   - `net8.0-android` (Android con emulador)
4. Presiona `F5`

✅ La app debería iniciar. Verás la pantalla de login.

---

### Opción B: Desde PowerShell (Terminal)

#### Backend (API)

Abre **PowerShell** en:

```powershell
cd "C:\Users\User\Desktop\DojoAppGlobal\Dojo-App\Api_Dojo_App"
dotnet run
```

Verás: `Now listening on: https://localhost:7088/`

#### Frontend (Windows)

Abre otra **PowerShell** en:

```powershell
cd "C:\Users\User\Desktop\DojoAppGlobal\DojoAppMaui"
dotnet run -f net8.0-windows10.0.19041.0
```

---

## 🔐 Credenciales de Prueba

Las credenciales de prueba **no se versionan** en este repositorio (un manual en git
es visible para cualquiera con acceso al historial). Pídelas al responsable del
proyecto, o crea una cuenta nueva con el botón **"Registrarse"**.

---

## 📍 URLs del Sistema

| Componente | URL | Puerto |
|-----------|-----|--------|
| **Backend API** | `https://localhost:7088/` | 7088 |
| **Android Emulator** | `http://10.0.2.2:5221/` | 5221 |

---

## 📊 Estructura del Proyecto

```
DojoAppGlobal/
│
├── Dojo-App/
│   └── Api_Dojo_App/              ← Backend (ASP.NET Core)
│       ├── Controllers/
│       ├── Models/
│       ├── Services/
│       ├── Migrations/
│       └── appsettings.json
│
└── DojoAppMaui/                   ← Frontend (MAUI)
    ├── Views/
    ├── ViewModels/
    ├── Services/
    ├── Models/
    ├── Resources/
    │   ├── AppIcon/
    │   ├── Splash/
    │   └── Images/
    └── DojoAppMaui.csproj
```

---

## 5. Solucionar Problemas

### ❌ "dotnet: No se reconoce como comando"

**Solución:** Reinicia PowerShell o instala .NET 8 SDK

```powershell
# Verificar
dotnet --version
```

---

### ❌ "Cannot connect to localhost:7088"

**Solución:**
1. Asegúrate de ejecutar el Backend primero
2. Verifica que no haya otro proceso en puerto 7088:

```powershell
netstat -ano | findstr :7088
```

Si hay algo, mata el proceso:

```powershell
taskkill /PID <número> /F
```

---

### ❌ "Database not found"

**Solución:**

```powershell
cd "C:\Users\User\Desktop\DojoAppGlobal\Dojo-App\Api_Dojo_App"
dotnet ef database update
```

---

### ❌ "MAUI workload not found"

**Solución:**

```powershell
dotnet workload restore
```

---

### ❌ "Port 5221 is already in use"

**Solución:**

```powershell
netstat -ano | findstr :5221
taskkill /PID <número> /F
```

---

### ❌ "Token is null in response"

**Solución:**
1. Verifica que el usuario existe en la BD y tiene el email confirmado
2. Revisa los logs del servidor en la consola
3. Asegúrate de que la BD se creó correctamente

---

### ❌ "Image resources not found"

**Solución:** Verifica que estos archivos existan en `DojoAppMaui/Resources/`:

```
✅ AppIcon/icono_icon.png
✅ Splash/icono_spalsh.png
✅ Images/icono_image.png
```

Si faltan, descárgalos de la carpeta del proyecto.

---

## ✅ Checklist de Instalación

Antes de empezar, verifica:

- [ ] .NET 8 SDK instalado
- [ ] Visual Studio 2022/2026 instalado
- [ ] Carpeta en `C:\Users\User\Desktop\DojoAppGlobal\`
- [ ] Backend restaurado: `dotnet restore` ✅
- [ ] Frontend restaurado: `dotnet restore` ✅
- [ ] Entity Framework CLI: `dotnet tool install --global dotnet-ef` ✅
- [ ] BD creada: `dotnet ef database update` ✅
- [ ] Backend ejecutándose en localhost:7088 ✅
- [ ] Frontend mostrando pantalla de login ✅
- [ ] Login funciona con las credenciales de prueba (pídelas al responsable) ✅

---

## 📝 Notas Importantes

- La **API debe ejecutarse antes que la app MAUI**
- Si tienes problemas, revisa los **logs de la consola**
- Para desarrolladores: Los debug statements están en los archivos `Services/`

---

## 🎯 Siguientes Pasos

Después de que todo funcione:

1. Explora las diferentes páginas de la app
2. Crea nuevas cuentas de usuario
3. Revisa los endpoints de la API en `https://localhost:7088/swagger`
4. Modifica estilos en `Views/` si lo deseas

---

**¿Necesitas ayuda?** Revisa la sección **"Solucionar Problemas"** anterior.

**Versión:** 3.0  
**Última actualización:** 2024
