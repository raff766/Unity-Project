using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace VoxelModule {
    static class VoxelDataConverter {
        public static Dictionary<Vector3, Voxel> TextToVoxels(string text) {
            string[] lines = text.Split('\n');
            var voxels = new Dictionary<Vector3, Voxel>();
            bool reachedHeadingEnd = false;
            foreach (string value in lines) {
                if (!reachedHeadingEnd) {
                    if (value.Contains("end_header")) reachedHeadingEnd = true;
                    continue;
                }
                if (value.Length == 0) continue;
                string[] values = value.Split(' ');
                NumberFormatInfo numberFormat = CultureInfo.InvariantCulture.NumberFormat;
                Vector3 position = new Vector3(float.Parse(values[0], numberFormat) * -1, float.Parse(values[2], numberFormat), float.Parse(values[1], numberFormat) * -1);
                Color32 color = new Color32(byte.Parse(values[3]), byte.Parse(values[4]), byte.Parse(values[5]), 255);
                voxels.Add(position, new Voxel(position, color));
            }
            return voxels;
        }
    }
}