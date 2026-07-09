# Plan de mejoras DojoApp (MAUI + API)

> Auditoría completa realizada el 09/07/2026. Complementa a `EstudioMejoras.xdoc` (06/07/2026).
> Estado: Fase 1 completada (commit `dec6799`) · Fase 2 implementada sin commitear · Fase 2.5 NUEVA propuesta.
>
> Este documento sirve como **plan de acción** y como **documento de formación**: cada punto incluye una explicación de por qué el problema existe, qué riesgo real supone y cuál es el concepto técnico detrás.

---

## Registro de progreso

| Etapa | Estado | Fecha | Commit |
|---|---|---|---|
| Fase 1 — Seguridad del backend | ✅ Completada | 06/07/2026 | `dec6799` |
| Paso 0 — Remates y commit de la Fase 2 | ✅ Completada | 09/07/2026 | `19bb1da` |
| Fase 2 — Sesión en la app | ✅ Completada | 09/07/2026 | `19bb1da` |
| Fase 2.5 — Lógica de autorización backend | ✅ Completada | 09/07/2026 | `b805863` |
| Fase 3 — App: red y configuración (estructural) | ✅ Completada | 09/07/2026 | `7133e71` |
| Fase 4 — App: funcionalidad y UX | ⬜ Pendiente | — | — |
| Fase 5 — Operación | ⬜ Pendiente | — | — |
| Fase 6 — Despliegue a producción | ⬜ Pendiente (requiere hosting) | — | — |

> Regla acordada el 09/07/2026: este registro (y los checkboxes de cada fase) se actualizan en el momento en que se completa cada etapa, para tener constancia visual de lo ya implementado.

---

## Estado actual

**Fase 1 (backend, COMPLETADA — commit `dec6799`)**: `[Authorize]` en todos los controllers, roles en BD, secretos en user-secrets con clave JWT rotada, middleware global de errores, precio recalculado server-side, rate limiting en auth, HTTPS solo en prod. Verificada en auditoría: bien cerrada. El catálogo (GET products/colors/variants/campaigns) queda público a propósito (modo demo offline).

**Fase 2 (app, HECHA EN WORKING TREE — pendiente de commit)**:
- `AuthHttpHandler` (DelegatingHandler): añade Bearer y ante 401 cierra sesión y vuelve al login.
- Logout con confirmación en `UsuarioPage` (`ClearSession` selectivo, conserva perfil local).
- Auto-login en `LoginPage.OnAppearing` validando expiración del JWT (token demo excluido).
- Rol real desde el backend (eliminado hardcode `test1` de `StaticServices.cs`).
- "Recordar usuario" en login.

### Remates antes de commitear la Fase 2

**1. Carrera en `_redirectingToLogin` (`Services/AuthHttpHandler.cs:17`)**

*Qué pasa:* el flag es un `static bool` que se lee y se escribe desde varios hilos sin ninguna sincronización. Cuando una pantalla hace 2-3 llamadas HTTP a la vez (algo normal: cargar productos + colores + campaña) y el token ha caducado, **todas** reciben 401 casi simultáneamente. Cada hilo lee el flag, lo ve en `false`, y todos entran a la vez en el bloque de redirección.

*Por qué importa:* reemplazar `Application.Current.MainPage` dos o tres veces seguidas desde hilos distintos puede provocar excepciones de UI (la MainPage solo puede tocarse desde el hilo principal) o navegaciones dobles con pantallas fantasma. Es el tipo de bug que "a veces pasa y a veces no", el más difícil de reproducir y depurar.

*Concepto:* a esto se le llama **condición de carrera (race condition)**: el resultado depende del orden en que los hilos ejecutan instrucciones, que no controlas. La solución estándar en .NET para "solo el primer hilo pasa" es `Interlocked.CompareExchange`, una operación **atómica** (el hardware garantiza que leer-comparar-escribir ocurre como un solo paso indivisible): el primer hilo cambia el flag y entra; los demás ven que ya estaba cambiado y salen.

**2. Decidir `ExpireMinutes` 60 → 10080 (7 días) en `appsettings.json`**

*Qué pasa:* el diff sin commitear amplía la vida del JWT de 1 hora a 7 días, presumiblemente para que el auto-login sea útil (con 1 hora, el usuario tendría que reloguearse casi siempre).

*Por qué hay que pensarlo:* un JWT es **autocontenido**: el servidor no guarda ninguna lista de tokens emitidos, solo verifica la firma y la fecha de expiración. Eso significa que **no se puede revocar**: si a alguien le roban el token (móvil perdido, backup extraído, log filtrado), ese token funciona hasta que expire, y el servidor no tiene forma de invalidarlo. Con 1 hora, la ventana de abuso es pequeña; con 7 días, quien tenga el token es tú durante una semana. El logout de la app solo borra el token local — el token robado sigue vivo.

*Cómo lo resuelve la industria:* con **refresh tokens** — un token de acceso corto (15-60 min) más un token de refresco largo guardado en el servidor (y por tanto revocable) que se usa para pedir tokens de acceso nuevos. Es más trabajo de implementar. Para esta app, un término medio razonable es 2-3 días: suficiente para que el auto-login sea cómodo entre entrenamientos, sin regalar una semana entera a un token robado.

**3. `PedidosPage.xaml.cs` crea `new HttpClient(new AuthHttpHandler())` dentro de métodos (líneas ~122, 270, 315)**

*Por qué importa:* ver la explicación completa de socket exhaustion en la Fase 3. Como mínimo, unificarlo en un campo de la clase hasta que la Fase 3 lo arregle bien con `IHttpClientFactory`.

