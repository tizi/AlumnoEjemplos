using System.Collections.Generic;
using AlumnoEjemplos.RandomGroup.src.meshUtils;
using AlumnoEjemplos.RandomGroup.src.shootTechniques;
using AlumnoEjemplos.RandomGroup.src.walls;
using AlumnoEjemplos.RandomGroup.src.weapons;
using TgcViewer.Example;
using TgcViewer;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Sound;
using System;

namespace AlumnoEjemplos.RandomGroup.src
{
    public class EjemploAlumno : TgcExample
    {
        FpsCamera camera;
        TgcSkyBox skyBox;
        TgcText2d textoCamara;
        List<Projectile> projectilesList = new List<Projectile>();
        List<ParedSolida> solidWallsList = new List<ParedSolida>();
        List<ParedSolida> ground = new List<ParedSolida>();
        List<ParedDeformable> deformableWallsList = new List<ParedDeformable>();
        List<MeshPropio> decoration = new List<MeshPropio>();
        ProjectileWeapon weapon;
        GrillaRegular grilla;
        TgcStaticSound paredSolidaSound = new TgcStaticSound();
        TgcStaticSound paredDeformableSound = new TgcStaticSound();
        TgcStaticSound proyectilSound = new TgcStaticSound();

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
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            loadSounds(alumnoMediaFolder);

            configureCamera();

            createModifiers();

            //Creo texto para mostrar datos de camara
            textoCamara = new TgcText2d { Text = "Inicial", Color = Color.White };

            createScene(alumnoMediaFolder);

            createGrid();
        }

        public override void render(float elapsedTime)
        {
            textoCamara.Text = GuiController.Instance.CurrentCamera.getPosition().ToString();
            textoCamara.render();

            ///////////////INPUT//////////////////
            if (GuiController.Instance.D3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                projectilesList.AddRange(weapon.doAction());
            }

            deteccionDeColisiones(elapsedTime);

            weapon = (ProjectileWeapon)GuiController.Instance.Modifiers["Armas"];
            weapon.technique = (ShootTechnique)GuiController.Instance.Modifiers["Tecnicas de Disparo"];
            weapon.render();

            bool showGrid = (bool)GuiController.Instance.Modifiers["showGrid"];
            grilla.render(GuiController.Instance.Frustum, (bool)GuiController.Instance.Modifiers["showGrid"], elapsedTime);

            skyBox.render();
        }
      
