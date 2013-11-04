using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.RandomGroup
{
    public class ParedDeformable
    {
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

        public static readonly VertexElement[] PositionNormalTextured_VertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                                    DeclarationMethod.Default,
                                    DeclarationUsage.Position, 0),
            
            new VertexElement(0, 12, DeclarationType.Float3,
                                     DeclarationMethod.Default,
                                     DeclarationUsage.Color, 0),

            new VertexElement(0, 24, DeclarationType.Float2,
                                     DeclarationMethod.Default,
                                     DeclarationUsage.TextureCoordinate, 0),
            VertexElement.VertexDeclarationEnd 
        };

        
        //CONSTRUCTOR
        public ParedDeformable(Vector3 origen, Vector3 direccionHorizontal, int cantCuadradosLado, string texturePath)
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
                SpecularSharpness = 15.0F
            };
            device.Material = material;

            //Estas paredes crecen en Y, se les tiene que dar una dirección horizontal con Y = 0
            direccionHorizontal.Normalize();
            Vector3 direccionVertical = new Vector3(0, 1, 0);
            
            //Normal
            normal = Vector3.Cross(direccionVertical, direccionHorizontal);                        

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
            Vector3 posUltimoVertice = verticesPared[numVertices - 1].Position;
            Vector3 posEsquina1 = verticesPared[verticesLado].Position;
            Vector3 posEsquina2 = verticesPared[verticesLado * (verticesLado - 1)].Position;
            obb = TgcObb.computeFromPoints(new[] { origen, posUltimoVertice, posEsquina1, posEsquina2 });
        }

        public void render(float elapsedTime)
        {
            device.VertexDeclaration = vertexDeclaration;
            device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            device.Indices = indexBuffer;
            device.SetStreamSource(0, vertexBuffer, 0);
            device.SetTexture(0, texture);
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, triangleCount);

            //Renderizar OBB
            if ((bool)GuiController.Instance.Modifiers["boundingBox"]) obb.render();
        }



        public void deformarPared(Projectile proyectil)
        {
            float radio = proyectil.boundingBall.Radius;
            Vector3 posicionImpacto = proyectil.getPosition();
            Vector3 direccion = proyectil.direction;
            direccion.Normalize();

            float DefoMod = proyectil.getSpeed() * proyectil.mass / 10;

            //MAGIA DE DEFORMACION            
            for (int i = 0; i < numVertices; i++)
            {                
                float distanciaCentroVertex = Vector3.Length(verticesPared[i].Position - posicionImpacto);

                //Controlar el radio de la deformacion
                if (distanciaCentroVertex > (radio) + DefoMod) continue;

                //Cantidad de deformación
                float deformacion = (1 / distanciaCentroVertex) * DefoMod;

                //HACK PARA QUE NO SE HAGAN PINCHES
                if (deformacion > 10) deformacion = 10;
                //FIN HACK

                Vector3 vectorDeformacion = direccion * deformacion;

                //Se desplaza cada vertice
                verticesPared[i].Position += vectorDeformacion;
            }

            vertexBuffer.SetData(verticesPared, 0, LockFlags.None);
            //FIN MAGIA DEFORMACION
        }

        public void dispose()
        {
        }



    }
}