- [x] Arreglar carrera del flag en `AuthHttpHandler` (`Interlocked.CompareExchange`). *(09/07/2026)*
- [x] Expiración del JWT decidida: **3 días (4320 min)** — equilibrio auto-login/ventana de robo. *(09/07/2026)*
- [x] Unificar los `HttpClient` de `PedidosPage` en un campo. *(09/07/2026)*
- [ ] Commit de la Fase 2 (13 archivos + `AuthHttpHandler.cs` nuevo).

---

## Hallazgos críticos — Backend

### ALTA-1 · Suplantación en pedidos: el `UserId` viene del cliente
`PedidosController.cs:29,101-102`

*Qué pasa:* `POST /api/pedidos/create` lee `request.UserId` y `request.CustomerName` del **cuerpo de la petición** y los guarda tal cual. El `[Authorize]` solo comprueba que quien llama tiene *algún* token válido — no comprueba que el `UserId` del body sea el suyo.

*Por qué es grave:* cualquier usuario registrado puede abrir Postman (o modificar la app) y enviar `{"userId": 7, ...}` para crear pedidos a nombre del usuario 7. En esta app el daño concreto es: pedidos falsos que el admin marcará como pendientes de pago a la persona equivocada, reportes y totales por usuario corruptos, y discusiones de "yo no pedí esto". No hace falta ser un hacker: basta interceptar la petición con cualquier proxy.

*Concepto:* esto es un **IDOR** (Insecure Direct Object Reference), una de las vulnerabilidades más comunes del OWASP Top 10 (categoría "Broken Access Control", la nº 1 en 2021). La regla de oro: **la identidad siempre sale del token, jamás del body**. El token está firmado criptográficamente por el servidor — el cliente no puede falsificarlo; el body lo escribe el cliente — puede poner lo que quiera. El propio controller ya hace lo correcto en otro sitio: `IsOwnerOrAdmin` (línea ~257) lee el claim `NameIdentifier` del token. Es aplicar ese mismo patrón en la creación.

*Regla general que enseña este bug:* todo dato del cliente es hostil hasta que se demuestre lo contrario. El servidor debe derivar (identidad) o validar (resto de campos) todo lo que recibe.

### ALTA-2 · El borrado en cascada destruye el histórico de pedidos
`ProductsController.cs:69-80` + FKs de `InitialCreate` (Cascade en `Product→ProductVariants` y `ProductVariant→PedidoItems`)

*Qué pasa:* las claves foráneas se crearon con `ON DELETE CASCADE`: al borrar una fila padre, la base de datos borra automáticamente todas las hijas. La cadena aquí es `Product → ProductVariant → PedidoItem`. Si un admin borra un producto del catálogo (algo legítimo: "ya no vendemos este kimono"), SQLite borra sus variantes **y las líneas de pedidos ya realizados** que apuntaban a esas variantes.

*Por qué es grave:* los pedidos históricos quedan mutilados en silencio — un pedido de 3 artículos pasa a tener 2, su total ya no cuadra con lo que se cobró, y los reportes de campañas pasadas mienten. No hay error, no hay aviso, no hay vuelta atrás (salvo backup). Es **pérdida de datos de negocio provocada por una operación de mantenimiento rutinaria**.

*Concepto:* la cascada es apropiada para datos que "pertenecen" al padre y no tienen valor propio (ej.: borrar un usuario → borrar sus preferencias). Es peligrosa cuando las filas hijas son **hechos históricos**: un `PedidoItem` no es un apéndice del producto, es el registro de una transacción que ocurrió. Los hechos no se borran por rebote. Los colores ya están bien protegidos con `Restrict` (`AppDbContext.cs:36,42`) — la BD rechaza el borrado si hay referencias; productos y campañas no lo están.

*Soluciones (de menos a más trabajo):*
1. Cambiar las FKs a `Restrict` (una migración): borrar un producto con pedidos da error explícito. Honesto pero incómodo.
2. **Soft-delete** (recomendado): añadir `IsActive` a `Product`; "borrar" = marcarlo inactivo. Desaparece del catálogo pero el histórico queda íntegro. Es el patrón estándar en sistemas de venta: los productos no se borran, se retiran.

### ALTA-3 · Links de confirmación de email rotos fuera del emulador
`appsettings.json:21` → `AuthController.cs:133`

*Qué pasa:* el email de confirmación construye su enlace con `AppSettings:FrontendUrl`, que vale `http://10.0.2.2:5221`. Esa IP es un artefacto del **emulador de Android**: dentro del emulador, `10.0.2.2` es un alias que redirige al `localhost` del PC anfitrión (el emulador es una máquina virtual con su propia red, así que su `localhost` es él mismo, no tu PC). Fuera del emulador, `10.0.2.2` no es nada.

*Por qué es grave:* el usuario que se registre desde un móvil real recibirá un correo cuyo enlace no abre nada. No podrá confirmar su cuenta y, por el flujo actual, no podrá iniciar sesión jamás (ver ALTA-4). Es un fallo que no se detecta en desarrollo porque en el emulador *sí* funciona — la clase de bug más traicionera: solo aparece en producción.

*Concepto:* las URLs dependen del **entorno** (desarrollo/producción) y por eso pertenecen a configuración por entorno (`appsettings.Development.json` vs `appsettings.Production.json`, o variables de entorno), nunca a un valor único compartido. ASP.NET Core tiene este mecanismo integrado: la configuración se superpone por capas según `ASPNETCORE_ENVIRONMENT`.

### ALTA-4 · Cuenta huérfana irrecuperable si falla el email
`AuthController.cs:127-142`, `EmailService.cs:61-65`

*Qué pasa:* el registro hace dos operaciones en orden: (1) guarda el usuario en BD, (2) envía el email de confirmación. Si el paso 2 falla (Gmail caído, sin red, app-password inválida…), el usuario **ya está guardado** pero sin confirmar. Consecuencias en cadena: no puede iniciar sesión (el login exige cuenta confirmada), no puede volver a registrarse (username y email ya existen en BD), y **no existe endpoint de reenvío** del correo. Cuenta bloqueada para siempre, y el usuario no entiende por qué. Encima, el fallo del email solo se registra con `Debug.WriteLine`, que en Release no escribe nada: en producción ni te enterarías.

