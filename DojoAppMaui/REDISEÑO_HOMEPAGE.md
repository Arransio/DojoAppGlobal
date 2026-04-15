# Rediseño del flujo de selección paso a paso en HomePage

## Descripción General

Se ha implementado un flujo de selección **progresivo y guiado** para mejorar significativamente la experiencia del usuario al seleccionar productos. El nuevo diseño implementa un patrón de **acordeón expandible** donde cada paso depende del anterior.

## Cambios Realizados

### 1. **Modelo de datos mejorado** (`Models/Product.cs`)
Se añadieron propiedades para soportar el flujo paso a paso:
- `IsExpanded`: Controla si el producto está expandido o colapsado
- `AvailableSizes`: ObservableCollection de tallas disponibles
- `SelectedSizeStep`: Talla seleccionada en el paso 1
- `AvailablePrimaryColors`: Colores primarios disponibles para la talla seleccionada
- `SelectedPrimaryColor`: Color primario seleccionado en el paso 2
- `AvailableSecondaryColors`: Colores secundarios disponibles para la combinación seleccionada
- `SelectedSecondaryColor`: Color secundario seleccionado en el paso 3
- `SelectedVariant`: La variante del producto finalmente seleccionada
- **Propiedades computed**: `CanSelectSize`, `CanSelectPrimaryColor`, `CanSelectSecondaryColor`, `CanAddToCart`

Nueva clase `ColorOption` para representar opciones de color con `Id` y `Name`.

### 2. **Interfaz XAML rediseñada** (`Views/HomePage.xaml`)
La nueva estructura implementa un flujo visual claro:

```
┌─────────────────────────────────────┐
│ 🔽 Nombre del Producto | Precio    │  ← Header expandible
├─────────────────────────────────────┤
│ Step 1: Selecciona una talla        │
│ [Talla 1] [Talla 2] [Talla 3]      │  ← Solo visible si expandido
├─────────────────────────────────────┤
│ Step 2: Selecciona color principal  │
│ [Color1] [Color2] [Color3]          │  ← Solo visible si talla seleccionada
├─────────────────────────────────────┤
│ Step 3: Selecciona color secundario  │
│ [Color1] [Color2] [Color3]          │  ← Solo visible si color primario seleccionado
├─────────────────────────────────────┤
│ [Añadir al carrito]                 │  ← Solo habilitado si paso 3 completado
├─────────────────────────────────────┤
│ Seleccionado: Talla, Color Principal │  ← Resumen dinámico
│              Color Secundario         │
└─────────────────────────────────────┘
```

**Características:**
- Producto mostrado como Frame expandible con header
- Cada paso se muestra únicamente cuando el anterior se completa
- Resumen dinámico de selecciones
- Colores visuales distintos para cada tipo de botón:
  - Azul (#2196F3) para tallas
  - Naranja (#FF9800) para colores primarios
  - Rojo (#F44336) para colores secundarios
  - Verde (#4CAF50) para "Añadir al carrito"

### 3. **Lógica mejorada** (`Views/HomePage.xaml.cs`)
Métodos principales del nuevo flujo:

- **`OnProductHeaderClicked()`**: Toggle de expansión, reinicia selecciones al cerrar
- **`OnSizeStepClicked()`**: Selecciona talla y carga colores primarios disponibles
- **`LoadPrimaryColorsForSize()`**: Filtra colores primarios según talla seleccionada
- **`OnPrimaryColorClicked()`**: Selecciona color primario y carga colores secundarios disponibles
- **`LoadSecondaryColorsForSizeAndPrimaryColor()`**: Filtra colores secundarios según combinación seleccionada
- **`OnSecondaryColorClicked()`**: Selecciona color secundario y almacena la variante final
- **`FindProductInHierarchy()`**: Busca el producto en el árbol visual (reemplaza accesos complejos al padre)
- **`ResetProductSelection()`**: Reinicia todas las selecciones de un producto

### 4. **Convertidores de valores** (`Converters/SelectionConverters.cs`)
Convertidores XAML simplificados:
- `BoolToOpacityConverter`: Controla opacidad del botón (1.0 si habilitado, 0.5 si deshabilitado)
- `IsNotNullConverter`: Determina visibilidad basada en si un valor es null

### 5. **Configuración de recursos** (`App.xaml`)
Se registraron los convertidores en los recursos de la aplicación para su uso global en XAML.

## Flujo de Interacción Esperado

1. **Usuario hace clic en un producto** → Se expande el acordeón
2. **Se muestran tallas disponibles** → Usuario selecciona una talla
3. **Se cargan colores primarios** → Se muestran únicamente los disponibles para esa talla
4. **Usuario selecciona color primario** → Se cargan colores secundarios disponibles
5. **Se muestran colores secundarios** → Se muestran únicamente los disponibles para esa combinación
6. **Usuario selecciona color secundario** → Se habilita el botón "Añadir al carrito"
7. **Usuario hace clic en "Añadir al carrito"** → El producto se añade al carrito con la variante completa seleccionada

## Ventajas del nuevo diseño

✅ **Progresivo**: Cada paso depende del anterior, evitando opciones inválidas
✅ **Guiado**: Interfaz clara que muestra exactamente qué hacer en cada paso
✅ **Eficiente**: Solo se muestran opciones válidas según las selecciones previas
✅ **Intuitivo**: Visual hierarchy clara con colores y estructura jerárquica
✅ **Flexible**: Permite reiniciar desde cualquier paso mediante colapsado
✅ **Mantenible**: Código limpio y bien estructurado con métodos reutilizables

## Compatibilidad

- ✅ Mantiene propiedades legacy (`Sizes`, `SelectedSize`) para compatibilidad con otros componentes
- ✅ Compatible con el CarritoService existente
- ✅ No requiere cambios en la API de backend

## Notas técnicas

- Se usa `ObservableCollection<T>` para que los cambios se reflejen automáticamente en la UI
- Los tipos de datos en DataTemplates están especificados con `x:DataType` para mejorar IntelliSense y performance
- El método `FindProductInHierarchy()` proporciona una forma robusta de acceder al contexto de datos sin depender de la estructura visual exacta
