using System;
using Microsoft.DirectX;

namespace AlumnoEjemplos.RandomGroup
{
    static class MathUtil
    {
        private static float[] values = new float[360 * 8];

        static MathUtil()
        {
            //step = 
            //for (int i=0;i<values.Length;i++){
            //    values[i] = Math.Asin(step);
            //}
        }

        public static float getDegree(Vector2 v1, Vector2 v2)
        {
            return (float)Math.Acos(Vector2.Dot(v1, v2) / Math.Abs(v1.LengthSq() * v2.LengthSq()));
        }
        public static float getDegree(float opuesto, float adyacente)
        {
            return (float)Math.Atan2(opuesto, adyacente);
        }
    }
}
