using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.RANDOM.src.meshUtils
{
    public class XMLMesh : MeshPropio
    {
        TgcMesh mesh;


        public new void dispose()
        {
            mesh.dispose();
        }

        public Vector3 escala = new Vector3(1,1,1);
        protected float angleY = 0;
        private static readonly Vector3 vectAxisY = new Vector3(0,1,0);

        float angleXZ = 0;
        Vector3 vectAxisXZ;

        private XMLMesh(TgcMesh mesh)
        {
            this.mesh = mesh;
        }
        public XMLMesh(string meshPath)
        {
            TgcSceneLoader cargador = new TgcSceneLoader();                        
            mesh = cargador.loadSceneFromFile(meshPath).Meshes[0];
            mesh.AutoTransformEnable=false;
            mesh.AutoUpdateBoundingBox = true;

           
        }

        public override void renderReal()
        {
            //render();
        }

        public override void render(float elapsedTime) {
            render();
        }

        public override void render()
        {
            //Agrego una multiplicacion mas para las rotaciones pero me da mas y mejor flexibilidad.
            mesh.Transform = Matrix.Scaling(escala)
                    * Matrix.RotationAxis(vectAxisY, angleY)
                    * Matrix.RotationAxis(vectAxisXZ, angleXZ)
                    * Matrix.Translation(position);
            mesh.BoundingBox.transform(mesh.Transform);
            if ((bool)GuiController.Instance.Modifiers["boundingBox"]) mesh.BoundingBox.render();
            mesh.render();
        }

        public override Drawable clone()
        {
            return new XMLMesh(mesh).scale(escala);
        }

        public override float getRadiusSize()
        {
            mesh.Transform = Matrix.Scaling(escala)
                    * Matrix.RotationAxis(vectAxisY, angleY)
                    * Matrix.RotationAxis(vectAxisXZ, angleXZ)
                    * Matrix.Translation(position);
            mesh.BoundingBox.transform(mesh.Transform);
            return mesh.BoundingBox.calculateBoxRadius();
        }

        public override void setPosition(Vector3 position)
        {
            this.position = position;
        }
        public override void move(Vector3 direction)
        {
            position.Add(direction);
        }
        public override void moveOrientedZ(float movement) {
            Vector3 tmpVectZ = GuiController.Instance.CurrentCamera.getLookAt() - GuiController.Instance.CurrentCamera.getPosition();
            tmpVectZ.Normalize();
            tmpVectZ.Multiply(movement);
            position.Add(tmpVectZ);
        }
        public override void moveOrientedX(float movement)
        {
            Vector3 tmpVectZ = GuiController.Instance.CurrentCamera.getLookAt() - GuiController.Instance.CurrentCamera.getPosition();
            Vector3 tmpVectX = Vector3.Cross(tmpVectZ, new Vector3(0, 1, 0));
            tmpVectX.Normalize();
            tmpVectX.Multiply(movement);
            position.Add(tmpVectX);
        }
        public override void moveOrientedY(float movement)
        {
            Vector3 tmpVectZ = GuiController.Instance.CurrentCamera.getLookAt() - GuiController.Instance.CurrentCamera.getPosition();
            Vector3 tmpVectX = Vector3.Cross(tmpVectZ, new Vector3(0, 1, 0));
            Vector3 tmpVectY = Vector3.Cross(tmpVectX, tmpVectZ);
            tmpVectY.Normalize();
            tmpVectY.Multiply(movement);
            position.Add(tmpVectY);
        }
        public override void setRotationY(float angle)
        {
            angleY = angle;
        }

        public override void setRotationXZ(Vector3 vectAxisXZ, float angleXZ)
        {
            this.vectAxisXZ = vectAxisXZ;
            this.angleXZ = angleXZ;
        }

        public override MeshPropio scale(Vector3 scale)
        {
            escala = scale;
            return this;
        }
    }
}
