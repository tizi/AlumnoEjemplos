using System.Collections.Generic;
using AlumnoEjemplos.RandomGroup.src.meshUtils;
using Microsoft.DirectX;
using TgcViewer;

namespace AlumnoEjemplos.RandomGroup.src.shootTechniques
{
    public class SimpleShoot : ShootTechnique
    {
        public int bulletsAmount = 7;

        public SimpleShoot()
        {
            bulletDrawing = MeshFactory.getMesh(GuiController.Instance.AlumnoEjemplosMediaDir + 
                "Random\\Meshes\\Weapons\\MetalSphere.xml").scale(new Vector3(0.02f, 0.02f, 0.02f));
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