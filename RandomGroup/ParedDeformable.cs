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
    public class ParedDeformable
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
        VertexBuffer vertexBuffer;
        Texture texture;
        Material material;
        Device device = GuiController.Instance.D3dDevice;

        public TgcBoundingBox BoundingBox;

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
        ParedVertex[] Pared = new ParedVertex[0];
        ParedVertex[] Vertices = new ParedVertex[0];
        int[] ParedIndices = new int[0];

        VertexFormats ParedVertexFormat = (VertexFormats.Position | VertexFormats.Normal | VertexFormats.Diffuse | VertexFormats.Texture0 | VertexFormats.Texture1);
        VertexDeclaration ParedVertexDeclaration;

        public ParedDeformable(Vector3 Origen, float Alto, float Ancho, string Orientation, float Tessellation, string Texture)
        {
            numVertices = (int)FastMath.Ceiling(Alto * Ancho * (1 / Tessellation) * (1 / Tessellation) * (1 / Tessellation));
            ParedVertex v = new ParedVertex();
            Array.Resize(ref Pared, numVertices);
            Array.Resize(ref Vertices, numVertices);
            //Array.Resize(ref ParedIndices, numVertices);
            int i = 0;
            float x = 0, y = 0, z = 0;
            Random rand = new Random();
            orientation = Orientation;
            shaderEnabled = false;
            teselation = Tessellation;
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
                for (y = 0; y < (float)(Alto); y = y + Tessellation)
                {
                    for (x = 0; x < (float)(Ancho); x = x + Tessellation)
                    {
                        v.Position = new Vector3(Origen.X + x, Origen.Y + y, Origen.Z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = x / (Ancho - Tessellation);
                        v.Tv = y / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        Pared[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X + x, Origen.Y + y + Tessellation, Origen.Z);
                        //v.Normal = v.Position;
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = x / (Ancho - Teselation);
                        v.Tv = (y + Tessellation) / Alto;
                        Pared[i] = v;
                        i++;
                    }

                    y = y + Tessellation;

                    for (x = (float)(Ancho - Tessellation); x >= 0; x = x - Tessellation)
                    {
                        v.Position = new Vector3(Origen.X + x, Origen.Y + y, Origen.Z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = x / (Ancho - Tessellation);
                        v.Tv = y / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        Pared[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X + x, Origen.Y + y + Tessellation, Origen.Z);
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = x / Ancho;
                        v.Tv = (y + Tessellation) / Alto;
                        Pared[i] = v;
                        i++;
                    }

                }
                size = Origen + (new Vector3((float)Ancho, (float)Alto, 0));
                BoundingBox.setExtremes(Origen, new Vector3(Origen.X + (float)Ancho, Origen.Y + (float)Alto, Origen.Z));
            }
            else if (Orientation == "XZ")
            {
                Normal = new Vector3(0, 1, 0);
                for (z = 0; z < (float)(Alto - teselation); z = z + Tessellation)
                {
                    for (x = 0; x < (float)(Ancho); x = x + Tessellation)
                    {
                        v.Position = new Vector3(Origen.X + x, Origen.Y, Origen.Z + z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = x / Ancho;
                        v.Tv = z / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        Pared[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X + x, Origen.Y, Origen.Z + z + Tessellation);
                        //v.Normal = new Vector3(0, 1, 0);
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = x / Ancho;
                        v.Tv = (z + Tessellation) / Alto;
                        Pared[i] = v;
                        i++;
                    }
                    z = z + Tessellation;
                    for (x = (float)(Ancho - Tessellation); x >= 0; x = x - Tessellation)
                    {
                        v.Position = new Vector3(Origen.X + x, Origen.Y, Origen.Z + z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = x / Ancho;
                        v.Tv = z / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        Pared[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X + x, Origen.Y, Origen.Z + z + Tessellation);
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = x / Ancho;
                        v.Tv = (z + Tessellation) / Alto;
                        Pared[i] = v;
                        i++;

                    }
                }
                size = Origen + (new Vector3((float)Ancho, 0, (float)Alto));
                BoundingBox.setExtremes(Origen, new Vector3(Origen.X + (float)Ancho, Origen.Y, Origen.Z + (float)Alto));

            }
            else if (Orientation == "YZ")
            {
                Normal = new Vector3(1, 0, 0);
                for (y = 0; y < (float)Alto; y = y + Tessellation)
                {
                    for (z = 0; z < (float)Ancho; z = z + Tessellation)
                    {
                        v.Position = new Vector3(Origen.X, Origen.Y + y, Origen.Z + z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = z / Ancho;
                        v.Tv = y / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        Pared[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X, Origen.Y + y + Tessellation, Origen.Z + z);
                        //v.Normal = v.Position;
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = z / Ancho;
                        v.Tv = (y + Tessellation) / Alto;
                        Pared[i] = v;
                        i++;
                    }
                    y = y + Tessellation;
                    for (z = (float)(Ancho - Tessellation); z >= 0; z = z - Tessellation)
                    {
                        v.Position = new Vector3(Origen.X, Origen.Y + y, Origen.Z + z);
                        v.Normal = Normal;
                        v.Color = Color.White.ToArgb();
                        v.Tu = z / Ancho;
                        v.Tv = y / Alto;
                        v.Tu1 = 0;
                        v.Tv1 = 0;
                        Pared[i] = v;
                        i++;

                        v.Position = new Vector3(Origen.X, Origen.Y + y + Tessellation, Origen.Z + z);
                        //v.Color = Color.White.ToArgb();
                        //v.Tu = z / Ancho;
                        v.Tv = (y + Tessellation) / Alto;
                        Pared[i] = v;
                        i++;

                    }
                }
                size = Origen + (new Vector3(0, (float)Alto, (float)Ancho));
                BoundingBox.setExtremes(Origen, new Vector3(Origen.X, Origen.Y + (float)Alto, Origen.Z + (float)Ancho));
            }

            vertexBuffer.SetData(Pared, 0, LockFlags.None);
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
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, Pared.Length - 2);
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
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, Pared.Length - 2);
            }
        }

       

        public Boolean deformarPared(Projectile proyectil, TgcBoundingBox BB)
        {
            float Radio = 5f;
            Vector3 Position = proyectil.getPosition(); // new Vector3(30, 30, 0);
            Vector3 Direccion = proyectil.direction; // new Vector3(5, 2, 1);
            int i=0;
            float DefoMod = 100*15f;
            Boolean deformo = false;
            float deformacion = 0;
            //GuiController.Instance.Logger.log("Position: " + Position.ToString());
            //GuiController.Instance.Logger.log("Direccion: " + Direccion.ToString());
            //GuiController.Instance.Logger.log("Entre!!! Inicio: " + BB.inicio.ToString() + " Fin: " + BB.fin.ToString());
            Vertices = (ParedVertex[])vertexBuffer.Lock(0, typeof(ParedVertex), LockFlags.None, numVertices);
            for (i = 0; i < Vertices.Length; i++)
            {
                if (TgcVectorUtils.lengthSq(Vertices[i].Position, Position) <= (Radio) + DefoMod)
                {
                    Vertices[i].Color = Color.Green.ToArgb();
                    deformo = true;
                    deformacion = Math.Sign(Vector3.Dot(Vertices[i].Normal, Direccion)) * (FastMath.Pow2(1 / TgcVectorUtils.lengthSq(Vertices[i].Position, Position)) * DefoMod);
                    if (deformacion > 1) deformacion = 1;
                    if (deformacion < -1) deformacion = -1;

                    if (orientation == "XY")
                    {
                        Vertices[i].Position.Z += deformacion;
                        Vertices[i].Position.Y += deformacion;
                        Vertices[i].Position.X += deformacion;
                    }
                    else if (orientation == "XZ")
                    {
                        Vertices[i].Position.Z += deformacion;
                        Vertices[i].Position.Y += deformacion;
                        Vertices[i].Position.X += deformacion;
                    }
                    else if (orientation == "YZ")
                    {
                        Vertices[i].Position.Z += deformacion;
                        Vertices[i].Position.Y += deformacion;
                        Vertices[i].Position.X += deformacion;
                    }
                }
            }
            vertexBuffer.Unlock();

            //Agrando el bounding box para que las colisiones futuras chequeen contra las deformaciones
            if (deformo == true)
                if (orientation == "XY")
                if (Math.Sign(Vector3.Dot(normal, Direccion)) > 0)
                {
                    BoundingBox.setExtremes(BoundingBox.PMin, BoundingBox.PMax + new Vector3(0, 0, deformacion));
                }
                else
                {
                    BoundingBox.setExtremes(BoundingBox.PMin + new Vector3(0, 0, deformacion), BoundingBox.PMax);

                }
            else if (orientation == "XZ")
                if (Math.Sign(Vector3.Dot(normal, Direccion)) > 0)
                {
                    BoundingBox.setExtremes(BoundingBox.PMin, BoundingBox.PMax + new Vector3(0, deformacion, 0));
                }
                else
                {
                    BoundingBox.setExtremes(BoundingBox.PMin + new Vector3(0, deformacion, 0), BoundingBox.PMax);
                }
            else if (orientation == "YZ")
                if (Math.Sign(Vector3.Dot(normal, Direccion)) > 0)
                {
                    BoundingBox.setExtremes(BoundingBox.PMin, BoundingBox.PMax + new Vector3(deformacion, 0, 0));
                }
                else
                {
                    BoundingBox.setExtremes(BoundingBox.PMin + new Vector3(deformacion, 0, 0), BoundingBox.PMax);
                }

            return deformo;
        }

        public void dispose()
        {
            
        }



    }
}
