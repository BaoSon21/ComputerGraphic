using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THGK
{
    class Shape
    {
        public enum shapeType { NONE, LINE, CIRCLE, RECTANGLE, ELLIPSE, TRIANGLE, PENTAGON, HEXAGON, POLYGON }
        public shapeType type;// type of shape in enum shapeType
        
       
        Color color; //color edge
        float width; //witdth of edge

        public bool isColored; //shape colored or not
        public Color fillColor;//fill color

        //List controlPoints
        public List<Point> controlPoints;
        //List points
        public List<Point> listPoints;
        
        public Point extraPoint; //control point use to rotate and scale
        //Center point
        public Tuple<double, double> centerPoint;

        //List fillpoints
        public List<Point> fillPoints;

        public Shape(Color userColor, float userWidth, shapeType userType)
        {
            color = userColor;
            width = userWidth;
            type = userType;
            listPoints = new List<Point>();
            controlPoints = new List<Point>();
            extraPoint = new Point(-1, -1);
            isColored = false;
            fillColor = Color.Black;
            fillPoints = new List<Point>();
        }

        //function draw every pixel
        public void Draw(OpenGL gl)
        { 
            gl.Color(color.R, color.G, color.B, color.A);
            gl.PointSize(width);
            //add Vertex drawpoint
            gl.Begin(OpenGL.GL_POINTS);
            for (int i = 0; i < listPoints.Count; i++)
                gl.Vertex(listPoints[i].X, gl.RenderContextProvider.Height - listPoints[i].Y);
            gl.End();
        }

        //function fill every pixel
        public void Fill(OpenGL gl)
        {
            gl.Color(fillColor.R, fillColor.G, fillColor.B);
            gl.Begin(OpenGL.GL_POINTS);
            for (int j = 0; j < fillPoints.Count; j++)
                gl.Vertex(fillPoints[j].X, gl.RenderContextProvider.Height - fillPoints[j].Y);
            gl.End();
        }

        //function using to clone a shape
        public Shape Clone()
        {
         
            Shape clone = new Shape(color, width, type);

            for (int i = 0; i < controlPoints.Count; i++)
                clone.controlPoints.Add(new Point(controlPoints[i].X, controlPoints[i].Y));

            for (int i = 0; i < listPoints.Count; i++)
                clone.listPoints.Add(new Point(listPoints[i].X, listPoints[i].Y));

            clone.extraPoint = new Point(extraPoint.X, extraPoint.Y);
            clone.centerPoint = new Tuple<double, double>(centerPoint.Item1, centerPoint.Item2);

            clone.isColored = isColored;
            clone.fillColor = fillColor;

            return clone;
        }

    }

}
