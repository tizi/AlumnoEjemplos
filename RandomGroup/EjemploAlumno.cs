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
        FpsCamera camera;
        TgcSkyBox skyBox;
        TgcText2d textoCamara;
        List<Projectile> projectilesList = new List<Projectile>();
        List<ParedSolida> solidWallsList = new List<ParedSolida>();
        List<ParedDeformable> deformableWallsList = new List<ParedDeformable>();
        ProjectileWeapon weapon;
        //int cantMaximaProyectiles;


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
            return "Para pausar el juego presionar P; Click Izquierdo para disparar.";
        }


        public override void init()
        {
            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            camera = new FpsCamera();
            camera.MovementSpeed = 100;
            camera.JumpSpeed = 50;
            camera.setCamera(new Vector3(-150f, 40f, 175f), new Vector3(120f, 60f, 50f));
            GuiController.Instance.CurrentCamera = camera;

            ///////////////MODIFIERS//////////////////
            GuiController.Instance.Modifiers.addFloat("Gravedad", -0.2f, 0.2f, 0.02f);
            GuiController.Instance.Modifiers.addFloat("Velocidad", 50f, 500f, 200f);
            GuiController.Instance.Modifiers.addFloat("Masa", 1f, 50f, 5f);
            //GuiController.Instance.Modifiers.addFloat("Cantidad Maxima Proyectiles", 2, 30, 10);
            ShootTechnique[] opciones = { new ShootTechnique(), new ShrapnelShoot()};
            ProjectileWeapon[] armas = { WeaponFactory.getCannon(), WeaponFactory.getTanque(), WeaponFactory.getGun() };
            GuiController.Instance.Modifiers.addInterval("Tecnicas de Disparo", opciones, 0);
            GuiController.Instance.Modifiers.addInterval("Armas", armas, 0);
            GuiController.Instance.Modifiers.addBoolean("boundingSphere", "Mostrar Bounding Sphere", false);
            GuiController.Instance.Modifiers.addBoolean("boundingBox", "Mostrar Bounding Box", false);

            // modifiers shader
            //Modifiers de la luz
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            GuiController.Instance.Modifiers.addVertex3f("lightPos", new Vector3(-200, -100, -200), new Vector3(200, 200, 300), new Vector3(60, 35, 250));
            GuiController.Instance.Modifiers.addColor("lightColor", Color.White);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 150, 20);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            GuiController.Instance.Modifiers.addFloat("specularEx", 0, 20, 9f);

            //Crear SkyBox
            createSkyBox(alumnoMediaFolder);

            //Creo texto para mostrar datos de camara
            textoCamara = new TgcText2d { Text = "Inicial", Color = Color.White };

            //suelo
            TgcTexture.createTexture(d3dDevice, alumnoMediaFolder + "Random\\Textures\\Terrain\\tileable_grass.jpg");
            solidWallsList.Add(new ParedSolida(new Vector3(-2500, 0, -2500), new Vector3(5000, 0, 5000), "XZ", alumnoMediaFolder + "Random\\Textures\\Terrain\\tileable_grass.jpg"));
            //Paredes deformables
            deformableWallsList.Add(new ParedDeformable(new Vector3(0, 0, 0), new Vector3(1, 0, 0), 100, alumnoMediaFolder + "Random\\Textures\\Walls\\concrete.jpg"));
            deformableWallsList.Add(new ParedDeformable(new Vector3(100, 0, 0), new Vector3(1, 0, 2), 100, alumnoMediaFolder + "Random\\Textures\\Walls\\concrete.jpg"));
            deformableWallsList.Add(new ParedDeformable(new Vector3(145, 0, 90), new Vector3(-30, 0, 30), 100, alumnoMediaFolder + "Random\\Textures\\Walls\\concrete.jpg"));
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
            weapon = (ProjectileWeapon)GuiController.Instance.Modifiers["Armas"];
            weapon.technique = (ShootTechnique)GuiController.Instance.Modifiers["Tecnicas de Disparo"];
            //cantMaximaProyectiles = (int)(float)GuiController.Instance.Modifiers["Cantidad Maxima Proyectiles"];

            ///////////////INPUT//////////////////
            //Capturar Input teclado 

            //Capturar Input Mouse
            if (GuiController.Instance.D3dInput.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                projectilesList.AddRange(weapon.doAction());
                /*if (projectilesList.Count > cantMaximaProyectiles)
                {
                    for (int i = 0; i < projectilesList.Count - cantMaximaProyectiles; i++)
                    {
                        projectilesList.RemoveAt(0);
                    }
                }*/

            }
            
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
            if (camera.isLocked)
            {
                camera.lockUnlock();
            }
        }


        private void deteccionDeColisiones(float elapsedTime)
        {
            for (int i = 0; i <= (projectilesList.Count - 1); i++)
            {
                Projectile proyectil = projectilesList[i];
                Vector3 posActual = proyectil.getPosition();
                Vector3 posAnterior = proyectil.posicionCuadroAnt;
                
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
                    // Como se q va a colisionar, me fijo q no haya salto de cuadros
                    // Hago un segmento entre la posicion actual y la del cuadro anterior, y me fijo si hubo colision con la pared
                    //if (!(proyectil.posicionCuadroAnt.Equals(proyectil.getPosition())))
                    if (!(posAnterior.Equals(posActual)))
                    {
                        //GuiController.Instance.Logger.log("Entro al distinto");
                        Vector3 ptoColision;
                        if (RandomCollisionUtils.intersectSegmentOBB(posAnterior, posActual, pared.obb, out ptoColision))
                        {
                            GuiController.Instance.Logger.log("Hubo colision " + posAnterior.ToString() + " - " + posActual.ToString());
                    
                            // Muevo la pelota hasta el pto real de colision
                            //proyectil.setPosition(ptoColision - (proyectil.boundingBall.Radius*proyectil.direction*-1));
                            proyectil.collisionWithDeformableWall(pared);
                        }
                    }
                    proyectil.posicionCuadroAnt = posActual;
                }

                //Dibujado y borrado en caso de que se acaba su lifeTime
                if (proyectil.update(elapsedTime)) projectilesList.Remove(proyectil);
                else proyectil.render();
            }
        }



        private void createSkyBox(string alumnoMediaFolder)
        {
            //Crear SkyBox 
            skyBox = new TgcSkyBox
            {
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
