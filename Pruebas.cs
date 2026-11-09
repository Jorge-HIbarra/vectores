using System.Numerics;
using static LaboratorioSistemasVivos.Matematicas2D;

namespace LaboratorioSistemasVivos;

internal static class Pruebas
{
    public static int Ejecutar()
    {
        int errores = 0;
        void Comprobar(string nombre, Vector2 obtenido, Vector2 esperado)
        {
            bool ok = Vector2.Distance(obtenido, esperado) < 0.001f;
            Console.WriteLine($"{(ok ? "OK" : "ERROR")} {nombre}: {obtenido}, esperado {esperado}");
            if (!ok) errores++;
        }

        Comprobar("Movimiento 3-4-5", MoverHacia(new(2, 2), new(5, 6), 2, .5f), new(2.6f, 2.8f));
        Comprobar("Vector cero", MoverHacia(new(2, 2), new(2, 2), 2, .5f), new(2, 2));
        Comprobar("Sin sobrepasar", MoverHacia(new(0, 0), new(.1f, 0), 2, .5f), new(.1f, 0));
        Comprobar("Identidad", AplicarPunto(Identidad(), new(1, 2)), new(1, 2));
        float[,] m = { { 2, 0, 3 }, { 0, 1, -1 }, { 0, 0, 1 } };
        Comprobar("Matriz del ejemplo", AplicarPunto(m, new(1, 2)), new(5, 1));
        Comprobar("Rotacion de 90", AplicarPunto(Rotacion(90), new(1, 0)), new(0, 1));
        var t = Traslacion(4, 1); var r = Rotacion(90); var s = Escala(2, 1);
        Comprobar("TRS", AplicarPunto(Multiplicar(t, Multiplicar(r, s)), new(1, 0)), new(4, 3));
        Comprobar("RTS", AplicarPunto(Multiplicar(r, Multiplicar(t, s)), new(1, 0)), new(-1, 6));
        Comprobar("TSR", AplicarPunto(Multiplicar(t, Multiplicar(s, r)), new(1, 0)), new(4, 2));
        var centro = new Vector2(2, 2); var pivot = RotarEnPivote(centro, 90);
        Comprobar("Punto alrededor del pivote", AplicarPunto(pivot, new(3, 2)), new(2, 3));
        Comprobar("Pivote fijo", AplicarPunto(pivot, centro), centro);
        Console.WriteLine($"Errores: {errores}");
        return errores == 0 ? 0 : 1;
    }
}