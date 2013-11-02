using System;
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


        Effect effect;
        VertexBuffer vertexBuffer;
        Texture texture;
        Device device = GuiController.Instance.D3dDevice;

        public TgcBoundingBox BoundingBox;

        public Vector3 Origin { get; set; }

        public Vector3 Size { get; set; }

        public Vector3 Normal { get; set; }

        public string orientation { get; set; }

        public float Alto { get; set; }

        public float Ancho { get; set; }

        public float Tessellation { get; set; }

        public float Energia { get; set; }

        public Boolean ShaderEnabled { get; set; }

        public string Technique { get; set; }

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

        int numVertices;
        ParedVertex[] Pared = new ParedVertex[0];
        ParedVertex[] Vertices = new ParedVertex[0];

        private const VertexFormats ParedVertexFormat = (VertexFormats.Position | VertexFormats.Normal | VertexFormats.Diffuse | VertexFormats.Texture0 | VertexFormats.Texture1);
        VertexDeclaration ParedVertexDeclaration;

        public ParedDeformable(Vector3 origen, Vector3 normal, int verticesLado, string texturePath, float sizeMultiplier)
        {
            Vector3 direccionNormalizada = Vector3.Cross(normal, new Vector3(0, 1, 0));
            direccionNormalizada.Normalize();

            //crear la textura
            texture = TextureLoader.FromFile(device, texturePath);
            numVertices = verticesLado * verticesLado;

            //settear atributos
            Normal = normal;
            Origin = origen;
            Array.Resize(ref Pared, numVertices);
            Array.Resize(ref Vertices, numVertices);
            BoundingBox = new TgcBoundingBox();

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
                        Position = new Vector3(Origin.X + j * direccionNormalizada.X * sizeMultiplier,
                                                Origin.Y + i * sizeMultiplier,
                                                Origin.Z + j * direccionNormalizada.Z * sizeMultiplier),
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
            BoundingBox.setExtremes(Origin, new Vector3(verticesLado * direccionNormalizada.X * sizeMultiplier,
                sizeMultiplier * verticesLado * verticesLado,
                verticesLado * direccionNormalizada.Z * sizeMultiplier
                ));

            vertexBuffer.SetData(Pared, 0, LockFlags.None);
        }



        public ParedDeformable(Vector3 Origen, float Alto, float Ancho, string Orientation, float Tessellation, string Texture)
        {
            numVertices = (int)FastMath.Ceiling(Alto * Ancho * (1 / Tessellation) * (1 / Tessellation) * (1 / Tessellation));
            ParedVertex v = new ParedVertex();
            Array.Resize(ref Pared, numVertices);
            Array.Resize(ref Vertices, numVertices);
            //Array.Resize(ref ParedIndices, numVertices);
            int i = 0;
            float x, y, z;
            orientation = Orientation;
            ShaderEnabled = false;
            this.Tessellation = Tessellation;
            this.Ancho = Ancho;
            this.Alto = Alto;
            Energia = Alto * Ancho;
            Origin = Origen;


            //crear bounding box 
 
            BoundingBox = new TgcBoundingBox();
            

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
                            //v.Tu = x / (Ancho - tessellation);
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
                    BoundingBox.setExtremes(Origen, new Vector3(Origen.X + Ancho, Origen.Y + Alto, Origen.Z));
                    break;
                case "XZ":
                    Normal = new Vector3(0, 1, 0);
                    for (z = 0; z < Alto - Tessellation; z = z + Tessellation)
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
                    BoundingBox.setExtremes(Origen, new Vector3(Origen.X + Ancho, Origen.Y, Origen.Z + Alto));
                    break;
                case "YZ":
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
                    BoundingBox.setExtremes(Origen, new Vector3(Origen.X, Origen.Y + Alto, Origen.Z + Ancho));
                    break;
            }

            vertexBuffer.SetData(Pared, 0, LockFlags.None);
        }


        public void render(float elapsedTime)
        {
            //Device device = GuiController.Instance.D3dDevice;
            if (ShaderEnabled)
            {
                effect = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "Shaders\\BasicWall.fx");
                effect.Technique = "RenderScene";// Technique; //"RenderScene";
                effect.SetValue("matWorld", device.Transform.World);
                effect.SetValue("matWorldView", device.Transform.World * device.Transform.View);
                effect.SetValue("matWorldViewProj", device.Transform.World * device.Transform.View * device.Transform.Projection);
                effect.SetValue("time", elapsedTime);
                device.VertexDeclaration = ParedVertexDeclaration;
                device.VertexFormat = (ParedVertexFormat);
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
                device.VertexFormat = (ParedVertexFormat);
                device.SetStreamSource(0, vertexBuffer, 0);
                device.SetTexture(0, texture);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, Pared.Length - 2);
            }
        }

       

        public Boolean deformarPared(Projectile proyectil, TgcBoundingBox BB)
        {
            float Radio = proyectil.boundingBall.Radius;
            Vector3 Position = proyectil.getPosition(); // new Vector3(30, 30, 0);
            Vector3 Direccion = proyectil.direction; // new Vector3(5, 2, 1);
            int i = 0;
            float DefoMod = proyectil.getSpeed() * proyectil.mass * 0.1f;  //100 * 15f;
            Boolean deformo = false;
            float deformacion = 0;


            //MAGIA DE DEFORMACION
            Vertices = (ParedVertex[])vertexBuffer.Lock(0, typeof(ParedVertex), LockFlags.None, numVertices);
            for (i = 0; i < Vertices.Length; i++)
            {
                float distanciaCentroVertex = TgcVectorUtils.lengthSq(Vertices[i].Position, Position);
                if (distanciaCentroVertex > (Radio) + DefoMod) continue;

                Vertices[i].Color = Color.Green.ToArgb();
                
                deformacion = Math.Sign(Vector3.Dot(Vertices[i].Normal, Direccion)) * (FastMath.Pow2(1 / distanciaCentroVertex) * DefoMod);
                
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

                deformo = true;
            }
            vertexBuffer.Unlock();
            //FIN MAGIA DEFORMACION

            //Agrando el bounding box para que las colisiones futuras chequeen contra las deformaciones
            if (!deformo) return false;            
            switch (orientation)
            {
                case "XY":
                    if (Math.Sign(Vector3.Dot(Normal, Direccion)) > 0)
                    {
                        BoundingBox.setExtremes(BoundingBox.PMin, BoundingBox.PMax + new Vector3(0, 0, deformacion));
                    }
                    else
                    {
                        BoundingBox.setExtremes(BoundingBox.PMin + new Vector3(0, 0, deformacion), BoundingBox.PMax);

                    }
                    break;
                case "XZ":
                    if (Math.Sign(Vector3.Dot(Normal, Direccion)) > 0)
                    {
                        BoundingBox.setExtremes(BoundingBox.PMin, BoundingBox.PMax + new Vector3(0, deformacion, 0));
                    }
                    else
                    {
                        BoundingBox.setExtremes(BoundingBox.PMin + new Vector3(0, deformacion, 0), BoundingBox.PMax);
                    }
                    break;
                case "YZ":
                    if (Math.Sign(Vector3.Dot(Normal, Direccion)) > 0)
                    {
                        BoundingBox.setExtremes(BoundingBox.PMin, BoundingBox.PMax + new Vector3(deformacion, 0, 0));
                    }
                    else
                    {
                        BoundingBox.setExtremes(BoundingBox.PMin + new Vector3(deformacion, 0, 0), BoundingBox.PMax);
                    }
                    break;
            }

            return true;
        }

        public void dispose()
        {
            
        }



    }
}
