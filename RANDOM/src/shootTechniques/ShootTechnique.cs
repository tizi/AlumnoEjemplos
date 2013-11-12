using System;
using System.Collections.Generic;
using AlumnoEjemplos.RANDOM.src.meshUtils;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.Input;

namespace AlumnoEjemplos.RANDOM.src.shootTechniques
{
    public abstract class ShootTechnique
    {
        public double lastShootTime = 0;
        public float distance = 8;
        public int timeBetweenShoots = 1000;
        public Vector3 initDir;
        public Vector3 pos;
        public Vector3 dir;
        public Vector3 initPos;
        public TgcCamera camera;
        public Drawable bulletDrawing;


        public List<Projectile> getShoot()
        {
            List<Projectile> tmpList = new List<Projectile>();
            if (DateTime.Now.TimeOfDay.TotalMilliseconds - lastShootTime > timeBetweenShoots)
            {
                lastShootTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

                camera = GuiController.Instance.CurrentCamera;

                initPos = camera.getLookAt();
                initDir = initPos - camera.getPosition();
                initPos.Add(initDir * distance);
                initDir.Scale((float)GuiController.Instance.Modifiers["Velocidad"]);

                getRealShoot(tmpList);
            }
            return tmpList;
        }

        public abstract void getRealShoot(List<Projectile> tmpList);

        public abstract override string ToString();
    }
}
