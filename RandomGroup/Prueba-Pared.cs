using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;


namespace AlumnoEjemplos.Random.Prueba1
{
    /// <summary>

    /// </summary>
    public class Pared : TgcExample
    {

        Microsoft.DirectX.Direct3D.Effect effect;
        IndexBuffer indexBuffer;
        VertexBuffer vertexBuffer;
        int vertexCount;
        int indexCount;
        int triangleCount;
        VertexDeclaration vertexDeclaration;
        Texture texture0;
        Texture texture1;
        Texture texture2;
        MyCustomVertex[] vertexData;
        float acumTime;
        float dir;
        //TgcPickingRay pickingRay;
        //bool selected;
        //Vector3 collisionPoint;



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
                this.Position = pos;
                this.Normal = normal;
                this.Color = color;

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
        VertexFormats MyCustomVertexFormat =
            VertexFormats.Position | //Position
            VertexFormats.Normal | //Normal
            VertexFormats.Diffuse | //Color
            VertexFormats.Texture0 | //texcoord0
            VertexFormats.Texture1 | //texcoord1
            VertexFormats.Texture2 | //texcoord2
            VertexFormats.Texture3 | //auxValue1 (se mandan como coordenadas de textura float3)
            VertexFormats.Texture4  //auxValue2 (se mandan como coordenadas de textura float3)
            ;

        /// <summary>
        /// Vertex declaration para el vertice customizado. Hay que tener cuidado con la suma de bytes
        /// 1 float = 4 bytes
        /// Vector2 = 8 bytes
        /// Vector3 = 12 bytes
        /// Color = 4 bytes (es un int)
        /// </summary>
        public static readonly VertexElement[] MyCustomVertexElements = new VertexElement[]
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
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

           /* 
            *Tratando de localizar el punto en que hay una colision.. no salio porque usaba los bounding box de los mesh, creo 
            pickingRay = new TgcPickingRay();

            UserVars para mostrar en que punto hubo colision
            GuiController.Instance.UserVars.addVar("CollP-X:");
            GuiController.Instance.UserVars.addVar("CollP-Y:");
            GuiController.Instance.UserVars.addVar("CollP-Z:"); 
            
            */

