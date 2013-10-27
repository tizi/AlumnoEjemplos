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
            //weapon.initPosition = new Vector3(20, -0.8f, -1.1f);
            //weapon.initRotation = new Vector2(3, 3.14f / 2);
            weapon.name = "Pistola";
            return weapon;
        }

        public static FireGunWeapon getCannion()
        {
            TgcSprite tmpSprite = new TgcSprite();
            tmpSprite.Texture = TgcTexture.createTexture(path + "Random\\crosshair.png");
            tmpSprite.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - tmpSprite.Texture.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - tmpSprite.Texture.Height / 2, 0));
            XMLMesh mesh = new XMLMesh(path + "Random\\cannion_3dm\\cannion-TgcScene.xml");
            //XMLMesh mesh = new XMLMesh(path + "Random\\cannon_ext\\cannon-TgcScene.xml");
            FireGunWeapon weapon = new FireGunWeapon(mesh, tmpSprite, path + "Random\\CannonShoot.wav");
            //mesh.scale(new Vector3(0.02f, 0.02f, 0.02f));
            mesh.scale(new Vector3(0.01f, 0.01f, 0.01f));
            //weapon.initPosition = new Vector3(-0.75f, -0.8f, 1.2f);
            weapon.initPosition = new Vector3(-0.5f, -0.4f, 1.1f);
            weapon.initRotation = new Vector2(2.5f , 3.14f / 2);
            ///weapon.initRotation = new Vector2(3, 3.14f / 2);
            //XMLMesh tmpDrawing = new XMLMesh(path + "ISeeDeadPixels\\Pez\\Pez-TgcScene.xml");
            //tmpDrawing.scale(new Vector3(0.1f, 0.1f, 0.1f));
            //weapon.bulletDrawing = tmpDrawing;
            weapon.bulletDrawing = MeshFactory.getMesh(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\MetalSphere.xml").scale(new Vector3(0.02f, 0.02f, 0.02f));
            weapon.name = "Canion";
            return weapon;
        }

    }
}
