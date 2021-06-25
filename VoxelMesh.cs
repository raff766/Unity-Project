using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelModule {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class VoxelMesh : MonoBehaviour {
        public static readonly Dictionary<int, VoxelMesh> VoxelMeshes = new Dictionary<int, VoxelMesh>();
        public float Scale { get => scale; private set => scale = value; }
        public Dictionary<Vector3, Voxel> Voxels { get; set; }

        [SerializeField] private TextAsset voxelData;
        [SerializeField] private float scale = 0.1f;
        private Mesh mesh;
        private BoxCollider boxCollider;

        void Awake() {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            TryGetComponent(out boxCollider);
            if (voxelData != null) Voxels = VoxelDataConverter.TextToVoxels(voxelData.ToString());
            VoxelMeshes.Add(GetInstanceID(), this);
            CalculateNeighbors();
            UpdateMesh();
        }

        public void CalculateNeighbors() {
            foreach (var pair in Voxels) {
                Vector3 position = pair.Key;
                Voxel voxel = pair.Value;
                Vector3 topPosition = new Vector3(position.x, position.y + 1, position.z);
                Vector3 bottomPosition = new Vector3(position.x, position.y - 1, position.z);
                Vector3 frontPosition = new Vector3(position.x, position.y, position.z + 1);
                Vector3 backPosition = new Vector3(position.x, position.y, position.z - 1);
                Vector3 rightPosition = new Vector3(position.x + 1, position.y, position.z);
                Vector3 leftPosition = new Vector3(position.x - 1, position.y, position.z);
                Voxels.TryGetValue(topPosition, out Voxel topVoxel);
                Voxels.TryGetValue(bottomPosition, out Voxel bottomVoxel);
                Voxels.TryGetValue(frontPosition, out Voxel frontVoxel);
                Voxels.TryGetValue(backPosition, out Voxel backVoxel);
                Voxels.TryGetValue(rightPosition, out Voxel rightVoxel);
                Voxels.TryGetValue(leftPosition, out Voxel leftVoxel);
                voxel.TopVoxel = topVoxel;
                voxel.BottomVoxel = bottomVoxel;
                voxel.FrontVoxel = frontVoxel;
                voxel.BackVoxel = backVoxel;
                voxel.RightVoxel = rightVoxel;
                voxel.LeftVoxel = leftVoxel;
            }
        }

        public void UpdateMesh() {
            if (Voxels == null) return;
            Voxel[] voxels = Voxels.Values.ToArray();
            IEnumerable<Voxel> voxelsToBeRendered = voxels.Where(voxel => !voxel.IsDestroyed && (voxel.TopVoxel == null ||
                voxel.TopVoxel.IsDestroyed || voxel.BottomVoxel == null || voxel.BottomVoxel.IsDestroyed ||
                voxel.FrontVoxel == null || voxel.FrontVoxel.IsDestroyed || voxel.BackVoxel == null ||
                voxel.BackVoxel.IsDestroyed || voxel.RightVoxel == null || voxel.RightVoxel.IsDestroyed ||
                voxel.LeftVoxel == null || voxel.LeftVoxel.IsDestroyed));
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var colors = new List<Color32>();
            foreach (Voxel voxel in voxelsToBeRendered) {
                Vector3 position = voxel.Position * scale;
                //top
                if (voxel.TopVoxel == null || voxel.TopVoxel.IsDestroyed) {
                    int count = vertices.Count;
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y + 0.5f * Scale, position.z + 0.5f * Scale)); //top front right
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y + 0.5f * Scale, position.z + 0.5f * Scale)); //top front left
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y + 0.5f * Scale, position.z - 0.5f * Scale)); //top back right
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y + 0.5f * Scale, position.z - 0.5f * Scale)); //top back left
                    triangles.Add(count);
                    triangles.Add(count + 2);
                    triangles.Add(count + 1);
                    triangles.Add(count + 1);
                    triangles.Add(count + 2);
                    triangles.Add(count + 3);
                    colors.AddRange(Enumerable.Repeat(voxel.Color, 4));
                }
                //bottom
                if (voxel.BottomVoxel == null || voxel.BottomVoxel.IsDestroyed) {
                    int count = vertices.Count;
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y - 0.5f * Scale, position.z + 0.5f * Scale)); //bottom front right
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y - 0.5f * Scale, position.z + 0.5f * Scale)); //bottom front left
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y - 0.5f * Scale, position.z - 0.5f * Scale)); //bottom back right
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y - 0.5f * Scale, position.z - 0.5f * Scale)); //bottom back left
                    triangles.Add(count);
                    triangles.Add(count + 1);
                    triangles.Add(count + 2);
                    triangles.Add(count + 2);
                    triangles.Add(count + 1);
                    triangles.Add(count + 3);
                    colors.AddRange(Enumerable.Repeat(voxel.Color, 4));
                }
                //front
                if (voxel.FrontVoxel == null || voxel.FrontVoxel.IsDestroyed) {
                    int count = vertices.Count;
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y + 0.5f * Scale, position.z + 0.5f * Scale)); //top front right
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y + 0.5f * Scale, position.z + 0.5f * Scale)); //top front left
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y - 0.5f * Scale, position.z + 0.5f * Scale)); //bottom front right
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y - 0.5f * Scale, position.z + 0.5f * Scale)); //bottom front left
                    triangles.Add(count);
                    triangles.Add(count + 1);
                    triangles.Add(count + 2);
                    triangles.Add(count + 2);
                    triangles.Add(count + 1);
                    triangles.Add(count + 3);
                    colors.AddRange(Enumerable.Repeat(voxel.Color, 4));
                }
                //back
                if (voxel.BackVoxel == null || voxel.BackVoxel.IsDestroyed) {
                    int count = vertices.Count;
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y + 0.5f * Scale, position.z - 0.5f * Scale)); //top back right
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y + 0.5f * Scale, position.z - 0.5f * Scale)); //top back left
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y - 0.5f * Scale, position.z - 0.5f * Scale)); //bottom back right
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y - 0.5f * Scale, position.z - 0.5f * Scale)); //bottom back left
                    triangles.Add(count);
                    triangles.Add(count + 2);
                    triangles.Add(count + 1);
                    triangles.Add(count + 1);
                    triangles.Add(count + 2);
                    triangles.Add(count + 3);
                    colors.AddRange(Enumerable.Repeat(voxel.Color, 4));
                }
                //right
                if (voxel.RightVoxel == null || voxel.RightVoxel.IsDestroyed) {
                    int count = vertices.Count;
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y + 0.5f * Scale, position.z + 0.5f * Scale)); //top front right
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y + 0.5f * Scale, position.z - 0.5f * Scale)); //top back right
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y - 0.5f * Scale, position.z + 0.5f * Scale)); //bottom front right
                    vertices.Add(new Vector3(position.x + 0.5f * Scale, position.y - 0.5f * Scale, position.z - 0.5f * Scale)); //bottom back right
                    triangles.Add(count);
                    triangles.Add(count + 2);
                    triangles.Add(count + 1);
                    triangles.Add(count + 1);
                    triangles.Add(count + 2);
                    triangles.Add(count + 3);
                    colors.AddRange(Enumerable.Repeat(voxel.Color, 4));
                }
                //left
                if (voxel.LeftVoxel == null || voxel.LeftVoxel.IsDestroyed) {
                    int count = vertices.Count;
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y + 0.5f * Scale, position.z + 0.5f * Scale)); //top front left
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y + 0.5f * Scale, position.z - 0.5f * Scale)); //top back left
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y - 0.5f * Scale, position.z + 0.5f * Scale)); //bottom front left
                    vertices.Add(new Vector3(position.x - 0.5f * Scale, position.y - 0.5f * Scale, position.z - 0.5f * Scale)); //bottom back left
                    triangles.Add(count);
                    triangles.Add(count + 1);
                    triangles.Add(count + 2);
                    triangles.Add(count + 2);
                    triangles.Add(count + 1);
                    triangles.Add(count + 3);
                    colors.AddRange(Enumerable.Repeat(voxel.Color, 4));
                }
            }
            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors32 = colors.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (boxCollider != null) {
                boxCollider.center = mesh.bounds.center;
                boxCollider.size = mesh.bounds.size;
            }
        }

        /*private void OnDrawGizmos() {
            if (vertices == null) {
                return;
            }
            foreach (Vector3 vertex in vertices) {
                Gizmos.DrawSphere(vertex, .1f);
            }
        }*/
    }
}