            //Dimensiones de la pared
            Vector3 center = new Vector3(0, 0, 0);
            Vector3 size = new Vector3(15, 15, 15);
            int color1 = Color.Blue.ToArgb();
            int color2 = Color.Green.ToArgb();
            int color3 = Color.Red.ToArgb();
            int color4 = Color.Yellow.ToArgb();
            int color5 = Color.Purple.ToArgb();
            int color6 = Color.Black.ToArgb();
            int color7 = Color.White.ToArgb();
            int color8 = Color.Orange.ToArgb();
            int color9 = Color.Brown.ToArgb();
            Vector3 extents = size * 1.5f;
            Vector3 min = center - extents;
            Vector3 med = center;
            Vector3 max = center + extents;


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
            vertexData[3] = new MyCustomVertex(new Vector3(min.X, med.Y, med.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color4);
            vertexData[4] = new MyCustomVertex(new Vector3(med.X, med.Y, med.Z), Vector3.Normalize(new Vector3(5, 1, 1)), color5);
            vertexData[5] = new MyCustomVertex(new Vector3(max.X, med.Y, med.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color6);
            //superior
            vertexData[6] = new MyCustomVertex(new Vector3(min.X, max.Y, max.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color7);
            vertexData[7] = new MyCustomVertex(new Vector3(med.X, max.Y, max.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color8);
            vertexData[8] = new MyCustomVertex(new Vector3(max.X, max.Y, max.Z), Vector3.Normalize(new Vector3(1, 1, 1)), color9);

            //Setear información en VertexBuffer
            vertexBuffer.SetData(vertexData, 0, LockFlags.None);


            //Crear IndexBuffer con 24 vertices para los 8 triangulos que forman la pared
            indexCount = 24;
            triangleCount = indexCount / 3;
            short[] indexData = new short[indexCount];
            int iIdx = 0;
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
            indexData[iIdx++] = 5;
            indexData[iIdx++] = 1;
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
            indexData[iIdx++] = 5;

            


            //Setear información en IndexBuffer
            indexBuffer.SetData(indexData, 0, LockFlags.None);


            //Cargar shader customizado para este ejemplo
            string shaderPath = GuiController.Instance.ExamplesMediaDir + "Shaders\\EjemploBoxDirectX.fx";
            string compilationErrors;
            this.effect = Microsoft.DirectX.Direct3D.Effect.FromFile(d3dDevice, shaderPath, null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader: " + shaderPath + ". Errores: " + compilationErrors);
            }

            //Setear el único technique que tiene
            this.effect.Technique = "EjemploBoxDirectX";

            

            //Cargamos 3 texturas cualquiera para mandar al shader
            texture0 = TextureLoader.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\BumpMapping_DiffuseMap.jpg");
            texture1 = TextureLoader.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\BumpMapping_NormalMap.jpg");
            texture2 = TextureLoader.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\efecto_alarma.png");


            dir = 1;
            

            GuiController.Instance.RotCamera.Enable = true;
            GuiController.Instance.RotCamera.CameraDistance = 100;
        }

        
        public override void render(float elapsedTime)
            
        {

            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;


            acumTime += elapsedTime;
            float speed = 5 * elapsedTime;
            if (acumTime > 0.5f)
            {
                acumTime = 0;
                dir *= -1;
            }


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
                    MyCustomVertex v = vertexData[i];
                    vertexData[4].Position.Y -= 0.01f;
                }
                vertexBuffer.SetData(vertexData, 0, LockFlags.None); //Manda la informacion actualizada a la GPU
            }

            /* Queria tratar de, clickeando, que localice el punto para que hunda esa parte.. por ahora no puedo 
             * 
            Si hacen clic con el mouse, ver si hay colision RayAABB
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Actualizar Ray de colisión en base a posición del mouse
                pickingRay.updateRay();

                selected = true;

                //Renderizar
                if (selected)
                {
                    //Cargar punto de colision
                    GuiController.Instance.UserVars.setValue("CollP-X:", GuiController.Instance.D3dInput.Xpos);
                    GuiController.Instance.UserVars.setValue("CollP-Y:", GuiController.Instance.D3dInput.Ypos);
                    GuiController.Instance.UserVars.setValue("CollP-Z:", pickingRay.ray.rayOrigin);                    
                }
                else
                {
                    //Reset de valores
                    GuiController.Instance.UserVars.setValue("CollP-X:", 0);
                    GuiController.Instance.UserVars.setValue("CollP-Y:", 0);
                    GuiController.Instance.UserVars.setValue("CollP-Z:", 0);
                }
            }

            */



            //Cargar vertex declaration
            d3dDevice.VertexDeclaration = vertexDeclaration;

            //Cargar vertexBuffer e indexBuffer
            d3dDevice.SetStreamSource(0, vertexBuffer, 0);
            d3dDevice.Indices = indexBuffer;

            
            //Arrancar shader
            effect.Begin(0);
            effect.BeginPass(0);

            //Cargar matrices en shader
            Matrix matWorldView = d3dDevice.Transform.View;
            Matrix matWorldViewProj = matWorldView * d3dDevice.Transform.Projection;
            effect.SetValue("matWorld", Matrix.Identity);
            effect.SetValue("matWorldView", matWorldView);
            effect.SetValue("matWorldViewProj", matWorldViewProj);
            effect.SetValue("matInverseTransposeWorld", Matrix.Identity);

            //Cargar las 3 texturas en el shader
            effect.SetValue("tex0", texture0);
            effect.SetValue("tex1", texture1);
            effect.SetValue("tex2", texture2);

            //Dibujar los triangulos haciendo uso del indexBuffer
            d3dDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexCount, 0, triangleCount);

            //Finalizar shader
            effect.EndPass();
            effect.End();
             
        }

        public override void close()
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            effect.Dispose();
            vertexDeclaration.Dispose();
            texture0.Dispose();
            texture1.Dispose();
            texture2.Dispose();
        }

    }
}
