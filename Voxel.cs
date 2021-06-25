using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelModule {
    public class Voxel : IEquatable<Voxel> {
        public Color32 Color { get; set; }
        public bool IsDestroyed { get; set; }
        public Voxel TopVoxel { get; set; }
        public Voxel BottomVoxel { get; set; }
        public Voxel FrontVoxel { get; set; }
        public Voxel BackVoxel { get; set; }
        public Voxel RightVoxel { get; set; }
        public Voxel LeftVoxel { get; set; }
        public readonly Vector3 Position;

        public Voxel(Vector3 position, Color32 color) {
            Position = position;
            Color = color;
            IsDestroyed = false;
        }

        public HashSet<Voxel> GetAllConnectedVoxels(bool setIsDestroyed = false) {
            var connectedVoxels = new HashSet<Voxel> {new Voxel(Position, Color)};
            if (setIsDestroyed) IsDestroyed = true;
            foreach (Voxel neighbor in GetActiveNeighbors().Where(neighbor => !connectedVoxels.Contains(neighbor))) {
                connectedVoxels.Add(new Voxel(neighbor.Position, neighbor.Color));
                connectedVoxels.UnionWith(GetConnectedVoxels(neighbor));
                if (setIsDestroyed) neighbor.IsDestroyed = true;
            }
            Debug.Log(connectedVoxels.Count);
            return connectedVoxels;

            HashSet<Voxel> GetConnectedVoxels(Voxel voxel) {
                foreach (Voxel neighbor in voxel.GetActiveNeighbors().Where(neighbor => !connectedVoxels.Contains(neighbor))) {
                    connectedVoxels.Add(new Voxel(neighbor.Position, neighbor.Color));
                    connectedVoxels.UnionWith(GetConnectedVoxels(neighbor));
                    if (setIsDestroyed) neighbor.IsDestroyed = true;
                }
                return connectedVoxels;
            }
        }

        public List<Voxel> GetActiveNeighbors() {
            var adjacentVoxels = new List<Voxel> { TopVoxel, BottomVoxel, FrontVoxel, BackVoxel, RightVoxel, LeftVoxel };
            adjacentVoxels.RemoveAll(voxel => voxel == null || voxel.IsDestroyed);
            return adjacentVoxels;
        }

        public List<Voxel> GetAllNeighbors() {
            var adjacentVoxels = new List<Voxel> { TopVoxel, BottomVoxel, FrontVoxel, BackVoxel, RightVoxel, LeftVoxel };
            adjacentVoxels.RemoveAll(voxel => voxel == null);
            return adjacentVoxels;
        }

        public bool IsSurrounded() {
            return TopVoxel != null && !TopVoxel.IsDestroyed && BottomVoxel != null && !BottomVoxel.IsDestroyed
            && FrontVoxel != null && !FrontVoxel.IsDestroyed && BackVoxel != null && !BackVoxel.IsDestroyed
            && RightVoxel != null && !RightVoxel.IsDestroyed && LeftVoxel != null && !LeftVoxel.IsDestroyed
            && TopVoxel.FrontVoxel != null && !TopVoxel.FrontVoxel.IsDestroyed && TopVoxel.BackVoxel != null && !TopVoxel.BackVoxel.IsDestroyed
            && TopVoxel.LeftVoxel != null && !TopVoxel.LeftVoxel.IsDestroyed && TopVoxel.RightVoxel != null && !TopVoxel.RightVoxel.IsDestroyed
            && BottomVoxel.FrontVoxel != null && !BottomVoxel.FrontVoxel.IsDestroyed && BottomVoxel.BackVoxel != null && !BottomVoxel.BackVoxel.IsDestroyed
            && BottomVoxel.LeftVoxel != null && !BottomVoxel.LeftVoxel.IsDestroyed && BottomVoxel.RightVoxel != null && !BottomVoxel.RightVoxel.IsDestroyed
            && LeftVoxel.FrontVoxel != null && !LeftVoxel.FrontVoxel.IsDestroyed && LeftVoxel.BackVoxel != null && !LeftVoxel.BottomVoxel.IsDestroyed
            && RightVoxel.FrontVoxel != null && !RightVoxel.FrontVoxel.IsDestroyed && RightVoxel.BackVoxel != null && !RightVoxel.BackVoxel.IsDestroyed
            && TopVoxel.FrontVoxel.LeftVoxel != null && !TopVoxel.FrontVoxel.LeftVoxel.IsDestroyed
            && TopVoxel.FrontVoxel.RightVoxel != null && !TopVoxel.FrontVoxel.RightVoxel.IsDestroyed
            && TopVoxel.BackVoxel.LeftVoxel != null && !TopVoxel.BackVoxel.LeftVoxel.IsDestroyed
            && TopVoxel.BackVoxel.RightVoxel != null && !TopVoxel.BackVoxel.RightVoxel.IsDestroyed
            && BottomVoxel.FrontVoxel.LeftVoxel != null && !BottomVoxel.FrontVoxel.LeftVoxel.IsDestroyed
            && BottomVoxel.FrontVoxel.RightVoxel != null && !BottomVoxel.FrontVoxel.RightVoxel.IsDestroyed
            && BottomVoxel.BackVoxel.LeftVoxel != null && !BottomVoxel.BackVoxel.LeftVoxel.IsDestroyed
            && BottomVoxel.BackVoxel.RightVoxel != null && !BottomVoxel.BackVoxel.RightVoxel.IsDestroyed;
        }

        public bool CheckAdjacencyWith(Voxel voxel) {
            return voxel.Equals(TopVoxel) || voxel.Equals(BottomVoxel) || voxel.Equals(FrontVoxel) || voxel.Equals(BackVoxel) || voxel.Equals(RightVoxel) || voxel.Equals(LeftVoxel);
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) return false;
            return ((Voxel)obj).Position == Position;
        }

        public override int GetHashCode() {
            return Position.GetHashCode();
        }

        public bool Equals(Voxel other) {
            return other != null && other.Position == Position;
        }
    }
}
