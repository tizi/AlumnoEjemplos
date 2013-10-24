using TgcViewer;
using AlumnoEjemplos.RandomGroup;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using System.Drawing;

namespace AlumnoEjemplos.RandomGroup
{
    class WeaponFactory
    {
        static Size screenSize = GuiController.Instance.Panel3d.Size;
        static string path = GuiController.Instance.AlumnoEjemplosMediaDir;

        public static FireGunWeapon getGun()
        {
            TgcSprite tmpSprite = new TgcSprite();
            tmpSprite.Texture = TgcTexture.createTexture(path + "Random\\crosshair.png");
            tmpSprite.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - tmpSprite.Texture.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - tmpSprite.Texture.Height / 2, 0));
            FireGunWeapon weapon = new FireGunWeapon(new XMLMesh(path + "Random\\Cannon\\pistol-TgcScene.xml"), tmpSprite, path + "Random\\CannonShoot.wav");
            weapon.bulletDrawing = MeshFactory.getMesh(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\MetalSphere.xml").scale(new Vector3(0.02f, 0.02f, 0.02f));
            weapon.initPosition = new Vector3(-0.4f, -0.4f, 1.1f);
            weapon.initRotation = new Vector2(2.8f, 3.14f / 2);
            weapon.name = "Pistola";
            return weapon;
        }

    }
}
