using System.Collections.Generic;
using System.Linq;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Example;
using TgcViewer;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcGeometry;
using Device = Microsoft.DirectX.Direct3D.Device;

namespace AlumnoEjemplos.RandomGroup
{
    public class EjemploAlumno : TgcExample
    {
        TgcSkyBox skyBox;
        TgcText2d textoCamara;
        List<Projectile> projectilesList = new List<Projectile>();
        List<ParedSolida> solidWallsList = new List<ParedSolida>();
        List<ParedDeformable> deformableWallsList = new List<ParedDeformable>();
        ProjectileWeapon weapon;


        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "Random";
        }
        
        public override string getDescription()
        {
            return "Para que el cañon quede alineado con la camara, mantener el click izquierdo apretado; Click Derecho para disparar.";
        }


        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 50;
            GuiController.Instance.FpsCamera.JumpSpeed = 50;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-150f, 20f, 40f), new Vector3(1f, 1f, 1f));
             
            ///////////////MODIFIERS//////////////////
            GuiController.Instance.Modifiers.addFloat("gravity", -0.2f, 0.2f, 0.02f);
            GuiController.Instance.Modifiers.addFloat("speed", 50f, 500f, 200f);
            ShootTechnique[] opciones = { new ShootTechnique() };
            ProjectileWeapon[] armas = { WeaponFactory.getTanque(), WeaponFactory.getCannon(), WeaponFactory.getGun() };
            GuiController.Instance.Modifiers.addInterval("tecnicas", opciones, 0);
            GuiController.Instance.Modifiers.addInterval("armas", armas, 0);
            GuiController.Instance.Modifiers.addFloat("mass", 1, 20f, 1);
            GuiController.Instance.Modifiers.addBoolean("boundingSphere", "Mostrar Bounding Sphere", false);
            GuiController.Instance.Modifiers.addBoolean("boundingBox", "Mostrar Bounding Box", false);

            //Crear SkyBox
            createSkyBox(alumnoMediaFolder);

            //Creo texto para mostrar datos de camara
            textoCamara = new TgcText2d {Text = "Inicial", Color = Color.White};

            //suelo
            TgcTexture.createTexture(d3dDevice, alumnoMediaFolder + "Random\\Textures\\Terrain\\tileable_grass.jpg");
            solidWallsList.Add(new ParedSolida(new Vector3(-2500, 0, -2500), new Vector3(5000, 0, 5000), "XZ", alumnoMediaFolder + "Random\\Textures\\Terrain\\tileable_grass.jpg"));
            //pared deformable
            deformableWallsList.Add(new ParedDeformable(new Vector3(0, 0, 0), 60, 60, "XY", 0.5F, alumnoMediaFolder + "Random\\Textures\\Walls\\concrete.jpg"));
            deformableWallsList.Add(new ParedDeformable(new Vector3(0, 0, 0), 60, 60, "YZ", 0.5F, alumnoMediaFolder + "Random\\Textures\\Walls\\bricks.jpg"));
            deformableWallsList.Add(new ParedDeformable(new Vector3(0, 0, 60), 60, 60, "YZ", 0.5F, alumnoMediaFolder + "Random\\Textures\\Walls\\bricks.jpg"));
            deformableWallsList.Add(new ParedDeformable(new Vector3(-60, 0, 0), 60, 60, "XY", 0.5F, alumnoMediaFolder + "Random\\Textures\\Walls\\bricks.jpg"));
        }



        /// elapsedTime: Tiempo en segundos transcurridos desde el último frame
        public override void render(float elapsedTime)
        {
            //Device d3dDevice = GuiController.Instance.D3dDevice;

            textoCamara.Text = GuiController.Instance.CurrentCamera.getPosition().ToString();
            textoCamara.render();
            
            foreach (ParedSolida pared in solidWallsList)
            {
                pared.render(elapsedTime);
            }
            foreach (ParedDeformable pared in deformableWallsList)
            {
                pared.render(elapsedTime);
            }

            //Obtener valores de Modifiers
            weapon = (ProjectileWeapon)GuiController.Instance.Modifiers["armas"];

            ///////////////INPUT//////////////////
            //Capturar Input teclado 
            if (GuiController.Instance.D3dInput.keyPressed(Key.F))
            {
            }

            //Capturar Input Mouse
            if (GuiController.Instance.D3dInput.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_RIGHT))
            {
                projectilesList.AddRange(weapon.doAction());

            }
            /*for (int i = 0; i <= (projectilesList.Count - 1); i++)
            {
                Projectile collisionable = projectilesList[i];
                if (collisionable.update(elapsedTime)) projectilesList.Remove(collisionable); else collisionable.render();
            }*/
            deteccionDeColisiones(elapsedTime);
            weapon.render();
            skyBox.render();
        }

       
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.        
        public override void close()
        {
            skyBox.dispose();

            foreach (ParedSolida pared in solidWallsList)
            {
                pared.dispose();
            }

            foreach (ParedDeformable pared in deformableWallsList)
            {
                pared.dispose();
            }
        }


        private void deteccionDeColisiones(float elapsedTime)
        {
            for (int i = 0; i <= (projectilesList.Count - 1); i++)
            {
                Projectile proyectil = projectilesList[i];

                //Deteccion entre pelotas
                for (int j = i + 1; j < projectilesList.Count; j++)
                {
                    if (TgcCollisionUtils.testSphereSphere(proyectil.boundingBall, projectilesList[j].boundingBall))
                    {
                        proyectil.collisionWithProjectile(projectilesList[j]);
                    }
                }

                //Deteccion contra las paredes no deformables
                foreach (ParedSolida pared in solidWallsList)
                {
                    if (TgcCollisionUtils.testSphereAABB(proyectil.boundingBall, pared.getBoundingBox()))
                    {
                        proyectil.collisionWithSolidWall(pared);
                    }
                }                

                //Deteccion contra las paredes SI deformables
                foreach (ParedDeformable pared in deformableWallsList)
                {
                    if (TgcCollisionUtils.testSphereAABB(proyectil.boundingBall, pared.BoundingBox))
                    {
                        proyectil.collisionWithDeformableWall(pared);
                    }
                }

                //Dibujado y borrado en caso de que se acaba su lifeTime
                if (proyectil.update(elapsedTime)) projectilesList.Remove(proyectil); 
                    else proyectil.render();
            }
        }

        private void createSkyBox(string alumnoMediaFolder)
        {
            //Crear SkyBox 
            skyBox = new TgcSkyBox {
                Center = new Vector3(0, 0, 0), 
                Size = new Vector3(10000, 10000, 10000)
            };

            //Configuracion de las texturas para cada una de las 6 caras
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, alumnoMediaFolder + "Random\\Textures\\SkyBox2\\Skybox-Top.bmp");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, alumnoMediaFolder + "Random\\Textures\\SkyBox2\\Skybox-Bottom.bmp");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, alumnoMediaFolder + "Random\\Textures\\SkyBox2\\Skybox-Back.bmp");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, alumnoMediaFolder + "Random\\Textures\\SkyBox2\\Skybox-Front.bmp");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, alumnoMediaFolder + "Random\\Textures\\SkyBox2\\Skybox-Left.bmp");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, alumnoMediaFolder + "Random\\Textures\\SkyBox2\\Skybox-Right.bmp");

            //Actualizacion de los valores
            skyBox.updateValues();
        }

    }
}
