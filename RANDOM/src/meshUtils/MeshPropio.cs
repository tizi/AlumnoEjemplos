using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.RANDOM.src.meshUtils
{
    public abstract class MeshPropio : Drawable, ElementoEstatico
    {
        private bool enabled;
        public TgcBoundingBox boundingBox;
        public virtual TgcBoundingBox getBoundingBox()
        {
            return boundingBox;
        }

        public bool getEnabled()
        {
            return enabled;
        }
        public void setEnabled(bool setting)
        {
            enabled = setting;
        }

        public override void dispose()
        {
            boundingBox.dispose();            
        }

        public string MeshPath { get; set; }

        public string RutaDeLaTextura { get; set; }

        public short[] bufferDeIndices;
        public CustomVertex.PositionNormalTextured[] bufferDeVertices;
        public abstract MeshPropio scale(Vector3 scale);
        public abstract void render(float elapsedTime);
    }
}
