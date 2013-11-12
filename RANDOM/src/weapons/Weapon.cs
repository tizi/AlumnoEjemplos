using System;
using System.Collections.Generic;
using AlumnoEjemplos.RANDOM.src.meshUtils;
using AlumnoEjemplos.RANDOM.src.shootTechniques;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils._2D;
using AlumnoEjemplos.RANDOM.src.camera;

namespace AlumnoEjemplos.RANDOM.src.weapons
{
    public abstract class Weapon
    {
        public String name = "";
        public Drawable weaponDrawing;
        protected TgcSprite crosshair = new TgcSprite();
        FpsCamera camera = (FpsCamera)GuiController.Instance.CurrentCamera;

        private static readonly Vector3 axisY = new Vector3(0, 1, 0);
        public Vector3 bulletsInitPosition;
        public Vector2 initRotation = new Vector2(0, 0);
        public Vector3 initPosition = new Vector3(0, 0, 0);
        protected Vector3 tmpRotationXZ;
        protected float tmpRotationY;
        private Vector3 tmplookAt;

        public virtual void render()
        {
            update();
            weaponDrawing.render();
            GuiController.Instance.Drawer2D.beginDrawSprite();
            crosshair.render();
            GuiController.Instance.Drawer2D.endDrawSprite();
        }

        public virtual void update()
        {
            //Hay que poner segun cada arma la distancia a la camara.
            weaponDrawing.setPosition(camera.getPosition());
            weaponDrawing.moveOrientedZ(initPosition.Z);
            weaponDrawing.moveOrientedX(initPosition.X);
            weaponDrawing.moveOrientedY(initPosition.Y); 

            //Y rotarla segun rota la camara
            tmplookAt = camera.getLookAt() - camera.getPosition();
            tmpRotationY = MathUtil.getDegree(tmplookAt.X, tmplookAt.Z) + initRotation.X;
            weaponDrawing.setRotationY(tmpRotationY);
            tmpRotationXZ = Vector3.Cross(tmplookAt, axisY);
            weaponDrawing.setRotationXZ(tmpRotationXZ, initRotation.Y - (float)Math.Acos(Vector3.Dot(tmplookAt, axisY)));
        }

        public abstract List<Projectile> doAction();

        public override string ToString()
        {
            return name;
        }
    }
}
