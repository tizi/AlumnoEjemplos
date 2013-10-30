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
        public void setEnabled(bool setting)
        {
            enabled = setting;
        }

        Effect effect;
        
        VertexBuffer vertexBuffer;
        Texture texture;
        Device device = GuiController.Instance.D3dDevice;

        public struct BBOpt
        {
            public TgcBoundingBox BBoxOpt;
            public int inicio;
            public int fin;
        }

        public List<BBOpt> LBBoxOpt = new List<BBOpt>();


        public Vector3 Origin { get; set; }

        public Vector3 Size { get; set; }

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

        public float Alto { get; set; }

        public float Ancho { get; set; }

        float teselation;
        public float Teselation
        {
            get { return teselation; }
            set { teselation = value; }
        }

        public float Energia { get; set; }


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

        TgcBoundingBox boundingBox;
        public TgcBoundingBox getBoundingBox()
        {
            return boundingBox;
        }

        public static readonly VertexElement[] ParedVertexElement =
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

        ParedVertex[] Pared = new ParedVertex[0];
        ParedVertex[] Vertices = new ParedVertex[0];

        private const VertexFormats ParedVertexFormat = (VertexFormats.Position | VertexFormats.Normal | VertexFormats.Diffuse | VertexFormats.Texture0 | VertexFormats.Texture1);
        VertexDeclaration ParedVertexDeclaration;

        public ParedDeformable(Vector3 origen, Vector3 normal, int verticesLado, string texturePath, float sizeMultiplier)
        {
            Vector3 direccionNormalizada = Vector3.Cross(normal, new Vector3(0,1,0));            
            direccionNormalizada.Normalize();

            //crear la textura
            texture = TextureLoader.FromFile(device, texturePath);
            int numVertices = verticesLado*verticesLado;
            
            //settear atributos
            //La pared va de abajo para arriba asi 
            this.normal = normal;
            Origin = origen;
            Array.Resize(ref Pared, numVertices);
            Array.Resize(ref Vertices, numVertices);            
            boundingBox = new TgcBoundingBox();

            //setear el material
            Material material = new Material
            {
                Diffuse = Color.White,
                Specular = Color.LightGray,
                SpecularSharpness = 15.0F
            };
            device.Material = material;

            //Vertex Declaration y Vertex Buffer
            ParedVertexDeclaration = new VertexDeclaration(device, ParedVertexElement);
            vertexBuffer = new VertexBuffer(typeof(ParedVertex), numVertices, device, Usage.Dynamic, ParedVertexFormat, Pool.Default);

            //horizontal
            int i;
            for (i = 0; i < verticesLado; i++)
            {
                //vertical
                for (int j = 0; j < verticesLado; j++)
                {                    
                    ParedVertex paredVertex = new ParedVertex
                    {
                        //La posicion es el origen, mas la direccion, mas la elevacion
                        Position = new Vector3(Origin.X+j*direccionNormalizada.X*sizeMultiplier,
                                                Origin.Y+i*sizeMultiplier, 
                                                Origin.Z+j*direccionNormalizada.Z*sizeMultiplier),
                        Normal = Normal,
                        Color = Color.White.ToArgb(),
                        Tu = i,
                        Tv = j,
                        Tu1 = 0,
                        Tv1 = 0
                    };
                    Pared[i] = paredVertex;                    
                }
            }            
            boundingBox.setExtremes(Origin, new Vector3(verticesLado*direccionNormalizada.X*sizeMultiplier, 
                sizeMultiplier*verticesLado*verticesLado,
                verticesLado * direccionNormalizada.Z * sizeMultiplier
                ));

            vertexBuffer.SetData(Pared, 0, LockFlags.None);
        }

        public ParedDeformable(Vector3 Origen, float Alto, float Ancho, string Orientation, float Tessellation, string Texture)
        {
            int numVertices = (int)FastMath.Ceiling(Alto * Ancho * (1 / Tessellation) * (1 / Tessellation) * (1 / Tessellation));
            ParedVertex v = new ParedVertex();
            Array.Resize(ref Pared, numVertices);
            Array.Resize(ref Vertices, numVertices);
            //Array.Resize(ref ParedIndices, numVertices);
            int i = 0;
            float x, y, z;            
            orientation = Orientation;
            shaderEnabled = false;
            teselation = Tessellation;
            this.Ancho = Ancho;
            this.Alto = Alto;
            Energia = Alto * Ancho;
            Origin = Origen;


            //crear bounding box 
            boundingBox = new TgcBoundingBox();

            //setear el material
            Material material = new Material
            {
                Diffuse = Color.White,
                Specular = Color.LightGray,
                SpecularSharpness = 15.0F
            };
            device.Material = material;

            //crear la textura
            texture = TextureLoader.FromFile(device, Texture);

            //Vertex Declaration
            ParedVertexDeclaration = new VertexDeclaration(device, ParedVertexElement);

            //Crear vertexBuffer
            vertexBuffer = new VertexBuffer(typeof(ParedVertex), numVertices, device, Usage.Dynamic, ParedVertexFormat, Pool.Default);

            switch (Orientation)
            {
                case "XY":
                {
                    Normal = new Vector3(0, 0, 1);
                    for (y = 0; y < Alto; y = y + Tessellation)
                    {
                        for (x = 0; x < Ancho; x = x + Tessellation)
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

                        for (x = Ancho - Tessellation; x >= 0; x = x - Tessellation)
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
                    Size = Origen + (new Vector3(Ancho, Alto, 0));
                    BBOpt BB = new BBOpt();
                    for (int xy = 0; xy < Alto; xy++)
                    {
                        BB.BBoxOpt = new TgcBoundingBox();
                        BB.BBoxOpt.setExtremes(Origen + new Vector3(0, xy, 0), Origen + new Vector3(Ancho, xy + 2, 0));
                        BB.inicio = (xy * (int)(2 * Ancho * (1 / Tessellation)) * 4) / 2;
                        BB.fin = (int)(2 * Ancho * (1 / Tessellation)) * 4;
                        LBBoxOpt.Add(BB);
                        xy++;
                    }
                    boundingBox.setExtremes(Origen, new Vector3(Origen.X + Ancho, Origen.Y + Alto, Origen.Z));
                }
                    break;

                case "XZ":
                {
                    Normal = new Vector3(0, 1, 0);
                    for (z = 0; z < Alto - teselation; z = z + Tessellation)
                    {
                        for (x = 0; x < Ancho; x = x + Tessellation)
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
                        for (x = Ancho - Tessellation; x >= 0; x = x - Tessellation)
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
                    Size = Origen + (new Vector3(Ancho, 0, Alto));
                    BBOpt BB = new BBOpt();
                    for (int xy = 0; xy < Alto; xy++)
                    {
                        BB.BBoxOpt = new TgcBoundingBox();
                        BB.BBoxOpt.setExtremes(Origen + new Vector3(0, 0, xy), Origen + new Vector3(Ancho, 0, xy + 2));
                        BB.inicio = (xy * (int)(2 * Ancho * (1 / Tessellation)) * 4) / 2;
                        BB.fin = (int)(2 * Ancho * (1 / Tessellation)) * 4;
                        LBBoxOpt.Add(BB);
                        xy++;
                    }
                    boundingBox.setExtremes(Origen, new Vector3(Origen.X + Ancho, Origen.Y, Origen.Z + Alto));

                }
                    break;

                case "YZ":
                {
                    Normal = new Vector3(1, 0, 0);
                    for (y = 0; y < Alto; y = y + Tessellation)
                    {
                        for (z = 0; z < Ancho; z = z + Tessellation)
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
                        for (z = Ancho - Tessellation; z >= 0; z = z - Tessellation)
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
                    Size = Origen + (new Vector3(0, Alto, Ancho));
                    BBOpt BB = new BBOpt();
                    for (int xy = 0; xy < Alto; xy++)
                    {
                        BB.BBoxOpt = new TgcBoundingBox();
                        BB.BBoxOpt.setExtremes(Origen + new Vector3(0, xy, 0), Origen + new Vector3(0, xy + 2, Ancho));
                        BB.inicio = (xy * (int)(2 * Ancho * (1 / Tessellation)) * 4) / 2;
                        BB.fin = (int)(2 * Ancho * (1 / Tessellation)) * 4;
                        LBBoxOpt.Add(BB);
                        xy++;
                    }
                    boundingBox.setExtremes(Origen, new Vector3(Origen.X, Origen.Y + Alto, Origen.Z + Ancho));
                }
                    break;
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

       

        public Boolean deformarPared(Projectile proyectil, BBOpt BB)
        {
            float radio = proyectil.boundingBall.Radius;
            Vector3 posicionContacto = proyectil.getPosition();
            Vector3 direccionProyectil = proyectil.getDirection();            
            float defoMod = proyectil.getSpeed() * proyectil.mass * 0.001F;
            Boolean deformo = false;
            float deformacion = 0;
            Vertices = (ParedVertex[])vertexBuffer.Lock(BB.inicio * 44, typeof(ParedVertex), LockFlags.None, BB.fin);

            GuiController.Instance.Logger.log("DefoMod: " + defoMod +
                "\nDireccion Proyectil: "+ direccionProyectil.ToString() + "Contacto: "+ posicionContacto.ToString());
            //GuiController.Instance.Logger.log("Direccion: " + Direccion.ToString());
            //GuiController.Instance.Logger.log("Entre!!! Inicio: " + BB.inicio.ToString() + " Fin: " + BB.fin.ToString());

            
            for (int i = 0; i < Vertices.Length; i++)
            {                
                float distanciaVerticeCentro = FastMath.Pow2(TgcVectorUtils.lengthSq(Vertices[i].Position, posicionContacto));
                if (distanciaVerticeCentro > FastMath.Pow2(radio) + defoMod) continue;

                Vertices[i].Color = Color.Red.ToArgb();
                deformo = true;

                deformacion = Math.Sign(Vector3.Dot(Vertices[i].Normal, direccionProyectil))*
                               (1/distanciaVerticeCentro) * defoMod;                              

                if (deformacion > 1) deformacion = 1;
                if (deformacion < -1) deformacion = -1;

                /*Vertices[i].Position.Z += /*deformacion * FastMath.Abs(direccionProyectil.Z / 75);
                Vertices[i].Position.Y += /*deformacion * FastMath.Abs(direccionProyectil.Y / 75);
                Vertices[i].Position.X += /*deformacion * FastMath.Abs(direccionProyectil.X / 75);*/

                Vertices[i].Position.Z += direccionProyectil.Z * defoMod / 100;
                Vertices[i].Position.Y += direccionProyectil.Y * defoMod / 100;
                Vertices[i].Position.X += direccionProyectil.X * defoMod / 100;

            }
            vertexBuffer.Unlock();


            //Agrando el bounding box para que las colisiones futuras chequeen contra las deformaciones
            if (!deformo) return false;

            switch (orientation)
            {
                case "XY":
                    if (Math.Sign(Vector3.Dot(normal, direccionProyectil)) > 0)
                    {
                        boundingBox.setExtremes(boundingBox.PMin, boundingBox.PMax + new Vector3(0, 0, deformacion));
                        BB.BBoxOpt.setExtremes(BB.BBoxOpt.PMin, BB.BBoxOpt.PMax + new Vector3(0, 0, deformacion));
                    }
                    else
                    {
                        boundingBox.setExtremes(boundingBox.PMin + new Vector3(0, 0, deformacion), boundingBox.PMax);
                        BB.BBoxOpt.setExtremes(BB.BBoxOpt.PMin + new Vector3(0, 0, deformacion), BB.BBoxOpt.PMax);
                    }
                    break;

                case "XZ":
                    if (Math.Sign(Vector3.Dot(normal, direccionProyectil)) > 0)
                    {
                        boundingBox.setExtremes(boundingBox.PMin, boundingBox.PMax + new Vector3(0, deformacion, 0));
                        BB.BBoxOpt.setExtremes(BB.BBoxOpt.PMin, BB.BBoxOpt.PMax + new Vector3(0, deformacion, 0));
                    }
                    else
                    {
                        boundingBox.setExtremes(boundingBox.PMin + new Vector3(0, deformacion, 0), boundingBox.PMax);
                        BB.BBoxOpt.setExtremes(BB.BBoxOpt.PMin + new Vector3(0, deformacion, 0), BB.BBoxOpt.PMax);
                    }
                    break;

                case "YZ":
                    if (Math.Sign(Vector3.Dot(normal, direccionProyectil)) > 0)
                    {
                        boundingBox.setExtremes(boundingBox.PMin, boundingBox.PMax + new Vector3(deformacion, 0, 0));
                        BB.BBoxOpt.setExtremes(BB.BBoxOpt.PMin, BB.BBoxOpt.PMax + new Vector3(deformacion, 0, 0));
                    }
                    else
                    {
                        boundingBox.setExtremes(boundingBox.PMin + new Vector3(deformacion, 0, 0), boundingBox.PMax);
                        BB.BBoxOpt.setExtremes(BB.BBoxOpt.PMin + new Vector3(deformacion, 0, 0), BB.BBoxOpt.PMax);
                    }
                    break;
            }

            //Se deformo
            return true;
        }

        public void dispose()
        {
            
        }

    }
}
