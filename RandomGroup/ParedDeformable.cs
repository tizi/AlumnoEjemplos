using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.RandomGroup
{
    class ChapaDeformable
    {
        private bool enabled;
        public bool getEnabled()
        {
            return enabled;
        }
        public void setEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        Effect effect;
        float time;
        VertexBuffer vertexBuffer;
        Texture texture;
        Material material;
        Light light;
        Device device = GuiController.Instance.D3dDevice;

        public struct BBOpt
        {
            public TgcBoundingBox BBoxOpt;
            public int inicio;
            public int fin;
        }

        public List<BBOpt> LBBoxOpt = new List<BBOpt>();


        Vector3 origin;
        public Vector3 Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        Vector3 size;
        public Vector3 Size
        {
            get { return size; }
            set { size = value; }
        }

        Vector3 normal;
        public Vector3 Normal
        {
            get { return normal; }
            set { normal = value; }
        }

        string orientation;
        public string Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        float alto;
        public float Alto
        {
            get { return alto; }
            set { alto = value; }
        }
        float ancho;
        public float Ancho
        {
            get { return ancho; }
            set { ancho = value; }
        }
        float teselation;
        public float Teselation
        {
            get { return teselation; }
            set { teselation = value; }
        }
        float energia;
        public float Energia
        {
            get { return energia; }
            set { energia = value; }
        }



        Boolean shaderEnabled;
        public Boolean ShaderEnabled
        {
            get { return shaderEnabled; }
            set { shaderEnabled = value; }
        }

        protected string technique;
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        TgcBoundingBox BoundingBox;
        public TgcBoundingBox getBoundingBox()
        {
            return BoundingBox;
        }

        public static readonly VertexElement[] ParedVertexElement = new VertexElement[]
        {
            new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
            new VertexElement(0, 24, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
            new VertexElement(0, 28, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
            new VertexElement(0, 36, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 1),
            VertexElement.VertexDeclarationEnd 
        };

        public struct ParedVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public int Color;
            public float Tu; //Textura 0
            public float Tv; //Textura 0
            public float Tu1; //Textura 1
            public float Tv1; //Textura 1
        };

        int numVertices = 0;
        ParedVertex[] ParedChapa = new ParedVertex[0];
        ParedVertex[] Vertices = new ParedVertex[0];
        int[] ParedChapaIndices = new int[0];

        VertexFormats ParedVertexFormat = (VertexFormats.Position | VertexFormats.Normal | VertexFormats.Diffuse | VertexFormats.Texture0 | VertexFormats.Texture1);
        VertexDeclaration ParedVertexDeclaration;

        public ChapaDeformable(Vector3 Origen, float Alto, float Ancho, string Orientation, float Teselation, string Texture)
        {
            numVertices = (int)FastMath.Ceiling(Alto * Ancho * (1 / Teselation) * (1 / Teselation) * (1 / Teselation));
            ParedVertex v = new ParedVertex();
            Array.Resize(ref ParedChapa, numVertices);
            Array.Resize(ref Vertices, numVertices);
            //Array.Resize(ref ParedChapaIndices, numVertices);
            int i = 0;
            float x = 0, y = 0, z = 0;
            Random rand = new Random();
            orientation = Orientation;
            shaderEnabled = false;
            teselation = Teselation;
            ancho = Ancho;
            alto = Alto;
            energia = Alto * Ancho;
            origin = Origen;


            //crear bounding box 
            BoundingBox = new TgcBoundingBox();

            //setear el material
            material = new Material();
            material.Diffuse = Color.White;
            material.Specular = Color.LightGray;
            material.SpecularSharpness = 15.0F;
            device.Material = material;

            //luz
            /*light = new Light();
            device.RenderState.Lighting = true;
            device.Lights[0].Type = LightType.Directional;
            device.Lights[0].Diffuse = Color.Red;
            device.Lights[0].Position = new Vector3(13, 1, 13);
            device.Lights[0].Direction = new Vector3(13, 3, 13);
            device.Lights[0].Range = 10;
            device.Lights[0].Enabled = true;*/

            //crear la textura
            texture = TextureLoader.FromFile(device, Texture);

            //Vertex Declaration
            ParedVertexDeclaration = new VertexDeclaration(device, ParedVertexElement);

            //Crear vertexBuffer
            vertexBuffer = new VertexBuffer(typeof(ParedVertex), numVertices, device, Usage.Dynamic, ParedVertexFormat, Pool.Default);

            if (Orientation == "XY")
            {
                Normal = new Vector3(0, 0, 1);
                for (y = 0; y < (float)(Alto); y = y + Teselation)
                {
                    for (x = 0; x < (float)(Ancho); x = x + Teselation)
                    {
                        v.Position = new Vector3(Origen.X + x, Origen.Y + y, Origen.Z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = x / (Ancho - Teselation);
                        v.Tv = y / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        ParedChapa[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X + x, Origen.Y + y + Teselation, Origen.Z);
                        //v.Normal = v.Position;
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = x / (Ancho - Teselation);
                        v.Tv = (y + Teselation) / Alto;
                        ParedChapa[i] = v;
                        i++;
                    }

                    y = y + Teselation;

                    for (x = (float)(Ancho - Teselation); x >= 0; x = x - Teselation)
                    {
                        v.Position = new Vector3(Origen.X + x, Origen.Y + y, Origen.Z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = x / (Ancho - Teselation);
                        v.Tv = y / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        ParedChapa[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X + x, Origen.Y + y + Teselation, Origen.Z);
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = x / Ancho;
                        v.Tv = (y + Teselation) / Alto;
                        ParedChapa[i] = v;
                        i++;
                    }

                }
                size = Origen + (new Vector3((float)Ancho, (float)Alto, 0));
                BBOpt BB = new BBOpt();
                for (int xy = 0; xy < Alto; xy++)
                {
                    BB.BBoxOpt = new TgcBoundingBox();
                    BB.BBoxOpt.setExtremes(Origen + new Vector3(0, xy, 0), Origen + new Vector3(Ancho, xy + 2, 0));
                    BB.inicio = (xy * (int)(2 * Ancho * (1 / Teselation)) * 4) / 2;
                    BB.fin = (int)(2 * Ancho * (1 / Teselation)) * 4;
                    LBBoxOpt.Add(BB);
                    xy++;
                }
                BoundingBox.setExtremes(Origen, new Vector3(Origen.X + (float)Ancho, Origen.Y + (float)Alto, Origen.Z));
            }
            else if (Orientation == "XZ")
            {
                Normal = new Vector3(0, 1, 0);
                for (z = 0; z < (float)(Alto - teselation); z = z + Teselation)
                {
                    for (x = 0; x < (float)(Ancho); x = x + Teselation)
                    {
                        v.Position = new Vector3(Origen.X + x, Origen.Y, Origen.Z + z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = x / Ancho;
                        v.Tv = z / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        ParedChapa[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X + x, Origen.Y, Origen.Z + z + Teselation);
                        //v.Normal = new Vector3(0, 1, 0);
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = x / Ancho;
                        v.Tv = (z + Teselation) / Alto;
                        ParedChapa[i] = v;
                        i++;
                    }
                    z = z + Teselation;
                    for (x = (float)(Ancho - Teselation); x >= 0; x = x - Teselation)
                    {
                        v.Position = new Vector3(Origen.X + x, Origen.Y, Origen.Z + z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = x / Ancho;
                        v.Tv = z / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        ParedChapa[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X + x, Origen.Y, Origen.Z + z + Teselation);
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = x / Ancho;
                        v.Tv = (z + Teselation) / Alto;
                        ParedChapa[i] = v;
                        i++;

                    }
                }
                size = Origen + (new Vector3((float)Ancho, 0, (float)Alto));
                BBOpt BB = new BBOpt();
                for (int xy = 0; xy < Alto; xy++)
                {
                    BB.BBoxOpt = new TgcBoundingBox();
                    BB.BBoxOpt.setExtremes(Origen + new Vector3(0, 0, xy), Origen + new Vector3(Ancho, 0, xy + 2));
                    BB.inicio = (xy * (int)(2 * Ancho * (1 / Teselation)) * 4) / 2;
                    BB.fin = (int)(2 * Ancho * (1 / Teselation)) * 4;
                    LBBoxOpt.Add(BB);
                    xy++;
                }
                BoundingBox.setExtremes(Origen, new Vector3(Origen.X + (float)Ancho, Origen.Y, Origen.Z + (float)Alto));

            }
            else if (Orientation == "YZ")
            {
                Normal = new Vector3(1, 0, 0);
                for (y = 0; y < (float)Alto; y = y + Teselation)
                {
                    for (z = 0; z < (float)Ancho; z = z + Teselation)
                    {
                        v.Position = new Vector3(Origen.X, Origen.Y + y, Origen.Z + z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = z / Ancho;
                        v.Tv = y / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        ParedChapa[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X, Origen.Y + y + Teselation, Origen.Z + z);
                        //v.Normal = v.Position;
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = z / Ancho;
                        v.Tv = (y + Teselation) / Alto;
                        ParedChapa[i] = v;
                        i++;
                    }
                    y = y + Teselation;
                    for (z = (float)(Ancho - Teselation); z >= 0; z = z - Teselation)
                    {
                        v.Position = new Vector3(Origen.X, Origen.Y + y, Origen.Z + z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = z / Ancho;
                        v.Tv = y / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        ParedChapa[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X, Origen.Y + y + Teselation, Origen.Z + z);
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = z / Ancho;
                        v.Tv = (y + Teselation) / Alto;
                        ParedChapa[i] = v;
                        i++;

                    }
                }
                size = Origen + (new Vector3(0, (float)Alto, (float)Ancho));
                BBOpt BB = new BBOpt();
                for (int xy = 0; xy < Alto; xy++)
                {
                    BB.BBoxOpt = new TgcBoundingBox();
                    BB.BBoxOpt.setExtremes(Origen + new Vector3(0, xy, 0), Origen + new Vector3(0, xy + 2, Ancho));
                    BB.inicio = (xy * (int)(2 * Ancho * (1 / Teselation)) * 4) / 2;
                    BB.fin = (int)(2 * Ancho * (1 / Teselation)) * 4;
                    LBBoxOpt.Add(BB);
                    xy++;
                }
                BoundingBox.setExtremes(Origen, new Vector3(Origen.X, Origen.Y + (float)Alto, Origen.Z + (float)Ancho));
            }

            vertexBuffer.SetData(ParedChapa, 0, LockFlags.None);
            time = 0;
        }


        public void render(float elapsedTime)
        {
            //Device device = GuiController.Instance.D3dDevice;
            if (shaderEnabled)
            {
                effect = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "Shaders\\BasicWall.fx");
                effect.Technique = "RenderScene";// Technique; //"RenderScene";
                effect.SetValue("matWorld", device.Transform.World);
                effect.SetValue("matWorldView", device.Transform.World * device.Transform.View);
                effect.SetValue("matWorldViewProj", device.Transform.World * device.Transform.View * device.Transform.Projection);
                effect.SetValue("time", elapsedTime);
                device.VertexDeclaration = ParedVertexDeclaration;
                //device.VertexFormat = (ParedVertexFormat);
                device.SetStreamSource(0, vertexBuffer, 0);
                device.SetTexture(0, texture);
                int numPasses = effect.Begin(0);
                for (int n = 0; n < numPasses; n++)
                {
                    effect.SetValue("texDiffuseMap", texture);
                    effect.BeginPass(n);
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, ParedChapa.Length - 2);
                    effect.EndPass();
                }
                effect.End();
            }
            else
            {
                device.VertexDeclaration = ParedVertexDeclaration;
                //device.VertexFormat = (ParedVertexFormat);
                device.SetStreamSource(0, vertexBuffer, 0);
                device.SetTexture(0, texture);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, ParedChapa.Length - 2);
            }
        }

       

        public void dispose()
        {

        }



    }
}