*Concepto:* esto es un problema de **operaciones parcialmente completadas** (falta de atomicidad entre pasos). Cuando un proceso tiene varios pasos y alguno puede fallar, hay que diseñar el camino de recuperación. Opciones estándar:
1. **Endpoint de reenvío de confirmación** (`POST /api/auth/resend-confirmation`): convierte el estado "creado sin confirmar" en recuperable. Es la solución mínima y la que usan todos los servicios reales ("¿No recibiste el correo? Reenviar").
2. Compensación: si el email falla, borrar el usuario recién creado y devolver un error claro ("inténtalo de nuevo"). Más simple, pero pierde el registro si el fallo del email es transitorio.

La opción 1 es mejor porque además cubre el caso "el correo llegó a spam y lo borré".

### Media — Backend

**Mass-assignment en los POST de admin** (`ProductsController.cs:44`, `CampaignsController.cs:35`, `ColorsController`, `ProductVariantsController`)

*Qué pasa:* los endpoints de creación reciben directamente la **entidad de EF** (`Product`, `Campaign`…) como parámetro. El model binder de ASP.NET rellena *todas* las propiedades públicas que vengan en el JSON — incluidas las que nunca deberían venir del cliente: `Id`, `CampaignId`, o grafos anidados completos (`Product.ProductVariants` con lo que sea dentro).

*Por qué importa:* un cliente puede fijar un `Id` que pise otro registro, colgar el producto de otra campaña o inyectar variantes arbitrarias en la misma petición. Hoy el riesgo está acotado (solo admin puede llamar), pero es una mina: cualquier relajación futura de permisos o cualquier admin con la sesión robada tiene mucho más poder del previsto.

*Concepto:* se llama **over-posting / mass-assignment** y el antídoto son los **DTOs de entrada** (Data Transfer Objects): una clase que declara *exactamente* los campos que el endpoint acepta (`CreateProductRequest { Name, Price, Sizes }`) y nada más. El controller mapea DTO→entidad a mano. Beneficio extra: el contrato de la API deja de estar acoplado al esquema de la BD — puedes renombrar una columna sin romper la app móvil. `PedidosController` ya lo hace bien (`CreatePedidoRequest`); es extender el patrón.

**Fuga de `ex.Message` al cliente** (`AuthController.cs:153,229,279`)

*Qué pasa:* varios `try/catch` devuelven al cliente el mensaje de la excepción (`$"Error al registrarse: {ex.Message}"`). Los mensajes de excepción pueden contener rutas de archivos, SQL, nombres de tablas, versiones — información que ayuda a un atacante a mapear el sistema (a esto se le llama *information disclosure*).

*La ironía:* en Fase 1 se montó un `GlobalExceptionMiddleware` precisamente para responder errores genéricos al cliente y loguear el detalle en el servidor. Pero estos `try/catch` locales capturan la excepción **antes** de que llegue al middleware, puenteándolo. La corrección es dejar que las excepciones inesperadas suban (quitar los catch genéricos o relanzar) y capturar localmente solo lo que se vaya a manejar de verdad.

**Header `Authorization` completo volcado a Debug** (`AuthController.cs:162-165`)

*Por qué importa:* el header contiene el JWT completo. Un token en un log es un token robable: los logs se copian, se comparten para depurar, acaban en sistemas de terceros. Regla: **las credenciales nunca se loguean** — ni tokens, ni contraseñas, ni app-passwords. Si necesitas depurar, loguea "token presente: sí/no" o sus primeros 8 caracteres.

**`GET /api/productvariants/ensure/{productId}/{size}` crea datos siendo GET, y lo puede llamar cualquier usuario autenticado**

*Por qué importa:* dos problemas. (1) Semántica HTTP: GET debe ser **seguro e idempotente** (no modificar estado) — los intermediarios (cachés, prefetchers, crawlers) asumen que pueden repetir o cachear un GET sin consecuencias; un GET que inserta filas puede ejecutarse sin que nadie lo pida. (2) Autorización: crear variantes es una operación de catálogo → debería ser `[Authorize(Roles="admin")]` como el resto de POST de catálogo. Corrección: convertirlo en POST y restringirlo a admin.

**Contraseña mínima de 4 caracteres** (`AuthController.cs:83-84`)

*Por qué importa:* 4 caracteres se fuerzan por diccionario en segundos. Aunque hay rate limiting (10/min por IP), este se evade con IPs rotadas. Con BCrypt bien usado (como aquí) el hash es lento, pero eso no salva contraseñas de 4 caracteres. Estándar actual (NIST): mínimo 8, comprobar contra listas de contraseñas comunes, y **no** exigir reglas arcanas de símbolos (generan patrones predecibles tipo `Nombre1!`). Subir a 8 es una línea.

**Logging con `Debug.WriteLine` en `AuthController` y `EmailService`**

*Por qué importa:* `Debug.WriteLine` se **elimina en compilación Release** (está condicionado a `#if DEBUG`). Todo lo que se loguea así es invisible en producción: fallos de registro, de email, de confirmación… ocurren sin dejar rastro. `ILogger<T>` (que `PedidosController` ya usa) es la abstracción correcta: escribe en los proveedores configurados (consola, archivo, etc.), tiene niveles (`Information`, `Warning`, `Error`) y filtrado por configuración. Es unificar el proyecto en el patrón que ya existe a medias.

**Secretos en el historial de git**

