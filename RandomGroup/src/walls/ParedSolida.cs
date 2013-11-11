using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.RandomGroup.src.walls
{
    public class ParedSolida : ElementoEstatico
    {
        private bool enabled;
        public bool getEnabled()
        {
            return enabled;
        }
        public void setEnabled(bool setting)
        {
            enabled = setting;
        }

        public TgcPlaneWall wall;

        public ParedSolida(Vector3 Origen, Vector3 Dimension, string Orientation, TgcTexture currentTexture)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear pared
            wall = new TgcPlaneWall();

            //textura
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
            if ((bool)GuiController.Instance.Modifiers["boundingBox"]) this.wall.BoundingBox.render();
            wall.render();
        }

        public void dispose()
        {
            wall.dispose();
        }
    }
}
