using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtraVehicleEffects
{

    class AudioUtil
    {

        public class AudioInfoPlaybackState
        {
            public AudioInfo info;
            public float volume;
            public float pitch;
            public AudioInfoPlaybackState(AudioInfo info, float volumeWeight, float pitch){ this.info = info; this.volume = volumeWeight; this.pitch = pitch; }
        }

        public class PiecewiseLinear
        {
            public const byte X = 0;
            public const byte Y = 1;
            public float[,] points { get; private set; }
            private float[] slopes;
            private float neutralElement;

            public float value(float param)
            {
                for (int i = 0; i < points.GetLength(0) - 1; ++i)
                    if (param > points[i, X] && param < points[i + 1, X])
                        return points[i, Y] + slopes[i] * (param - points[i, X]);
                return neutralElement;//return if we leave function domain or the point array is empty
            }

            public void setPoint(uint index, float x, float y)
            {
                points[index, X] = x; points[index, Y] = y;
                if (index < slopes.Length)
                    slopes[index] = (points[index + 1, Y] - points[index, Y]) / (points[index + 1, X] - points[index, X]);
                if (index > 0)
                    slopes[index - 1] = (points[index, Y] - points[index - 1, Y]) / (points[index, X] - points[index - 1, X]);
            }

            public PiecewiseLinear(float neutralElement = 1.0f) { this.neutralElement = neutralElement; }
            public PiecewiseLinear(float[,] points, float neutralElement = 1.0f)
            {
                this.points = points;
                this.neutralElement = neutralElement;
                slopes = new float[points.GetLength(0) - 1];
                for (int i = 0; i < points.GetLength(0) - 1; ++i)
                    slopes[i] = (points[i + 1, Y] - points[i, Y]) / (points[i + 1, X] - points[i, X]);
            }
        }


    }
}
