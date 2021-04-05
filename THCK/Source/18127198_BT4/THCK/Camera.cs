using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THCK
{
    class Camera
    {

        static int id = 0;
        public string name;

        public double viewX;
        public double viewY;
        public double viewZ;


        public Camera()
        {
            id++;
            //name = "Camera" + id.ToString();
            viewX = 8;
            viewY = 8;
            viewZ = 8;
        }

        public void zoomIn()
        {
            viewX /= 1.1;
            viewY /= 1.1;
            viewZ /= 1.1;
        }

        public void zoomOut()
        {
            viewX *= 1.1;
            viewY *= 1.1;
            viewZ *= 1.1;
        }

        public void horizontalRotate(double deg)  //horizontal rotation
        {
            // transform to radians
            double radians = deg * Math.PI / 180.0f;
            double oldviewX = viewX, oldviewY = viewY;
            viewX = oldviewX * Math.Cos(radians) - oldviewY * Math.Sin(radians);
            viewY = oldviewX * Math.Sin(radians) + oldviewY * Math.Cos(radians);

        }

        public void verticalRotate(double deg) //vertical rotation
        {            
            //distance 0->camera through Oxy
            double hypoXY = Math.Sqrt(viewX * viewX + viewY * viewY);           
            // distance O->camera
            double hypoXYZ = Math.Sqrt(hypoXY * hypoXY + viewZ * viewZ);           
            // angle create bt Ocamera with Oxy
            double rootAngle = Math.Acos(hypoXY / hypoXYZ); // radians
            double cosX = viewX / hypoXY;
            double cosY = viewY / hypoXY;
            //convert to degree
            double rootDeg = rootAngle * 180 / Math.PI;
            //update rootdeg
            rootDeg += deg;          
            //cal coordinate of camera from new update angle
            double radians = rootDeg * Math.PI / 180;
            hypoXY = hypoXYZ * Math.Cos(radians);
            viewX = hypoXY * cosX;
            viewY = hypoXY * cosY;
            viewZ = hypoXYZ * Math.Sin(radians);
        }

    }
}
