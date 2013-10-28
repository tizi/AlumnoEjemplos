using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcGeometry;
using AlumnoEjemplos.RandomGroup;

namespace AlumnoEjemplos.RandomGroup
{
    public class EjemploAlumno : TgcExample
    {
        TgcSkyBox skyBox;
        TgcBox suelo;
        TgcText2d textoCamara;
        ParedDeformable pared;
        List<Projectile> collisionableList = new List<Projectile>();
        ProjectileWeapon weapon;


        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "Random group";
        }
        
        public override string getDescription()
        {
            return "Deformaciones - Una descripcion";
        }


        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;



            /*
            ///////////////CONFIGURAR CAMARA ROTACIONAL//////////////////
            //Es la camara que viene por default, asi que no hace falta hacerlo siempre
            GuiController.Instance.RotCamera.Enable = true;
            //Configurar centro al que se mira y distancia desde la que se mira
            GuiController.Instance.RotCamera.setCamera(new Vector3(40, 650, 2200), 500);
            */

            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 400;
            GuiController.Instance.FpsCamera.JumpSpeed = 400;
            //Configurar posicion y hacia donde se mira
            GuiController.Instance.FpsCamera.setCamera(new Vector3(65f, 5f, -325f), new Vector3(379.7143f, 12.9713f, 336.3295f));

            /*
            ///////////////CONFIGURAR CAMARA TERCERA PERSONA//////////////////
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(new Vector3(61.8657f, 403.7024f, -527.558f), 20, -120);
            GuiController.Instance.ThirdPersonCamera.TargetDisplacement = new Vector3(0, 45, 0);
            */
             
            ///////////////MODIFIERS//////////////////

            GuiController.Instance.Modifiers.addFloat("gravity", -0.2f, 0.2f, 0.02f);
            GuiController.Instance.Modifiers.addFloat("speed", 50f, 500f, 200f);
            ShootTechnique[] opciones = new ShootTechnique[] { new ShootTechnique() };
            ProjectileWeapon[] armas = new ProjectileWeapon[] { WeaponFactory.getTanque(), WeaponFactory.getCannon(), WeaponFactory.getGun() };
            GuiController.Instance.Modifiers.addInterval("tecnicas", opciones, 0);
            GuiController.Instance.Modifiers.addInterval("armas", armas, 0);
            GuiController.Instance.Modifiers.addFloat("mass", 1, 50f, 1);
            GuiController.Instance.Modifiers.addBoolean("boundingSphere", "Mostrar Bounding Sphere", false);
            GuiController.Instance.Modifiers.addBoolean("boundingBox", "Mostrar Bounding Box", false);

            /*Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("valorFloat", -50f, 200f, 0f);

            //Crear un modifier para un ComboBox con opciones
            string[] opciones = new string[] { "opcion1", "opcion2", "opcion3" };
            GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un vértice
            GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50), new Vector3(0, 0, 0));
            */

            //Crear SkyBox
            createSkyBox(alumnoMediaFolder);

            //suelo
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice, alumnoMediaFolder + "Random\\Textures\\Terrain\\tileable_grass.jpg");
            suelo = TgcBox.fromSize(new Vector3(500, 0, 500), new Vector3(7000, 0, 7000), pisoTexture);



            //Creo texto para mostrar datos de camara
            textoCamara = new TgcText2d();
            textoCamara.Text = "Inicial";
            textoCamara.Color = Color.White;

            ///////////////USER VARS//////////////////

            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("variablePrueba");

            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("variablePrueba", 5451);


            pared = new ParedDeformable(new Vector3(0, 0, 0), 60, 60, "XY", 0.5F, alumnoMediaFolder + "Random\\Textures\\Walls\\concrete.jpg");
            //weapon = WeaponFactory.getGun();
            weapon = WeaponFactory.getTanque();
        }



        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// elapsedTime: Tiempo en segundos transcurridos desde el último frame
        public override void render(float elapsedTime)
        {
            //Device de DirectX para renderizar
            Device d3dDevice = GuiController.Instance.D3dDevice;

            textoCamara.Text = GuiController.Instance.CurrentCamera.getPosition().ToString();
            textoCamara.render();
            skyBox.render();
            //scene.renderAll();
            suelo.render();
            pared.render(elapsedTime);
            

            //Obtener valor de UserVar (hay que castear)
            int valor = (int)GuiController.Instance.UserVars.getValue("variablePrueba");


            //Obtener valores de Modifiers
            weapon = (ProjectileWeapon)GuiController.Instance.Modifiers["armas"];
            /*
            float valorFloat = (float)GuiController.Instance.Modifiers["valorFloat"];
            string opcionElegida = (string)GuiController.Instance.Modifiers["valorIntervalo"];
            Vector3 valorVertice = (Vector3)GuiController.Instance.Modifiers["valorVertice"];
            */

            ///////////////INPUT//////////////////
            //conviene deshabilitar ambas camaras para que no haya interferencia

            //Capturar Input teclado 
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F))
            {
                foreach (ParedDeformable.BBOpt BB in pared.LBBoxOpt)
                    pared.deformarPared(BB);
            }

            //Capturar Input Mouse
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_RIGHT))
            {
                collisionableList.AddRange(weapon.doAction());

            }
            for (int i = 0; i <= (collisionableList.Count - 1); i++)
            {
                Projectile collisionable = collisionableList[i];
                if (collisionable.update(elapsedTime)) collisionableList.Remove(collisionable); else collisionable.render();
            }
            weapon.update();
            weapon.render();

        }

       
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.        
        public override void close()
        {
            skyBox.dispose();
            //scene.disposeAll();
            suelo.dispose();
        }
       
 






        private void createSkyBox(string alumnoMediaFolder)
        {
            //Crear SkyBox 
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);

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