*Situación:* la clave JWT del historial ya no vale (se rotó en Fase 1 — los tokens firmados con la vieja ya no validan). La app-password de Gmail del commit `268dd34` **sigue siendo válida**; decisión asumida de no rotarla.

*Concepto (para el documento de formación):* quitar un secreto del último commit **no lo elimina**: git guarda cada versión de cada archivo para siempre, y `git log -p` o cualquier herramienta de escaneo (trufflehog, gitleaks) lo encuentra en segundos. Por eso la respuesta correcta a un secreto commiteado nunca es "borrarlo del archivo" sino **rotarlo** (invalidar el viejo y emitir uno nuevo). La reescritura de historial (`git filter-repo`) es cosmética y problemática en repos compartidos; la rotación es lo que de verdad cierra la puerta.

---

## Hallazgos críticos — App MAUI

### ALTA-5 · La app solo funciona en el emulador de Android
`AuthService.cs:22,210`, `ProductService.cs:17`, `PedidosService.cs:16`, `OrderReportService.cs:13-14`, `DebugAuthService.cs:13`, `PedidosPage.xaml.cs:19`

*Qué pasa:* la URL `http://10.0.2.2:5221/` está escrita a mano en seis sitios. Como se explicó en ALTA-3, `10.0.2.2` solo existe dentro del emulador Android. Únicamente `ApiService` distingue plataforma (`ApiService.cs:11-15`) — y para colmo su rama no-Android apunta a `https://localhost:7088`, que no coincide **ni en puerto ni en esquema** con lo que usan los demás servicios. Resultado: en Windows, iOS o un móvil Android real, el login y todo el flujo de datos apuntan a una dirección que no responde.

*Por qué importa más allá del bug:* es el ejemplo perfecto de por qué la configuración duplicada es deuda técnica — hubo seis oportunidades de equivocarse y al menos dos valores distintos conviviendo. Cuando despliegues el API a un servidor real habría que cambiar seis archivos (y te dejarías alguno).

*Concepto:* **single source of truth**. Una clase `AppConfig` (o similar) con la URL base resuelta una sola vez según plataforma y entorno (Debug→emulador, Release→servidor real), y todos los servicios la consumen. Nadie más escribe URLs. En iOS hay un problema adicional: ATS (App Transport Security) bloquea `http://` por defecto en dispositivos reales — otra razón por la que solo el emulador Android funciona.

### ALTA-6 · `HttpClient` nuevo por servicio, y a veces por llamada
`ApiService.cs:21`, `AuthService.cs:20`, `ProductService.cs:12`, `PedidosService.cs:21`, `OrderReportService.CreateClient()` (uno por método), `PedidosPage.xaml.cs:122,270,315`

*Qué pasa:* cada servicio (y en `OrderReportService`, cada *llamada*) crea su propio `new HttpClient()`.

*Por qué es un problema real y no teórico:* `HttpClient` implementa `IDisposable`, pero desecharlo **no libera el socket TCP inmediatamente**. El sistema operativo mantiene la conexión en estado `TIME_WAIT` unos 2-4 minutos (es parte del protocolo TCP: asegura que no lleguen paquetes rezagados a una conexión nueva que reutilice el puerto). Si creas un cliente por llamada, cada petición deja un socket zombi; con uso intensivo se agotan los puertos disponibles y empiezan los `SocketException` aleatorios. A esto se le llama **socket exhaustion** y es probablemente el error más documentado del ecosistema .NET.

*Concepto:* la solución oficial es **`IHttpClientFactory`** (paquete `Microsoft.Extensions.Http`): gestiona un pool de handlers compartidos y reciclados (evita tanto el agotamiento de sockets como el problema opuesto — un handler eterno que no se entera de cambios de DNS). Además permite **clientes tipados**: registras en DI "el cliente de `ProductService` usa esta URL base, este timeout y lleva `AuthHttpHandler` en la cadena", y el servicio lo recibe por constructor ya configurado. Configuración de red en un solo sitio, en `MauiProgram.cs`.

### ALTA-7 · Sin timeouts, sin comprobación de conectividad, sin feedback de carga

*Qué pasa:* ningún `HttpClient` fija `Timeout` (el default es **100 segundos**), no se consulta `Connectivity.NetworkAccess` antes de llamar, y no existe **ni un solo** `ActivityIndicator` en las vistas (hay un estilo definido en `Styles.xaml` que nadie usa; `BaseViewModel.IsBusy` existe pero no está enlazado a nada).

*Por qué importa — la cadena completa:* usuario abre el catálogo con el API caído → la app lanza la petición → no hay spinner, así que la pantalla parece muerta → a los 100 segundos salta la excepción → que se traga un catch y muestra lista vacía. El usuario ha visto minuto y medio de nada seguido de un catálogo vacío falso. Para una app móvil (red móvil, cobertura intermitente) el manejo de red no es un extra, es el caso normal.

*Qué se espera de una app móvil bien hecha:* (1) timeout de 10-15 s — si el servidor no contestó en 15 s, no va a contestar; (2) comprobar `Connectivity.NetworkAccess != NetworkAccess.Internet` antes de llamar y avisar "sin conexión" al instante; (3) spinner mientras `IsBusy`; (4) distinguir en la UI "no hay datos" de "no se pudo cargar" (con botón reintentar).

### ALTA-8 · Carrito en memoria, frágil y sin cantidades
`CarritoService.cs:13,15-17,22,32`, `App.xaml.cs:9,17`, `CarritoPage.xaml.cs:105`, `PedidosService.cs:48`

