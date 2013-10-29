using TgcViewer;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Sound;

namespace AlumnoEjemplos.RandomGroup
{
    public class Projectile
    {
        public TgcBoundingSphere boundingBall;
        private float speed = (float)GuiController.Instance.Modifiers["speed"];
        public double lifeTime = 10000000;
        public float mass = (float) GuiController.Instance.Modifiers["mass"];

        private double creationTime;
        private Drawable drawing;
        public Vector3 direction;
        GuiController instance =GuiController.Instance;

        public TgcStaticSound paredSolidaSound = new TgcStaticSound();
        TgcStaticSound paredDeformableSound = new TgcStaticSound();
        TgcStaticSound proyectilSound = new TgcStaticSound();


        public Projectile setSpeed(float speed){
            direction.Multiply(speed/this.speed);
            this.speed = speed;
            return this;
        }

        public Vector3 getDirection()
        {
            return this.direction;
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
            direction.Y -= (float)instance.Modifiers["gravity"];
            boundingBall.moveCenter(direction * time);
            drawing.setPosition(boundingBall.Center);
            if ((System.DateTime.Now.TimeOfDay.TotalMilliseconds - creationTime > this.lifeTime)) return true;  
            return false;
        }

        public Projectile(Vector3 position, Drawable drawing, Vector3 direction)
        {
            this.drawing = drawing;
            this.direction = direction;
            creationTime = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
            boundingBall = new TgcBoundingSphere(position, drawing.getRadiusSize());
            paredSolidaSound.loadSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\MetalHitsSolid.wav");
            paredDeformableSound.loadSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\MetalHitsSolid.wav");
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
            this.setSpeed((this.getSpeed() * this.mass + projectile.getSpeed() * projectile.mass) / projectile.mass * projectile.mass);
            this.direction = -this.direction;
            projectile.setSpeed((this.getSpeed() * this.mass + projectile.getSpeed() * projectile.mass) / this.mass * this.mass);
            projectile.direction = -projectile.direction;
            proyectilSound.play();
        }

        public void collisionWithSolidWall(ParedSolida pared)
        {
            Vector3 tmpPos = this.getPosition();
            float radius = this.boundingBall.Radius;
            switch (pared.wall.Orientation)
            {
                case TgcPlaneWall.Orientations.XYplane:
                    this.direction.Z *= -1;
                    tmpPos.Z = pared.wall.Position.Z;
                    if (pared.wall.Position.Z - boundingBall.Center.Z > 0) tmpPos.Z -= radius; else tmpPos.Z += radius;
                    this.setPosition(tmpPos);
                    break;
                case TgcPlaneWall.Orientations.XZplane:
                    this.direction.Y *= -1;
                    tmpPos.Y = radius;
                    this.setPosition(tmpPos);
                    break;
                case TgcPlaneWall.Orientations.YZplane:
                    this.direction.X *= -1;
                    tmpPos.X = pared.wall.Position.X;
                    if (pared.wall.Position.X - boundingBall.Center.X > 0) tmpPos.X -= radius; else tmpPos.X += radius;
                    break;
            }
            paredSolidaSound.play();
            this.setSpeed(this.getSpeed() * 0.9f);
        }

        public void collisionWithDeformableWall(ParedDeformable pared)
        {
            paredDeformableSound.play();
            foreach (ParedDeformable.BBOpt BB in pared.LBBoxOpt)
            {
                if (TgcCollisionUtils.testSphereAABB(this.boundingBall, BB.BBoxOpt))
                {
                    GuiController.Instance.Logger.log("Direccion inicial pelota: " + this.direction.ToString());
                    //GuiController.Instance.Logger.log("Entre!!! Inicio: " + BB.inicio.ToString() + " Fin: " + BB.fin.ToString());
                    pared.deformarPared(this, BB);
                    paredDeformableSound.play();
                    //this.direction = -this.direction;
                    //GuiController.Instance.Logger.log("Direccion actual pelota: " + this.direction.ToString());
                    //GuiController.Instance.Logger.log("Entre!!! Inicio: " + BB.inicio.ToString() + " Fin: " + BB.fin.ToString());
                    this.lifeTime = 0;
                    
                }
            }
        }
    }
}

