using System;
using System.Collections.Generic;
using AlumnoEjemplos.RandomGroup.src.meshUtils;
using Microsoft.DirectX;
using TgcViewer;

namespace AlumnoEjemplos.RandomGroup.src.shootTechniques
{
    public class ShrapnelShoot : ShootTechnique
    {
        private Random rand = new Random();
        public int bulletsAmount = 7;

        public ShrapnelShoot()
        {
            timeBetweenShoots = 1500;
            bulletDrawing = MeshFactory.getMesh(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\Meshes\\Weapons\\SmallSphere.xml").scale(new Vector3(0.01f, 0.01f, 0.01f));
        }

        public override void getRealShoot(List<Projectile> tmpList)
        {
            for (int i = 0; i < bulletsAmount; i++)
            {
                initDir.Add(new Vector3((-1 + (float)rand.NextDouble() * 2) / 5, (-0.2f + (float)rand.NextDouble() * 2) / 5, (-1 + (float)rand.NextDouble() * 2) / 5));//Una direccion al azar para cada bala
                initDir.Normalize();
                initDir.Scale((float)GuiController.Instance.Modifiers["Velocidad"]);
                Vector3 tmpPos = new Vector3((-3 + (float)rand.NextDouble() * 6), (-2 + (float)rand.NextDouble() * 6), (-3 + (float)rand.NextDouble() * 6));
                Projectile tmpProjectile = new Projectile(initPos + tmpPos, bulletDrawing.clone(), initDir)
                {
                    mass = (float) rand.NextDouble()*50
                };
                tmpList.Add(tmpProjectile);
            }
        }

        public override string ToString()
        {
            return "Shrapnel Shoot";
        }
    }
}