using System;
using System.Collections.Generic;
using TgcViewer;
using Microsoft.DirectX;

namespace AlumnoEjemplos.RandomGroup
{
    public class SimpleShoot : ShootTechnique
    {
        private Random rand = new Random();
        public int bulletsAmount = 7;

        public SimpleShoot()
        {
            bulletDrawing = MeshFactory.getMesh(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\Meshes\\MetalSphere.xml").scale(new Vector3(0.02f, 0.02f, 0.02f));
        }

        public override void getRealShoot(List<Projectile> tmpList)
        {
            Projectile tmpColisionador = new Projectile(initPos, bulletDrawing.clone(), initDir);
            tmpList.Add(tmpColisionador);
        }

        public override string ToString()
        {
            return "Simple Shoot";
        }
    }
}