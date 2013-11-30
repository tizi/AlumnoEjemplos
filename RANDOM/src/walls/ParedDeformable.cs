using System;
using System.Drawing;
using AlumnoEjemplos.RANDOM.src.shootTechniques;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using AlumnoEjemplos.RANDOM.src.meshUtils;
using TgcViewer.Utils.Shaders;

namespace AlumnoEjemplos.RANDOM.src.walls
{
    public class ParedDeformable : ElementoEstatico
    {
        public bool enabled;
        private Device device { get; set; }
        private Texture texture { get; set; }        
        public Vector3 origen { get; set; }
        public Vector3 normal { get; set; }                             
        public TgcObb obb { get; set; }
        private int numVertices { get; set; }
        private int triangleCount { get; set; }
        public int indexCount { get; set; }
        public IndexBuffer indexBuffer { get; set; }
        public VertexBuffer vertexBuffer { get; set; }
        public CustomVertex.PositionNormalTextured[] verticesPared { get; set; }
        public VertexDeclaration vertexDeclaration { get; set; }
        public TgcBoundingBox boundingBox;
        private Vector3 posUltimoVertice ;
        private Vector3 posEsquina1;
        private Vector3 posEsquina2;


        
        Effect currentShader = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "Random\\Shaders\\Nuestro_MeshPointLightShader.fx");
           /*GuiController.Instance.Shaders.TgcMeshPointLightShader */
        //string shadersPath = GuiController.Instance.AlumnoEjemplosMediaDir;
        //float time;


        public TgcBoundingBox getBoundingBox()
        {
            return boundingBox;
        }
        public bool getEnabled()
        {
            return enabled;
        }
        public void setEnabled(bool value)
        {
            enabled = value;
        }

