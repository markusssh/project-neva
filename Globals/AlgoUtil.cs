using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ProjectNeva.Globals;

public partial class AlgoUtil : Node
{
    private static Image FloodFill(
        Image image,
        Vector2I position,
        Color replaceColor,
        Color fillColor)
    {
        //If color is the same, we don't use fill instrument.
        if (replaceColor == fillColor)
            return image;

        var width = image.GetWidth();
        var height = image.GetHeight();
        
        var cR = fillColor.R8;
        var cG = fillColor.G8;
        var cB = fillColor.B8;
        var cA = fillColor.A8;
        
        //R G B A bytes of each pixel one after another.
        var iData = image.GetData();
        var dataSize = iData.Length;
        
        var idx = (position.Y * width + position.X) * 4;
        var idxToCheck = new Queue<int>();
        idxToCheck.Enqueue(idx);
        
        var checkMask = new bool[dataSize];
        checkMask[idx] = true;
        
        //Byte offset for each neighbour
        var neighbours = new List<int>
        {
            4,
            width * 4,
            -4,
            -width * 4
        };

        while (idxToCheck.Count > 0)
        {
            idx = idxToCheck.Dequeue();

            ///////Replace pixel//////
            iData[idx] = (byte)cR;
            iData[idx + 1] = (byte)cG;
            iData[idx + 2] = (byte)cB;
            iData[idx + 3] = (byte)cA;
            /////////////////////////

            foreach (var nIdx in
                     neighbours
                         .Select(neighbour => idx + neighbour)
                         .Where(nIdx => nIdx < dataSize && nIdx >= 0 &&
                                                  !checkMask[nIdx]))
            {
                //Reverse order for uint conversion
                var b = new[] { iData[nIdx + 3], iData[nIdx + 2], iData[nIdx + 1], iData[nIdx]};
                var neighbourColor = new Color(BitConverter.ToUInt32(b));
                if (neighbourColor.IsEqualApprox(replaceColor))
                {
                    idxToCheck.Enqueue(nIdx);
                }
                checkMask[nIdx] = true;
            }
        }

        return Image.CreateFromData(width, height, false, image.GetFormat(), iData);
    }
}