using SharpGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace THGK
{
    public partial class Form1 : Form
    {
        bool isDrawing = false;
        bool isPolygonDrawing = false;
        bool isTransforming = false;

        Color userColor = Color.White;
        float userWidth = 1.0f;

        Shape.shapeType userType = Shape.shapeType.NONE;
        Point pStart = new Point(0, 0), pEnd = new Point(0, 0);
        List<Shape> shapes = new List<Shape>();
        bool isShapesChanged = true;
        const double epsilon = 50.0;
        int choosingShape = -1;
        Shape backupShape;
        int choosingRaster = -1;
        int choosingControl = -2;
        int rotateOrScale = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void openGLControl1_Resized(object sender, EventArgs e)
        {
           
            OpenGL gl = openGLControl1.OpenGL;
            gl.MatrixMode(OpenGL.GL_PROJECTION); 
            gl.LoadIdentity();
            gl.Viewport(0, 0, openGLControl1.Width, openGLControl1.Height);
            gl.Ortho2D(0, openGLControl1.Width, 0, openGLControl1.Height);
            isShapesChanged = true;
        }

        private void openGLControl1_OpenGLDraw(object sender, SharpGL.RenderEventArgs args)
        {
            OpenGL gl = openGLControl1.OpenGL;
            if (isShapesChanged)
            {
                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                if (shapes.Count > 0)
                {
                    for (int i = 0; i < shapes.Count - 1; i++)
                    {
                        shapes[i].Draw(gl);

                        if (shapes[i].fillColor != Color.Black)
                            shapes[i].Fill(gl);
                    }
                    //calculate time to draw shape
                    Stopwatch watch = Stopwatch.StartNew();
                    shapes.Last().Draw(gl);
                    watch.Stop();
                    label2.Text = watch.ElapsedTicks.ToString() + " ticks";

                    if (shapes.Last().fillColor != Color.Black)
                        shapes.Last().Fill(gl);
                }

                gl.Flush();
                isShapesChanged = false;
                // draw control point of shape is selected again
                if (choosingShape >= 0)
                {
                    gl.PointSize(5.0f);
                    gl.Begin(OpenGL.GL_POINTS);
                    //Control point
                    gl.Color(230.0, 230.0, 0);
                    for (int i = 0; i < shapes[choosingShape].controlPoints.Count; i++)
                        gl.Vertex(shapes[choosingShape].controlPoints[i].X, gl.RenderContextProvider.Height - shapes[choosingShape].controlPoints[i].Y);

                    //Extrapoint (rotate/scale point)
                    gl.Color(0, 100.0, 100.0);
                    gl.Vertex(shapes[choosingShape].extraPoint.X, gl.RenderContextProvider.Height - shapes[choosingShape].extraPoint.Y);
                    gl.End();
                    gl.Flush();
                }
            }
        }

        private void openGLControl1_OpenGLInitialized(object sender, EventArgs e)
        {
            OpenGL gl = openGLControl1.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
        }

        private void openGLControl1_MouseDown(object sender, MouseEventArgs e)
        {
            //get location of mouse click
            pStart = pEnd = e.Location;
            //click left mouse =>draw new shape or select other shape
            if (e.Button == MouseButtons.Left)
            {
                //If userType is NONE => select shape
                if (userType == Shape.shapeType.NONE)
                {
                    Thread thread = new Thread
                    (
                        delegate ()
                        {
                            //affine transform shape selected
                            if (choosingShape >= 0)
                            {
                                //get control point near cursor
                                double minDistance = 99999999999999999999999.0;
                                int closestControl = -2;

                                int dx, dy;
                                double distance;

                                for (int j = 0; j < shapes[choosingShape].controlPoints.Count; j++)
                                {
                                    dx = shapes[choosingShape].controlPoints[j].X - e.Location.X;
                                    dy = shapes[choosingShape].controlPoints[j].Y - e.Location.Y;
                                    distance = dx * dx + dy * dy;

                                    if (distance < minDistance)
                                    {
                                        minDistance = distance;
                                        closestControl = j;
                                    }
                                }

                                dx = shapes[choosingShape].extraPoint.X - e.Location.X;
                                dy = shapes[choosingShape].extraPoint.Y - e.Location.Y;
                                distance = dx * dx + dy * dy;

                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    closestControl = -1;
                                }

                                // select point have min distance < epsilon
                                if (minDistance <= epsilon)
                                {
                                    choosingControl = closestControl;
                                    isTransforming = true;
                                    return;
                                }
                                //opposite, select other shape
                                choosingShape = choosingRaster = -1;
                                choosingControl = -2;
                            }
                            //If not any shape is selecting, select shape again
                            if (choosingShape == -1)
                            {
                                //get control point near cursor
                                double minDistance = 99999999999999999999999.0;
                                int closestRaster = -1;

                                int dx, dy;
                                double distance;

                                for (int i = 0; i < shapes.Count; i++)
                                    for (int j = 0; j < shapes[i].listPoints.Count; j++)
                                    {
                                        dx = shapes[i].listPoints[j].X - e.Location.X;
                                        dy = shapes[i].listPoints[j].Y - e.Location.Y;
                                        distance = dx * dx + dy * dy;

                                        if (distance < minDistance)
                                        {
                                            choosingShape = i;
                                            minDistance = distance;
                                            closestRaster = j;
                                        }
                                    }

                                //select point have min distance < epsilon
                                if (minDistance <= epsilon)
                                {
                                    choosingRaster = closestRaster;
                                    backupShape = shapes[choosingShape].Clone();
                                    isTransforming = true;
                                    isShapesChanged = true;
                                    return;
                                }

                                //opposite, not select any shape
                                choosingShape = choosingRaster = -1;
                                choosingControl = -2;
                                isShapesChanged = true;
                                return;
                            }
                        }
                    );
                    thread.IsBackground = true;
                    thread.Start();
                    return;
                }

                //if userType != NONE => draw new shape
                choosingShape = -1;
                isDrawing = true;

                //If userType != POLYGON => create new shape and add to list shape
                if (userType != Shape.shapeType.POLYGON)
                {
                    shapes.Add(new Shape(userColor, userWidth, userType));
                    shapes.Last().controlPoints.Add(pStart);
                    shapes.Last().controlPoints.Add(pEnd);
                }
                //userType is POLYGON => 2 cases
                else if (userType == Shape.shapeType.POLYGON)
                {
                    // if not drawing, create new shape
                    if (isPolygonDrawing == false)
                    {
                        isPolygonDrawing = true;
                        shapes.Add(new Shape(userColor, userWidth, userType));
                        shapes.Last().controlPoints.Add(pStart);
                        shapes.Last().controlPoints.Add(pEnd);
                    }
                    // if is drawing, add point location mouse to list control points.
                    else
                    {
                        shapes.Last().controlPoints.Add(pEnd);
                    }
                }
            }
            else if (isPolygonDrawing)
                {
                    isDrawing = isPolygonDrawing = false;
                }
                else
                {
                    if (isDrawing)
                    {
                        shapes.RemoveAt(shapes.Count - 1);
                        isDrawing = isPolygonDrawing = false;
                        choosingShape = choosingRaster = -1;
                        choosingControl = -2;
                        isShapesChanged = true;
                    }
                    else
                    {
                        label1.Text = "Mode: Picking";
                        userType = Shape.shapeType.NONE;
                        isDrawing = isPolygonDrawing = false;
                        choosingShape = choosingRaster = -1;
                        choosingControl = -2;
                        isShapesChanged = true;
                        
                    }
                }
            }

 
        private void openGLControl1_MouseMove(object sender, MouseEventArgs e)
        {
            pEnd = e.Location;
            //If is drawing, draw continue with new pEnd coor.
            if (isDrawing)
            {
                Thread thread = new Thread
                (
                    delegate ()
                    {
                        //update last control point with pEnd
                        shapes.Last().controlPoints[shapes.Last().controlPoints.Count - 1] = pEnd;
                        //Clear list point, draw with new list point
                        shapes.Last().listPoints.Clear();

                        switch (userType)
                        {
                            case Shape.shapeType.LINE:
                                DrawingAlgorithm.Line(shapes.Last(), pStart, pEnd);
                                break;
                            case Shape.shapeType.CIRCLE:
                                DrawingAlgorithm.Circle(shapes.Last(), pStart, pEnd);
                                break;
                            case Shape.shapeType.RECTANGLE:
                                DrawingAlgorithm.Rectangle(shapes.Last(), pStart, pEnd);
                                break;
                            case Shape.shapeType.ELLIPSE:
                                DrawingAlgorithm.Ellipse(shapes.Last(), pStart, pEnd);
                                break;
                            case Shape.shapeType.TRIANGLE:
                                DrawingAlgorithm.Triangle(shapes.Last(), pStart, pEnd);
                                break;
                            case Shape.shapeType.PENTAGON:
                                DrawingAlgorithm.Pentagon(shapes.Last(), pStart, pEnd);
                                break;
                            case Shape.shapeType.HEXAGON:
                                DrawingAlgorithm.Hexagon(shapes.Last(), pStart, pEnd);
                                break;
                            case Shape.shapeType.POLYGON:
                                DrawingAlgorithm.Polygon(shapes.Last());
                                break;
                        }
                        isShapesChanged = true;
                    }
                );
                thread.IsBackground = true;
                thread.Start();
            }

            //Use affine transform to list control points
            else if (isTransforming)
            {
                Thread thread = new Thread
                (
                    delegate ()
                    {
                        //turn off fill color
                        if (shapes[choosingShape].isColored == true)
                        {
                            shapes[choosingShape].isColored = false;
                            shapes[choosingShape].fillPoints.Clear();
                        }

                        AffineTransformation transformer = new AffineTransformation();
                        //If select extrapoint(rotate/scale)
                        if (choosingControl == -1)
                        {
                            //Rotate
                            if (rotateOrScale == 0)
                            {
                                //calculate angle rotate
                                Tuple<double, double> vecA = new Tuple<double, double>(backupShape.extraPoint.X - backupShape.centerPoint.Item1, backupShape.extraPoint.Y - backupShape.centerPoint.Item2);
                                Tuple<double, double> vecB = new Tuple<double, double>(pEnd.X - backupShape.centerPoint.Item1, pEnd.Y - backupShape.centerPoint.Item2);
                                double lenA = Math.Sqrt(vecA.Item1 * vecA.Item1 + vecA.Item2 * vecA.Item2);
                                double lenB = Math.Sqrt(vecB.Item1 * vecB.Item1 + vecB.Item2 * vecB.Item2);
                                double phi = Math.Acos((vecA.Item1 * vecB.Item1 + vecA.Item2 * vecB.Item2) / (lenA * lenB));
                                if (vecA.Item1 * vecB.Item2 - vecA.Item2 * vecB.Item1 < 0)
                                    phi = -phi;

                                //rotate step
                                transformer.LoadIdentity();
                                transformer.Translate(-backupShape.centerPoint.Item1, -backupShape.centerPoint.Item2);
                                transformer.Rotate(phi);
                                transformer.Translate(backupShape.centerPoint.Item1, backupShape.centerPoint.Item2);

                                //change list control points
                                for (int i = 0; i < shapes[choosingShape].controlPoints.Count; i++)
                                    shapes[choosingShape].controlPoints[i] = transformer.Transform(backupShape.controlPoints[i]);
                            }
                            //Scale shape
                            else if (rotateOrScale == 1)
                            {
                                //calculate scale coef 
                                Tuple<double, double> vecA = new Tuple<double, double>(backupShape.extraPoint.X - backupShape.centerPoint.Item1, backupShape.extraPoint.Y - backupShape.centerPoint.Item2);
                                Tuple<double, double> vecB = new Tuple<double, double>(pEnd.X - backupShape.centerPoint.Item1, pEnd.Y - backupShape.centerPoint.Item2);
                                double sx = vecB.Item1 / vecA.Item1;
                                double sy = vecB.Item2 / vecA.Item2;
                                double s = Math.Max(sx, sy);

                                //Scale step
                                transformer.LoadIdentity();
                                transformer.Translate(-backupShape.centerPoint.Item1, -backupShape.centerPoint.Item2);
                                transformer.Scale(s, s);
                                transformer.Translate(backupShape.centerPoint.Item1, backupShape.centerPoint.Item2);
                                //change list control points
                                for (int i = 0; i < shapes[choosingShape].controlPoints.Count; i++)
                                    shapes[choosingShape].controlPoints[i] = transformer.Transform(backupShape.controlPoints[i]);
                            }
                        }
                        //select control point-> change coordinates of control point
                        else if (choosingControl >= 0)
                        {
                            shapes[choosingShape].controlPoints[choosingControl] = pEnd;
                        }
                        //select a control point, translation
                        else if (choosingRaster >= 0)
                        {
                            //transliteration
                            transformer.LoadIdentity();
                            transformer.Translate(pEnd.X - pStart.X, pEnd.Y - pStart.Y);
                            //Transform control point
                            for (int i = 0; i < shapes[choosingShape].controlPoints.Count; i++)
                                shapes[choosingShape].controlPoints[i] = transformer.Transform(backupShape.controlPoints[i]);
                        }

                        //delete and create new list points
                        shapes[choosingShape].listPoints.Clear();

                        Point controlPoint0 = shapes[choosingShape].controlPoints[0];
                        Point controlPoint1 = shapes[choosingShape].controlPoints[1];

                        switch (shapes[choosingShape].type)
                        {
                            case Shape.shapeType.LINE:
                                DrawingAlgorithm.Line(shapes[choosingShape], controlPoint0, controlPoint1);
                                break;
                            case Shape.shapeType.CIRCLE:
                                DrawingAlgorithm.Circle(shapes[choosingShape], controlPoint0, controlPoint1);
                                break;
                            case Shape.shapeType.RECTANGLE:
                                DrawingAlgorithm.Polygon(shapes[choosingShape]);
                                break;
                            case Shape.shapeType.ELLIPSE:
                                DrawingAlgorithm.Ellipse(shapes[choosingShape], controlPoint0, controlPoint1);
                                break;
                            case Shape.shapeType.TRIANGLE:
                                DrawingAlgorithm.Polygon(shapes[choosingShape]);
                                break;
                            case Shape.shapeType.PENTAGON:
                                DrawingAlgorithm.Polygon(shapes[choosingShape]);
                                break;
                            case Shape.shapeType.HEXAGON:
                                DrawingAlgorithm.Polygon(shapes[choosingShape]);
                                break;
                            case Shape.shapeType.POLYGON:
                                DrawingAlgorithm.Polygon(shapes[choosingShape]);
                                break;
                        }

                        isShapesChanged = true;
                    }
                );
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void openGLControl1_MouseUp(object sender, MouseEventArgs e)
        {
            //mouseUp, end draw process
            if (userType != Shape.shapeType.POLYGON)
                isDrawing = false;
            
            if (isTransforming == true)
            {
                backupShape = shapes[choosingShape].Clone();
                isTransforming = false;
            }
        }

        private void btCircleClick(object sender, EventArgs e)
        {
            userType = Shape.shapeType.CIRCLE;
            label1.Text = "Mode: Circle";
        }

        private void btRectangleClick(object sender, EventArgs e)
        {
            userType = Shape.shapeType.RECTANGLE;
            label1.Text = "Mode: Rectangle";
        }

        private void btColorClick(object sender, EventArgs e)
        {
            label1.Text = "Mode: Color";
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                userColor = colorDialog1.Color;
                button8.BackColor = colorDialog1.Color;
            }

        }

        private void btEllipseClick(object sender, EventArgs e)
        {
            userType = Shape.shapeType.ELLIPSE;
            label1.Text = "Mode: Ellipse";
        }

        private void btTriangleClick(object sender, EventArgs e)
        {
            userType = Shape.shapeType.TRIANGLE;
            label1.Text = "Mode: Triangle";
        }

        private void btPentagonClick(object sender, EventArgs e)
        {
            userType = Shape.shapeType.PENTAGON;
            label1.Text = "Mode: Pentagon";
        }

        private void btHexagonClick(object sender, EventArgs e)
        {
            userType = Shape.shapeType.HEXAGON;
            label1.Text = "Mode: Hexagon";
        }

        private void btWidthClick(object sender, EventArgs e)
        {
            userWidth += 0.5f;
            if (userWidth > 3)
                userWidth = 1.0f;
            button9.Text = "Width: " + userWidth.ToString();
        }

        private void btClearClick(object sender, EventArgs e)
        {
            shapes.Clear();
            isDrawing = isPolygonDrawing = false;
            choosingShape = choosingControl = choosingRaster = -1;
            isShapesChanged = true;
            
        }

        private void btModeClick(object sender, EventArgs e)
        {
            if (button11.Text == "Rotate")
            {
                button11.Text = "Scale";
                rotateOrScale = 1;
            }
            else
            {
                button11.Text = "Rotate";
                rotateOrScale = 0;
            }
        }

        private void btPolygonClick(object sender, EventArgs e)
        {
            userType = Shape.shapeType.POLYGON;
            label1.Text = "Mode: Polygon";
        }

        private void openGLControl1_MouseClick(object sender, MouseEventArgs e)
        {
            
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                //MessageBox.Show("Right click");

                if (isPolygonDrawing)
                {
                    isDrawing = isPolygonDrawing = false;
                }
                else
                if (isDrawing)
                {
                    shapes.RemoveAt(shapes.Count - 1);
                    isDrawing = isPolygonDrawing = false;
                    choosingShape = choosingRaster = -1;
                    choosingControl = -2;
                    isShapesChanged = true;
                }
                else
                {
                    label1.Text = "Mode: Picking";
                    userType = Shape.shapeType.NONE;
                    isDrawing = isPolygonDrawing = false;
                    choosingShape = choosingRaster = -1;
                    choosingControl = -2;
                    isShapesChanged = true;
                   
                }
            }
        }

        private void btScanFIllClick(object sender, EventArgs e)
        {
            if (shapes[choosingShape].type != Shape.shapeType.CIRCLE && shapes[choosingShape].type != Shape.shapeType.ELLIPSE && shapes[choosingShape].controlPoints.Count < 3)
                return;

            if (choosingShape >= 0 && shapes[choosingShape].fillColor != userColor)
            {
                Thread thread = new Thread
                (
                    () => ScanlineFiller.Fill(shapes[choosingShape], userColor, ref isShapesChanged)
                );
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void btBloodFillClick(object sender, EventArgs e)
        {
            if (choosingShape >= 0 && shapes[choosingShape].fillColor != userColor)
            {
                if (shapes[choosingShape].type != Shape.shapeType.CIRCLE && shapes[choosingShape].type != Shape.shapeType.ELLIPSE && shapes[choosingShape].controlPoints.Count < 3)
                    return;
                
                Thread thread = new Thread(() => FloodFiller.Fill(shapes[choosingShape], userColor, ref isShapesChanged));
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }


        private void btLineClick(object sender, EventArgs e)
        {
            userType = Shape.shapeType.LINE;
            label1.Text = "Mode: Line";
        }
    }
}
