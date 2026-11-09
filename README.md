# Laboratorio matemático de Sistemas Vivos


<img width="1202" height="839" alt="image" src="https://github.com/user-attachments/assets/fe97f886-3a5a-4439-bcf2-0a319a5bbecd" />

<img width="1202" height="839" alt="image" src="https://github.com/user-attachments/assets/bbec0201-f75f-46db-8802-ee4d592972bc" />









Aplicación de apoyo para las cuatro prácticas. Console App de C# con .NET 8 y Raylib-cs 8.0.0. No contiene llamadas HTTP: lee datos simulados locales compatibles con el borrador de polinizadores. El contrato real y sus unidades se acuerdan con primero.

## Ejecución

Abre esta carpeta en VS Code. En su terminal:

```powershell
dotnet restore
dotnet build
dotnet run
```

Requiere SDK de .NET 8 y acceso a NuGet para restaurar Raylib-cs. Para las pruebas numéricas, sin abrir la ventana:

```powershell
dotnet run -- --probar
```

## Controles

| Modo | Controles |
|---|---|
| F1 Vectores | Clic cambia destino. Flechas horizontales cambian rapidez. 1 y 2 eligen las flores. 3 prueba horizontal. 4 coloca destino en posición actual. |
| F2 Matrices | I activa identidad. E activa M del ejemplo. |
| F3 T R S | T selecciona traslación, S escala y O rotación. Las flechas modifican el parámetro activo. En S, J usa energía del JSON. |
| F4 Composición | 1 TRS, 2 RTS y 3 TSR. V alterna pivote. P anima o pausa. J carga energía y orientación. |
| Comunes | R restablece. P pausa. J recarga archivos locales. ESC sale. |

En F4 la pausa está activa inicialmente para verificar exactamente 90 grados.

## Organización

- `Matematicas2D.cs`: movimiento, matrices y producto. Convención de vectores columna.
- `Program.cs`: interacción, estado, conversión de coordenadas y dibujo.
- `Pruebas.cs`: once comprobaciones numéricas sin framework externo.
- `datos`: request y response simulados. La tecla J vuelve a leerlos.

## Coordenadas

El mundo usa x a la derecha e y hacia arriba. La conversión de dibujo utiliza origen de pantalla (80,700) y 40 píxeles por unidad. F2, F3 y el caso de composición de F4 añaden un ancla visual fija (8,5) para que los puntos locales negativos aparezcan en la ventana. Las cifras del panel muestran los resultados SIN esa ancla. El caso de pivote usa directamente C=(2,2).

## Mapeo didáctico

- `flores[].x/y`: posiciones de escenario en unidades de mundo.
- `resultado.ruta`: identificadores de flores, no puntos. El cliente resuelve cada id.
- `visual.velocidad`: rapidez en unidades de mundo por segundo.
- `visual.direccion`: dirección usada para orientar el frente +x.
- `resultado.energiaFinal`: valor de 0 a 100 para una escala visual entre 0.6 y 1.4. Esta regla no representa una ley biológica.

Los archivos contienen valores elegidos para los ejercicios. No son resultados científicos ni fijan por sí solos el contrato final. La integración HTTP, los estados de carga y el respaldo de una API se desarrollarán en otro bloque.
