using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary
{
    public delegate Vector3 Function (float u, float v, float t);
    
    public enum FunctionName { Wave, MultiWave, Ripple, Sphere, Torus}

    static readonly Function[] Functions = { Wave, MultiWave, Ripple, Sphere, Torus};

    public static int FunctionCount => Functions.Length;

    public static Function GetFunction (FunctionName name)
    {
        return Functions[(int)name];
    }
    
    public static FunctionName GetNextFunctionName (FunctionName name) {
        return (int)name < Functions.Length - 1 ? name + 1 : 0;
    }
    
    public static FunctionName GetRandomFunctionNameOtherThan (FunctionName name) {
        var choice = (FunctionName)Random.Range(1, Functions.Length);
        return choice == name ? 0 : choice;
    }
    
    public static Vector3 Morph (float u, float v, float t, Function from, Function to, float progress)
    {
        return Vector3.LerpUnclamped(
            from(u, v, t), to(u, v, t), SmoothStep(0f, 1f, progress)
        );
    }
    /// <summary>
    /// Simple sin wave
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private static Vector3 Wave (float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + v + t));
        p.z = v;
        return p;
    }

    /// <summary>
    /// Sum of three different waves
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private static Vector3 MultiWave (float u, float v, float t) {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + 0.5f * t));
        p.y += 0.5f * Sin(2f * PI * (v + t));
        p.y += Sin(PI * (u + v + 0.25f * t));
        p.y *= (1f / 2.5f);
        p.z = v;
        return p;
    }
    
    /// <summary>
    /// Simple ripple function based on distance from center
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private static Vector3 Ripple (float u, float v, float t) {
        float d = Sqrt(u*u + v*v);
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (4f * d - t));
        p.y /= 1f + 10f * d;
        p.z = v;
        return p;
    }

    /// <summary>
    /// Twisting Sphere function
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private static Vector3 Sphere (float u, float v, float t) {
        float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t));
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
    
    /// <summary>
    /// Torus function
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private static Vector3 Torus (float u, float v, float t)
    {
        float r1 = 0.7f + 0.1f * Sin(PI * (6f * u + 0.5f * t));
        float r2 = 0.15f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));
        float s = r1 + r2 * Cos(PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
}