*Qué pasa — tres problemas encadenados:*
1. **No persiste**: es una `List<CartItem>` en memoria. Android mata procesos en segundo plano con total normalidad para liberar RAM — el usuario añade 3 productos, le llaman por teléfono, vuelve, y el carrito está vacío. No es un caso raro: es el ciclo de vida normal de Android.
2. **Encapsulación rota**: `GetItems()` devuelve la **lista interna** (no una copia), y `CarritoPage` la vacía con `GetItems().Clear()`. Funciona *por accidente*. Cualquier código puede mutar el estado del carrito sin pasar por el servicio — cuando haya un bug de "el carrito se vació solo", será imposible saber quién lo hizo. Regla: una clase que gestiona estado debe exponer copias o interfaces de solo lectura (`IReadOnlyList`), y ofrecer métodos explícitos (`Clear()`, `Remove(item)`).
3. **Sin cantidades**: `Quantity = 1` hardcodeado; añadir dos veces el mismo producto crea entradas duplicadas en vez de agrupar (2 × kimono talla 4 = dos líneas). El backend ya soporta `Quantity` — es puramente un hueco del cliente.

*Solución:* serializar el carrito a JSON en `Preferences` en cada cambio y recargarlo al arrancar (para un carrito de <20 ítems es trivial y suficiente); al añadir, buscar si ya existe el mismo producto+talla+colores e incrementar cantidad; exponer la lista como solo lectura.

### ALTA-9 · Todos los pedidos van a la campaña 1
`CarritoPage.xaml.cs:96-97` (con su propio `// TODO` delatándolo)

*Qué pasa:* al confirmar el pedido se envía `campaignId = 1` fijo. El concepto de "campaña activa" existe en el backend (`GET /api/campaigns/active`) y la app tiene el método para consultarlo (`ApiService.GetActiveCampaignAsync`) — pero el flujo de pedido no lo llama.

*Por qué importa:* el día que se cree la campaña 2 (nueva temporada), **todos los pedidos nuevos seguirán registrándose en la campaña 1**: los reportes por campaña saldrán mal y no habrá error visible que lo delate. Es una bomba de relojería silenciosa: hoy funciona porque solo existe la campaña 1.

*Nota de diseño:* idealmente el `campaignId` ni siquiera debería venir del cliente — el servidor sabe cuál es la campaña activa y debería asignarla él (mismo principio que ALTA-1: no confiar en el cliente para datos que el servidor puede derivar).

### Media / estructural — App MAUI

**La inyección de dependencias es decorativa** (`MauiProgram.cs:24-26`)

*Qué pasa:* el contenedor DI solo registra `ApiService`, `MainViewModel` y `MainPage` — y esas tres cosas solo las usan páginas huérfanas que nunca se muestran. Todo lo que la app ejecuta de verdad se construye con `new` a mano dentro de las vistas (`new ProductService()`, `new OrderReportService()`, `new LoginViewModel()`…).

*Por qué importa:* la DI no es burocracia — resuelve problemas concretos. (1) **Un solo punto de configuración**: si `ProductService` necesita el `HttpClient` de la factory (Fase 3), con DI lo cambias en un sitio; con `new` disperso, en cada vista que lo crea. (2) **Testabilidad**: una vista que hace `new ProductService()` es imposible de probar sin API real; una que recibe `IProductService` acepta un fake. (3) **Ciclos de vida**: singleton vs transient gestionados por el framework en vez de con estáticos improvisados como `App.CarritoService`. La migración puede ser gradual: registrar servicios primero, páginas después.

**MVVM solo existe en Login; el resto es code-behind**

*Qué pasa:* `PedidosPage.xaml.cs` hace peticiones HTTP, deserializa JSON y aplica reglas de negocio directamente en el code-behind (líneas 26-61, 268-323). Existe la carpeta `ViewModels/` y un `BaseViewModel` con `IsBusy`, pero solo `LoginViewModel` se usa de verdad.

*Por qué importa:* el patrón MVVM separa **qué se muestra** (View: XAML + code-behind mínimo) de **qué está pasando** (ViewModel: estado + comandos) y de **cómo se obtienen los datos** (Services). Sin esa separación: no puedes probar la lógica sin arrancar UI, no puedes enlazar `IsBusy` a un spinner porque la lógica no vive en un objeto bindeable, y cada pantalla reinventa su manejo de errores. No hace falta rehacer la app: basta ir extrayendo la lógica a ViewModels pantalla a pantalla, empezando por las que toque modificar en Fase 4 (regla del boy scout: se refactoriza lo que ya ibas a tocar). `CommunityToolkit.Mvvm` (paquete oficial) elimina casi todo el boilerplate con `[ObservableProperty]` y `[RelayCommand]`.

**Código muerto: `MainPage`, `ProductsPage`, `MainViewModel`, `AppShell`, `DebugAuthService`**

*Por qué importa:* el código muerto no es neutro — cuesta tiempo de lectura ("¿esto se usa?"), aparece en las búsquedas, y puede morder: `AppShell.xaml.cs:10` **reasigna `MainPage` en su constructor**; si alguien lo instancia algún día por error, secuestra la navegación de toda la app. `AppShell` además registra rutas que sugieren un sistema de navegación que en realidad no se usa (la app navega con `NavigationPage` + reemplazo de `MainPage` desde `BottomNavBar`) — confunde sobre cómo funciona realmente la navegación. Borrar es gratis: git lo guarda todo si hiciera falta recuperarlo.

**Fallback silencioso de `TokenStorage` a `Preferences`** (`StaticServices.cs`, múltiples `catch {}`)

*Qué pasa:* `TokenStorage` intenta `SecureStorage` (cifrado: Keystore en Android, Keychain en iOS) y, ante **cualquier** excepción, cae a `Preferences` — que es un XML/plist **en texto plano**. El catch es genérico y silencioso: no distingue "estoy en un emulador sin Keystore" (el caso previsto) de cualquier otro fallo en un dispositivo real.

