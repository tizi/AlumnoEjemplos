using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Example;
using TgcViewer.Utils.Input;
using Device = Microsoft.DirectX.Direct3D.Device;

namespace AlumnoEjemplos.RandomGroup
{
    /// <summary>

    /// </summary>
    public class Pared : TgcExample
    {
        IndexBuffer indexBuffer;
        VertexBuffer vertexBuffer;
        int vertexCount;
        int indexCount;
        int triangleCount;
        VertexDeclaration vertexDeclaration;

        MyCustomVertex[] vertexData;

        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "Prueba-Pared";
        }

        public override string getDescription()
        {
            return "Pared";
        }



        /// <summary>
        /// Estructura de vertice personalizada
        /// </summary>
        struct MyCustomVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public int Color;

            //Varias coordenadas de textura (se puede guardar cualquier cosa ahi adentro)
            public Vector2 texcoord0;
            public Vector2 texcoord1;
            public Vector2 texcoord2;

            //Varios Vector3 auxiliares para guardar cualquier cosa
            public Vector3 auxValue1;
            public Vector3 auxValue2;


            public MyCustomVertex(Vector3 pos, Vector3 normal, int color)
            {
                Position = pos;
                Normal = normal;
                Color = color;

                //Los demas valores los llenamos con cualquier cosa porque para este ejemplo no se usan para nada
                texcoord0 = new Vector2(0, 1);
                texcoord1 = new Vector2(1, 1);
                texcoord2 = new Vector2(0.5f, 0.75f);
                auxValue1 = new Vector3(10, 10, 10);
                auxValue2 = new Vector3(0, 0, 5);
            }
        }

        /// <summary>
        /// Formato de vertice customizado
        /// </summary>
        private const VertexFormats MyCustomVertexFormat =
            VertexFormats.Position | //Position
            VertexFormats.Normal | //Normal
            VertexFormats.Diffuse | //Color
            VertexFormats.Texture0 | //texcoord0
            VertexFormats.Texture1 | //texcoord1
            VertexFormats.Texture2 | //texcoord2
            VertexFormats.Texture3 | //auxValue1 (se mandan como coordenadas de textura float3)
            VertexFormats.Texture4 //auxValue2 (se mandan como coordenadas de textura float3)
            ;

        /// <summary>
        /// Vertex declaration para el vertice customizado. Hay que tener cuidado con la suma de bytes
        /// 1 float = 4 bytes
        /// Vector2 = 8 bytes
        /// Vector3 = 12 bytes
        /// Color = 4 bytes (es un int)
        /// </summary>
        public static readonly VertexElement[] MyCustomVertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Position, 0), //Position
                                    
            new VertexElement(0, 12, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Normal, 0), //Normal

            new VertexElement(0, 24, DeclarationType.Color,
                DeclarationMethod.Default,
                DeclarationUsage.Color, 0), //Color

            new VertexElement(0, 28, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 0), //texcoord0

            new VertexElement(0, 36, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 1), //texcoord1

            new VertexElement(0, 44, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 2), //texcoord2

            new VertexElement(0, 56, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 3), //auxValue1 (se mandan como coordenadas de textura float3)

            new VertexElement(0, 68, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 4), //auxValue2 (se mandan como coordenadas de textura float3)

            VertexElement.VertexDeclarationEnd 
        };

        

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Dimensiones de la pared
            Vector3 center = new Vector3(0, 0, 0);
            Vector3 size = new Vector3(15, 15, 15);
            
            Vector3 extents = size * 1.5f;
            Vector3 min = center - extents;
            Vector3 med = center;
            Vector3 max = center + extents;

            int color1 = Color.Blue.ToArgb();
            int color2 = Color.Green.ToArgb();
            int color3 = Color.Red.ToArgb();
            int color4 = Color.Yellow.ToArgb();
            int color5 = Color.Purple.ToArgb();
            int color6 = Color.Black.ToArgb();
            int color7 = Color.White.ToArgb();
            int color8 = Color.Orange.ToArgb();
            int color9 = Color.Brown.ToArgb();

            //Crear vertex declaration
            vertexDeclaration = new VertexDeclaration(d3dDevice, MyCustomVertexElements);

            //Crear un VertexBuffer con 9 vertices
            vertexCount = 9;
            vertexData = new MyCustomVertex[vertexCount];
            vertexBuffer = new VertexBuffer(typeof(MyCustomVertex), vertexCount, d3dDevice, Usage.Dynamic | Usage.WriteOnly, MyCustomVertexFormat, Pool.Default);

            //Llenar array con los 9 vertices de la pared
            //La normal se carga combinando la dirección de las 3 caras que toca.. esto no lo entendi todavia
            //inferiores
            vertexData[0] = new MyCustomVertex(new Vector3(min.X, min.Y, min.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color1);
            vertexData[1] = new MyCustomVertex(new Vector3(med.X, min.Y, min.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color2);
            vertexData[2] = new MyCustomVertex(new Vector3(max.X, min.Y, min.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color3);
            //medio
            vertexData[3] = new MyCustomVertex(new Vector3(min.X, med.Y, min.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color4);
            vertexData[4] = new MyCustomVertex(new Vector3(med.X, med.Y, min.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color5);
            vertexData[5] = new MyCustomVertex(new Vector3(max.X, med.Y, min.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color6);
            //superior
            vertexData[6] = new MyCustomVertex(new Vector3(min.X, max.Y, min.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color7);
            vertexData[7] = new MyCustomVertex(new Vector3(med.X, max.Y, min.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color8);
            vertexData[8] = new MyCustomVertex(new Vector3(max.X, max.Y, min.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color9);

            //Setear información en VertexBuffer
            vertexBuffer.SetData(vertexData, 0, LockFlags.None);


            //Crear IndexBuffer con 24 vertices para los 8 triangulos que forman la pared
            indexCount = 24;
            triangleCount = indexCount / 3;
            var indexData = new short[indexCount];
            var iIdx = 0;
            indexBuffer = new IndexBuffer(typeof(short), indexCount, d3dDevice, Usage.None, Pool.Default);
            
            /* 8____7____6
             * |   /|\   |
             * |  / | \  |  
             * | /  |  \ |
             * 5____4____3
             * |\   |   /|
             * | \  |  / |  
             * |  \ | /  |
             * 2____1____0
             * 
             * Asi queda la pared con un poco de imaginacion
             * 
             */

            
            //Abajo - Izquierda
            indexData[iIdx++] = 2;
            indexData[iIdx++] = 1;
            indexData[iIdx++] = 5;
            indexData[iIdx++] = 1;
            indexData[iIdx++] = 5;
            indexData[iIdx++] = 4;

            //Abajo - Derecha
            indexData[iIdx++] = 1;
            indexData[iIdx++] = 0;
            indexData[iIdx++] = 3;
            indexData[iIdx++] = 3;
            indexData[iIdx++] = 4;
            indexData[iIdx++] = 1;

            //Arriba - Derecha
            indexData[iIdx++] = 4;
            indexData[iIdx++] = 3;
            indexData[iIdx++] = 7;
            indexData[iIdx++] = 3;
            indexData[iIdx++] = 6;
            indexData[iIdx++] = 7;

            //Arriba - Izquierda
            indexData[iIdx++] = 5;
            indexData[iIdx++] = 4;
            indexData[iIdx++] = 7;
            indexData[iIdx++] = 7;
            indexData[iIdx++] = 8;
            indexData[iIdx] = 5;
            
            //Setear información en IndexBuffer
            indexBuffer.SetData(indexData, 0, LockFlags.None);                         
            
            //Camara
            GuiController.Instance.RotCamera.Enable = true;
            GuiController.Instance.RotCamera.CameraDistance = 100;
        }


        public override void render(float elapsedTime)
        {

            Device d3dDevice = GuiController.Instance.D3dDevice;
           
            //Actualizamos el vertexBuffer. 
            //Aca se muestra para poder explicar como se hace
            //Actualizar el vertexBuffer es una operatoria lenta porque debe traer los datos de GPU a CPU y luego volverlos a mandar
            //En este ejemplo empujamos el centro de la pared
            //No se leen los datos que ya estan en el vertexBuffer sino que se usa una copia local (vertexData) y se pisan los del vertexBuffer

            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;

            //Por ahora lo unico que pasa es que el punto del medio se hunde (el punto 4)

            if (d3dInput.keyDown(Key.Space))
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    //MyCustomVertex v = vertexData[i];
                    vertexData[4].Position.Z -= 0.01f;
                }
                vertexBuffer.SetData(vertexData, 0, LockFlags.None); //Manda la informacion actualizada a la GPU
            }       

            //Cargar vertex declaration
            d3dDevice.VertexDeclaration = vertexDeclaration;

            //Cargar vertexBuffer e indexBuffer
            d3dDevice.SetStreamSource(0, vertexBuffer, 0);
            d3dDevice.Indices = indexBuffer;

            //Dibujar los triangulos haciendo uso del indexBuffer
            d3dDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexCount, 0, triangleCount);
            
        }

        public override void close()
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();            
            vertexDeclaration.Dispose();            
        }

    }
}
