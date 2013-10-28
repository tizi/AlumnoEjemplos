using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.RandomGroup
{
    public class ParedSolida
    {
        private bool enabled;
        public bool getEnabled()
        {
            return enabled;
        }
        public void setEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        public TgcPlaneWall wall;
        TgcTexture currentTexture;

        public ParedSolida(Vector3 Origen, Vector3 Dimension, string Orientation, string TexturePath)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear pared
            wall = new TgcPlaneWall();

            //textura
            currentTexture = TgcTexture.createTexture(d3dDevice, TexturePath);
            wall.setTexture(currentTexture);

            //parametros basicos
            wall.Origin = Origen;
            wall.Size = Dimension;
            wall.AutoAdjustUv = false;
            wall.UTile = 4;
            wall.VTile = 4;

            if (Orientation == "XY")
                wall.Orientation = TgcPlaneWall.Orientations.XYplane;
            else if (Orientation == "XZ")
                wall.Orientation = TgcPlaneWall.Orientations.XZplane;
            else if (Orientation == "YZ")
                wall.Orientation = TgcPlaneWall.Orientations.YZplane;

            wall.BoundingBox.setExtremes(Origen, Dimension);

            wall.updateValues();

        }

        public TgcBoundingBox getBoundingBox()
        {
            return wall.BoundingBox;
        }

        public void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            wall.render();
        }

        public void dispose()
        {
            wall.dispose();
        }
    }
}
