using System;
using TgcViewer;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Sound;

namespace AlumnoEjemplos.RandomGroup
{
    public class Projectile
    {
        public TgcBoundingSphere boundingBall;
        private float speed;
        public double lifeTime = 10000;
        public float mass;

        private double creationTime;
        private Drawable drawing;
        public Vector3 direction;
        public Vector3 posicionCuadroAnt;
        GuiController instance = GuiController.Instance;

        public TgcStaticSound paredSolidaSound = new TgcStaticSound();
        TgcStaticSound paredDeformableSound = new TgcStaticSound();
        TgcStaticSound proyectilSound = new TgcStaticSound();


        public Projectile setSpeed(float speedValue){
            direction.Multiply(speedValue/speed);
            speed = speedValue;
            return this;
        }

        public Vector3 getDirection()
        {
            return direction;
        }

        public void setDirection(Vector3 direction)
        {
            this.direction = direction;
        }

        public float getSpeed(){
            return speed;
        }

        public Vector3 getPosition(){
            return boundingBall.Center;
        }

        public void setPosition(Vector3 position)
        {
            boundingBall.setCenter(position);
        }
        public bool update(float time)
        {
            direction.Y -= (float)instance.Modifiers["Gravedad"];
            boundingBall.moveCenter(direction * time);
            drawing.setPosition(boundingBall.Center);
            return (DateTime.Now.TimeOfDay.TotalMilliseconds - creationTime > lifeTime);
        }

        public Projectile(Vector3 position, Drawable drawing, Vector3 direction)
        {
            posicionCuadroAnt = position;
            this.drawing = drawing;
            this.direction = direction;
            mass = (float)GuiController.Instance.Modifiers["Masa"];
            speed = (float)GuiController.Instance.Modifiers["Velocidad"];
            creationTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            boundingBall = new TgcBoundingSphere(position, drawing.getRadiusSize());

            paredSolidaSound.loadSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\MetalHitsSolid.wav");
            paredDeformableSound.loadSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\DeformableHit.wav");
            proyectilSound.loadSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\MetalHitsSolid.wav");
        }

        public Projectile setDrawing(Drawable drawing) {
            this.drawing = drawing;
            return this;
        }

        public void render()
        {
            if ((bool)GuiController.Instance.Modifiers["boundingSphere"])  boundingBall.render();
            drawing.render();
        }

        public void collisionWithProjectile(Projectile projectile)
        {
            setSpeed((getSpeed() * mass + projectile.getSpeed() * projectile.mass) / projectile.mass * projectile.mass);
            direction *= -1.0f;
            projectile.setSpeed((getSpeed() * mass + projectile.getSpeed() * projectile.mass) / mass * mass);
            projectile.direction *= -1.0f;
            proyectilSound.play();
        }

        public void collisionWithSolidWall(ParedSolida pared)
        {
            GuiController.Instance.Logger.log("Colisione con el piso");
            Vector3 tmpPos = getPosition();
            float radius = boundingBall.Radius;
            switch (pared.wall.Orientation)
            {
                case TgcPlaneWall.Orientations.XYplane:
                    direction.Z *= -1;
                    tmpPos.Z = pared.wall.Position.Z;
                    if (pared.wall.Position.Z - boundingBall.Center.Z > 0) tmpPos.Z -= radius; else tmpPos.Z += radius;
                    setPosition(tmpPos);
                    break;
                case TgcPlaneWall.Orientations.XZplane:
                    direction.Y *= -1.0f;
                    //if (pared.wall.Position.Y - boundingBall.Center.Y > 0) tmpPos.Y -= radius; else tmpPos.Y += radius;
                    //tmpPos.Y = radius;
                    //setPosition(tmpPos);
                    break;
                case TgcPlaneWall.Orientations.YZplane:
                    direction.X *= -1;
                    tmpPos.X = pared.wall.Position.X;
                    if (pared.wall.Position.X - boundingBall.Center.X > 0) tmpPos.X -= radius; else tmpPos.X += radius;
                    break;
            }
            paredSolidaSound.play();
            setSpeed(getSpeed() * 0.8f);            
        }

        public void collisionWithDeformableWall(ParedDeformable pared)
        {
            pared.deformarPared(this);
            paredDeformableSound.play();
            direction *= -1;
            setSpeed(getSpeed() * 0.4f);
            lifeTime = 0;
        }
    }
}

