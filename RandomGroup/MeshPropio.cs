using System;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.RandomGroup
{
    public abstract class MeshPropio : Drawable
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
        public void setEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
        String meshPath;
        public String MeshPath
        {
            get{ return meshPath; }
            set { meshPath = value; }
        }
        //public MeshPropio malla;
        String rutaDeLaTextura;
        public String RutaDeLaTextura
        {
            get { return rutaDeLaTextura; }
            set { rutaDeLaTextura = value; }
        }
        public short[] bufferDeIndices;
        public CustomVertex.PositionNormalTextured[] bufferDeVertices;
        public abstract MeshPropio scale(Vector3 scale);
        public abstract void render(float elapsedTime);
    }
}
