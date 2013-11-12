using System.Drawing;
using AlumnoEjemplos.RANDOM.src.meshUtils;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils._2D;

namespace AlumnoEjemplos.RANDOM.src.weapons
{
    class WeaponFactory
    {
        static Size screenSize = GuiController.Instance.Panel3d.Size;
        static string path = GuiController.Instance.AlumnoEjemplosMediaDir;

        public static ProjectileWeapon getGun()
        {
            TgcSprite tmpSprite = new TgcSprite
            {
                Texture = TgcTexture.createTexture(path + "Random\\Textures\\crosshair.png")
            };
            tmpSprite.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - tmpSprite.Texture.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - tmpSprite.Texture.Height / 2, 0));
            ProjectileWeapon weapon = new ProjectileWeapon(new XMLMesh(path + "Random\\Meshes\\Weapons\\Pistol.xml"), tmpSprite, path + "Random\\Sounds\\CannonShoot.wav")
            {
                initPosition = new Vector3(-0.4f, -0.4f, 1.1f),
                initRotation = new Vector2(2.8f, 3.14f/2),
                name = "Pistola"
            };
            //weapon.bulletDrawing = MeshFactory.getMesh(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\Meshes\\MetalSphere.xml").scale(new Vector3(0.02f, 0.02f, 0.02f));
            //weapon.initPosition = new Vector3(20, -0.8f, -1.1f);
            //weapon.initRotation = new Vector2(3, 3.14f / 2);
            return weapon;
        }

        public static ProjectileWeapon getCannon()
        {
            TgcSprite tmpSprite = new TgcSprite
            {
                Texture = TgcTexture.createTexture(path + "Random\\Textures\\crosshair.png")
            };
            tmpSprite.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - tmpSprite.Texture.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - tmpSprite.Texture.Height / 2, 0));
            XMLMesh mesh = new XMLMesh(path + "Random\\Meshes\\Weapons\\Cannon.xml");
            //XMLMesh mesh = new XMLMesh(path + "Random\\cannon_ext\\cannon-TgcScene.xml");
            ProjectileWeapon weapon = new ProjectileWeapon(mesh, tmpSprite, path + "Random\\Sounds\\CannonShoot.wav");
            //mesh.scale(new Vector3(0.02f, 0.02f, 0.02f));
            mesh.scale(new Vector3(0.01f, 0.01f, 0.01f));
            //weapon.initPosition = new Vector3(-0.75f, -0.8f, 1.2f);
            weapon.initPosition = new Vector3(0.16f, -0.3f, 1.4f);
            weapon.initRotation = new Vector2(-1.85f , 1.5f);
            //weapon.bulletDrawing = MeshFactory.getMesh(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\Meshes\\MetalSphere.xml").scale(new Vector3(0.02f, 0.02f, 0.02f));
            weapon.name = "Canion";
            return weapon;
        }

        public static ProjectileWeapon getTanque()
        {
            TgcSprite tmpSprite = new TgcSprite
            {
                Texture = TgcTexture.createTexture(path + "Random\\Textures\\crosshair.png")
            };
            tmpSprite.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - tmpSprite.Texture.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - tmpSprite.Texture.Height / 2, 0));
            XMLMesh mesh = new XMLMesh(path + "Random\\Meshes\\Weapons\\tanque-TgcScene.xml");
            ProjectileWeapon weapon = new ProjectileWeapon(mesh, tmpSprite, path + "Random\\Sounds\\CannonShoot.wav");
            //weapon.bulletDrawing = MeshFactory.getMesh(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\Meshes\\MetalSphere.xml").scale(new Vector3(0.02f, 0.02f, 0.02f));
            mesh.scale(new Vector3(0.005f, 0.005f, 0.005f));
            weapon.initPosition = new Vector3(-0.4f, -0.4f, 1.1f);
            weapon.initRotation = new Vector2(2.8f, 3.14f / 2);
            weapon.name = "Tanque";
            return weapon;
        }

    }
}
