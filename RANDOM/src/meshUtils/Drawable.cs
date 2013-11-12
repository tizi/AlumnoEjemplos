using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;

namespace AlumnoEjemplos.RANDOM.src.meshUtils
{
    public abstract class Drawable
    {
        public abstract void dispose();

        public Vector3 position;
        public Matrix tran = Matrix.Identity;

        public abstract float getRadiusSize();
        public abstract Drawable clone();
        public abstract void renderReal();
        public virtual void render()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            d3dDevice.Transform.World = tran;
            renderReal();
        }
        public virtual void move(Vector3 position)
        {

        }
        public virtual void moveOrientedY(float movement)
        {

        }
        public virtual void moveOrientedX(float movement)
        {

        }
        public virtual void moveOrientedZ(float movement)
        {

        }
        public virtual void setPosition(Vector3 position)
        {
            this.position = position;
            tran.Translate(position);
        }
        public virtual void setRotationY(float angle)
        {

        }
        public virtual void setRotationXZ(Vector3 vectAxisXZ, float angleXZ)
        {

        }
    }
}
