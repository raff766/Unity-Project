using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace VoxelModule {
    [RequireComponent(typeof(VoxelMesh))]
    public class VoxelDestructor : MonoBehaviour {
        private VoxelMesh voxelMesh;

        void Start() {
            voxelMesh = GetComponent<VoxelMesh>();
        }

        public void Destroy(params Voxel[] voxels) {
            foreach (Voxel voxel in voxels) {
                voxel.IsDestroyed = true;
            }
            int count = 0;
            foreach (Voxel voxel in voxels) {
                StartCoroutine(DestroyCoroutine(voxel, () => {
                    count++;
                    if (count >= voxels.Length) {
                        voxelMesh.UpdateMesh();
                    }
                }));
            }
        }

        IEnumerator DestroyCoroutine(Voxel voxel, Action onFinished) {
            List<Voxel> neighbors = voxel.GetActiveNeighbors();
            var voxelPairs = new HashSet<HashSet<Voxel>>();
            foreach (Voxel start in neighbors) {
                foreach (Voxel end in neighbors) {
                    var newPair = new HashSet<Voxel> { start, end };
                    if (!voxelPairs.Any(pair => pair.SetEquals(newPair)) && newPair.Count == 2) {
                        voxelPairs.Add(newPair);
                    }
                }
            }
            var alreadyChecked = new HashSet<Voxel>();
            foreach (HashSet<Voxel> pair in voxelPairs) {
                Voxel a = pair.ElementAt(0);
                Voxel b = pair.ElementAt(1);
                if (alreadyChecked.Contains(a) || alreadyChecked.Contains(b)) continue;
                var resultContainerA = new NativeArray<bool>(1, Allocator.TempJob);
                var resultContainerB = new NativeArray<bool>(1, Allocator.TempJob);
                var jobA = new CheckSeparationJob(new Node(a.Position, (a.Position - b.Position).sqrMagnitude, 0), new Node(b.Position, 0, (b.Position - a.Position).sqrMagnitude), voxelMesh.GetInstanceID(), resultContainerA);
                var jobB = new CheckSeparationJob(new Node(b.Position, (a.Position - b.Position).sqrMagnitude, 0), new Node(a.Position, 0, (b.Position - a.Position).sqrMagnitude), voxelMesh.GetInstanceID(), resultContainerB);
                JobHandle handleA = jobA.Schedule();
                JobHandle handleB = jobB.Schedule();
                JobHandle.ScheduleBatchedJobs();
                yield return new WaitUntil(() => handleA.IsCompleted || handleB.IsCompleted);
                bool result;
                if (handleA.IsCompleted) {
                    handleA.Complete();
                    result = resultContainerA[0];
                    resultContainerA.Dispose();
                    if (result) {
                        alreadyChecked.Add(a);
                        HashSet<Voxel> connectedVoxelsToA = a.GetAllConnectedVoxels(true);
                        alreadyChecked.UnionWith(connectedVoxelsToA);
                        if (connectedVoxelsToA.Count > 2) CreateSeparateObject(connectedVoxelsToA);
                    }
                    yield return new WaitUntil(() => handleB.IsCompleted);
                    handleB.Complete();
                    resultContainerB.Dispose();
                } else if (handleB.IsCompleted) {
                    handleB.Complete();
                    result = resultContainerB[0];
                    resultContainerB.Dispose();
                    if (result) {
                        HashSet<Voxel> connectedVoxelsToB = b.GetAllConnectedVoxels(true);
                        alreadyChecked.UnionWith(connectedVoxelsToB);
                        if (connectedVoxelsToB.Count > 2) CreateSeparateObject(connectedVoxelsToB);
                    }
                    yield return new WaitUntil(() => handleA.IsCompleted);
                    handleA.Complete();
                    resultContainerA.Dispose();
                }
            }
            onFinished.Invoke();
        }

        void CreateSeparateObject(IEnumerable<Voxel> voxels) {
            GameObject separateObject = new GameObject();
            VoxelMesh voxelMesh = separateObject.AddComponent<VoxelMesh>();
            voxelMesh.Voxels = voxels.ToDictionary(voxel => voxel.Position);
            separateObject.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
            voxelMesh.CalculateNeighbors();
            voxelMesh.UpdateMesh();
        }

        struct Node : IComparable<Node>, IEquatable<Node> {
            public float FCost => HCost + GCost;
            public float HCost { get; } //distance from ending node
            public float GCost { get; set; } //distance from starting node
            public readonly Vector3 Position;

            public Node(Vector3 position, float hCost, float gCost) {
                Position = position;
                HCost = hCost;
                GCost = gCost;
            }

            public int CompareTo(Node node) {
                if (FCost < node.FCost) return -1;
                return Position.Equals(node.Position) ? 0 : 1;
            }

            public bool Equals(Node other) {
                return Position.Equals(other.Position);
            }
            
            public override int GetHashCode() {
                return Position.GetHashCode();
            }
        }

        struct CheckSeparationJob : IJob {
            public NativeArray<bool> ResultContainer;
            private int voxelMeshID;
            private Node start;
            private Node goal;

            public CheckSeparationJob(Node start, Node goal, int voxelMeshID, NativeArray<bool> resultContainer) {
                this.start = start;
                this.goal = goal;
                this.voxelMeshID = voxelMeshID;
                ResultContainer = resultContainer;
            }

            public void Execute() {
                Dictionary<Vector3, Voxel> voxels = VoxelMesh.VoxelMeshes[voxelMeshID].Voxels;
                var open = new List<Node>();
                var closed = new List<Node>();
                open.Add(start);
                while (open.Count > 0) {
                    Node current = open.Min();
                    open.Remove(current);
                    closed.Add(current);
                    if (current.Equals(goal)) {
                        ResultContainer[0] = false;
                        return;
                    }
                    foreach (Voxel neighborVoxel in voxels[current.Position].GetAllNeighbors().Where(voxel => !voxel.IsSurrounded())) {
                        if (neighborVoxel.Position == goal.Position) {
                            ResultContainer[0] = false;
                            return;
                        }
                        if (neighborVoxel.IsDestroyed) continue;
                        if (closed.Exists(node => node.Position.Equals(neighborVoxel.Position))) continue;
                        Node neighborNode = open.Find(node => node.Position.Equals(neighborVoxel.Position));
                        if (neighborNode.FCost == 0) {
                            open.Add(new Node(neighborVoxel.Position, (neighborVoxel.Position - goal.Position).sqrMagnitude, current.GCost + 10));
                        } else if (neighborNode.GCost > current.GCost + 10) {
                            neighborNode.GCost = current.GCost + 10;
                        }
                    }
                }
                ResultContainer[0] = true;
            }
        }
    }
}