        public static readonly VertexElement[] PositionNormalTextured_VertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                                    DeclarationMethod.Default,
                                    DeclarationUsage.Position, 0),
            
            new VertexElement(0, 12, DeclarationType.Float3,
                                     DeclarationMethod.Default,
                                     DeclarationUsage.Normal, 0),

            new VertexElement(0, 24, DeclarationType.Float2,
                                     DeclarationMethod.Default,
                                     DeclarationUsage.TextureCoordinate, 0),
            VertexElement.VertexDeclarationEnd 
        };

        
        //CONSTRUCTOR
        public ParedDeformable(Vector3 origen, Vector3 direccionHorizontal, Vector3 direccionVertical, int cantCuadradosLado, string texturePath)
        {
            device = GuiController.Instance.D3dDevice;
            vertexDeclaration = new VertexDeclaration(device, PositionNormalTextured_VertexElements);


            //Crear la textura
            texture = TextureLoader.FromFile(device, texturePath);

            //Material
            Material material = new Material
            {
                Diffuse = Color.White,
                Specular = Color.LightGray,
                SpecularSharpness = 5.0F
            };
            device.Material = material;

            //Estas paredes crecen en Y, se les tiene que dar una dirección horizontal con Y = 0
            direccionHorizontal.Normalize();
            direccionVertical.Normalize();
            
            //Normal
            normal = Vector3.Cross(direccionVertical, direccionHorizontal);
            //normal = new Vector3(0, 1, 0);

            //Contadores
            int verticesLado = cantCuadradosLado + 1;
            triangleCount = cantCuadradosLado * cantCuadradosLado * 2;
            numVertices = Convert.ToInt32(FastMath.Pow2(verticesLado));
            indexCount = triangleCount * 3;
            
            //Estructuras para dibujar los triangulos
            var indexData = new int[indexCount];
            indexBuffer = new IndexBuffer(typeof(int), indexCount, device, Usage.None, Pool.Default);
            vertexBuffer = new VertexBuffer(
                typeof(CustomVertex.PositionNormalTextured), 
                numVertices, 
                device, 
                Usage.Dynamic, CustomVertex.PositionNormalTextured.Format, 
                Pool.Default
                );
            verticesPared = new CustomVertex.PositionNormalTextured[numVertices];

            for (int i = 0; i < verticesLado; i++)
            {
                for (int j = 0; j < verticesLado; j++)
                {
                    //Posicion
                    Vector3 posicion = origen;
                    posicion.Add(direccionHorizontal * j);
                    posicion.Add(direccionVertical * i);

                    //Coordendas de textura
                    Vector2 textura = new Vector2((float)i / verticesLado, (float)j / verticesLado);

                    //Se genera el vertice
                    verticesPared[j + i * verticesLado] = new CustomVertex.PositionNormalTextured(
                        posicion,
                        normal,
                        textura.X,
                        textura.Y
                    );
                }
            }

            int idx = 0;
            for (int i = 0; i < cantCuadradosLado; i++)
            {
                for (int j = 0; j < cantCuadradosLado; j++)
                {
                    /* 6____7____8
                     * |\   |\   |
                     * | \  | \  |  
                     * |  \ |  \ |
                     * 3____4____5
                     * |\   | \  |
                     * | \  |  \ |  
                     * |  \ |   \|
                     * 0____1____2
                     * Esta forma hacen los triangulos de 4 cuadrados despues de indexar
                     * Cada iteración hace 1 cuadrado
                     */                     
                    indexData[idx] = j + i * verticesLado;
                    indexData[idx + 1] = j + i * verticesLado + 1;
                    indexData[idx + 2] = j + verticesLado * (i + 1);

                    indexData[idx + 3] = j + i * verticesLado + 1;
                    indexData[idx + 4] = j + verticesLado * (i + 1);
                    indexData[idx + 5] = j + verticesLado * (i + 1) + 1;
                    idx += 6;
                }
            }

            indexBuffer.SetData(indexData, 0, LockFlags.None);
            vertexBuffer.SetData(verticesPared, 0, LockFlags.None);

            //Busco cuatro puntos para generar automaticamente el OBB
            posUltimoVertice = verticesPared[numVertices - 1].Position;
            posEsquina1 = verticesPared[verticesLado].Position;
            posEsquina2 = verticesPared[verticesLado * (verticesLado - 1)].Position;
            obb = TgcObb.computeFromPoints(new[] { origen, posUltimoVertice, posEsquina1, posEsquina2 });
            boundingBox = TgcBoundingBox.computeFromPoints(new[] { origen, posUltimoVertice, posEsquina1, posEsquina2 });

        }

        public void render(float elapsedTime)
        {
            device.VertexDeclaration = vertexDeclaration;
            device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            device.Indices = indexBuffer;
            device.SetStreamSource(0, vertexBuffer, 0);
            device.SetTexture(0, texture);

            // Comienza prueba shader
            //TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            //Habilitar luz
            bool lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];
            //time += elapsedTime;

            //Actualzar posición de la luz
            Vector3 lightPos = (Vector3)GuiController.Instance.CurrentCamera.getPosition();

            if (lightEnable)
            {
                //currentShader = TgcShaders.loadEffect(shadersPath + "Random\\Shaders\\Nuestro_MeshPointLightShader.fx");
                currentShader.SetValue("matWorld", Matrix.Identity);
                currentShader.SetValue("matWorldView", device.Transform.View);
                currentShader.SetValue("matWorldViewProj", device.Transform.View * device.Transform.Projection);
                currentShader.SetValue("matInverseTransposeWorld", Matrix.TransposeMatrix(Matrix.Invert(Matrix.Identity)));

                //Cargar variables shader de la luz
                currentShader.SetValue("lightColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["lightColor"]));
                currentShader.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                currentShader.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.FpsCamera.getPosition()));
                currentShader.SetValue("lightIntensity", (float)GuiController.Instance.Modifiers["lightIntensity"]);
                currentShader.SetValue("lightAttenuation", (float)GuiController.Instance.Modifiers["lightAttenuation"]);
                currentShader.Technique = "DIFFUSE_MAP";

                 //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                 currentShader.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                 currentShader.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
                 currentShader.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                 currentShader.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
                 currentShader.SetValue("materialSpecularExp", (float)GuiController.Instance.Modifiers["specularEx"]);

                int numPasses = currentShader.Begin(0);
                //texturesManager.clear(0);
                for (int n = 0; n < numPasses; n++)
                {
                    currentShader.SetValue("texDiffuseMap", texture);
                    currentShader.BeginPass(n);
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, triangleCount);
                    currentShader.EndPass();
                }
                currentShader.End();
            }
            else
            {
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, triangleCount);
            }

            //Renderizar OBB
            if ((bool)GuiController.Instance.Modifiers["boundingBox"]) obb.render();
        }



        public void deformarParedRedondamente(Projectile proyectil, Vector3 ptoColision)
        {            
            float radio = Math.Abs(proyectil.boundingBall.Radius);
            Vector3 direccion = proyectil.direction;

            if (radio < 1) radio = 1;
            direccion.Normalize();

            //500 es un numero magico que mas o menos acomoda las cosas
            float DefoMod = proyectil.getSpeed() * (proyectil.mass/radio) / 500;

            //MAGIA DE DEFORMACION            
           
            //Redundante, para probar variaciones del radio de deformacion
            float radioDeformacion = radio*2;
            
            for (int i = 0; i < numVertices; i++)
            {
                float distanciaCentroVertex = Vector3.Length(verticesPared[i].Position - ptoColision);

                //Controlar el radio de la deformacion
                if (distanciaCentroVertex > radioDeformacion) continue;

                //Cantidad de deformación, es la raiz cuadrada de una 
                //funcion cuadratica decreciente: x^2+r^2 con r constante
                // -distanciaAlCentro^2+radio^2
                float deformacion = DefoMod*FastMath.Sqrt(
                    -FastMath.Pow2(distanciaCentroVertex)+1.5f*FastMath.Pow2(radioDeformacion));

                Vector3 vectorDeformacion = direccion * deformacion;

                //Se desplaza
                verticesPared[i].Position += vectorDeformacion;
                
                //uso el vectorDefo para la normal
                //vectorDeformacion.Normalize();
                //verticesPared[i].Normal = vectorDeformacion;
                if (i < 3)
                {
                    Vector3 nuevaNormal = Vector3.Cross((verticesPared[0].Position - verticesPared[1].Position), (verticesPared[2].Position - verticesPared[1].Position));
                    nuevaNormal.Normalize();
                    verticesPared[i].Normal = nuevaNormal;
                }
                else
                {
                    Vector3 nuevaNormal = Vector3.Cross((verticesPared[i - 1].Position - verticesPared[i].Position), (verticesPared[i - 2].Position - verticesPared[i].Position));
                    nuevaNormal.Normalize();
                    verticesPared[i].Normal = nuevaNormal;
                }

            }

            vertexBuffer.SetData(verticesPared, 0, LockFlags.None);
            //FIN MAGIA DEFORMACION
            //Recalculo la obb
            //if (maximoDeformado != new Vector3(1, 1, 1) * float.MinValue &&
            //    minimoDeformado != new Vector3(1, 1, 1) * float.MaxValue)
            //    obb = TgcObb.computeFromPoints(new[] { origen, posUltimoVertice, posEsquina1, posEsquina2, maximoDeformado, minimoDeformado });
        }
        
        public void deformarPared(Projectile proyectil, Vector3 ptoColision)
        {
            float radio = proyectil.boundingBall.Radius;
            Vector3 direccion = proyectil.direction;
            //Vector3 maximoDeformado = posUltimoVertice; //cualquier vertice dentro de la pared
            //Vector3 minimoDeformado = posUltimoVertice; //cualquier vertice dentro de la pared
            direccion.Normalize();

            float DefoMod = proyectil.getSpeed() * proyectil.mass / 10;

            //MAGIA DE DEFORMACION            
            //HACK PARA QUE EL RADIO DE DEFORMACION NO SEA MUY GRANDE NI MUY CHICO
            float radioDeformacion;
            if (DefoMod + radio > radio * 10)
            {
                radioDeformacion = radio * 10;
            }
            else
            {
                radioDeformacion = DefoMod + radio;
            }
            if (radioDeformacion < radio * 2)
            {
                radioDeformacion = radio * 2;
            }
            //FIN DE HACK
            for (int i = 0; i < numVertices; i++)
            {
                float distanciaCentroVertex = Vector3.Length(verticesPared[i].Position - ptoColision);

                //Controlar el radio de la deformacion
                if (distanciaCentroVertex > radioDeformacion) continue;

                //Cantidad de deformación
                float deformacion = (1 / distanciaCentroVertex) * DefoMod;

                //HACK PARA QUE NO SE HAGAN PINCHES
                if (deformacion > 5) deformacion = 5;
                //FIN HACK

                Vector3 vectorDeformacion = direccion * deformacion;
                Vector3 posicionInial = verticesPared[i].Position;
                //Se desplaza cada vertice
                if (distanciaCentroVertex >= 1)
                {
                    Vector3 vectoraux = posicionInial;
                    vectoraux.X += (vectorDeformacion.X / distanciaCentroVertex);
                    vectoraux.Y += (vectorDeformacion.Y / distanciaCentroVertex);
                    vectoraux.Z += (vectorDeformacion.Z / distanciaCentroVertex);
                    verticesPared[i].Position = vectoraux;
                }
                else
                {
                    verticesPared[i].Position += vectorDeformacion;
                    /*if (verticesPared[i].Position.X > maximoDeformado.X)
                    {
                        maximoDeformado.X = verticesPared[i].Position.X;
                    }
                    if (verticesPared[i].Position.Y > maximoDeformado.Y)
                    {
                        maximoDeformado.Y = verticesPared[i].Position.Y;
                    }
                    if (verticesPared[i].Position.Z > maximoDeformado.Z)
                    {
                        maximoDeformado.Z = verticesPared[i].Position.Z;
                    }
                    if (verticesPared[i].Position.X < minimoDeformado.X)
                    {
                        minimoDeformado.X = verticesPared[i].Position.X;
                    }
                    if (verticesPared[i].Position.Y < minimoDeformado.Y)
                    {
                        minimoDeformado.Y = verticesPared[i].Position.Y;
                    }
                    if (verticesPared[i].Position.Z < minimoDeformado.Z)
                    {
                        minimoDeformado.Z = verticesPared[i].Position.Z;
                    }*/

                }
                //calculo de la nueva normal
                if (i < 3)
                {
                    Vector3 nuevaNormal = Vector3.Cross((verticesPared[0].Position - verticesPared[1].Position), (verticesPared[2].Position - verticesPared[1].Position));
                    nuevaNormal.Normalize();
                    verticesPared[i].Normal = nuevaNormal;
                }
                else
                {
                    Vector3 nuevaNormal = Vector3.Cross((verticesPared[i-1].Position - verticesPared[i].Position), (verticesPared[i-2].Position - verticesPared[i].Position));
                    nuevaNormal.Normalize();
                    verticesPared[i].Normal = nuevaNormal;
                }
                /*Vector3 vectorDesplazamiento = verticesPared[i].Position - posicionInial;
                vectorDesplazamiento.Normalize();
                Vector3 nuevaNormal = vectorDesplazamiento + verticesPared[i].Normal;
                nuevaNormal.Normalize();
                verticesPared[i].Normal = nuevaNormal;
                */
            }

            vertexBuffer.SetData(verticesPared, 0, LockFlags.None);
            //FIN MAGIA DEFORMACION
            //Recalculo la obb
            //obb = TgcObb.computeFromPoints(new[] { origen, posUltimoVertice, posEsquina1, posEsquina2, maximoDeformado, minimoDeformado });
        }

        public void dispose()
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            currentShader.Dispose();
            vertexDeclaration.Dispose();
            texture.Dispose();
            
        }



    }
}