*Por qué importa:* si `SecureStorage` falla en un móvil real por cualquier motivo, el JWT queda en claro en el almacenamiento de la app sin que nadie se entere. Combinado con `allowBackup="true"` en el manifest (permite extraer los datos de la app vía `adb backup` en dispositivos sin root), el token es exfiltrable. Correcciones: limitar el fallback a `#if DEBUG` o registrar una advertencia visible cuando ocurra, y poner `allowBackup="false"` (una app con sesión no debería ser respaldable en claro).

**`usesCleartextTraffic="true"`** (`AndroidManifest.xml:4`)

*Por qué existe y cuál es el plan:* Android 9+ bloquea HTTP sin TLS por defecto; este flag lo permite globalmente, y hoy es **necesario** porque el API de desarrollo va por `http://`. El riesgo: el token viaja legible para cualquiera en la misma red WiFi. El plan correcto no es quitar el flag ya (rompería el desarrollo), sino: producción por HTTPS, y el permiso de cleartext limitado a debug (con `network_security_config.xml` se puede permitir cleartext solo hacia `10.0.2.2`).

**Manejo de errores por substring del mensaje** (`LoginViewModel.cs:192,199`)

*Qué pasa:* `ex.Message.Contains("401")` / `Contains("conexión")` para decidir qué mostrar al usuario.

*Por qué es frágil:* los mensajes de excepción son texto para humanos — cambian entre versiones de .NET y **entre idiomas del sistema operativo** (en un Android en inglés, "conexión" no aparecerá jamás). Lo robusto es usar la información estructurada: el tipo de excepción (`HttpRequestException` = problema de red) y el código de estado (`HttpRequestException.StatusCode` en .NET 5+, o comprobar `response.StatusCode` antes de lanzar).

**`async void` fuera de manejadores de eventos** (`PedidosPage.xaml.cs:26,331`, `LoginViewModel.cs:355`)

*Concepto:* un método `async void` es "dispara y olvida" de verdad: **no se puede esperar ni capturar sus excepciones** — si lanza, la excepción sube directa al hilo y **tumba la app entera**. Es tolerable únicamente en manejadores de eventos (la firma lo exige), y aun así deben envolver todo en try/catch. `LoadProducts()` como `async void` llamado desde `OnAppearing` significa: cualquier fallo de red no controlado durante la carga = crash. Regla: `async Task` siempre que la firma lo permita; `async void` solo en eventos y blindado.

**Historial de pedidos: placeholder** (`UsuarioPage.xaml.cs:206-209`)

El endpoint `GET /api/pedidos/user/{userId}` ya existe, está protegido con `IsOwnerOrAdmin`, y devuelve lo necesario. Solo falta la pantalla. Es de lo de más valor visible para el usuario a menor coste (Fase 4).

**Credenciales admin en `MANUAL_INSTALACION.md:184`** (`admin`/`Admin123!`)

Un manual versionado con credenciales en claro tiene el mismo problema que los secretos en git: quien tenga el repo, tiene la llave. Si esas credenciales existen en alguna BD real, rotarlas; el manual debe decir "credenciales: pídelas al administrador" o documentar cómo crear el primer admin.

---

## Hallazgos — Operación

### El API no aplica migraciones al arrancar
`Program.cs:86-111` (ni `Migrate()` ni `EnsureCreated()`; seed de roles con consulta síncrona)

*Qué pasa:* EF Core tiene migraciones (8, bien llevadas), pero nadie las aplica automáticamente: hay que ejecutar `dotnet ef database update` a mano. Peor: el bloque de seed de roles consulta `db.Users` **durante el arranque**; contra una BD nueva (tabla inexistente), esa consulta lanza excepción y **la aplicación no arranca**.

*Por qué importa:* cada despliegue depende de que alguien recuerde un paso manual no documentado. El día que añadas una columna y despliegues sin migrar, el API arrancará pero fallará en las consultas que toquen esa columna — errores 500 en producción por un olvido. Y montar el proyecto en otra máquina requiere conocimiento tribal.

*Solución:* `db.Database.Migrate()` al arranque (aplica solo las migraciones pendientes; si no hay ninguna, no hace nada) y el seed después, asíncrono y con try/catch + log. Para una app de este tamaño con SQLite, migrar en el arranque es la opción correcta sin matices (en sistemas grandes con múltiples instancias se separa, pero no es el caso).

### Backups manuales y un incidente real de corrupción
Directorio del API: `app.db`, `app - backup.db`, `app - backup2.db`, **`app.corrupt-20260701.db`**

*Qué cuenta ese archivo:* el 01/07/2026 la BD se corrompió de verdad — no es un riesgo teórico. Los backups actuales son copias manuales que se hacen "cuando alguien se acuerda"; la pregunta que define un backup es *¿cuántos datos pierdes si la BD muere ahora mismo?*, y hoy la respuesta es "todos desde la última copia manual, fecha desconocida".

*Solución proporcionada al proyecto:* una tarea al arranque del API (o programada) que copie `app.db` con fecha en el nombre y conserve las últimas N (rotación). SQLite tiene un mecanismo de backup en caliente (`VACUUM INTO` o la API de backup) que copia de forma consistente aunque haya conexiones abiertas — mejor que un `File.Copy` a pelo, que puede copiar un estado a medio escribir (posiblemente lo que produjo el `.corrupt`).

### Async incorrecto en endpoints clave
`Login` y `CreatePedidoFromCart` son síncronos (`SaveChanges`, `FirstOrDefault`)

*Concepto:* ASP.NET Core atiende cada petición con un hilo del *thread pool*. Una consulta síncrona a BD **bloquea ese hilo** mientras espera el disco; con `await` + métodos `Async`, el hilo se libera y atiende otras peticiones mientras tanto. Con poco tráfico no se nota; con un pico (todo el dojo pidiendo la semana de cierre de campaña), los endpoints síncronos agotan el pool y las peticiones se encolan. La corrección es mecánica: `SaveChanges()` → `await SaveChangesAsync()`, `FirstOrDefault()` → `await FirstOrDefaultAsync()`, y la firma a `async Task<IActionResult>`.

