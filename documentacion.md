# Sistema PQRS — Guía de uso y funcionamiento

Esta guía explica, paso a paso y sin tecnicismos, cómo se usa el sistema PQRS. Está escrita para **usuarios** (quienes radican y atienden solicitudes), **administradores** (quienes configuran el sistema) y **desarrolladores nuevos** (al final hay una sección técnica corta).

---

## Contenido

1. [¿Qué es el sistema PQRS?](#1-qué-es-el-sistema-pqrs)
2. [Conceptos básicos](#2-conceptos-básicos)
3. [Iniciar y cerrar sesión](#3-iniciar-y-cerrar-sesión)
4. [Roles: qué puede hacer cada persona](#4-roles-qué-puede-hacer-cada-persona)
5. [Recorrido por la pantalla principal](#5-recorrido-por-la-pantalla-principal)
6. [Nueva solicitud (radicar)](#6-nueva-solicitud-radicar)
7. [Bandeja de solicitudes](#7-bandeja-de-solicitudes)
8. [Detalle de una solicitud (atenderla)](#8-detalle-de-una-solicitud-atenderla)
9. [Cómo se calculan las fechas de vencimiento](#9-cómo-se-calculan-las-fechas-de-vencimiento)
10. [Reportes](#10-reportes)
11. [Administración](#11-administración)
12. [Cómo crear un usuario (paso a paso)](#12-cómo-crear-un-usuario-paso-a-paso)
13. [Puesta en marcha para una organización nueva](#13-puesta-en-marcha-para-una-organización-nueva)
14. [Preguntas frecuentes y solución de problemas](#14-preguntas-frecuentes-y-solución-de-problemas)
15. [Sección para desarrolladores](#15-sección-para-desarrolladores)

---

## 1. ¿Qué es el sistema PQRS?

PQRS significa **P**eticiones, **Q**uejas, **R**eclamos y **S**ugerencias. Es el canal con el que las personas le comunican algo a una organización (una empresa, una entidad pública, un colegio, una clínica, etc.):

| Tipo | Ejemplo |
|---|---|
| **Petición** | "Necesito una copia de mi contrato." |
| **Queja** | "El funcionario de la ventanilla me atendió mal." |
| **Reclamo** | "Me cobraron dos veces la misma factura." |
| **Sugerencia** | "Deberían abrir una hora más temprano." |

Este programa sirve para **registrar** cada una de esas solicitudes, **asignarlas** a quien debe atenderlas, **responderlas** dentro de un plazo, y **llevar un historial** de todo lo que pasó. Además, permite sacar **reportes** para saber cómo va la atención.

Es una aplicación de escritorio para Windows: se abre como cualquier programa.

> Los nombres de los tipos (Petición, Queja…), las categorías, las áreas y el nombre de la organización **se pueden cambiar** desde la pantalla de Administración. Por eso el sistema sirve para cualquier organización.

---

## 2. Conceptos básicos

### 2.1 Solicitud y radicado
Cada PQRS registrada se llama **solicitud**. Al guardarla, el sistema le asigna un número único llamado **radicado**. Con ese número se puede encontrar la solicitud siempre.

Formato del radicado: `PREFIJO-AÑO-CONSECUTIVO`

Ejemplo: **`P-2026-000014`**
- `P` → prefijo del tipo (Petición).
- `2026` → año en que se radicó.
- `000014` → es la solicitud número 14 de ese tipo en ese año.

Cada tipo lleva su propia numeración y esta se reinicia cada año.

### 2.2 Estados de una solicitud
Una solicitud pasa por estados, como un trámite:

| Estado | Qué significa |
|---|---|
| **Radicada** | Recién registrada. Todavía nadie la ha empezado a atender. |
| **En trámite** | Ya alguien la está atendiendo. |
| **Pendiente de información** | Se necesita un dato o documento del solicitante antes de continuar. |
| **Respondida** | Ya se le entregó una respuesta oficial al solicitante. |
| **Cerrada** | El caso terminó definitivamente. |
| **Anulada** | Se canceló (por ejemplo, estaba duplicada o fue un error). |

**Recorrido normal:**

```
Radicada → En trámite → Respondida → Cerrada
                ↕
   Pendiente de información
```

Reglas del recorrido:
- Desde **Radicada** se puede pasar a *En trámite* o *Anulada*.
- Desde **En trámite** se puede pasar a *Pendiente de información*, *Respondida* (respondiendo) o *Anulada*.
- Desde **Pendiente de información** se puede volver a *En trámite* o *Anulada*.
- Desde **Respondida** se puede pasar a *Cerrada* o volver a *En trámite* (si el solicitante no quedó conforme y hay que reabrir).
- **Cerrada** y **Anulada** son finales: **ya no se puede modificar nada** (ni comentar, ni adjuntar, ni responder).
- Al estado *Respondida* **solo se llega escribiendo una respuesta**; no existe un botón para marcarla sin responder.

### 2.3 Prioridad
Cada solicitud tiene una prioridad: **Baja, Media, Alta o Urgente**. Sirve para que quien atiende identifique qué es más importante. **No cambia la fecha de vencimiento** (ver [sección 9](#9-cómo-se-calculan-las-fechas-de-vencimiento)).

### 2.4 Canal de recepción
Es por dónde llegó la solicitud: Presencial, Teléfono, Correo, Web, Redes sociales o Escrito.

### 2.5 Solicitud anónima
Una persona puede pedir no identificarse. Si se marca la casilla **"Solicitud anónima"**, no se guardan sus datos y la solicitud aparece con solicitante "Anónimo".

### 2.6 Historial
Todo lo que ocurre en una solicitud queda guardado en su **historial**: cuándo se radicó, quién la asignó, cada cambio de estado, cada comentario y cada archivo agregado, con fecha, hora y nombre del usuario. **El historial no se puede borrar.**

---

## 3. Iniciar y cerrar sesión

### Iniciar sesión
1. Abra el programa.
2. Escriba su **usuario** y su **contraseña**.
3. Presione **Ingresar** (o la tecla Enter).

Si los datos son incorrectos aparece el mensaje *"Usuario o contraseña incorrectos."* Por seguridad el sistema no dice cuál de los dos falló.

> Si es la primera vez que se usa el sistema, el administrador técnico le entregará el usuario inicial (ver [sección 15](#15-sección-para-desarrolladores)). Después de entrar por primera vez, **cambie esa contraseña** (ver [sección 12.4](#124-cambiar-la-contraseña-de-un-usuario)).

### Cerrar sesión
Arriba a la derecha de la ventana está el botón **Cerrar sesión**. Al presionarlo vuelve a la pantalla de ingreso. Hágalo siempre que deje el computador.

Al lado del botón verá su nombre y su rol, por ejemplo: *"María Pérez (Responsable)"*.

---

## 4. Roles: qué puede hacer cada persona

Cada usuario tiene **un rol** que decide qué puede hacer. Hay cuatro:

| Acción | Administrador | Supervisor | Responsable | Radicador |
|---|:---:|:---:|:---:|:---:|
| Iniciar sesión | ✅ | ✅ | ✅ | ✅ |
| Ver la bandeja y el detalle de las solicitudes | ✅ | ✅ | ✅ | ✅ |
| Radicar una nueva solicitud | ✅ | ✅ | ✅ | ✅ |
| Agregar comentarios | ✅ | ✅ | ✅ | ✅ |
| Adjuntar archivos | ✅ | ✅ | ✅ | ✅ |
| Cambiar el estado de una solicitud | ✅ | ✅ | ✅ | ❌ |
| Responder al solicitante | ✅ | ✅ | ✅ | ❌ |
| Eliminar adjuntos | ✅ | ✅ | ✅ | ❌ |
| Asignar un responsable | ✅ | ✅ | ❌ | ❌ |
| Ver **Reportes** y exportarlos a Excel | ✅ | ✅ | ❌ | ❌ |
| Usar **Administración** (usuarios, catálogos, organización) | ✅ | ❌ | ❌ | ❌ |

### Explicación de cada rol

**Administrador** — La persona de confianza que configura el sistema. Puede hacer todo. Es el único que crea usuarios y cambia los catálogos.

**Supervisor** — Coordina la atención. Reparte el trabajo (asigna responsables), atiende solicitudes y consulta los reportes para ver cómo va el equipo. No puede crear usuarios ni cambiar la configuración.

**Responsable** — Quien atiende las solicitudes: las pone en trámite, pide información, responde y cierra. No puede asignar trabajo a otros ni ver reportes.

**Radicador** — Quien recibe a las personas o llamadas y registra la solicitud (por ejemplo, el personal de ventanilla). Puede comentar y adjuntar documentos, pero no puede cambiar estados ni responder.

### Importante
- Lo que **no** le corresponde a su rol **no aparece** en pantalla (por ejemplo, un Radicador no ve los menús *Reportes* ni *Administración*).
- Además, aunque alguien lograra ver un botón, el sistema **vuelve a verificar su rol** al guardar. Si no tiene permiso, aparece el mensaje *"No tiene permisos para realizar esta acción."*
- Una solicitud **Cerrada** o **Anulada** no admite cambios de ningún rol.

---

## 5. Recorrido por la pantalla principal

Al iniciar sesión se abre la ventana principal. Tiene tres zonas:

```
┌───────────────────────────────────────────────────────────────┐
│ Nombre de la organización     Su nombre (Rol)  [Cerrar sesión]│  ← Barra superior
├───────────────┬───────────────────────────────────────────────┤
│ Bandeja de    │                                               │
│ solicitudes   │                                               │
│ Nueva         │        Aquí se muestra la pantalla            │
│ solicitud     │        que usted elija en el menú             │
│ Reportes *    │                                               │
│ Administración│                                               │
│ *             │                                               │
└───────────────┴───────────────────────────────────────────────┘
   Menú lateral              Zona de trabajo
```

- **Barra superior:** muestra el nombre de la organización, su usuario y el botón *Cerrar sesión*.
- **Menú lateral:** son los botones para cambiar de pantalla.
  - *Reportes* aparece solo para Administrador y Supervisor.
  - *Administración* aparece solo para Administrador.
- **Zona de trabajo:** el contenido de la pantalla elegida. Al entrar se abre la **Bandeja de solicitudes**.

A continuación, qué hace cada pantalla.

---

## 6. Nueva solicitud (radicar)

**Para qué sirve:** registrar una PQRS nueva. Es lo primero que se hace cuando una persona nos comunica algo.

**Cómo llegar:** menú lateral → **Nueva solicitud**.

### Campos del formulario
Los campos con asterisco (*) son obligatorios.

**Datos de la solicitud**

| Campo | Qué poner |
|---|---|
| **Tipo \*** | Petición, Queja, Reclamo, Sugerencia (u otros que haya creado el administrador). **Este campo define el plazo de respuesta.** |
| **Categoría \*** | El tema: por ejemplo "Facturación y pagos". La categoría puede asignar automáticamente el área que atiende. |
| **Canal de recepción** | Por dónde llegó (Presencial, Teléfono, Correo…). |
| **Prioridad** | Baja, Media, Alta o Urgente. |
| **Asunto \*** | Resumen corto (una línea). Ej.: "Cobro duplicado en factura de marzo". |
| **Descripción \*** | El detalle completo de lo que cuenta la persona. |

**Solicitante** (quien presenta la solicitud)

| Campo | Qué poner |
|---|---|
| **Solicitud anónima** | Marque esta casilla si la persona no quiere identificarse. Los campos del solicitante desaparecen. |
| **Tipo de documento** | CC, TI, CE, NIT, PAS u Otro. |
| **Número de documento \*** | Documento de identidad. |
| **Nombres \*** | Nombres de la persona. |
| **Apellidos** | Apellidos. |
| **Correo electrónico** | Opcional. Si lo escribe, debe tener formato válido (`nombre@dominio.com`). |
| **Teléfono** y **Dirección** | Opcionales. |

### Pasos
1. Elija el **Tipo** y la **Categoría**.
2. Escriba el **Asunto** y la **Descripción**.
3. Complete los datos del solicitante (o marque *Solicitud anónima*).
4. Presione **Radicar**.

### Qué pasa al radicar
- Aparece un mensaje verde, por ejemplo:
  *"Solicitud radicada con el número P-2026-000014. Vence el 06/02/2026."*
  **Anote o entregue ese radicado a la persona**, para que pueda consultar su caso.
- El formulario se limpia, listo para registrar otra solicitud.
- La solicitud queda en estado **Radicada** y aparece en la Bandeja.

### Datos del solicitante repetidos
Si la persona ya había presentado solicitudes antes (mismo tipo y número de documento), el sistema **reconoce a la persona** y no la duplica; solo actualiza sus datos de contacto con lo último que usted escribió.

### Si hay un error
Aparece un mensaje en rojo explicando qué falta (por ejemplo, *"El asunto es obligatorio."*). Corrija y presione **Radicar** otra vez. El botón **Limpiar** borra todo el formulario.

---

## 7. Bandeja de solicitudes

**Para qué sirve:** ver la lista de todas las solicitudes, buscarlas y abrir la que se quiera atender. Es la pantalla de trabajo diaria.

**Cómo llegar:** menú lateral → **Bandeja de solicitudes** (es la que se abre al iniciar sesión).

### La lista
Cada fila es una solicitud, con estas columnas: **Radicado, Tipo, Categoría, Asunto, Solicitante, Estado, Prioridad y Vence**.

- Las solicitudes aparecen de la **más reciente a la más antigua**.
- Las filas con **fondo rojizo** son solicitudes **vencidas** (se pasó el plazo y aún no se han respondido). Son las que hay que atender primero.
- Se muestran como máximo **500** solicitudes. Si hay más, use los filtros para acotar.
- Debajo de la lista aparece un resumen, por ejemplo: *"23 solicitudes (4 vencidas)"*.

### Los filtros
Arriba hay un recuadro gris para filtrar:

| Filtro | Para qué |
|---|---|
| **Buscar** | Escriba parte del radicado, del asunto o el número de documento del solicitante. Presione Enter o **Buscar**. |
| **Estado** | Muestra solo las que están en ese estado (o *Todos*). |
| **Tipo** | Muestra solo un tipo (o *Todos*). |
| **Desde / Hasta** | Rango de fechas de radicación (ambas fechas incluidas). |
| **Solo vencidas** | Marque la casilla para ver únicamente las vencidas. |

- **Buscar:** aplica los filtros.
- **Limpiar:** quita todos los filtros y vuelve a mostrar todo.

### Abrir una solicitud
Haga **doble clic** sobre la fila, o seleccione la fila y presione **Abrir solicitud**. Se abre la pantalla de detalle (sección siguiente).

**Ejemplo de uso diario:** al empezar el día, marque *Solo vencidas* y presione Buscar. Luego filtre por el estado *Radicada* para ver lo que aún nadie ha empezado.

---

## 8. Detalle de una solicitud (atenderla)

**Para qué sirve:** ver todo sobre una solicitud y hacer las gestiones: asignar, cambiar estado, responder, comentar y adjuntar.

**Cómo llegar:** desde la Bandeja, abriendo una solicitud. El botón **← Volver a la bandeja** regresa a la lista.

### Lo que se ve (de arriba hacia abajo)
1. **Encabezado:** el radicado, una etiqueta con el estado y, si corresponde, una etiqueta roja **"Vencida"**. Debajo, el asunto.
2. **Datos generales:** tipo, categoría, canal, área, responsable, prioridad, fecha de radicación, fecha de vencimiento y fecha de cierre.
3. **Solicitante:** nombre, documento y datos de contacto (o *"Solicitud anónima"*).
4. **Descripción:** el texto completo que escribió quien radicó.
5. **Respuestas:** las respuestas oficiales ya entregadas (en verde), con quién y cuándo.
6. **Acciones:** los botones para gestionar (según su rol y el estado).
7. **Adjuntos:** los archivos de la solicitud.
8. **Historial:** la línea de tiempo con todo lo ocurrido.

Si usted no tiene permiso para una acción, o la solicitud ya está cerrada o anulada, **esa acción no aparece**.

### 8.1 Asignar responsable
*(Administrador y Supervisor)*

Sirve para decidir **quién atiende** la solicitud.

1. En *Asignar responsable*, elija a la persona de la lista.
2. Presione **Asignar**.

Qué ocurre:
- La persona queda como **responsable**.
- Si la solicitud estaba *Radicada*, **pasa automáticamente a En trámite**.
- Si el responsable pertenece a un área, la solicitud toma esa área.
- Queda anotado en el historial como nota interna.

### 8.2 Cambiar estado
*(Administrador, Supervisor y Responsable)*

1. En *Cambiar estado*, elija el **nuevo estado** (la lista solo muestra los estados permitidos desde el actual).
2. Escriba el motivo o comentario. **Es obligatorio** si va a *Anular* o a pasar a *Pendiente de información*; en los demás casos es opcional.
3. Presione **Cambiar estado**.

Notas:
- Al **Cerrar** o **Anular**, se guarda la fecha de cierre y la solicitud queda bloqueada.
- *Respondida* no aparece en esta lista: para llegar a ella hay que **responder** (siguiente punto).

### 8.3 Responder al solicitante
*(Administrador, Supervisor y Responsable)*

Solo está disponible cuando la solicitud está **En trámite**.

1. En *Responder al solicitante*, escriba la respuesta.
2. Presione **Enviar respuesta**.

Qué ocurre:
- La respuesta queda registrada en la sección *Respuestas*.
- La solicitud pasa a **Respondida**.
- Para terminar el caso, después se usa *Cambiar estado → Cerrada*.

> Nota: la versión actual **registra** la respuesta en el sistema; no envía correo automáticamente (eso está planeado para una próxima etapa). La respuesta debe hacerse llegar al solicitante por el canal acostumbrado.

### 8.4 Agregar comentario
*(Todos los roles)*

Sirve para dejar notas sin cambiar el estado.

1. Escriba el comentario en *Agregar comentario*.
2. Marque **"Comentario interno"** si solo debe verlo el personal (por ejemplo, "se llamó al cliente, no contestó").
3. Presione **Agregar comentario**.

### 8.5 Adjuntos (archivos)
*(Agregar: todos los roles. Eliminar: Administrador, Supervisor y Responsable)*

- **Agregar archivo:** se abre una ventana para elegir el archivo en su computador.
- **Abrir:** seleccione un archivo de la lista y presione *Abrir* (o haga doble clic). Se abre en el programa que corresponda (PDF, Word, etc.). Se abre una **copia**, así el original no se daña aunque lo edite.
- **Eliminar:** quita el archivo de la solicitud. Queda anotado en el historial.

Reglas de los archivos:
- Tipos permitidos: **PDF, Word (doc/docx), Excel (xls/xlsx), PowerPoint (ppt/pptx), texto (txt), CSV, imágenes (jpg, jpeg, png, gif) y ZIP.** Otros tipos (como programas `.exe`) se rechazan por seguridad.
- Tamaño máximo: **10 MB** por archivo.
- Máximo **20 archivos** por solicitud.
- No se puede adjuntar a solicitudes cerradas o anuladas.

### 8.6 Historial
Muestra, de la más antigua a la más reciente: fecha y hora, el usuario, el estado en que quedó y el comentario. Los comentarios internos tienen la marca **"Interno"**.

---

## 9. Cómo se calculan las fechas de vencimiento

Esta es la parte que más dudas genera, así que se explica con calma.

### 9.1 La regla
> **Fecha de vencimiento = fecha de radicación + los días hábiles que tenga definidos el TIPO de solicitud.**

El cálculo se hace **una sola vez, en el momento de radicar**, y queda guardado.

### 9.2 ¿Y la prioridad?
Esto es importante y suele confundirse:

> ⚠️ **La prioridad (Baja, Media, Alta, Urgente) NO cambia la fecha de vencimiento.**

La prioridad es solo una **etiqueta informativa** para que el equipo sepa qué es más delicado. El plazo depende **únicamente del tipo** de solicitud.

**¿Cómo logro que algo urgente tenga menos plazo?** Se hace con los tipos: el administrador puede crear un tipo adicional con menos días (por ejemplo, "Reclamo urgente — 3 días") y usarlo para esos casos. Ver [sección 11.2](#112-tipos-de-solicitud).

### 9.3 Días por defecto de cada tipo
Al instalar el sistema vienen estos plazos (el administrador puede cambiarlos):

| Tipo | Prefijo | Días hábiles para responder |
|---|:---:|:---:|
| Petición | P | 15 |
| Queja | Q | 15 |
| Reclamo | R | 15 |
| Sugerencia | S | 30 |

### 9.4 ¿Qué es un "día hábil"?
Son los días de **lunes a viernes**. **Sábados y domingos no cuentan.**

> ⚠️ **Los festivos todavía no se tienen en cuenta.** Si en el plazo cae un festivo, el sistema lo cuenta como día hábil normal. La fecha real de vencimiento podría ser un día más tarde de lo que muestra el sistema. (Es una mejora prevista.)

### 9.5 Ejemplos
**Ejemplo 1 — plazo corto.** Un tipo con **5 días hábiles**, radicado un **lunes**:
martes (1), miércoles (2), jueves (3), viernes (4), lunes siguiente (5) → **vence el lunes de la semana siguiente**. El fin de semana no se cuenta.

**Ejemplo 2 — radicado un viernes.** Un tipo con **3 días hábiles**, radicado un **viernes**:
lunes (1), martes (2), miércoles (3) → **vence el miércoles**.

**Ejemplo 3 — radicado en fin de semana.** Un tipo con **1 día hábil**, radicado un **sábado**:
el lunes cuenta como día 1 → **vence el lunes**.

> El conteo empieza **el día siguiente** a la radicación (el día en que se radica no cuenta).

### 9.6 ¿Cuándo se considera "vencida" una solicitud?
Una solicitud se marca como **vencida** cuando cumple las dos condiciones:
1. Sigue **abierta**: su estado es *Radicada*, *En trámite* o *Pendiente de información*.
2. Ya pasó su fecha y hora de vencimiento (el sistema conserva la hora exacta en que se radicó).

Cuando está vencida:
- En la Bandeja, la fila se ve con **fondo rojizo**.
- En el detalle aparece la etiqueta roja **"Vencida"**.
- Cuenta en el indicador **Vencidas** de los reportes.

Una solicitud **Respondida**, **Cerrada** o **Anulada** **ya no se marca como vencida**, aunque se haya respondido tarde.

### 9.7 ¿Cómo se mide "respondida a tiempo" en los reportes?
Se considera **a tiempo** si la **primera respuesta** se dio **a más tardar el día del vencimiento** (se compara solo la fecha, no la hora).

---

## 10. Reportes

**Para qué sirve:** ver de un vistazo cómo va la atención y sacar un archivo de Excel.

**Quién puede usarla:** Administrador y Supervisor.

**Cómo llegar:** menú lateral → **Reportes**.

### Cómo se usa
1. Elija las fechas **Desde** y **Hasta**. Por defecto muestra lo que va del año.
2. Presione **Generar**. (Al entrar, se genera solo con las fechas por defecto.)
3. Revise los indicadores y las barras.
4. Si quiere llevarse los datos, presione **Exportar a Excel**, elija dónde guardar el archivo y el sistema lo abrirá cuando termine.

Las fechas filtran por la **fecha de radicación** de las solicitudes.

### Los indicadores (tarjetas)

| Indicador | Qué significa |
|---|---|
| **Total de solicitudes** | Todas las radicadas en el periodo. |
| **Abiertas** | Las que siguen en Radicada, En trámite o Pendiente de información. |
| **Vencidas** | Abiertas cuyo plazo ya pasó (en rojo: requieren atención). |
| **Respondidas o cerradas** | Las que ya tuvieron respuesta o terminaron. |
| **Anuladas** | Las canceladas. |
| **Días promedio de respuesta** | Cuántos días, en promedio, tardó la primera respuesta desde la radicación. Muestra "—" si aún no hay respuestas. |
| **Respondidas a tiempo** | Porcentaje de solicitudes cuya primera respuesta se dio dentro del plazo. Muestra "—" si aún no hay respuestas. |

### Las barras
Cada barra muestra **cuántas solicitudes** hay y **qué porcentaje** del total representan, agrupadas por:
- **Estado**
- **Tipo de solicitud**
- **Categoría**
- **Área** (las que no tienen área aparecen como *Sin asignar*)

Sirven para responder preguntas como: *¿Qué tema genera más reclamos? ¿Qué área recibe más solicitudes?*

### El archivo de Excel
Tiene dos hojas:
- **Resumen:** el periodo, los indicadores y las tablas por estado, tipo, categoría y área.
- **Solicitudes:** una fila por cada solicitud del periodo (radicado, tipo, categoría, área, asunto, solicitante, estado, prioridad, fechas y si está vencida). Trae filtros listos para usar en Excel.

Si aparece el error *"No se pudo escribir el archivo"*, casi siempre es porque ese mismo archivo está **abierto en Excel**. Ciérrelo o guarde con otro nombre.

---

## 11. Administración

**Para qué sirve:** configurar el sistema (usuarios, catálogos, nombre de la organización) sin necesidad de un programador.

**Quién puede usarla:** solo el **Administrador**.

**Cómo llegar:** menú lateral → **Administración**. Tiene cinco pestañas:

1. Usuarios
2. Tipos de solicitud
3. Categorías
4. Áreas
5. Organización

### Cómo funcionan las pestañas (todas son iguales)
- A la **izquierda** hay una **lista** de lo que ya existe.
- A la **derecha** hay un **formulario**.
- Para **crear** algo nuevo: presione **Nuevo** (o **Nueva**), llene el formulario y presione **Guardar**.
- Para **modificar** algo existente: haga clic en la fila de la lista, cambie lo que necesite y presione **Guardar**.
- Aparece un mensaje verde cuando se guardó, o rojo si algo está mal.

### ⚠️ Nada se borra, se desactiva
En ninguna pestaña hay botón de "eliminar". Para quitar algo de uso, **desmárquele la casilla "Activo/Activa"** y guarde. Esto es a propósito: las solicitudes antiguas siguen mostrando sus datos históricos y no se pierde información.
- Un elemento **desactivado** ya no aparece para elegir en solicitudes nuevas.
- Para volver a usarlo, vuelva a marcar la casilla.

### 11.1 Usuarios
Aquí se crean las cuentas de las personas que usarán el sistema. El procedimiento completo está en la [sección 12](#12-cómo-crear-un-usuario-paso-a-paso).

### 11.2 Tipos de solicitud
Define las clases de PQRS y **su plazo de respuesta**.

| Campo | Explicación |
|---|---|
| **Nombre \*** | Cómo se llama (Petición, Queja…). No puede repetirse. |
| **Descripción** | Opcional. Explica para qué sirve el tipo. |
| **Prefijo del radicado \*** | De 1 a 5 letras o números, en mayúscula. Es el comienzo del número de radicado (por ejemplo `P`). **No puede repetirse** entre tipos. |
| **Días hábiles para responder \*** | De 1 a 365. **Este número define la fecha de vencimiento** (ver [sección 9](#9-cómo-se-calculan-las-fechas-de-vencimiento)). |
| **Activo** | Si está desmarcado, no se puede elegir al radicar. |

Si cambia el prefijo o los días de un tipo, **solo afecta a las solicitudes nuevas**; las ya radicadas conservan su radicado y su fecha de vencimiento.

### 11.3 Categorías
Son los **temas** de las solicitudes (por ejemplo "Atención y servicio", "Facturación y pagos").

| Campo | Explicación |
|---|---|
| **Nombre \*** | No puede repetirse. |
| **Descripción** | Opcional. |
| **Área que atiende por defecto** | Si elige una, toda solicitud de esta categoría **queda asignada automáticamente a esa área** al radicarse. |
| **Activa** | Si está desmarcada, no se puede elegir al radicar. |

### 11.4 Áreas
Son las dependencias o departamentos que atienden solicitudes (por ejemplo "Atención al Usuario", "Área Técnica").

| Campo | Explicación |
|---|---|
| **Nombre \*** | No puede repetirse. |
| **Correo de notificación** | Opcional (formato válido). Se usará para avisos por correo en una etapa futura. |
| **Activa** | Si está desmarcada, no se puede elegir. |

### 11.5 Organización
Datos generales de su institución.

| Campo | Explicación |
|---|---|
| **Nombre de la organización \*** | Aparece en la **barra superior** de la aplicación. Cámbielo por el nombre de su empresa o entidad. |
| **Correo remitente de notificaciones** | Opcional. El correo desde el que se enviarán avisos en una etapa futura. |

Al guardar, el nombre de la barra superior se actualiza en el momento.

---

## 12. Cómo crear un usuario (paso a paso)

Solo lo puede hacer un **Administrador**.

### 12.1 Crear un usuario nuevo
1. Entre al sistema con una cuenta de Administrador.
2. En el menú lateral, presione **Administración**.
3. Abra la pestaña **Usuarios**.
4. Presione **Nuevo** (limpia el formulario).
5. Complete los datos:

| Campo | Qué poner | Reglas |
|---|---|---|
| **Nombre completo \*** | Nombre y apellido de la persona. | Máximo 200 caracteres. |
| **Usuario \*** | El "apodo" con el que entrará. Ej.: `mperez`. | De 3 a 50 caracteres. Solo letras, números, punto, guion o guion bajo. **Sin espacios.** No puede repetirse. |
| **Correo electrónico** | Opcional. | Con formato válido. |
| **Rol \*** | Administrador, Supervisor, Responsable o Radicador. | Ver [sección 4](#4-roles-qué-puede-hacer-cada-persona). |
| **Área** | El área a la que pertenece (o *Sin área*). | Opcional. |
| **Contraseña** | La clave inicial. | **Obligatoria al crear.** Mínimo 8 caracteres, con letras **y** números. |
| **Usuario activo** | Déjela marcada. | Si se desmarca, la persona no podrá entrar. |

6. Presione **Guardar**.
7. Verá el mensaje verde *"Usuario guardado."* y el usuario aparecerá en la lista de la izquierda.
8. Entregue a la persona su usuario y su contraseña inicial **de forma privada**, y pídale que la cambie al entrar.

**¿Qué rol le pongo?**
- Personal de ventanilla o recepción → **Radicador**.
- Quien resuelve los casos → **Responsable**.
- Jefe o coordinador del equipo → **Supervisor**.
- Persona que configura el sistema → **Administrador** (conviene que sean pocas).

### 12.2 Modificar un usuario
1. En **Administración → Usuarios**, haga clic sobre la persona en la lista.
2. Sus datos aparecen en el formulario. Cambie lo que necesite (nombre, rol, área…).
3. Presione **Guardar**.

### 12.3 Desactivar un usuario (cuando alguien ya no trabaja ahí)
1. Seleccione al usuario en la lista.
2. Desmarque **Usuario activo**.
3. Presione **Guardar**.

La persona ya no podrá iniciar sesión, pero todo lo que hizo sigue en el historial de las solicitudes. **No se elimina el usuario.**

### 12.4 Cambiar la contraseña de un usuario
Sirve tanto para quien **olvidó su clave** como para cambiar la clave inicial.
1. En **Administración → Usuarios**, seleccione al usuario.
2. Escriba la **nueva contraseña** en el campo *Contraseña* (debajo dice "Déjela vacía para conservar la actual").
3. Presione **Guardar**.

Si deja el campo vacío, la contraseña actual **no cambia**. Cada usuario nuevo o modificado debe cumplir la regla: mínimo 8 caracteres, con letras y números.

> Nadie puede ver una contraseña, ni siquiera el administrador: se guardan cifradas. Si se olvida, solo se puede **reemplazar** por una nueva.

### 12.5 Protecciones del sistema
Para evitar accidentes, el sistema **no permite**:
- Que usted **desactive su propio usuario** ni se quite a sí mismo el rol de Administrador.
- Dejar el sistema **sin ningún administrador activo**.
- Crear **dos usuarios con el mismo nombre de usuario**.

Si lo intenta, verá un mensaje rojo explicando el motivo.

---

## 13. Puesta en marcha para una organización nueva

Si va a usar el sistema por primera vez en su organización, siga este orden en **Administración**:

1. **Organización:** escriba el nombre de su entidad.
2. **Áreas:** cree las dependencias que atenderán solicitudes.
3. **Categorías:** cree los temas y asocie a cada uno el área que lo atiende.
4. **Tipos de solicitud:** revise los 4 tipos por defecto; ajuste los días según su normativa o políticas. Cree otros si los necesita.
5. **Usuarios:** cree las cuentas del equipo con su rol. Cambie la contraseña del administrador inicial.
6. **Prueba:** radique una solicitud de prueba, asígnela, respóndala y ciérrela, para comprobar que todo funciona. Luego mire el reporte.

> Los datos de ejemplo que trae el sistema (áreas como "Atención al Usuario" y categorías como "Facturación y pagos") se pueden modificar o desactivar libremente.

---

## 14. Preguntas frecuentes y solución de problemas

**No puedo entrar: dice "Usuario o contraseña incorrectos."**
Revise mayúsculas y minúsculas y que no haya espacios de más. Si olvidó la clave, pida a un Administrador que le asigne una nueva (sección 12.4). Si el usuario fue desactivado, un administrador debe reactivarlo.

**Aparece "No se pudo conectar" al iniciar sesión.**
El programa no logra comunicarse con la base de datos. Verifique que el computador esté en la red de la organización y avise al responsable técnico (ver sección 15).

**No veo el menú Reportes o Administración.**
Es normal: dependen del rol. *Reportes* es para Administrador y Supervisor; *Administración* solo para Administrador.

**No me aparece el botón/sección para responder o cambiar el estado.**
Puede ser por tres motivos: (1) su rol no lo permite (los Radicadores no pueden); (2) la solicitud está *Cerrada* o *Anulada*; (3) para **responder** la solicitud debe estar *En trámite* (si está *Radicada*, primero hay que asignarla o pasarla a En trámite).

**Intenté asignar y dice "No tiene permisos".**
Solo Administrador y Supervisor pueden asignar responsables.

**Dice "No se puede pasar de X a Y".**
Ese cambio no está permitido por el flujo (ver el diagrama de la sección 2.2). Por ejemplo, no se puede pasar de *Radicada* directo a *Cerrada*.

**Dice "Debe indicar el motivo del cambio de estado."**
Al *Anular* o pasar a *Pendiente de información*, el comentario es obligatorio.

**No puedo adjuntar un archivo.**
Verifique: que sea de un tipo permitido (PDF, Word, Excel, PowerPoint, texto, imágenes, ZIP), que pese **menos de 10 MB**, que no esté vacío, que la solicitud no esté cerrada o anulada y que no supere los 20 adjuntos.

**El archivo adjunto no se abre ("no se encuentra en el almacenamiento").**
El archivo fue movido o borrado de la carpeta donde el sistema los guarda. Avise al responsable técnico.

**Una solicitud se ve en rojo / "Vencida". ¿Qué hago?**
Significa que pasó el plazo sin responderse. Atiéndala cuanto antes. Al responderla deja de marcarse como vencida.

**¿Por qué una solicitud con prioridad "Urgente" no vence antes?**
Porque la prioridad no afecta el plazo; el plazo lo define el tipo (sección 9.2).

**¿Se puede borrar una solicitud o un usuario?**
No. Las solicitudes y el historial no se borran, y los usuarios y catálogos solo se desactivan. Es para conservar la trazabilidad.

**Al exportar a Excel dice que no se puede escribir el archivo.**
Probablemente el archivo está abierto en Excel. Ciérrelo o use otro nombre.

**¿El solicitante recibe un correo automático?**
Todavía no. Las notificaciones por correo están previstas para una próxima etapa. Hoy la respuesta queda registrada en el sistema y debe comunicarse al solicitante por el canal habitual.

---

## 15. Sección para desarrolladores

Esta guía es para usuarios. La explicación **técnica** completa (arquitectura, tecnologías, modelo de datos, convenciones y trampas conocidas) está en el archivo [`context.md`](context.md). Aquí solo lo mínimo para ponerlo en marcha.

### 15.1 Requisitos
- Windows 10/11.
- **.NET 10 SDK** y Visual Studio 2026 (o la CLI de .NET).
- **SQL Server** (el proyecto se desarrolló con SQL Server 2019).

### 15.2 Preparar la base de datos
Ejecute los scripts de la carpeta `database` **en este orden**, guardados/leídos como **UTF-8** (para no dañar los acentos):

1. `01_schema.sql` — crea la base `PQRS`, las tablas, los 4 tipos de solicitud y los parámetros iniciales.
2. `02_datos_iniciales.sql` — crea áreas y categorías de ejemplo.

Con `sqlcmd`:

```
sqlcmd -S SERVIDOR -U USUARIO -P CLAVE -C -b -f 65001 -i database\01_schema.sql
sqlcmd -S SERVIDOR -U USUARIO -P CLAVE -C -b -f 65001 -i database\02_datos_iniciales.sql
```

### 15.3 Configurar la aplicación
Edite `src/PQRS.Desktop/appsettings.json`:

| Clave | Qué es |
|---|---|
| `ConnectionStrings:PqrsDb` | Cadena de conexión a SQL Server. |
| `Almacenamiento:RutaAdjuntos` | Carpeta donde se guardan los adjuntos. Vacía = `C:\ProgramData\PQRS\Adjuntos`. **Si varios equipos usan el sistema, debe ser una carpeta compartida de red accesible para todos.** |
| `Seed:AdminUsuario` / `Seed:AdminClave` | Usuario y contraseña del **primer administrador**. Se crea automáticamente la primera vez que se abre el programa y la tabla de usuarios está vacía. |

> 🔐 **Seguridad:** no deje contraseñas reales en `appsettings.json` al publicar. Use variables de entorno o *User Secrets*. Cambie la contraseña del administrador inicial al primer ingreso.

### 15.4 Ejecutar
1. Abra `PQRS.slnx` en Visual Studio.
2. Deje `PQRS.Desktop` como proyecto de inicio.
3. Presione **F5**.

O por consola: `dotnet build PQRS.slnx` y luego ejecute `PQRS.Desktop`.

### 15.5 Dónde cambiar las reglas que se explicaron en esta guía

| Si quiere cambiar… | Vaya a… |
|---|---|
| El cálculo de días hábiles (por ejemplo, agregar festivos) | `src/PQRS.Domain/Services/CalculadoraVencimiento.cs` |
| Que el plazo dependa también de la prioridad | `src/PQRS.Infrastructure/Services/SolicitudService.cs`, método `RadicarAsync` (donde se calcula `vencimiento`) |
| El flujo de estados permitido | `src/PQRS.Domain/Services/TransicionesEstado.cs` |
| La política de contraseñas | `src/PQRS.Domain/Services/PoliticaClaves.cs` |
| Quién puede hacer cada acción | Los servicios de `src/PQRS.Infrastructure/Services` (validan el rol) y los ViewModels de `src/PQRS.Desktop/ViewModels` (muestran u ocultan botones) |
| Extensiones y tamaño máximo de adjuntos | `src/PQRS.Infrastructure/Services/AdjuntoService.cs` |

Importante: los permisos se validan en dos lugares. Al cambiar uno, cambie el otro para que coincidan.

### 15.6 Pendientes conocidos
Notificaciones por correo y alertas de vencimiento, festivos en el cálculo de plazos, instalador y copias de seguridad, cambio de contraseña por el propio usuario, bloqueo por intentos fallidos y pruebas automatizadas. El detalle está en `context.md`.
