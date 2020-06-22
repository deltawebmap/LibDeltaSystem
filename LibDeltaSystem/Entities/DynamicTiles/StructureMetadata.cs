using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.DynamicTiles
{
    public class StructureMetadata
    {
        public string[] names;
        public string img;
        public float size;
        public string item; //Classname of the item used to place this
        public StructureMetadata_Size image_size;
        public StructureMetadata_Point[] outline;
        public StructureMetadata_Image tile;
    }

    public class StructureMetadata_Image
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }

    public class StructureMetadata_Size
    {
        public int width;
        public int height;
    }

    public class StructureMetadata_Point
    {
        public int x;
        public int y;
    }
}
