using TgcViewer;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;

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

        public Projectile setSpeed(float speed){
            direction.Multiply(speed/this.speed);
            this.speed = speed;
            return this;
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
            
            if ((System.DateTime.Now.TimeOfDay.TotalMilliseconds - creationTime > lifeTime)) return true;  
            return false;
        }

        public Projectile(Vector3 position, Drawable drawing, Vector3 direction)
        {
            this.drawing = drawing;
            this.direction = direction;
            creationTime = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
            boundingBall = new TgcBoundingSphere(position, drawing.getRadiusSize());
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
    }
}

