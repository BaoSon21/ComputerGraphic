using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace THCK
{
    public partial class Form1 : Form
    {
        Camera camera = new Camera();
        Shape cube = new Shape(Shape.ShapeType.CUBE, Color.White);
        Shape pyramid = new Shape(Shape.ShapeType.PYRAMID, Color.Red);
        Shape prismatic = new Shape(Shape.ShapeType.PRISMATIC, Color.Aqua);

        Texture texture = new Texture();
        bool EnableTextureCube = false;
        bool EnableTexturePyramid = false;
        AffineTransform affine = new AffineTransform();
        double affX, affY, affZ; //Variable parameter storage variables
        double divAffX, divAffY, divAffZ; //The variable saves the division for each transformation
        double frames, count = 0; //The variable stores the number of frames, and the counter indicates when the transformation is finished

        bool isTransform = false; //Check is tranfrom 

        bool skip = false; //skip time division when selecting scale mode
        Color userColor = Color.White;

        private void btColor(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                userColor = colorDialog1.Color;
                if (choosingShape == Shape.ShapeType.CUBE)
                {
                    cube.color = userColor;
                    EnableTextureCube = false;
                }
                else if (choosingShape == Shape.ShapeType.PYRAMID)
                {
                    pyramid.color = userColor;
                    EnableTexturePyramid = false;
                }
                else if (choosingShape == Shape.ShapeType.PRISMATIC)
                    prismatic.color = userColor;
            }
        }        
        // reset all shape to normal
        private void btReset(object sender, EventArgs e)
        {
            cube = new Shape(Shape.ShapeType.CUBE, Color.White);
            pyramid = new Shape(Shape.ShapeType.PYRAMID, Color.Red);
            prismatic = new Shape(Shape.ShapeType.PRISMATIC, Color.Aqua);
            EnableTextureCube = false;
            EnableTexturePyramid = false;
        }

        //select textutre button
        private void btTexture(object sender, EventArgs e)
        {        
            //get image texture
            openFileDialog1.ShowDialog();

            OpenGL gl = openGLControl1.OpenGL;

            //enable mode texture
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            texture.Bind(gl);
            if (choosingShape == Shape.ShapeType.CUBE)
            {
                EnableTextureCube = true;
                EnableTexturePyramid = false;
            }
            else if (choosingShape == Shape.ShapeType.PYRAMID)
            {
                EnableTexturePyramid = true;
                EnableTextureCube = false;
            }

            //del all old texture
            texture.Destroy(openGLControl1.OpenGL);
            //create new texture
            texture.Create(gl, openFileDialog1.FileName);
            //draw 
            openGLControl1.Invalidate();
            
        }

        private void btTransform(object sender, EventArgs e)
        {
            //Use the affine transformation on the selected shape
            affine.LoadIdentity();

            frames = 30;
            if (Double.TryParse(textBoxX.Text, out affX)) //Check for errors entered
            {
                if (frames != 0)
                    divAffX = affX / frames;
                else
                    divAffX = affX;
            }
            else
                divAffX = 0;

            if (Double.TryParse(textBoxY.Text, out affY))
            {
                if (frames != 0)
                    divAffY = affY / frames;
                else
                    divAffY = affY;
            }
            else
                divAffY = 0;

            if (Double.TryParse(textBoxZ.Text, out affZ))
            {
                if (frames != 0)
                    divAffZ = affZ / frames;
                else
                    divAffZ = affZ;
            }
            else
                divAffZ = 0;

            isTransform = true;

            if (comboBoxTranform.SelectedIndex == 0) //Move
            {
                affine.Translate(divAffX, divAffY, divAffZ);
            }
            else if (comboBoxTranform.SelectedIndex == 1) //Rotate
            {
                affine.RotateX(divAffX);
                affine.RotateY(divAffY);
                affine.RotateZ(divAffZ);
            }
            else if (comboBoxTranform.SelectedIndex == 2) //Scale
            {
                skip = true;
                affine.Scale(affX, affY, affZ);
            }
        }

        /*
         when use arrow button, it make error to others comboBox, so i use WASD key to replace.
         */
        private void openGLControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z)
            {
                //Z, Move the camera closer to the viewpoint
                camera.zoomIn();
                openGLControl1_Resized(sender, e);
            }
            else if (e.KeyCode == Keys.X)
            {
                //X, Move the camera away from the viewpoint
                camera.zoomOut();
                openGLControl1_Resized(sender, e);
            }
            
            else if (e.KeyCode == Keys.A)
            {
                //A, rotate the camera to the left
                camera.horizontalRotate(10);
                openGLControl1_Resized(sender, e);
            }
            else if (e.KeyCode == Keys.D)
            {
                //D, rotate the camera to the right
                camera.horizontalRotate(-10);
                openGLControl1_Resized(sender, e);
            }
            else if (e.KeyCode == Keys.W)
            {
                //W, rotate the camera to the up
                camera.verticalRotate(-10);
                openGLControl1_Resized(sender, e);
            }
            else if (e.KeyCode == Keys.S)
            {
                //S, rotate the camera to the down
                camera.verticalRotate(10);
                openGLControl1_Resized(sender, e);
            }
            
        }

        private void cbShape_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //Event select shape
            if (cbShape.SelectedIndex == 0)
                choosingShape = Shape.ShapeType.CUBE;
            else if (cbShape.SelectedIndex == 1)
                choosingShape = Shape.ShapeType.PYRAMID;
            else if (cbShape.SelectedIndex == 2)
                choosingShape = Shape.ShapeType.PRISMATIC;
            else
                choosingShape = Shape.ShapeType.NONE;
        }
        
        Shape.ShapeType choosingShape = Shape.ShapeType.NONE;
        private void openGLControl1_OpenGLInitialized(object sender, EventArgs e)
        {
            //Sự kiện "khởi tạo", xảy ra khi chương trình vừa được khởi chạy

            //Lấy đối tượng OpenGL
            OpenGL gl = openGLControl1.OpenGL;

            //Set màu nền (đen)
            gl.ClearColor(0, 0, 0, 1);

            //Xóa toàn bộ drawBoard
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
        }

        private void openGLControl1_Resized(object sender, EventArgs e)
        {
            //Sự kiện "thay đổi kích thước cửa sổ"

            //Lấy đối tượng OpenGL
            OpenGL gl = openGLControl1.OpenGL;

            //Set viewport theo kích thước cửa sổ
            gl.Viewport(0, 0, openGLControl1.Width, openGLControl1.Height);

            //Set ma trận chiếu
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(60.0, (double)(openGLControl1.Width) / openGLControl1.Height, 1.0, 100.0);

            //Set ma trận model view
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
            gl.LookAt(
                     camera.viewX, camera.viewY, camera.viewZ,
                     0, 0, 0,
                     0, 0, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            //this.renderShape();
            //this.drawGrid(gl);
            //gl.Flush();
        }

        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs args)
        {
            //The "draw" event, which occurs continuously, is repeated infinitely
            OpenGL gl = openGLControl1.OpenGL;
            //Clear drawBoard
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            //Vẽ quang cảnh
            gl.LineWidth(3.0f);
            gl.Begin(OpenGL.GL_LINES);
            //Mặt phẳng đáy
            for (int i = 0; i <= 14; i++)
            {
                if (i == 0 || i == 7 || i == 14)
                    gl.Color(1.0f, 1.0f, 1.0f, 1.0f);
                else
                    gl.Color(0.5f, 0.5f, 0.5f, 1.0f);

                gl.Vertex(-14.0f + 2 * i, -14.0f, 0.0f);
                gl.Vertex(-14.0f + 2 * i, 14.0f, 0.0f);
                gl.Vertex(-14.0f, -14.0f + 2 * i, 0.0f);
                gl.Vertex(14.0f, -14.0f + 2 * i, 0.0f);
            }
            //Trục Ox
            gl.Color(1.0f, 0.0f, 0.0f, 1.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(10.0f, 0.0f, 0.0f);
            //Trục Oy
            gl.Color(0.0f, 1.0f, 0.0f, 1.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 10.0f, 0.0f);
            //Trục Oz
            gl.Color(0.0f, 0.0f, 1.0f, 1.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 10.0f);
            gl.End();
            gl.LineWidth(1.0f);

            //Affine transformation
            if (isTransform)
            {
                if (choosingShape == Shape.ShapeType.CUBE)
                {
                    for (int i = 0; i < cube.vertex.Count; i++)
                        cube.vertex[i] = affine.Transform(cube.vertex[i]);
                }
                
				else if (choosingShape == Shape.ShapeType.PYRAMID)
				{
					for (int i = 0; i < pyramid.vertex.Count; i++)
						pyramid.vertex[i] = affine.Transform(pyramid.vertex[i]);
				}
				else if (choosingShape == Shape.ShapeType.PRISMATIC)
				{
					for (int i = 0; i < prismatic.vertex.Count; i++)
						prismatic.vertex[i] = affine.Transform(prismatic.vertex[i]);
				}
                
                if (skip) //Ignore the resize time
                {
                    isTransform = false;
                    skip = false;
                }

                count++; //Calculate when to stop the transformation
                if (count >= frames) //When adding the divisions again is greater than the first parameter
                {
                    count = 0;
                    isTransform = false;
                }
            }

            //draw shapes
            if (EnableTextureCube == true)
            {
                Texturing texturing = new Texturing();
                texturing.BlindTexture(cube, texture, gl);
            }
            else
            {
                cube.Draw(gl);
            }
            
            
			if (EnableTexturePyramid == true)
			{
				Texturing texturing = new Texturing();
				texturing.BlindTexture(pyramid, texture, gl);
			}
			else
			{
				pyramid.Draw(gl);
			}
            
            prismatic.Draw(gl);

            //Highlight the edge of the currently selected block

            if (choosingShape == Shape.ShapeType.CUBE)
            {
                cube.DrawEdge(gl, Color.Orange, 4.0f);
                pyramid.DrawEdge(gl, Color.Black, 1.0f);
                prismatic.DrawEdge(gl, Color.Black, 1.0f);
            }
            
			else if (choosingShape == Shape.ShapeType.PYRAMID)
			{
				cube.DrawEdge(gl, Color.Black, 1.0f);
				pyramid.DrawEdge(gl, Color.Orange, 4.0f);
				prismatic.DrawEdge(gl, Color.Black, 1.0f);
			}
			else if (choosingShape == Shape.ShapeType.PRISMATIC)
			{
				cube.DrawEdge(gl, Color.Black, 1.0f);
				pyramid.DrawEdge(gl, Color.Black, 1.0f);
				prismatic.DrawEdge(gl, Color.Orange, 4.0f);
			}
			else
			{
				cube.DrawEdge(gl, Color.Black, 1.0f);
				pyramid.DrawEdge(gl, Color.Black, 1.0f);
				prismatic.DrawEdge(gl, Color.Black, 1.0f);
			}           
            gl.Flush();
            
        }

        public Form1()
        {
            InitializeComponent();
        }


    }
}
