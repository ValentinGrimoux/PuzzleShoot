using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.SuperCloner {

    public static class MathUtils {
        
        // Quite complicated stuff to allow looping over instances in any order.

        /// <summary>
        /// From <c>int[] { 3, 4, 5 }</c> returns <c>[1, 3, 12]</c>
        /// </summary>
        public static int[] GetArrayScales(params int[] dimensions) {
            int size = dimensions.Length;
            int scale = 1;
            int[] scales = new int[size];
            for (int i = 0; i < size; i++) {
                scales[i] = scale;
                scale *= dimensions[i];
            }
            return scales;
        }

        /// <summary>
        /// Imagine a array like: [3, 3, 2].<br/>
        /// What could be the "flat" index of the item @ { 1, 2, 1 }?<br/>
        /// "16" returns <c>ComputeIndex(GetArrayScales(3, 3, 2), 1, 2, 1)</c>.<br/>
        /// </summary>
        public static int ComputeIndex(int[] scales, params int[] coords) {
            int index = 0;
            for (int i = 0; i < coords.Length; i++) {
                index += coords[i] * scales[i];
            }
            return index;
        }

        /// <summary>
        /// Inverse operation of ComputeIndex.
        /// </summary>
        public static int[] ComputeCoords(int[] scales, int index) {
            int[] coords = new int[scales.Length];
            for (int i = scales.Length - 1; i >= 0 ; i--) {
                int s = scales[i];
                int x = index / s;
                coords[i] = x;
                index -= s * x;
            }
            return coords;
        }

        /// <summary>
        /// Iterate over dimensions.
        /// </summary>
        public static IEnumerable<int[]> ForCoordsIn(params int[] dimensions) {
            int size = dimensions.Length;
            int max = dimensions.Aggregate(1, (t, x) => t * x);
            int[] coords = new int[size];
            int index = 0;
            while (true) {
                yield return coords;
                if (++index == max) {
                    break;
                }
                int coordIndex = 0;
                while (coords[coordIndex] == dimensions[coordIndex] - 1) {
                    coords[coordIndex] = 0;
                    coordIndex++;
                }
                coords[coordIndex]++;
            }
        }

        /// <summary>
        /// Allow to iterate over indexes, progressing along a given dimension (dimIndex).
        /// </summary>
        public static IEnumerable<IEnumerable<int>> ForIndexesIn(int[] dimensions, int dimIndex) {
            int[] scales = GetArrayScales(dimensions);
            var beforeDimensions = dimensions.Take(dimIndex);
            var afterDimensions = dimensions.Skip(dimIndex + 1);
            var otherDimensions = beforeDimensions.Concat(afterDimensions).ToArray();
            int max = dimensions[dimIndex];
            for (int i = 0; i < max; i++) {
                yield return ForCoordsIn(otherDimensions).Select(coords => {
                    int[] fullCoords = coords.Take(dimIndex).Append(i).Concat(coords.Skip(dimIndex)).ToArray();
                    return ComputeIndex(scales, fullCoords);
                });
            }
        }        
    }

}