### Otros
- **`SmtpClient` → MailKit**: `System.Net.Mail.SmtpClient` está oficialmente marcado obsoleto por Microsoft (implementación incompleta de protocolos modernos); MailKit es la recomendación oficial. No urgente — el envío actual funciona — pero es la dirección correcta.
- **`JwtBearer` 8.0.5 vs resto de paquetes en 8.0.25**: mantener las versiones de un mismo release train alineadas evita incompatibilidades sutiles de dependencias transitivas — precisamente la clase de problema que ya mordió en Fase 1 (el conflicto de versiones de IdentityModel que rompía toda la validación JWT).
- **`RNGCryptoServiceProvider` deprecado** → `RandomNumberGenerator.Fill` (API moderna equivalente, cambio de dos líneas).
- **Sin Dockerfile ni perfiles de publicación ni CI**: el despliegue es artesanal. Opcional para el tamaño actual, pero un Dockerfile documenta *de forma ejecutable* todo lo que el API necesita para correr.

---

## Plan de acción por fases

### Paso 0 — Cerrar y commitear la Fase 2 (inmediato)
*Por qué primero:* hay ~2 sesiones de trabajo bueno viviendo solo en el working tree. Un `git checkout` descuidado, un problema de disco, o simplemente el paso del tiempo mezclándolo con otros cambios, y se pierde o se vuelve incommiteable. Commitear pronto y pequeño es la primera regla de higiene con git.
- [x] Arreglar carrera del flag en `AuthHttpHandler` (`Interlocked.CompareExchange`). *(09/07/2026)*
- [x] Expiración del JWT decidida: **3 días (4320 min)**. *(09/07/2026)*
- [x] Unificar los `HttpClient` de `PedidosPage` en un campo. *(09/07/2026)*
- [ ] Commit de la Fase 2.

### Fase 2.5 — NUEVA: lógica de autorización backend (~1 sesión)
*Por qué esta fase existe y va primero:* la Fase 1 protegió las **puertas** (quién puede llamar a qué endpoint); estos hallazgos son de **lógica interior** (qué hace el endpoint una vez dentro). La suplantación de pedidos (ALTA-1) y la cascada destructiva (ALTA-2) son los dos únicos puntos donde un usuario real puede causar daño hoy — datos falsos e histórico destruido — y ambos se corrigen en medio día.
- [x] `CreatePedidoFromCart`: `UserId` del claim del token (se eliminó del DTO junto con los precios); `CustomerName` validado (no vacío, máx. 100). *(→ ALTA-1, 09/07/2026)*
- [x] FKs `Product→Variant→PedidoItem` (y `Campaign→Products/Pedidos`) a `Restrict` + soft-delete `IsActive` en `Product` con filtro en todo el catálogo. Migración `SoftDeleteProductsAndRestrictDeletes` aplicada (con `defaultValue: true` corregido a mano: el scaffold generaba `false`, que habría ocultado el catálogo existente). *(→ ALTA-2, 09/07/2026)*
- [x] Sin `ex.Message` al cliente (Register/ConfirmEmail delegan en el middleware; el confirm por link devuelve HTML genérico + log). Header Authorization ya no se loguea. `ILogger` en AuthController y EmailService. *(09/07/2026)*
- [x] `POST /api/auth/resend-confirmation`: respuesta genérica exista o no la cuenta (evita enumeración de usuarios); regenera token de 24h. *(→ ALTA-4, 09/07/2026)*
- [x] `FrontendUrl` por entorno: valor del emulador en `appsettings.Development.json`, fail-fast en `Program.cs` si falta (en prod habrá que configurar la URL pública). *(→ ALTA-3, 09/07/2026)*
- [x] `ensure` de variantes → POST (verificado: GET ahora devuelve 405). **Desviación del plan:** se mantiene `[Authorize]` para cualquier usuario, no solo admin — el flujo normal de la app lo invoca al seleccionar una talla sin variante. DTOs de entrada en los 4 POST admin (`AdminRequests.cs`). Contraseña mínima 8. `RNGCryptoServiceProvider` → `RandomNumberGenerator` (adelantado de Fase 5). *(09/07/2026)*

*Verificación 09/07/2026: API arrancado en local — `GET /api/products` 200 con catálogo intacto tras la migración, `resend-confirmation` responde genérico con email inexistente, `ensure` por GET rechazado con 405. App MAUI recompilada OK tras el cambio a POST.*

### Fase 3 — App: red y configuración (parte ESTRUCTURAL)
*Por qué antes que la 4:* toda mejora de UX que se haga ahora heredaría los seis `10.0.2.2` y los `HttpClient` desechables. Arreglar los cimientos de red primero evita construir sobre ellos y luego re-tocar cada pantalla.

