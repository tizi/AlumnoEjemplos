using System;
using System.Collections.Generic;
using TgcViewer;
using Microsoft.DirectX;

namespace AlumnoEjemplos.RandomGroup
{
    public class ShrapnelShoot : ShootTechnique
    {
        private Random rand = new Random();
        public int bulletsAmount = 10;

        public ShrapnelShoot()
        {
            timeBetweenShoots = 1500;
        }

        public override void getRealShoot(List<Projectile> tmpList, Drawable bulletDrawing)
        {
            Vector3 tmpPos = new Vector3();
            Projectile tmpColisionador;
            for (int i = 0; i < bulletsAmount; i++)
            {
                initDir.Add(new Vector3((-1 + (float)rand.NextDouble() * 2) / 5, (-0.2f + (float)rand.NextDouble() * 2) / 5, (-1 + (float)rand.NextDouble() * 2) / 5));//Una direccion al azar para cada bala
                initDir.Normalize();
                initDir.Scale((float)GuiController.Instance.Modifiers["Velocidad"]);
                tmpPos = new Vector3((-3 + (float)rand.NextDouble() * 6), (-2 + (float)rand.NextDouble() * 6), (-3 + (float)rand.NextDouble() * 6));
                tmpColisionador = new Projectile(initPos + tmpPos, bulletDrawing.clone(), initDir);
                tmpColisionador.mass = (float)rand.NextDouble() * 50;
                tmpList.Add(tmpColisionador);
            }
        }

        public override string ToString()
        {
            return "Shrapnel Shoot";
        }
    }
}