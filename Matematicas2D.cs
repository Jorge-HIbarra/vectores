using System.Numerics;

namespace LaboratorioSistemasVivos;

// Convención común al papel: puntos columna, p nueva = M por p.
// Todas las matrices son afines 2D con última fila (0, 0, 1).
internal static class Matematicas2D
{
    public static Vector2 MoverHacia(Vector2 posicion, Vector2 objetivo, float rapidez, float dt)
    {
        Vector2 d = objetivo - posicion;
        float distancia = d.Length();
        if (distancia <= 0.0001f) return objetivo;
        Vector2 direccion = d / distancia;
        float paso = MathF.Min(MathF.Max(0, rapidez) * MathF.Max(0, dt), distancia);
        return posicion + direccion * paso;
    }

    public static float[,] Identidad() => new float[,]
    {
        { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 }
    };

    public static float[,] Traslacion(float tx, float ty) => new float[,]
    {
        { 1, 0, tx }, { 0, 1, ty }, { 0, 0, 1 }
    };

    public static float[,] Escala(float sx, float sy) => new float[,]
    {
        { sx, 0, 0 }, { 0, sy, 0 }, { 0, 0, 1 }
    };

    public static float[,] Rotacion(float anguloGrados)
    {
        float theta = anguloGrados * MathF.PI / 180f;
        float c = MathF.Cos(theta);
        float s = MathF.Sin(theta);
        return new float[,]
        {
            { c, -s, 0 }, { s, c, 0 }, { 0, 0, 1 }
        };
    }

    public static Vector2 AplicarPunto(float[,] m, Vector2 p)
    {
        float x = m[0, 0] * p.X + m[0, 1] * p.Y + m[0, 2];
        float y = m[1, 0] * p.X + m[1, 1] * p.Y + m[1, 2];
        return new Vector2(x, y);
    }

    public static float[,] Multiplicar(float[,] a, float[,] b)
    {
        float[,] c = new float[3, 3];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                for (int k = 0; k < 3; k++)
                    c[i, j] += a[i, k] * b[k, j];
        return c;
    }

    public static float[,] RotarEnPivote(Vector2 centro, float grados) => Multiplicar(
        Traslacion(centro.X, centro.Y),
        Multiplicar(Rotacion(grados), Traslacion(-centro.X, -centro.Y))
    );

    public static float EscalaPorEnergia(float energia) =>
        0.6f + 0.8f * Math.Clamp(energia / 100f, 0, 1);
}