﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities
{
    public class Vector2
    {
        public float x { get; set; }
        public float y { get; set; }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 Clone()
        {
            return new Vector2(x, y);
        }

        public void Add(float i)
        {
            x += i;
            y += i;
        }

        public void Subtract(float i)
        {
            x -= i;
            y -= i;
        }

        public void Multiply(float i)
        {
            x *= i;
            y *= i;
        }

        public void Divide(float i)
        {
            x /= i;
            y /= i;
        }
    }
}
