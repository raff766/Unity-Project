# Unity-Voxel-Module
Code for converting point cloud data into voxel-based meshes that are destructible.

The conversion portion of this module is completed, however the destruction of the voxels, or rather more specifically the ability for groups of voxels to separate into separate game objects is still being developed. The ability for groups of voxels to detect that they have been separated from another group has already mostly been implemented using the A* algorithm while also utilizing the new Unity Jobs System for much better performance.
