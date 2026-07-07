# Gestión de Ítems de Trabajo — Arquitectura de Microservicios

Aplicativo para gestionar ítems de trabajo y su distribución automática entre
usuarios, en **C# / ASP.NET Core**, con arquitectura de microservicios.

## Arquitectura
Dos servicios independientes, cada uno con sus capas `Models` → `Repositories`
→ `Services` → `Controllers`. Se comunican por HTTP (`IUsuariosApiClient`):
`ItemsTrabajo.Api` consulta el estado de los usuarios antes de asignar, y
notifica el resultado después.

**Usuarios.api** — lleva el seguimiento de cada usuario (ítems completados,
pendientes totales, pendientes de alta relevancia).

| Método | Ruta |
|---|---|
| GET | `/api/usuarios` |
| GET | `/api/usuarios/{nombre}` |
| POST | `/api/usuarios` |
| POST | `/api/usuarios/{nombre}/asignaciones` |
| POST | `/api/usuarios/{nombre}/completados` |

**ItemsTrabajo.Api** — dueño de los ítems y del algoritmo de distribución.

| Método | Ruta |
|---|---|
| GET | `/api/items` |
| POST | `/api/items` |
| POST | `/api/items/distribuir` |
| PUT | `/api/items/{id}/completar` |

## Algoritmo de distribución (`MotorDistribucion.cs`)

1. **Urgencia**: fecha de entrega a menos de 3 días → se asigna al usuario con
   menos pendientes, sin importar relevancia.
2. **Relevancia**: entre no urgentes, los de relevancia Alta se procesan
   primero, también al usuario con menos pendientes.
3. **Saturación**: usuario con más de 3 ítems de alta relevancia pendientes
   queda excluido.
4. **Reordenamiento**: tras cada asignación se reordena la lista de usuarios
   con el estado actualizado.

Validado con 6 pruebas unitarias en `ItemsTrabajo.Api.Tests`.

## Ejemplo:

```json
// POST /api/items
{ "titulo": "Corregir bug critico", "fechaEntrega": "2026-07-07", "relevancia": 1 }

// POST /api/items/distribuir → respuesta
{ "asignaciones": [{ "titulo": "Corregir bug critico", "usuarioAsignado": "jperez" }], "itemsSinAsignar": [] }
```

## Cómo ejecutar

1. Abrir `EDWIN_CORO.sln` en Visual Studio 2022.
2. Clic derecho en la solución → **Configurar proyectos de inicio** → **Varios
   proyectos de inicio** → `Usuarios.api` e `ItemsTrabajo.Api` en **Iniciar**.
3. F5. Se abren dos Swagger, uno por microservicio.
4. Probar: crear usuario → crear ítem → `POST /api/items/distribuir` → revisar
   `GET /api/usuarios`.

> Si el puerto de `Usuarios.api` no coincide con `UsuariosApi:BaseUrl` en
> `ItemsTrabajo.Api/appsettings.json`, ajustarlo ahí.

## Pruebas unitarias

`Prueba` → `Explorador de pruebas` → **Ejecutar todas las pruebas**.

## Modelo de datos

Almacenamiento en memoria (`ConcurrentDictionary`) detrás de interfaces de
repositorio, reemplazable por EF Core sin afectar las demás capas.