        public override void close()
        {
            skyBox.dispose();

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
                        proyectilSound.play();
                    }
                }

                //Deteccion contra las paredes no deformables
                for (int pared = 0; pared < solidWallsList.Count; pared++)
                {
                    if (TgcCollisionUtils.testSphereAABB(proyectil.boundingBall, solidWallsList[pared].getBoundingBox()))
                            {
                                proyectil.collisionWithSolidWall(solidWallsList[pared]);
                                paredSolidaSound.play();
                            }
                    
                }

                //Deteccion contra las paredes SI deformables
                for (int pared = 0; pared < deformableWallsList.Count; pared++)
                {
                    // Como se q va a colisionar, me fijo q no haya salto de cuadros
                    // Hago un segmento entre la posicion actual y la del cuadro anterior, y me fijo si hubo colision con la pared
                    //if (!(proyectil.posicionCuadroAnt.Equals(proyectil.getPosition())))
                    if (!(posAnterior.Equals(posActual)))
                    {
                        //GuiController.Instance.Logger.log("Entro al distinto");
                        Vector3 ptoColision;
                        if (RandomCollisionUtils.intersectSegmentOBB(posAnterior, posActual, deformableWallsList[pared].obb, out ptoColision))
                        {
                            GuiController.Instance.Logger.log("Hubo colision " + posAnterior + " - " + posActual);

                            // Muevo la pelota hasta el pto real de colision
                            //proyectil.setPosition(ptoColision - (proyectil.boundingBall.Radius*proyectil.direction*-1));
                            proyectil.collisionWithDeformableWall(deformableWallsList[pared], ptoColision);
                            paredDeformableSound.play();
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

        private void createGrid()
        {
            grilla = new GrillaRegular();
            List<ElementoEstatico> estaticos = new List<ElementoEstatico>();
            estaticos.AddRange(solidWallsList);
            estaticos.AddRange(deformableWallsList);
            estaticos.AddRange(decoration);
            List<TgcBoundingBox> allBoundingBoxes = new List<TgcBoundingBox>();
            for (int i = 0; i < estaticos.Count - 1; i++)
            {
                allBoundingBoxes.Add(estaticos[i].getBoundingBox());
            }
            TgcBoundingBox sceneBB = TgcBoundingBox.computeFromBoundingBoxes(allBoundingBoxes);
            grilla.create(estaticos, sceneBB);
            grilla.createDebugMeshes();
        }

        private void configureCamera()
        {
            camera = new FpsCamera { MovementSpeed = 100, JumpSpeed = 50 };
            camera.setCamera(new Vector3(-150f, 40f, 175f), new Vector3(120f, 60f, 50f));
            GuiController.Instance.CurrentCamera = camera;
        }

        private static void createModifiers()
        {
            GuiController.Instance.Modifiers.addFloat("Gravedad", -0.2f, 0.2f, 0.02f);
            GuiController.Instance.Modifiers.addFloat("Velocidad", 50f, 500f, 200f);
            GuiController.Instance.Modifiers.addFloat("Masa", 1f, 50f, 5f);
            ShootTechnique[] opciones = { new SimpleShoot(), new ShrapnelShoot(), new RiversEnemy() };
            ProjectileWeapon[] armas = { WeaponFactory.getCannon(), WeaponFactory.getTanque(), WeaponFactory.getGun() };
            GuiController.Instance.Modifiers.addInterval("Tecnicas de Disparo", opciones, 0);
            GuiController.Instance.Modifiers.addInterval("Armas", armas, 0);
            GuiController.Instance.Modifiers.addBoolean("boundingSphere", "Mostrar Bounding Sphere", false);
            GuiController.Instance.Modifiers.addBoolean("boundingBox", "Mostrar Bounding Box", false);
            GuiController.Instance.Modifiers.addBoolean("showGrid", "Show Grid", false);
            //Modifiers de la luz
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            GuiController.Instance.Modifiers.addColor("lightColor", Color.White);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 150, 35);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            GuiController.Instance.Modifiers.addFloat("specularEx", 0, 20, 9f);
        }

        private void loadSounds(string alumnoMediaFolder)
        {
            paredSolidaSound.loadSound(alumnoMediaFolder + "Random\\Sounds\\MetalHitsSolid.wav");
            paredDeformableSound.loadSound(alumnoMediaFolder + "Random\\Sounds\\DeformableHit.wav");
            proyectilSound.loadSound(alumnoMediaFolder + "Random\\Sounds\\MetalHitsSolid.wav");
        }

        private void createScene(string alumnoMediaFolder)
        {
            createSkyBox(alumnoMediaFolder);
            createGround(alumnoMediaFolder);
            createBoxesHouse(alumnoMediaFolder);
            createHouse101(alumnoMediaFolder);
            createFirstTest(alumnoMediaFolder);
            createVegetation(alumnoMediaFolder);
            createSandbox(alumnoMediaFolder);
        }

        private void createGround(string alumnoMediaFolder)
        {
            TgcTexture texturaSuelo = TgcTexture.createTexture(alumnoMediaFolder + "Random\\Textures\\Terrain\\tileable_grass.jpg");
            int x;
            const int largo = 2500;
            const int ancho = 2500;
            const int dimension = 250;

            for (x = -largo / 2; x < largo / 2; x += dimension)
            {
                int z;
                for (z = -ancho / 2; z < ancho / 2; z += dimension)
                {
                    if (!((x == -500 && z == 0) || (x == 250 && z == 250)))
                    {
                        ground.Add(new ParedSolida(new Vector3(x, 0, z), new Vector3(dimension, 0, dimension), "XZ", texturaSuelo));
                    }
                }
            }
            solidWallsList.AddRange(ground);
        }

        private void createFirstTest(string alumnoMediaFolder)
        {
            deformableWallsList.Add(new ParedDeformable(new Vector3(300, 0, -30), new Vector3(1, 0, 0), new Vector3(0, 1, 0), 100, alumnoMediaFolder + "Random\\Textures\\Walls\\concrete.jpg"));
            deformableWallsList.Add(new ParedDeformable(new Vector3(400, 0, -30), new Vector3(1, 0, 2), new Vector3(0, 1, 0), 100, alumnoMediaFolder + "Random\\Textures\\Walls\\concrete.jpg"));
            deformableWallsList.Add(new ParedDeformable(new Vector3(445, 0, 60), new Vector3(-30, 0, 30), new Vector3(0, 1, 0), 100, alumnoMediaFolder + "Random\\Textures\\Walls\\concrete.jpg"));
        }

        private void createBoxesHouse(string alumnoMediaFolder)
        {
            TgcTexture texturaHabitacion = TgcTexture.createTexture(alumnoMediaFolder + "Random\\Textures\\Walls\\metal.jpg");
            TgcTexture texturaSuelo = TgcTexture.createTexture(alumnoMediaFolder + "Random\\Textures\\Walls\\baldosas.jpg");

            ParedSolida paredAtras = new ParedSolida(new Vector3(-500, 0, 0), new Vector3(0, 150, 250), "YZ", texturaHabitacion);
            solidWallsList.Add(paredAtras);
            ParedSolida paredIzquierda = new ParedSolida(new Vector3(-500, 0, 0), new Vector3(250, 150, 0), "XY", texturaHabitacion);
            solidWallsList.Add(paredIzquierda);
            ParedSolida paredDerecha = new ParedSolida(new Vector3(-500, 0, 250), new Vector3(250, 150, 0), "XY", texturaHabitacion);
            solidWallsList.Add(paredDerecha);
            ParedSolida paredDelanteIzquierda = new ParedSolida(new Vector3(-250, 0, 0), new Vector3(0, 150, 100), "YZ", texturaHabitacion);
            solidWallsList.Add(paredDelanteIzquierda);
            ParedSolida paredDelanteDerecha = new ParedSolida(new Vector3(-250, 0, 150), new Vector3(0, 150, 100), "YZ", texturaHabitacion);
            solidWallsList.Add(paredDelanteDerecha);
            ParedSolida paredDelanteArriba = new ParedSolida(new Vector3(-250, 100, 100), new Vector3(0, 50, 50), "YZ", texturaHabitacion);
            solidWallsList.Add(paredDelanteArriba);
            ParedSolida suelo = new ParedSolida(new Vector3(-500, 0, 0), new Vector3(250, 0, 250), "XZ", texturaSuelo);
            solidWallsList.Add(suelo);
            ParedSolida techo = new ParedSolida(new Vector3(-500, 150, 0), new Vector3(250, 0, 250), "XZ", texturaHabitacion);
            solidWallsList.Add(techo);

            ParedDeformable cartel = new ParedDeformable(new Vector3(-250, 50, 100), new Vector3(0, 0, 1), new Vector3(0, 1, 0), 50, alumnoMediaFolder + "Random\\Textures\\cartelBoxesHouse.png");
            deformableWallsList.Add(cartel);

            crearCajaDeformable(new Vector3(-275, 0.1f, 5), 20, alumnoMediaFolder + "Random\\Textures\\Walls\\madera.jpg");
            crearCajaDeformable(new Vector3(-420, 20, 100), 50, alumnoMediaFolder + "Random\\Textures\\Walls\\tileable_metal.jpg");
            crearCajaDeformable(new Vector3(-465, 115, 215), 30, alumnoMediaFolder + "Random\\Textures\\Walls\\gold.bmp");
            crearCajaDeformable(new Vector3(-435, 85, 10), 35, alumnoMediaFolder + "Random\\Textures\\Walls\\wood.jpg");
            crearCajaDeformable(new Vector3(-270, 70, 50), 40, alumnoMediaFolder + "Random\\Textures\\Walls\\red_bricks.jpg");
            crearCajaDeformable(new Vector3(-490, 0.1f, 180), 60, alumnoMediaFolder + "Random\\Textures\\Walls\\madera.jpg");
            crearCajaDeformable(new Vector3(-260, 0.1f, 175), 20, alumnoMediaFolder + "Random\\Textures\\Walls\\gold.bmp");
            //La tele
            crearCajaDeformable(new Vector3(-350, 30, 230), 15, alumnoMediaFolder + "Random\\Textures\\Characters\\tele_frente.jpg", alumnoMediaFolder + "Random\\Textures\\Characters\\tele_lateral.jpg", alumnoMediaFolder + "Random\\Textures\\Characters\\tele_lateral.jpg", alumnoMediaFolder + "Random\\Textures\\Characters\\tele_lateral.jpg", alumnoMediaFolder + "Random\\Textures\\Characters\\tele_lateral.jpg", alumnoMediaFolder + "Random\\Textures\\Characters\\tele_superior.jpg");
        }

        private void createVegetation(string alumnoMediaFolder)
        {
            MeshPropio[] vegetation = {MeshFactory.getMesh(alumnoMediaFolder + "Random\\Meshes\\Vegetation\\flores.xml"),
                                       MeshFactory.getMesh(alumnoMediaFolder + "Random\\Meshes\\Vegetation\\pasto.xml"), 
                                       MeshFactory.getMesh(alumnoMediaFolder + "Random\\Meshes\\Vegetation\\helecho.xml"), 
                                       MeshFactory.getMesh(alumnoMediaFolder + "Random\\Meshes\\Vegetation\\palmera1.xml"), 
                                       MeshFactory.getMesh(alumnoMediaFolder + "Random\\Meshes\\Vegetation\\planta1.xml"), 
                                       MeshFactory.getMesh(alumnoMediaFolder + "Random\\Meshes\\Vegetation\\planta2.xml"), 
                                       MeshFactory.getMesh(alumnoMediaFolder + "Random\\Meshes\\Vegetation\\planta3.xml")
            };
            const int largo = 2500;
            const int ancho = 2500;
            Random randomizer = new Random();
            for (int i = 0; i < 100; i++)
            { 
                int numero = (int)(randomizer.NextDouble() * 7);
                MeshPropio tmpDibujable = (MeshPropio)vegetation[numero].clone();
                tmpDibujable.scale(new Vector3(1, 1, 1));
                tmpDibujable.setRotationY((float)randomizer.NextDouble() * 360);
                tmpDibujable.setPosition(new Vector3(-largo / 2 + (float)randomizer.NextDouble() * 500, 0, -ancho / 2 + 500 + (float)randomizer.NextDouble() * 1500));
                tmpDibujable.boundingBox = (new TgcBoundingBox(tmpDibujable.position - new Vector3(1, 1, 1), tmpDibujable.position + new Vector3(1, 1, 1)));
                decoration.Add(tmpDibujable);
            }
            for (int i = 0; i < 150; i++)
            { 
                int numero = (int)(randomizer.NextDouble() * 7);
                MeshPropio tmpDibujable = (MeshPropio)vegetation[numero].clone();
                tmpDibujable.scale(new Vector3(1, 1, 1));
                tmpDibujable.setRotationY((float)randomizer.NextDouble() * 360);
                tmpDibujable.setPosition(new Vector3(-largo / 2 + (float)randomizer.NextDouble() * 2500, 0, -ancho / 2 + (float)randomizer.NextDouble() * 500));
                tmpDibujable.boundingBox = (new TgcBoundingBox(tmpDibujable.position - new Vector3(1, 1, 1), tmpDibujable.position + new Vector3(1, 1, 1)));
                decoration.Add(tmpDibujable);
            }
            for (int i = 0; i < 100; i++)
            { 
                int numero = (int)(randomizer.NextDouble() * 7);
                MeshPropio tmpDibujable = (MeshPropio)vegetation[numero].clone();
                tmpDibujable.scale(new Vector3(1, 1, 1));
                tmpDibujable.setRotationY((float)randomizer.NextDouble() * 360);
                tmpDibujable.setPosition(new Vector3(largo / 2 - (float)randomizer.NextDouble() * 500, 0, -ancho / 2 + 500 + (float)randomizer.NextDouble() * 1500));
                tmpDibujable.boundingBox = (new TgcBoundingBox(tmpDibujable.position - new Vector3(1, 1, 1), tmpDibujable.position + new Vector3(1, 1, 1)));
                decoration.Add(tmpDibujable);
            }
            for (int i = 0; i < 150; i++)
            { 
                int numero = (int)(randomizer.NextDouble() * 7);
                MeshPropio tmpDibujable = (MeshPropio)vegetation[numero].clone();
                tmpDibujable.scale(new Vector3(1, 1, 1));
                tmpDibujable.setRotationY((float)randomizer.NextDouble() * 360);
                tmpDibujable.setPosition(new Vector3(-largo / 2 + (float)randomizer.NextDouble() * 2500, 0, ancho / 2 - (float)randomizer.NextDouble() * 500));
                tmpDibujable.boundingBox = (new TgcBoundingBox(tmpDibujable.position - new Vector3(1, 1, 1), tmpDibujable.position + new Vector3(1, 1, 1)));
                decoration.Add(tmpDibujable);
            }
            for (int i = 0; i < 150; i++)
            {
                int numero = (int)(randomizer.NextDouble() * 2);
                MeshPropio tmpDibujable = (MeshPropio)vegetation[numero].clone();
                tmpDibujable.scale(new Vector3(1, 1, 1));
                tmpDibujable.setRotationY((float)randomizer.NextDouble() * 360);
                bool continuar = false;
                while (!continuar)
                {
                    tmpDibujable.setPosition(new Vector3(-largo / 2 + 500 + (float)randomizer.NextDouble() * 1500, 0, ancho / 2 - 500 - (float)randomizer.NextDouble() * 1500));
                    tmpDibujable.boundingBox = (new TgcBoundingBox(tmpDibujable.position - new Vector3(1, 1, 1), tmpDibujable.position + new Vector3(1, 1, 1)));
                    for (int pedazo = 0; pedazo < ground.Count; pedazo++)
                    {
                        if (TgcCollisionUtils.testAABBAABB(ground[pedazo].getBoundingBox(), tmpDibujable.getBoundingBox()))
                        {
                            continuar = true;
                        }
                    }
                }
                decoration.Add(tmpDibujable);
            }
        }

        private void createHouse101(string alumnoMediaFolder)
        {
            TgcTexture texturaHabitacion = TgcTexture.createTexture(alumnoMediaFolder + "Random\\Textures\\Walls\\red_bricks.jpg");
            TgcTexture texturaTablas = TgcTexture.createTexture(alumnoMediaFolder + "Random\\Textures\\Walls\\wood.jpg");

            ParedSolida paredAtras = new ParedSolida(new Vector3(300, 0, -400), new Vector3(0, 150, 150), "YZ", texturaHabitacion);
            solidWallsList.Add(paredAtras);
            ParedSolida paredIzquierda1 = new ParedSolida(new Vector3(100, 0, -400), new Vector3(200, 50, 0), "XY", texturaHabitacion);
            solidWallsList.Add(paredIzquierda1);
            ParedSolida paredDerecha1 = new ParedSolida(new Vector3(100, 0, -250), new Vector3(200, 50, 0), "XY", texturaHabitacion);
            solidWallsList.Add(paredDerecha1);

            ParedSolida tabla1 = new ParedSolida(new Vector3(250, 50, -400), new Vector3(50, 0, 50), "XZ", texturaTablas);
            solidWallsList.Add(tabla1);
            ParedSolida tabla2 = new ParedSolida(new Vector3(250, 50, -300), new Vector3(50, 0, 50), "XZ", texturaTablas);
            solidWallsList.Add(tabla2);
            ParedSolida tabla3 = new ParedSolida(new Vector3(250, 100, -375), new Vector3(50, 0, 100), "XZ", texturaTablas);
            solidWallsList.Add(tabla3);

            ParedSolida paredMedio = new ParedSolida(new Vector3(200, 0, -400), new Vector3(0, 50, 150), "YZ", texturaHabitacion);
            solidWallsList.Add(paredMedio);

            ParedSolida paredIzquierda2 = new ParedSolida(new Vector3(200, 50, -400), new Vector3(100, 50, 0), "XY", texturaHabitacion);
            solidWallsList.Add(paredIzquierda2);
            ParedSolida paredDerecha2 = new ParedSolida(new Vector3(200, 50, -250), new Vector3(100, 50, 0), "XY", texturaHabitacion);
            solidWallsList.Add(paredDerecha2);

            crearCajaDeformable(new Vector3(270, 51, -270), 10, alumnoMediaFolder + "Random\\Textures\\Walls\\metal.jpg");
            crearCajaDeformable(new Vector3(270, 51, -370), 10, alumnoMediaFolder + "Random\\Textures\\Walls\\metal.jpg");
            crearCajaDeformable(new Vector3(270, 51, -390), 10, alumnoMediaFolder + "Random\\Textures\\Walls\\metal.jpg");
        }

        private void createSandbox(string alumnoMediaFolder)
        {
            TgcTexture texturaMadera = TgcTexture.createTexture(alumnoMediaFolder + "Random\\Textures\\Walls\\madera.jpg");
            ParedSolida areneroBase = new ParedSolida(new Vector3(240, -5, 240), new Vector3(270, 0, 270), "XZ", texturaMadera);
            solidWallsList.Add(areneroBase);
            crearCajaSolida(new Vector3(240, -5, 240), 270, 10, 10, texturaMadera);
            crearCajaSolida(new Vector3(500, -5, 240), 270, 10, 10, texturaMadera);
            crearCajaSolida(new Vector3(250, -5, 240), 10, 250, 10, texturaMadera);
            crearCajaSolida(new Vector3(250, -5, 500), 10, 250, 10, texturaMadera);
 
            ParedDeformable suelo = new ParedDeformable(new Vector3(250, 0, 250), new Vector3(1, 0, 0), new Vector3(0, 0, 1), 250, alumnoMediaFolder + "Random\\Textures\\Terrain\\sand.jpg");
            deformableWallsList.Add(suelo);

            //Kirby
            crearCajaDeformable(new Vector3(380, 40, 280), 20, alumnoMediaFolder + "Random\\Textures\\Characters\\kirby_arm.jpg", alumnoMediaFolder + "Random\\Textures\\Characters\\kirby_arm.jpg", alumnoMediaFolder + "Random\\Textures\\Characters\\kirby_front.jpg", alumnoMediaFolder + "Random\\Textures\\Characters\\kirby_back.jpg", alumnoMediaFolder + "Random\\Textures\\Characters\\kirby_bottom.jpg", alumnoMediaFolder + "Random\\Textures\\Characters\\kirby_back.jpg");
            TgcTexture texturaSangre = TgcTexture.createTexture(alumnoMediaFolder + "Random\\Textures\\Characters\\blood.png");
            crearCajaSolida(new Vector3(384, 44, 284), 12, 12, 12, texturaSangre);
        }

        private void crearCajaSolida(Vector3 origen, int largo, int ancho, int alto,TgcTexture textura)
        {
            ParedSolida atras = new ParedSolida(origen, new Vector3(ancho, alto, 0), "XY", textura);
            ParedSolida delante = new ParedSolida(origen + new Vector3(0, 0, largo), new Vector3(ancho, alto, 0), "XY", textura);

            ParedSolida izquierda = new ParedSolida(origen, new Vector3(0, alto, largo), "YZ", textura);
            ParedSolida derecha = new ParedSolida(origen + new Vector3(ancho, 0, 0), new Vector3(0, alto, largo), "YZ", textura);

            ParedSolida abajo = new ParedSolida(origen, new Vector3(ancho, 0, largo), "XZ", textura);
            ParedSolida arriba = new ParedSolida(origen + new Vector3(0, alto, 0), new Vector3(ancho, 0, largo), "XZ", textura);

            solidWallsList.Add(atras);
            solidWallsList.Add(delante);
            solidWallsList.Add(izquierda);
            solidWallsList.Add(derecha);
            solidWallsList.Add(abajo);
            solidWallsList.Add(arriba);
        }

        private void crearCajaDeformable(Vector3 origen, int tamanio, string textura)
        {
            crearCajaDeformable(origen, tamanio, textura, textura, textura, textura, textura, textura);
        }
    
        private void crearCajaDeformable(Vector3 origen, int tamanio, string texturaBack, string texturaFront, string texturaLeft, string texturaRight, string texturaBottom, string texturaUp)
        {
            ParedDeformable atras = new ParedDeformable(origen, new Vector3(1, 0, 0), new Vector3(0, 1, 0), tamanio, texturaBack);
            ParedDeformable delante = new ParedDeformable((origen + new Vector3(0, 0, tamanio)), new Vector3(1, 0, 0), new Vector3(0, 1, 0), tamanio, texturaFront);

            ParedDeformable izquierda = new ParedDeformable(origen, new Vector3(0, 0, 1), new Vector3(0, 1, 0), tamanio, texturaLeft);
            ParedDeformable derecha = new ParedDeformable((origen + new Vector3(tamanio, 0, 0)), new Vector3(0, 0, 1), new Vector3(0, 1, 0), tamanio, texturaRight);

            ParedDeformable abajo = new ParedDeformable(origen, new Vector3(1, 0, 0), new Vector3(0, 0, 1), tamanio, texturaBottom);
            ParedDeformable arriba = new ParedDeformable((origen + new Vector3(0, tamanio, 0)), new Vector3(1, 0, 0), new Vector3(0, 0, 1), tamanio, texturaUp);

            deformableWallsList.Add(atras);
            deformableWallsList.Add(delante);
            deformableWallsList.Add(izquierda);
            deformableWallsList.Add(derecha);
            deformableWallsList.Add(abajo);
            deformableWallsList.Add(arriba);
        }
    }

}
