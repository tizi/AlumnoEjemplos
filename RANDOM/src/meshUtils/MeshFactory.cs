using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;

namespace AlumnoEjemplos.RANDOM.src.meshUtils
{
    public class MeshFactory
    {
        public static MeshPropio getMesh(string meshPath)
        {
            MeshPropio newMesh;
            if (meshPath.EndsWith(".xml")) newMesh = new XMLMesh(meshPath);
            else throw new Exception("Se quiso cargar un mesh que no terminaba en .xml");
            newMesh.setPosition(new Vector3(50, 50, 50));
            return newMesh;
        }
        public static MeshPropio getMesh(string meshPath, string texturePath)
        {
            MeshPropio newMesh;
            if (meshPath.EndsWith(".x")) newMesh = new XMesh(meshPath, texturePath);            
            else if (meshPath.EndsWith(".xml")) newMesh = new XMLMesh(meshPath);
            else throw new Exception("Se quiso cargar un mesh que no terminaba en .xml o .x");
            newMesh.setPosition(new Vector3(50, 50, 50));
            return newMesh;
        }
        
    }
}