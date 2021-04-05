using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THCK
{
    class AffineTransform
    {
        List<double> transformMatrix;

        public AffineTransform()
        {
            //create unit matrix
            transformMatrix = new List<double> { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        }


        public void LoadIdentity()
        {
            //replaces the current matrix with the identity matrix
            transformMatrix = new List<double> { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        }

        
        public void Multiply(List<double> matrix)
        {
            //multip current matrix to other matrix
            List<double> retMatrix = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 4; k++)
                        retMatrix[i * 4 + j] += matrix[i * 4 + k] * transformMatrix[k * 4 + j];
            transformMatrix = retMatrix;
        }

        public void Translate(double dx, double dy, double dz)
        {         
            //Create transliteration matrix
            List<double> transformMatrix = new List<double> { 1, 0, 0, dx, 0, 1, 0, dy, 0, 0, 1, dz, 0, 0, 0, 1 };
            //Multip to current matrix
            Multiply(transformMatrix);
        }

        public void RotateX(double phi)
        {
            
            //The function creates a matrix around the x-axis
            phi = phi * (Math.PI) / 180;

            double cosPhi = Math.Cos(phi), sinPhi = Math.Sin(phi);
            List<double> transformMatrix = new List<double> { 1, 0, 0, 0, 0, cosPhi, -sinPhi, 0, 0, sinPhi, cosPhi, 0, 0, 0, 0, 1 };
            //Multip to current matrix
            Multiply(transformMatrix);
        }

        public void RotateY(double phi)
        {           
            //The function creates a matrix around the y-axis
            phi = phi * (Math.PI) / 180;

            double cosPhi = Math.Cos(phi), sinPhi = Math.Sin(phi);
            List<double> transformMatrix = new List<double> { cosPhi, 0, sinPhi, 0, 0, 1, 0, 0, -sinPhi, 0, cosPhi, 0, 0, 0, 0, 1 };
            //Multip to current matrix
            Multiply(transformMatrix);
        }

        public void RotateZ(double phi)
        {            
            //The function creates a matrix around the z-axis
            phi = phi * (Math.PI) / 180;

            double cosPhi = Math.Cos(phi), sinPhi = Math.Sin(phi);
            List<double> transformMatrix = new List<double> { cosPhi, -sinPhi, 0, 0, sinPhi, cosPhi, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            //Multip to current matrix
            Multiply(transformMatrix);
        }

        public void Scale(double sx, double sy, double sz)
        {
            //Create scale matrix
            List<double> transformMatrix = new List<double> { sx, 0, 0, 0, 0, sy, 0, 0, 0, 0, sz, 0, 0, 0, 0, 1 };
            //Multip to current matrix
            Multiply(transformMatrix);
        }

        public Tuple<double, double, double> Transform(Tuple<double, double, double> p)
        {
            // Transform a point
            List<double> oriPoint = new List<double> { p.Item1, p.Item2, p.Item3, 1.0 };
            List<double> retPoint = new List<double> { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    retPoint[i] += transformMatrix[i * 4 + j] * oriPoint[j];
            return new Tuple<double, double, double>(retPoint[0], retPoint[1], retPoint[2]);
        }
    }
}

