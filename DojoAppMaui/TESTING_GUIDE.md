# Guía de Testing - Rediseño HomePage

## Pruebas Funcionales Recomendadas

### 1. **Test: Expandir/Contraer Producto**
- ✅ Hacer clic en el header del producto (nombre + precio)
- ✅ Verificar que se expanda mostrando Step 1 (tallas)
- ✅ Hacer clic de nuevo para contraer
- ✅ Verificar que al contraer se reseteen todas las selecciones

### 2. **Test: Selección de Talla**
- ✅ Expandir un producto
- ✅ Los Step 2 y 3 NO deben ser visibles inicialmente
- ✅ Hacer clic en una talla
- ✅ Verificar que se resalte/cambie visualmente
- ✅ Verificar que aparezca Step 2 (colores primarios)

### 3. **Test: Filtrado de Colores Primarios**
- ✅ Seleccionar una talla
- ✅ Verificar que solo se muestren colores primarios válidos para esa talla
- ✅ Seleccionar otra talla
- ✅ Verificar que los colores primarios cambien según la nueva talla
- ✅ El botón "Añadir al carrito" NO debe estar habilitado aún

### 4. **Test: Selección de Color Primario**
- ✅ Con una talla seleccionada, hacer clic en un color primario
- ✅ Verificar que se resalte visualmente
- ✅ Verificar que aparezca Step 3 (colores secundarios)
- ✅ Los colores secundarios mostrados deben ser válidos para esa talla + color primario

### 5. **Test: Filtrado de Colores Secundarios**
- ✅ Con talla y color primario seleccionados, cambiar el color primario
- ✅ Verificar que los colores secundarios disponibles cambien
- ✅ Seleccionar un color secundario de la lista anterior (debería ser inválido)
- ✅ Verificar que se resetee la selección secundaria

### 6. **Test: Habilitación del Botón Añadir al Carrito**
- ✅ Con talla seleccionada solamente: botón NO visible
- ✅ Con talla + color primario: botón NO visible
- ✅ Con talla + color primario + color secundario: botón VISIBLE y HABILITADO
- ✅ Opacidad del botón debe cambiar (0.5 si deshabilitado, 1.0 si habilitado)

### 7. **Test: Resumen de Selección**
- ✅ Sin selecciones: el resumen NO es visible
- ✅ Con talla seleccionada: muestra solo "Talla: X"
- ✅ Con talla + color primario: muestra "Talla: X" y "Color Principal: Y"
- ✅ Con selección completa: muestra "Talla: X", "Color Principal: Y", "Color Secundario: Z"

### 8. **Test: Agregar al Carrito**
- ✅ Con selección completa, hacer clic en "Añadir al carrito"
- ✅ Verificar visual feedback (cambio a verde + "Añadido")
- ✅ Verificar que el carrito se actualice (contador + total)
- ✅ Verificar que se reseteen todas las selecciones del producto

### 9. **Test: Reinicio de Selecciones**
- ✅ Seleccionar talla → contraer producto
- ✅ Expandir de nuevo: selecciones deben estar vaciadas
- ✅ Seleccionar talla → seleccionar color primario → cambiar talla
- ✅ Verificar que color primario y secundario se reseteen

### 10. **Test: Múltiples Productos**
- ✅ Expandir múltiples productos a la vez
- ✅ Hacer selecciones en diferentes productos
- ✅ Verificar que las selecciones de cada producto sean independientes
- ✅ Agregar ambos productos al carrito

## Casos Edge

### Test: Sin variantes disponibles
- ✅ Si un producto no tiene variantes: mostrar todas las tallas pero sin colores
- ✅ El botón "Añadir al carrito" debería estar deshabilitado

### Test: Colores duplicados
- ✅ Si existen colores con el mismo nombre: deberían deduplicarse
- ✅ No mostrar opciones duplicadas

### Test: Scroll en lista larga
- ✅ Si hay muchas tallas/colores: scroll debe funcionar dentro del FlexLayout
- ✅ Verificar que los botones sean clickeables después del scroll

## Criterios de Aceptación

✅ El flujo debe ser lineal y no permitir saltos (no puedo seleccionar color sin talla)
✅ Cada selección debe reflejarse visualmente
✅ El resumen debe ser siempre preciso
✅ Las selecciones deben persistir hasta que el usuario las cambie
✅ Al contraer/expandir debe resetear
✅ El carrito debe actualizarse correctamente
✅ No debe haber saltos visuales o parpadeos
✅ Todos los textos deben ser legibles (contraste de colores)

## Performance

- ⏱️ La expansión de un producto debe ser inmediata
- ⏱️ El cambio de talla debe mostrar colores primarios en menos de 100ms
- ⏱️ El cambio de color primario debe mostrar colores secundarios en menos de 100ms
- ⏱️ ScrollView del CollectionView debe ser smooth sin lag
