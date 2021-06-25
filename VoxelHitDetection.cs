using System.Collections.Generic;
using UnityEngine;

namespace VoxelModule {
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(VoxelDestructor))]
    public class VoxelHitDetection : MonoBehaviour {
        [SerializeField] private new Camera camera;
        private Ray ray;
        private RaycastHit raycastHit;

        void Start() {
            camera = Camera.main;
        }

        void OnMouseDown() {
            ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit, 100f)) {
                VoxelMesh voxelMesh = raycastHit.transform.GetComponent<VoxelMesh>();
                Vector3 localLineStartPoint = raycastHit.transform.InverseTransformPoint(raycastHit.point);
                Vector3 localLineEndPoint = raycastHit.transform.InverseTransformPoint(raycastHit.point + ray.direction * 1000);
                float scale = voxelMesh.Scale;
                Vector3 intersectionHit = new Vector3();
                var intersectingVoxels = new List<Voxel>();
                foreach (var voxel in voxelMesh.Voxels.Values) {
                    Vector3 minBoxCoord = (voxel.Position * scale) + Vector3.one * (-0.5f * scale);
                    Vector3 maxBoxCoord = (voxel.Position * scale) + Vector3.one * (0.5f * scale);
                    if (!voxel.IsDestroyed && CheckLineBox(minBoxCoord, maxBoxCoord, localLineStartPoint, localLineEndPoint, ref intersectionHit)) {
                        intersectingVoxels.Add(voxel);
                    }
                }
                raycastHit.transform.GetComponent<VoxelDestructor>().Destroy(intersectingVoxels.ToArray());
                /*if (intersectingVoxels.Count > 0) {
                    Voxel closestVoxel = null;
                    float closestDistance = Mathf.Infinity;
                    foreach (Voxel voxel in intersectingVoxels) {
                        float sqrDistance = (raycastHit.point - voxel.Position * scale).sqrMagnitude;
                        if (sqrDistance < closestDistance) {
                            closestVoxel = voxel;
                            closestDistance = sqrDistance;
                        }
                    }
                    raycastHit.transform.GetComponent<VoxelDestructor>().Destroy(closestVoxel);
                }*/
            }
        }

        void Update() {
            Debug.DrawLine(camera.transform.position, raycastHit.point);
            Debug.DrawLine(raycastHit.point, raycastHit.point + ray.direction * 1000);
        }

        static bool CheckLineBox(Vector3 b1, Vector3 b2, Vector3 l1, Vector3 l2, ref Vector3 hit) {
            if (l2.x < b1.x && l1.x < b1.x) return false;
            if (l2.x > b2.x && l1.x > b2.x) return false;
            if (l2.y < b1.y && l1.y < b1.y) return false;
            if (l2.y > b2.y && l1.y > b2.y) return false;
            if (l2.z < b1.z && l1.z < b1.z) return false;
            if (l2.z > b2.z && l1.z > b2.z) return false;
            if (l1.x > b1.x && l1.x < b2.x &&
                l1.y > b1.y && l1.y < b2.y &&
                l1.z > b1.z && l1.z < b2.z) {
                hit = l1;
                return true;
            }
            return (GetIntersection(l1.x - b1.x, l2.x - b1.x, l1, l2, ref hit) && InBox(hit, b1, b2, 1))
                   || (GetIntersection(l1.y - b1.y, l2.y - b1.y, l1, l2, ref hit) && InBox(hit, b1, b2, 2))
                   || (GetIntersection(l1.z - b1.z, l2.z - b1.z, l1, l2, ref hit) && InBox(hit, b1, b2, 3))
                   || (GetIntersection(l1.x - b2.x, l2.x - b2.x, l1, l2, ref hit) && InBox(hit, b1, b2, 1))
                   || (GetIntersection(l1.y - b2.y, l2.y - b2.y, l1, l2, ref hit) && InBox(hit, b1, b2, 2))
                   || (GetIntersection(l1.z - b2.z, l2.z - b2.z, l1, l2, ref hit) && InBox(hit, b1, b2, 3));
        }

        static bool GetIntersection(float fDst1, float fDst2, Vector3 p1, Vector3 p2, ref Vector3 hit) {
            if ((fDst1 * fDst2) >= 0.0f) return false;
            if (fDst1 == fDst2) return false;
            hit = p1 + (p2 - p1) * (-fDst1 / (fDst2 - fDst1));
            return true;
        }

        static bool InBox(Vector3 hit, Vector3 b1, Vector3 b2, int axis) {
            if (axis == 1 && hit.z > b1.z && hit.z < b2.z && hit.y > b1.y && hit.y < b2.y) return true;
            if (axis == 2 && hit.z > b1.z && hit.z < b2.z && hit.x > b1.x && hit.x < b2.x) return true;
            if (axis == 3 && hit.x > b1.x && hit.x < b2.x && hit.y > b1.y && hit.y < b2.y) return true;
            return false;
        }
    }
}
