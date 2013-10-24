using System.Collections.Generic;
using TgcViewer;
using Microsoft.DirectX;
using TgcViewer.Utils.Input;
using AlumnoEjemplos.RandomGroup;

namespace AlumnoEjemplos.RandomGroup
{
    public class ShootTechnique
    {
        public double lastShootTime = 0;
        //public Drawable bulletDrawing= new Ball(1,5);

        public float distance = 8;
        public int timeBetweenShoots = 500;
        public Vector3 initDir;
        public Vector3 pos;
        public Vector3 dir;
        public Vector3 initPos;
        public Colisionador tmpBullet;
        TgcCamera camera;

        public List<Colisionador> getShoot(Drawable bulletDrawing)
        {
            List<Colisionador> tmpList = new List<Colisionador>();
            if (System.DateTime.Now.TimeOfDay.TotalMilliseconds - lastShootTime > timeBetweenShoots)
            {
                lastShootTime = System.DateTime.Now.TimeOfDay.TotalMilliseconds;

                camera = GuiController.Instance.CurrentCamera;

                initPos = camera.getLookAt();
                initDir = initPos - camera.getPosition();
                initPos.Add(initDir * distance);
                initDir.Scale((float)GuiController.Instance.Modifiers["speed"]);

                getRealShoot(tmpList, bulletDrawing);
            }
            return tmpList;
        }

        public virtual void getRealShoot(List<Colisionador> tmpList, Drawable bulletDrawing)
        {
            Colisionador tmpColisionador = new Colisionador(initPos, bulletDrawing.clone(), initDir);
            tmpList.Add(tmpColisionador);
        }

        public override string ToString()
        {
            return "Simple Shoot";
        }
    }
}
