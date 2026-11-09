using System.Numerics;
using System.Text.Json;
using Raylib_cs;
using static LaboratorioSistemasVivos.Matematicas2D;

namespace LaboratorioSistemasVivos;

internal static class Program
{
    private const float PxPorUnidad = 40;
    private static readonly Vector2 OrigenPantalla = new(80, 700);
    private static readonly Vector2 AnclaVisual = new(8, 5);
    private static readonly Vector2[] GeometriaOriginal =
    {
        new(1, 0), new(-1, 1), new(-1, -1)
    };

    private static int modo = 1;
    private static bool pausa;
    private static Vector2 posicion = new(2, 2), objetivo = new(5, 6);
    private static float rapidez = 2, angulo = 90, sx = 2, sy = 0.5f;
    private static Vector2 traslacion = new(3, 2);
    private static char operacion = 'T';
    private static int orden = 1;
    private static bool identidad, verPivote;
    private static string mensaje = "Datos simulados locales. No hay consultas HTTP en este laboratorio.";
    private static Peticion escenario = new();
    private static Respuesta respuesta = new();

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--probar")) return Pruebas.Ejecutar();

        CargarDatos();
        Raylib.InitWindow(1200, 800, "Sistemas Vivos | Laboratorio matematico 2D");
        Raylib.SetTargetFPS(60);
        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            LeerEntrada(dt);
            if (!pausa)
            {
                if (modo == 1) posicion = MoverHacia(posicion, objetivo, rapidez, dt);
                // En composición, P inicia o detiene la animación.
                if (modo == 4) angulo = (angulo + 30 * dt) % 360;
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);
            DibujarPlano();
            switch (modo)
            {
                case 1: DibujarMovimiento(); break;
                case 2: DibujarMatrices(); break;
                case 3: DibujarTransformacion(); break;
                case 4: DibujarComposicion(); break;
            }
            DibujarInterfaz();
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
        return 0;
    }

    private static Vector2 MundoAPantalla(Vector2 p) => new(
        OrigenPantalla.X + PxPorUnidad * p.X,
        OrigenPantalla.Y - PxPorUnidad * p.Y
    );

    private static Vector2 PantallaAMundo(Vector2 p) => new(
        (p.X - OrigenPantalla.X) / PxPorUnidad,
        (OrigenPantalla.Y - p.Y) / PxPorUnidad
    );

    private static void LeerEntrada(float dt)
    {
        for (int i = 1; i <= 4; i++)
            if (Raylib.IsKeyPressed((KeyboardKey)((int)KeyboardKey.F1 + i - 1)))
            {
                modo = i;
                Reiniciar();
            }

        if (Raylib.IsKeyPressed(KeyboardKey.R)) Reiniciar();
        if (Raylib.IsKeyPressed(KeyboardKey.P)) pausa = !pausa;
        if (Raylib.IsKeyPressed(KeyboardKey.J))
        {
            CargarDatos();
            if (modo == 1)
            {
                rapidez = respuesta.Visual.Velocidad;
                Flor? inicio = BuscarFlor(respuesta.Resultado.Ruta.First());
                Flor? destino = BuscarFlor(respuesta.Resultado.Ruta.Last());
                if (inicio is not null && destino is not null)
                {
                    posicion = new(inicio.X, inicio.Y);
                    objetivo = new(destino.X, destino.Y);
                }
            }
            if ((modo == 3 && operacion == 'S') || modo == 4)
            {
                sx = sy = EscalaPorEnergia(respuesta.Resultado.EnergiaFinal);
                Vector2 dir = new(respuesta.Visual.Direccion.X, respuesta.Visual.Direccion.Y);
                if (dir.LengthSquared() > 0.0001f)
                    angulo = MathF.Atan2(dir.Y, dir.X) * 180 / MathF.PI;
                pausa = true;
            }
        }

        if (modo == 1)
        {
            if (Raylib.IsKeyDown(KeyboardKey.Left)) rapidez -= dt;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) rapidez += dt;
            rapidez = Math.Clamp(rapidez, 0, 10);
            Vector2 mouse = Raylib.GetMousePosition();
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
                mouse.X >= 80 && mouse.X <= 800 && mouse.Y >= 220 && mouse.Y <= 700)
                objetivo = PantallaAMundo(mouse);
            if (Raylib.IsKeyPressed(KeyboardKey.One)) objetivo = new(2, 2);
            if (Raylib.IsKeyPressed(KeyboardKey.Two)) objetivo = new(5, 6);
            if (Raylib.IsKeyPressed(KeyboardKey.Three)) objetivo = new(12, posicion.Y);
            if (Raylib.IsKeyPressed(KeyboardKey.Four)) objetivo = posicion;
        }
        if (modo == 2)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.I)) identidad = true;
            if (Raylib.IsKeyPressed(KeyboardKey.E)) identidad = false;
        }
        if (modo == 3)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.T)) operacion = 'T';
            if (Raylib.IsKeyPressed(KeyboardKey.S)) operacion = 'S';
            if (Raylib.IsKeyPressed(KeyboardKey.O)) operacion = 'O';
            float dx = (Raylib.IsKeyDown(KeyboardKey.Right) ? 1 : 0) -
                       (Raylib.IsKeyDown(KeyboardKey.Left) ? 1 : 0);
            float dy = (Raylib.IsKeyDown(KeyboardKey.Up) ? 1 : 0) -
                       (Raylib.IsKeyDown(KeyboardKey.Down) ? 1 : 0);
            if (operacion == 'T') traslacion += new Vector2(dx, dy) * 2 * dt;
            if (operacion == 'S')
            {
                sx = Math.Clamp(sx + dx * dt, 0.1f, 3);
                sy = Math.Clamp(sy + dy * dt, 0.1f, 3);
            }
            if (operacion == 'O') angulo += dx * 60 * dt;
        }
        if (modo == 4)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.One)) orden = 1;
            if (Raylib.IsKeyPressed(KeyboardKey.Two)) orden = 2;
            if (Raylib.IsKeyPressed(KeyboardKey.Three)) orden = 3;
            if (Raylib.IsKeyPressed(KeyboardKey.V)) verPivote = !verPivote;
        }
    }

    private static void Reiniciar()
    {
        posicion = new(2, 2); objetivo = new(5, 6); rapidez = 2;
        angulo = 90; sx = 2; sy = modo == 4 ? 1 : 0.5f;
        traslacion = new(3, 2); operacion = 'T'; orden = 1;
        identidad = true; verPivote = false;
        pausa = modo == 4;
    }

    private static void DibujarPlano()
    {
        for (int x = 0; x <= 18; x++)
        {
            Vector2 a = MundoAPantalla(new(x, 0)), b = MundoAPantalla(new(x, 12));
            Raylib.DrawLineV(a, b, Color.LightGray);
            Raylib.DrawText(x.ToString(), (int)a.X - 5, (int)a.Y + 9, 15, Color.Gray);
        }
        for (int y = 0; y <= 12; y++)
        {
            Vector2 a = MundoAPantalla(new(0, y)), b = MundoAPantalla(new(18, y));
            Raylib.DrawLineV(a, b, Color.LightGray);
            Raylib.DrawText(y.ToString(), (int)a.X - 28, (int)a.Y - 6, 15, Color.Gray);
        }
    }

    private static void Punto(Vector2 p, Color color, string etiqueta)
    {
        Vector2 pantalla = MundoAPantalla(p);
        Raylib.DrawCircleV(pantalla, 6, color);
        Raylib.DrawText(etiqueta, (int)pantalla.X + 10, (int)pantalla.Y - 19, 17, color);
    }

    private static void Poligono(Vector2[] originales, float[,] matriz, Vector2 ancla, Color color)
    {
        // Cada cuadro parte de originales, nunca del resultado anterior.
        Vector2[] transformados = originales.Select(p => AplicarPunto(matriz, p) + ancla).ToArray();
        for (int i = 0; i < transformados.Length; i++)
            Raylib.DrawLineEx(MundoAPantalla(transformados[i]),
                MundoAPantalla(transformados[(i + 1) % transformados.Length]), 3, color);
    }

    private static void DibujarMovimiento()
    {
        foreach (Flor flor in escenario.Variables.Flores)
            Punto(new(flor.X, flor.Y), Color.DarkGreen, $"F{flor.Id}");
        Raylib.DrawLineEx(MundoAPantalla(posicion), MundoAPantalla(objetivo), 2, Color.Gray);
        Punto(objetivo, Color.Orange, "Objetivo");
        Punto(posicion, Color.DarkBlue, "Agente");
        DatosLado(new[] { $"P = ({posicion.X:F2}, {posicion.Y:F2})", $"Q = ({objetivo.X:F2}, {objetivo.Y:F2})",
            $"Rapidez = {rapidez:F2} u/s", $"Distancia = {Vector2.Distance(posicion, objetivo):F3}",
            "MoverHacia limita el paso", "y protege distancia cero.", "J recarga el JSON local." });
    }

    private static void DibujarMatrices()
    {
        Vector2[] originales = { new(1, 2), new(0, 0), new(-1, 2) };
        float[,] matriz = identidad ? Identidad() : new float[,] { { 2, 0, 3 }, { 0, 1, -1 }, { 0, 0, 1 } };
        Poligono(originales, Identidad(), AnclaVisual, Color.Gray);
        Poligono(originales, matriz, AnclaVisual, Color.DarkBlue);
        Vector2 nuevo = AplicarPunto(matriz, originales[0]);
        Vector2 nuevoB = AplicarPunto(matriz, originales[1]);
        Vector2 nuevoC = AplicarPunto(matriz, originales[2]);
        Vector2 mouseMundo = PantallaAMundo(Raylib.GetMousePosition());
        DatosLado(new[] { identidad ? "M = identidad" : "M = ejemplo", "Datos locales, sin ancla:", "p = (1,2)",
            $"M p = ({nuevo.X:F2},{nuevo.Y:F2})", $"M(0,0) = ({nuevoB.X:F2},{nuevoB.Y:F2})",
            $"M(-1,2) = ({nuevoC.X:F2},{nuevoC.Y:F2})", $"Mouse mundo: {mouseMundo.X:F2}, {mouseMundo.Y:F2}",
            "I: identidad; E: ejemplo", "Gris: original. Azul: nueva", "Ancla fija = (8,5)" });
    }

    private static void DibujarTransformacion()
    {
        float[,] m = operacion switch
        {
            'T' => Traslacion(traslacion.X, traslacion.Y),
            'S' => Escala(sx, sy),
            _ => Rotacion(angulo)
        };
        Poligono(GeometriaOriginal, Identidad(), AnclaVisual, Color.Gray);
        Poligono(GeometriaOriginal, m, AnclaVisual, Color.DarkBlue);
        Punto(AnclaVisual, Color.Orange, "Origen local");
        DatosLado(new[] { $"Operacion: {operacion}", $"tx = {traslacion.X:F2}", $"ty = {traslacion.Y:F2}",
            $"sx = {sx:F2}, sy = {sy:F2}", $"Angulo = {angulo:F1} grados", "T, S u O seleccionan",
            "Flechas cambian valores", "Ancla visual fija = (8,5)", "J aplica energia en modo S" });
    }

    private static void DibujarComposicion()
    {
        if (verPivote)
        {
            Vector2 centro = new(2, 2), original = new(3, 2);
            Vector2 nuevo = AplicarPunto(RotarEnPivote(centro, angulo), original);
            Punto(centro, Color.Orange, "C=(2,2)"); Punto(original, Color.Gray, "p=(3,2)");
            Punto(nuevo, Color.DarkBlue, "p nueva");
            Raylib.DrawLineEx(MundoAPantalla(centro), MundoAPantalla(nuevo), 2, Color.DarkBlue);
            DatosLado(new[] { "Rotacion en pivote", $"Angulo = {angulo:F1}", $"p nueva = ({nuevo.X:F2},", $"               {nuevo.Y:F2})",
                "C debe permanecer fijo", "R restablece 90 grados", "P anima o pausa", "V vuelve a composicion" });
            return;
        }

        float[,] t = Traslacion(4, 1), r = Rotacion(angulo), s = Escala(sx, sy);
        float[,] modelo = orden switch
        {
            1 => Multiplicar(t, Multiplicar(r, s)),
            2 => Multiplicar(r, Multiplicar(t, s)),
            _ => Multiplicar(t, Multiplicar(s, r))
        };
        Poligono(GeometriaOriginal, Identidad(), AnclaVisual, Color.Gray);
        Poligono(GeometriaOriginal, modelo, AnclaVisual, Color.DarkBlue);
        Vector2 calculado = AplicarPunto(modelo, new(1, 0));
        string nombre = orden == 1 ? "TRS" : orden == 2 ? "RTS" : "TSR";
        DatosLado(new[] { $"Orden = {nombre}", "T = (4,1)", $"S = ({sx:F2},{sy:F2})", $"R = {angulo:F1} grados",
            "Resultado de p=(1,0):", $"({calculado.X:F2},{calculado.Y:F2})", "Sin ancla visual (8,5)",
            "1,2,3 cambian orden", "V: ejemplo de pivote", "P: animar o pausar" });
    }

    private static void DatosLado(IEnumerable<string> lineas)
    {
        int y = 190;
        foreach (string linea in lineas)
        {
            Raylib.DrawText(linea, 850, y, 19, Color.DarkBlue);
            y += 32;
        }
    }

    private static void DibujarInterfaz()
    {
        string[] titulos = { "Vectores y movimiento", "Matrices homogeneas 2D", "Transformaciones aisladas", "Composicion y pivotes" };
        Raylib.DrawText(titulos[modo - 1], 40, 24, 30, Color.DarkBlue);
        Raylib.DrawText("F1: vectores  F2: matrices  F3: T/R/S  F4: composicion", 40, 68, 22, Color.DarkGray);
        Raylib.DrawText("R: reiniciar  P: pausa  J: datos locales  ESC: salir", 40, 100, 20, Color.Gray);
        Raylib.DrawText(pausa ? "PAUSA" : "ACTIVO", 850, 145, 22, Color.DarkGreen);
        Raylib.DrawText(mensaje, 40, 755, 17, Color.DarkGray);
    }

    private static Flor? BuscarFlor(int id) => escenario.Variables.Flores.FirstOrDefault(f => f.Id == id);

    private static void CargarDatos()
    {
        try
        {
            string carpeta = Path.Combine(AppContext.BaseDirectory, "datos");
            JsonSerializerOptions opciones = new() { PropertyNameCaseInsensitive = true };
            Peticion peticion = JsonSerializer.Deserialize<Peticion>(
                File.ReadAllText(Path.Combine(carpeta, "mock-request.json")), opciones)
                ?? throw new JsonException("Escenario nulo.");
            Respuesta resp = JsonSerializer.Deserialize<Respuesta>(
                File.ReadAllText(Path.Combine(carpeta, "mock-response.json")), opciones)
                ?? throw new JsonException("Respuesta nula.");

            if (peticion.Variables is null || peticion.Variables.Flores is null ||
                peticion.Variables.Flores.Count < 2 || resp.Visual is null || resp.Resultado is null ||
                resp.Resultado.Ruta is null || resp.Resultado.Ruta.Count < 2 || resp.Visual.Direccion is null ||
                !float.IsFinite(resp.Visual.Velocidad) || resp.Visual.Velocidad < 0 ||
                !float.IsFinite(resp.Resultado.EnergiaFinal) || resp.Resultado.EnergiaFinal < 0 ||
                resp.Resultado.EnergiaFinal > 100 || !float.IsFinite(resp.Visual.Direccion.X) ||
                !float.IsFinite(resp.Visual.Direccion.Y) ||
                peticion.Variables.Flores.Any(f => f is null || !float.IsFinite(f.X) || !float.IsFinite(f.Y)) ||
                resp.Resultado.Ruta.Any(id => !peticion.Variables.Flores.Any(f => f.Id == id)))
                throw new JsonException("Campos, rangos o ids de ruta invalidos.");

            escenario = peticion; respuesta = resp;
            mensaje = "Mock local cargado. J vuelve a leerlo solo cuando lo solicitas.";
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // El laboratorio no presenta valores inválidos como una respuesta válida.
            escenario = new(); respuesta = new();
            mensaje = "Error en archivo local. Se usan valores didacticos internos. Revisa la consola.";
            Console.WriteLine(ex.Message);
        }
    }
}

internal sealed class Flor
{
    public int Id { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Nectar { get; set; }
}
internal sealed class Variables
{
    public List<Flor> Flores { get; set; } = new()
    {
        new() { Id = 1, X = 2, Y = 2, Nectar = 30 },
        new() { Id = 2, X = 5, Y = 6, Nectar = 50 }
    };
}
internal sealed class Peticion { public Variables Variables { get; set; } = new(); }
internal sealed class PuntoDato { public float X { get; set; } = 0.6f; public float Y { get; set; } = 0.8f; }
internal sealed class Resultado
{
    public List<int> Ruta { get; set; } = new() { 1, 2 };
    public float EnergiaFinal { get; set; } = 80;
}
internal sealed class Visual
{
    public float Velocidad { get; set; } = 2;
    public PuntoDato Direccion { get; set; } = new();
}
internal sealed class Respuesta
{
    public Resultado Resultado { get; set; } = new();
    public Visual Visual { get; set; } = new();
}