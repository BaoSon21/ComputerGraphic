using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THGK
{
    class AffineTransformation
    {
        //Tranformation Matrix Affine
        List<double> transformMatrix;

        public AffineTransformation()
        {
            //create unit matrix
            transformMatrix = new List<double> { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        }

        //multip current matrix to other matrix
        public void Multiply(List<double> matrix)
        {
            List<double> retMatrix = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        retMatrix[i * 3 + j] += matrix[i * 3 + k] * transformMatrix[k * 3 + j];
            transformMatrix = retMatrix;
        }

        //replaces the current matrix with the identity matrix
        public void LoadIdentity()
        {
            transformMatrix = new List<double> { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        }

        //transliteration matrix
        public void Translate(double dx, double dy)
        {
            //Create transliteration matrix
            List<double> transformMatrix = new List<double> { 1, 0, dx, 0, 1, dy, 0, 0, 1 };
            //Multip to current matrix
            Multiply(transformMatrix);
        }
               
        public void Scale(double sx, double sy)
        {
            //Create scale matrix
            List<double> transformMatrix = new List<double> { sx, 0, 0, 0, sy, 0, 0, 0, 1 };
            //Multip to current matrix
            Multiply(transformMatrix);
        }

        public void Rotate(double phi)
        {
            //Creata rotate matrix
            double cosPhi = Math.Cos(phi), sinPhi = Math.Sin(phi);
            List<double> rotateMatrix = new List<double> { cosPhi, -sinPhi, 0, sinPhi, cosPhi, 0, 0, 0, 1 };
            //Multip to current matrix
            Multiply(rotateMatrix);
        }

        // Transform a point
        public Point Transform(Point p)
        {
            List<double> oriPoint = new List<double> { p.X, p.Y, 1.0 };
            List<double> retPoint = new List<double> { 0, 0, 0 };
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    retPoint[i] += transformMatrix[i * 3 + j] * oriPoint[j];
            return new Point((int)(Math.Round(retPoint[0])), (int)(Math.Round(retPoint[1])));
        }
    }
}
