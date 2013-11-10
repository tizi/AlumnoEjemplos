using System.Collections.Generic;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.RandomGroup
{
    /// <summary>
    /// Celda de una grilla regular
    /// </summary>
    public class GrillaRegularNode
    {
        private List<ElementoEstatico> models;
        /// <summary>
        /// Modelos de la celda
        /// </summary>
        public List<ElementoEstatico> Models
        {
          get { return models; }
          set { models = value; }
        }

        private TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox de la celda
        /// </summary>
        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
            set { boundingBox = value; }
        }


        /// <summary>
        /// Activar todos los modelos de la celda
        /// </summary>
        public void activateCellMeshes()
        {
            foreach (ElementoEstatico mesh in models)
            {
                mesh.setEnabled(true);
            }
        }
    }
}
