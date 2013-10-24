using System;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;

namespace AlumnoEjemplos.RandomGroup
{
    public class XMesh : MeshPropio
    {
        private ExtendedMaterial[] arrayMats;
        private Material[] meshMateriales;
        private Texture[] meshTexturas;
        private Mesh mesh;

        private XMesh(ExtendedMaterial[] arrayMats, Material[] meshMateriales, Texture[] meshTexturas, Mesh mesh)
        {
            this.arrayMats = arrayMats;
            this.meshMateriales = meshMateriales;
            this.meshTexturas = meshTexturas;
            this.mesh = mesh;
        }

        public XMesh(string meshPath, string texturePath)
        {
            Device device = GuiController.Instance.D3dDevice;
            //cargando la malla desde un archivo .X exportado con 3dsmax, la siguiente linea devuelve también arrayMats
            mesh = Mesh.FromFile(meshPath, MeshFlags.Managed, device, out arrayMats);
            //Cargando los materiales, analizando c/u de los subsets del mesh
            if ((arrayMats != null) && (arrayMats.Length > 0))
            {
                meshMateriales = new Material[arrayMats.Length];
                meshTexturas = new Texture[arrayMats.Length];
                // cargamos cada material y cada textura en los array creados atras
                for (int i = 0; i < arrayMats.Length; i++)
                {   // cargamos el material
                    meshMateriales[i] = arrayMats[i].Material3D;
                    // si hay textura tambien la cargamos
                    if ((arrayMats[i].TextureFilename != null) && (arrayMats[i].TextureFilename != string.Empty))
                    {   //tenemos textura, entonces cargaremos la textura con textureLoader
                        meshTexturas[i] = TextureLoader.FromFile(device, texturePath);
                    }
                }
            }
        }

        public override void render(float elapsedTime)
        {
            //TODO !
        }

        public override void renderReal()
        {
            //renderizando el mesh cargado desde el archivo.X que está formado por varios subsets, entonces hay que cargar c/u de ellos, ésta es una de las diferencias
            //con la carga de archivos.xml que solamente se rederizan con: nombreDeMesh.render();
            Device dispositivo = GuiController.Instance.D3dDevice;
            for (int i = 0; i < meshMateriales.Length; i++)
            {
                dispositivo.Transform.World = Matrix.Translation(position);
                dispositivo.Material = meshMateriales[i];
                dispositivo.SetTexture(0, meshTexturas[i]);
                mesh.DrawSubset(i);
            }
        }

        public override Drawable clone()
        {
            return new XMesh(arrayMats, meshMateriales, meshTexturas, mesh);
        }

        public override float getRadiusSize()
        {
            return 0;//TODO !!!
        }
        public override MeshPropio scale(Vector3 scale)
        {
            throw new NotImplementedException();
        }
    }
}
