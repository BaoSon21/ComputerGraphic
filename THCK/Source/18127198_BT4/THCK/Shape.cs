using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THCK
{
    class Shape
    {
        public enum ShapeType { NONE, CUBE, PYRAMID, PRISMATIC };
        public ShapeType type;// type of shape in enum shapeType
 
        public Color color;
        //Center of 
        public Tuple<double, double, double> center;
        //height
        public double h;
        //length of bottom edge
        public double a;

        //List vertex
        public List<Tuple<double, double, double>> vertex;

        // List draw_vertex
        public List<int> index;

        public Tuple<double, double, double> lastAngle = new Tuple<double, double, double>(0, 0, 0);

        public Shape(ShapeType userType, Color userColor)
        {
            a = h = 2;
            type = userType;
            color = userColor;

            if (type == ShapeType.CUBE)
            {
                
                center = new Tuple<double, double, double>(1, 1, 0);
                //8 vertexs
                vertex = new List<Tuple<double, double, double>>();
                vertex.Add(new Tuple<double, double, double>(0, 0, 0));
                vertex.Add(new Tuple<double, double, double>(0, 2, 0));
                vertex.Add(new Tuple<double, double, double>(2, 2, 0));
                vertex.Add(new Tuple<double, double, double>(2, 0, 0));
                vertex.Add(new Tuple<double, double, double>(0, 0, h));
                vertex.Add(new Tuple<double, double, double>(0, 2, h));
                vertex.Add(new Tuple<double, double, double>(2, 2, h));
                vertex.Add(new Tuple<double, double, double>(2, 0, h));

                //bot,top,left,right,front,back
                index = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 5, 4, 2, 3, 7, 6, 1, 2, 6, 5, 0, 3, 7, 4 };
            }
            else if (type == ShapeType.PYRAMID)
            {
    
                center = new Tuple<double, double, double>(1, 3, 0);
                //5 vertexs
                vertex = new List<Tuple<double, double, double>>();
                vertex.Add(new Tuple<double, double, double>(1, 3, h));
                vertex.Add(new Tuple<double, double, double>(0, 2, 0));
                vertex.Add(new Tuple<double, double, double>(0, 4, 0));
                vertex.Add(new Tuple<double, double, double>(2, 4, 0));
                vertex.Add(new Tuple<double, double, double>(2, 2, 0));

                //bot,left,right,back,front
                index = new List<int>() { 1, 2, 3, 4, 0, 1, 2, 0, 3, 4, 0, 2, 3, 0, 4, 1 };
            }
            else if (type == ShapeType.PRISMATIC)
            {
                //6 vertexs
                center = new Tuple<double, double, double>(3, a * Math.Sqrt(3) / 6, 0);

                vertex = new List<Tuple<double, double, double>>();
                vertex.Add(new Tuple<double, double, double>(2, 0, 0));
                vertex.Add(new Tuple<double, double, double>(3, 2 * Math.Sqrt(3) / 2, 0));
                vertex.Add(new Tuple<double, double, double>(4, 0, 0));
                vertex.Add(new Tuple<double, double, double>(2, 0, h));
                vertex.Add(new Tuple<double, double, double>(3, 2 * Math.Sqrt(3) / 2, h));
                vertex.Add(new Tuple<double, double, double>(4, 0, h));

                //bot,top,left,right,front
                index = new List<int>() { 0, 1, 2, 3, 4, 5, 0, 1, 4, 3, 1, 2, 5, 4, 0, 2, 5, 3 };
            }
        }

        public void Draw(OpenGL gl)
        {
            gl.Color(color.R, color.G, color.B, (byte)(50));

            if (type == ShapeType.CUBE)
            {
                //draw a cube with all rectangle sides.
                gl.Begin(OpenGL.GL_QUADS);
                for (int i = 0; i < index.Count; i++)
                    gl.Vertex(vertex[index[i]].Item1, vertex[index[i]].Item2, vertex[index[i]].Item3);
                gl.End();
            }
            else if (type == ShapeType.PYRAMID)
            {               
                //draw a pyramid with 1 rectangle bottom + triangle side panels
                gl.Begin(OpenGL.GL_QUADS);
                for (int i = 0; i < 4; i++)
                    gl.Vertex(vertex[index[i]].Item1, vertex[index[i]].Item2, vertex[index[i]].Item3);
                gl.End();

                gl.Begin(OpenGL.GL_TRIANGLES);
                for (int i = 4; i < index.Count; i++)
                    gl.Vertex(vertex[index[i]].Item1, vertex[index[i]].Item2, vertex[index[i]].Item3);
                gl.End();
            }
            else if (type == ShapeType.PRISMATIC)
            {                
                //draw a prismatic with 1 triangle bottom + rectangle side panels
                gl.Begin(OpenGL.GL_TRIANGLES);
                for (int i = 0; i < 6; i++)
                    gl.Vertex(vertex[index[i]].Item1, vertex[index[i]].Item2, vertex[index[i]].Item3);
                gl.End();

                gl.Begin(OpenGL.GL_QUADS);
                for (int i = 6; i < index.Count; i++)
                    gl.Vertex(vertex[index[i]].Item1, vertex[index[i]].Item2, vertex[index[i]].Item3);
                gl.End();
            }
        }

        //draw edges depend on the type
        public void DrawEdge(OpenGL gl, Color lineColor, float lineWidth)
        {           
            gl.Color(lineColor.R, lineColor.G, lineColor.B, lineColor.A);

            if (type == ShapeType.CUBE)
            {              
                List<int> lineIndex = new List<int>() { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7 };

                gl.LineWidth(lineWidth);
                gl.Begin(OpenGL.GL_LINES);
                for (int i = 0; i < lineIndex.Count; i++)
                    gl.Vertex(vertex[lineIndex[i]].Item1, vertex[lineIndex[i]].Item2, vertex[lineIndex[i]].Item3);
                gl.End();
                gl.LineWidth(1.0f);
            }
            else if (type == ShapeType.PYRAMID)
            {                
                List<int> lineIndex = new List<int>() { 1, 2, 2, 3, 3, 4, 4, 1, 0, 1, 0, 2, 0, 3, 0, 4 };

                gl.LineWidth(lineWidth);
                gl.Begin(OpenGL.GL_LINES);
                for (int i = 0; i < lineIndex.Count; i++)
                    gl.Vertex(vertex[lineIndex[i]].Item1, vertex[lineIndex[i]].Item2, vertex[lineIndex[i]].Item3);
                gl.End();
                gl.LineWidth(1.0f);
            }
            else if (type == ShapeType.PRISMATIC)
            {               
                List<int> lineIndex = new List<int>() { 0, 1, 1, 2, 2, 0, 3, 4, 4, 5, 5, 3, 0, 3, 1, 4, 2, 5 };

                gl.LineWidth(lineWidth);
                gl.Begin(OpenGL.GL_LINES);
                for (int i = 0; i < lineIndex.Count; i++)
                    gl.Vertex(vertex[lineIndex[i]].Item1, vertex[lineIndex[i]].Item2, vertex[lineIndex[i]].Item3);
                gl.End();
                gl.LineWidth(1.0f);
            }
        }
    }
}