*Decisión del 09/07/2026:* la app aún no está en producción (BD local, acceso solo desde emulador), así que esta fase se limita a la **estructura** — que es precisamente la preparación de la futura migración a hosting: con la URL centralizada, migrar será cambiar una línea de configuración en vez de mezclar cambio de infraestructura y refactor de seis archivos a la vez. Los **valores** de producción (URL pública, HTTPS obligatorio, cleartext solo debug, ATS de iOS) se aplazan a la nueva **Fase 6 — Despliegue**, cuando exista el hosting.
- [x] Fuente única de URL base por plataforma (`Services/AppConfig.cs`): eliminados los seis `10.0.2.2` dispersos y el `https://localhost:7088` desalineado de ApiService. *(→ ALTA-5, 09/07/2026)*
- [x] `IHttpClientFactory` con clientes tipados en `MauiProgram` + timeout de 15 s. `AuthService` va sin `AuthHttpHandler` a propósito: su 401 es "credenciales incorrectas", no "sesión caducada". `PedidosPage` ya no hace HTTP (colores/variantes/ensure movidos a `ProductService`). *(→ ALTA-6, ALTA-7, 09/07/2026)*
- [x] `Connectivity.NetworkAccess` con fallo rápido en `AuthHttpHandler` + aviso en login; errores por tipo de excepción (`HttpRequestException` = red, `TaskCanceledException` = timeout) en vez de substrings. *(09/07/2026)*
- [x] Código muerto eliminado: `MainPage`, `ProductsPage`, `MainViewModel`, `ProductVariantColorViewModel`, `AppShell`, `DebugAuthService`, `AuthService.TestAuth` (app) y `WeatherForecast.cs` (API). Credenciales fuera de `MANUAL_INSTALACION.md`. *(09/07/2026)*
- [x] DI real para los SERVICIOS (clientes tipados + `ServiceHelper` como puente desde el code-behind). Las **páginas** siguen creándose con `new` — se migrarán gradualmente al tocarlas en la Fase 4 (regla del boy scout). *(09/07/2026)*

*Extra de esta fase: `PedidosService` ya no envía `UserId` ni precios (alineado con el contrato server-side de la Fase 2.5) y el mínimo de contraseña en el registro de la app pasó a 8, alineado con el backend.*
*Verificación 09/07/2026: API y app compilan sin errores (commit `7133e71`). Prueba funcional en emulador pendiente de la próxima sesión de trabajo manual.*

### Fase 4 — App: funcionalidad y UX
*Por qué en este orden interno:* historial y cantidades son lo de mayor valor visible; loaders y estados vacíos multiplican la sensación de calidad de todo lo demás.
- [ ] Historial de pedidos del usuario (el endpoint ya existe y está protegido — solo UI).
- [ ] Cantidad por ítem con agrupación en el carrito; carrito persistente en `Preferences`; `GetItems()` de solo lectura. *(→ ALTA-8)*
- [ ] Loaders (`ActivityIndicator` enlazado a `IsBusy`) y estados vacíos con distinción "sin datos" vs "error de red" + reintentar. *(→ ALTA-7)*
- [ ] Usar la campaña activa real en el flujo de pedido (idealmente, que la asigne el servidor). *(→ ALTA-9)*
- [ ] Al tocar cada pantalla, extraer su lógica a un ViewModel (regla del boy scout; valorar `CommunityToolkit.Mvvm`).

### Fase 5 — Operación
*Por qué existe:* todo lo anterior es inútil si la BD se corrompe sin backup (ya pasó una vez) o si un despliegue falla por una migración olvidada.
- [ ] `db.Database.Migrate()` en el arranque + seed asíncrono con try/catch y log.
- [ ] Backup automatizado de `app.db` con rotación (idealmente `VACUUM INTO`, no `File.Copy`).
- [ ] Async real en `Login` y `CreatePedidoFromCart`; `ILogger` en todo el API.
- [ ] `SmtpClient` → MailKit; `JwtBearer` alineado a 8.0.25. (~~`RNGCryptoServiceProvider`~~ ya hecho en Fase 2.5.)
- [ ] `allowBackup="false"` en AndroidManifest.
- [ ] Opcional: Dockerfile / perfil de publicación / CI básico.

### Fase 6 — NUEVA: Despliegue a producción (cuando exista el hosting)
*Por qué es una fase aparte (decisión del 09/07/2026):* los valores de producción no se pueden configurar contra un servidor que no existe. Al separarlos de la estructura (Fase 3), el día de la migración solo cambia UNA variable — la infraestructura — y cualquier fallo se depura contra una app cuyo código de red ya está probado. Requisito previo: Fases 3 y 5 completadas (en especial `Migrate()` automático y backups, que en un servidor remoto son imprescindibles).
- [ ] Contratar/preparar hosting para el API + decidir dónde vive la BD (¿SQLite en disco persistente? ¿migrar a un motor servidor?).
- [ ] URL pública del API en la configuración de la app (un solo valor gracias a `AppConfig` de Fase 3, Release→producción / Debug→emulador).
- [ ] HTTPS obligatorio: certificado en el servidor, quitar `usesCleartextTraffic` global (si acaso, `network_security_config.xml` permitiendo cleartext solo hacia `10.0.2.2` en debug), verificar ATS en iOS.
- [ ] `AppSettings:FrontendUrl` de producción (los links de los emails de confirmación) vía variable de entorno o `appsettings.Production.json`.
- [ ] Secretos de producción (Jwt:Key, password SMTP) como variables de entorno del servidor — nunca en archivos versionados.
- [ ] Pruebas end-to-end desde dispositivos reales (Android físico; iOS si aplica): registro con email real, login, pedido completo, reporte admin.
- [ ] Revisar el rate limiting con tráfico real detrás de NAT/proxy (la partición por IP puede agrupar usuarios).

---

## Decisiones tomadas (histórico)
- La Fase 3 se ejecuta solo en su parte estructural mientras no haya hosting; los valores de producción pasan a la Fase 6 — Despliegue (09/07/2026).
- El registro de progreso y los checkboxes del plan se actualizan al completar cada etapa (09/07/2026).
- Expiración del JWT: 3 días / 4320 min (09/07/2026).
- Catálogo GET público para no romper el modo demo offline (06/07/2026).
- `UsersController` y POST `api/pedidos` legacy eliminados por no estar protegidos (06/07/2026).
- No se rota la app-password de Gmail: riesgo asumido, sigue en el historial git (06/07/2026, reconfirmado 09/07/2026).
- La app no gestiona pasarela de pagos: solo pedidos y marcado manual de pagos por admin.
- La clave JWT filtrada en el historial quedó inutilizada al rotarse en Fase 1.
