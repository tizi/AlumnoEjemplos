using System.Collections.Generic;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using System.Drawing;
using AlumnoEjemplos.SRC.RANDOM.src.meshUtils;

namespace AlumnoEjemplos.SRC.RANDOM.src.grid
{
    /// <summary>
    /// Herramienta para crear y usar la Grilla Regular
    /// </summary>
    public class GrillaRegular
    {
        //Tamaños de celda de la grilla 
        float CELL_WIDTH = 300;
        float CELL_HEIGHT = 300;
        float CELL_LENGTH = 300;


        List<ElementoEstatico> modelos;
        TgcBoundingBox sceneBounds;
        List<TgcDebugBox> debugBoxes;
        GrillaRegularNode[, ,] grid;

        public GrillaRegular()
        {
        }

        /// <summary>
        /// Crear una nueva grilla
        /// </summary>
        /// <param name="modelos">Modelos a contemplar</param>
        /// <param name="sceneBounds">Límites del escenario</param>
        public void create(List<ElementoEstatico> modelos, TgcBoundingBox sceneBounds)
        {
            this.modelos = modelos;
            this.sceneBounds = sceneBounds;

            //build
            grid = buildGrid(modelos, sceneBounds, new Vector3(CELL_WIDTH, CELL_HEIGHT, CELL_LENGTH));

            foreach (ElementoEstatico mesh in modelos)
            {
                mesh.setEnabled(false);
            }
        }

        /// <summary>
        /// Construye la grilla
        /// </summary>
        private GrillaRegularNode[, ,] buildGrid(List<ElementoEstatico> modelos, TgcBoundingBox sceneBounds, Vector3 cellDim)
        {
            Vector3 sceneSize = sceneBounds.calculateSize();

            int gx = (int)FastMath.Ceiling(sceneSize.X / cellDim.X) + 1;
            int gy = (int)FastMath.Ceiling(sceneSize.Y / cellDim.Y) + 1;
            int gz = (int)FastMath.Ceiling(sceneSize.Z / cellDim.Z) + 1;

            GrillaRegularNode[, ,] grid = new GrillaRegularNode[gx, gy, gz];

            //Construir grilla
            for (int x = 0; x < gx; x++)
            {
                for (int y = 0; y < gy; y++)
                {
                    for (int z = 0; z < gz; z++)
                    {
                        //Crear celda
                        GrillaRegularNode node = new GrillaRegularNode();
                        
                        //Crear BoundingBox de celda
                        Vector3 pMin = new Vector3(sceneBounds.PMin.X + x * cellDim.X, sceneBounds.PMin.Y + y * cellDim.Y, sceneBounds.PMin.Z + z * cellDim.Z);
                        Vector3 pMax = Vector3.Add(pMin, cellDim);
                        node.BoundingBox = new TgcBoundingBox(pMin, pMax);

                        //Cargar modelos en celda
                        node.Models = new List<ElementoEstatico>();
                        addModelsToCell(node, modelos);

                        grid[x, y, z] = node;
                    }
                }
            }

            return grid;
        }

        /// <summary>
        /// Agregar modelos a una celda
        /// </summary>
        private void addModelsToCell(GrillaRegularNode node, List<ElementoEstatico> modelos)
        {
            foreach (ElementoEstatico mesh in modelos)
            {
                if (TgcCollisionUtils.testAABBAABB(node.BoundingBox, mesh.getBoundingBox()))
                {
                    node.Models.Add(mesh);
                }
            }
        }

        /// <summary>
        /// Crear meshes debug
        /// </summary>
        public void createDebugMeshes()
        {
            debugBoxes = new List<TgcDebugBox>();

            for (int x = 0; x < grid.GetUpperBound(0); x++)
            {
                for (int y = 0; y < grid.GetUpperBound(1); y++)
                {
                    for (int z = 0; z < grid.GetUpperBound(2); z++)
                    {
                        GrillaRegularNode node = grid[x, y, z];
                        TgcDebugBox box = TgcDebugBox.fromExtremes(node.BoundingBox.PMin, node.BoundingBox.PMax, Color.Red);

                        debugBoxes.Add(box);
                    }
                }
            }
        }

        /// <summary>
        /// Dibujar objetos de la isla en forma optimizada, utilizando la grilla para Frustm Culling
        /// </summary>
        public void render(TgcFrustum frustum, bool debugEnabled, float elapsedTime)
        {
            Vector3 pMax = sceneBounds.PMax;
            Vector3 pMin = sceneBounds.PMin;
            findVisibleMeshes(frustum);

            //Renderizar
            foreach (ElementoEstatico mesh in modelos)
            {
                if (mesh.getEnabled())
                {
                    mesh.render(elapsedTime);
                    mesh.setEnabled(false);
                }
            }

            if (debugEnabled)
            {
                foreach (TgcDebugBox debugBox in debugBoxes)
                {
                    debugBox.render();
                }
            }
        }

        /// <summary>
        /// Activar modelos dentro de celdas visibles
        /// </summary>
        private void findVisibleMeshes(TgcFrustum frustum)
        {
            for (int x = 0; x < grid.GetUpperBound(0); x++)
            {
                for (int y = 0; y < grid.GetUpperBound(1); y++)
                {
                    for (int z = 0; z < grid.GetUpperBound(2); z++)
                    {
                        GrillaRegularNode node = grid[x, y, z];
                        TgcCollisionUtils.FrustumResult r = TgcCollisionUtils.classifyFrustumAABB(frustum, node.BoundingBox);

                        if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
                        {
                            node.activateCellMeshes();
                        }
                    }
                }
            }
        }
    }
}
