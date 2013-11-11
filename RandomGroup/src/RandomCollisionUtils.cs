using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.RandomGroup.src
{
    class RandomCollisionUtils
    {
        public static bool intersectSegmentOBB(Vector3 p0, Vector3 p1, TgcObb obb, out Vector3 q)
        {
            Vector3 segmentDir = p1 - p0;
            TgcRay ray = new TgcRay(p0, segmentDir);
            if (TgcCollisionUtils.intersectRayObb(ray, obb, out q))
            {
                float segmentLengthSq = segmentDir.LengthSq();
                Vector3 collisionDiff = q - p0;
                float collisionLengthSq = collisionDiff.LengthSq();
                if (collisionLengthSq <= segmentLengthSq)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
