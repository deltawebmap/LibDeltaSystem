using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.ArkEntries
{
    /// <summary>
    /// Contains info about the actual Ark map, not the save file.
    /// </summary>
    public class ArkMapEntry
    {
        public string displayName { get; set; } //The name displayed
        public bool isOfficial { get; set; } //Is official map that ships with the game
        public bool isStoryArk { get; set; } //Is a story ark
        public string backgroundColor { get; set; } //Background color around the map. Null if there is no one complete color, such as Extinction

        public float latLonMultiplier { get; set; } //To convert the Lat/Long map coordinates to UE coordinates, simply subtract 50 and multiply by the value
        public WorldBounds2D bounds { get; set; } //Bounds of the map in UE coords

        public Vector2 mapImageOffset { get; set; } //Offset to move the Ark position by in order for it to fit in the center of the image.
        public int captureSize { get; set; } //Size of the captured image, in game units

        public ArkMapDisplayData[] maps { get; set; } //Maps we can display

        /// <summary>
        /// Converts from Ark position to normalized, between (-0.5, 0.5).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Vector2 ConvertFromGamePositionToNormalized(Vector2 input)
        {
            Vector2 o = input.Clone();

            //Translate by the map image offset
            if (mapImageOffset != null)
            {
                o.x += mapImageOffset.x;
                o.y += mapImageOffset.y;
            }

            //Scale by the size of our image
            o.Divide(captureSize);

            //Move
            o.Add(0.5f);

            return o;
        }
    }

    /// <summary>
    /// Contains data about maps we can show
    /// </summary>
    public class ArkMapDisplayData
    {
        public string url { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int maximumZoom { get; set; }
    }

    public class WorldBounds2D
    {
        public int minX { get; set; }
        public int minY { get; set; }

        public int maxX { get; set; }
        public int maxY { get; set; }

        public int width
        {
            get
            {
                return maxX - minX;
            }
        }

        public int height
        {
            get
            {
                return maxY - minY;
            }
        }
    }
}
