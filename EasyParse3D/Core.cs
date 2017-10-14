﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyParse3D
{
    public class ParseObj
    {
        /* Object file name */
        public String filename;

        public int triCount;

        /* Vertices, normals, and triangles */
        public List<Vertex3> vertices = new List<Vertex3> { };
        public List<Normal3> normals = new List<Normal3> { };
        public List<Poly3> tris = new List<Poly3> { };

        public ParseObj(String filename)
        {
            this.filename = filename;

            /* Read file and make sure null is returned. */
            String[] fileLines = readFile();
            if (fileLines != null)
            {
                /* Parse lines given by file */
                parseLines(fileLines);
            }

            this.triCount = tris.Count;
        }

        public List<DLVertex3> getVertices(Poly3 Tri)
        {
            List<DLVertex3> DLVertices = new List<DLVertex3> { };
            for (int i = 0; i < 3; i++)
            {
                Vertex3 Vtx = vertices[Tri.verts[i]];
                Normal3 Norm = normals[Vtx.Normal];

                DLVertices.Add(new DLVertex3(Vtx, Norm));
            }

            return DLVertices;
        }

        private String[] readFile()
        {
            try
            {
                return System.IO.File.ReadAllLines(this.filename);
            }
            catch (System.IO.FileNotFoundException e)
            {
                Console.WriteLine("ERROR: File not found \'" + e.FileName + "\'");
                return null;
            }
        }


        private void parseLines(String[] Lines)
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                /* If there is no data on this line, skip the current iteraton */
                if (Lines[i] == null)
                    continue;

                /* Split current line into arguments */
                String[] Args = Lines[i].Split(' ');
                switch (Args[0])
                {
                    case "v":
                        parseVertex(Args);
                        break;
                    case "vn":
                        parseNormal(Args);
                        break;
                    case "f":
                        parseFace(Args);
                        break;
                }
            }
        }


        private void parseVertex(String[] Pos)
        {
            Vertex3 Vtx = new Vertex3(
                System.Convert.ToDecimal(Pos[1]),
                System.Convert.ToDecimal(Pos[2]),
                System.Convert.ToDecimal(Pos[3]));
            vertices.Add(Vtx);
        }

        private void parseNormal(String[] Pos)
        {
            Normal3 Norm = new Normal3(
                System.Convert.ToDecimal(Pos[1]),
                System.Convert.ToDecimal(Pos[2]),
                System.Convert.ToDecimal(Pos[3]));
            normals.Add(Norm);
        }

        private void parseFace(String[] Args)
        {
            String[] Vtxs = new String[3];
            String[] Norms = new String[3];

            Poly3 Face = new Poly3(new int[3]);

            for (int i = 0; i < 3; i++)
            {
                /* Split //'s */
                Vtxs[i] = Args[i + 1].Split('/')[0];
                Norms[i] = Args[i + 1].Split('/')[2];

                /* Convert indices to ints */
                int VtxIndex = System.Convert.ToInt16(Vtxs[i]) - 1;
                int NormIndex = System.Convert.ToInt16(Norms[i]) - 1;

                /* New face */
                Face.verts[i] = VtxIndex;

                /* Set Normal */
                vertices[VtxIndex].SetNormal(NormIndex);
            }

            tris.Add(Face);
        }
    }

    public class DLVertex3
    {
        public int vertPosX, vertPosY, vertPosZ, normPosX, normPosY, normPosZ;

        public DLVertex3(Vertex3 Vert, Normal3 Norm)
        {
            this.vertPosX = Vert.posX;
            this.vertPosY = Vert.posY;
            this.vertPosZ = Vert.posZ;

            this.normPosX = Norm.posX;
            this.normPosY = Norm.posY;
            this.normPosZ = Norm.posZ;
        }

        public override String ToString()
        {
            return "{ " + this.vertPosX + ", "
                              + this.vertPosY + ", "
                              + this.vertPosZ + ", 0, s, t, "
                              + this.normPosX + ", "
                              + this.normPosY + ", "
                              + this.normPosZ + ", 0xFF },";
        }
    }

    public class Vertex3
    {
        /* Vertex position */
        public int posX, posY, posZ;

        /* Corresponding normal index */
        public int Normal;

        public Vertex3(Decimal posX, Decimal posY, Decimal posZ)
        {
            this.posX = Convert.ToInt16(posX);
            this.posY = Convert.ToInt16(posY);
            this.posZ = Convert.ToInt16(posZ);

            this.Normal = 0;
        }

        public void SetNormal(int Normal)
        {
            this.Normal = Normal;
        }
    }

    public class Normal3
    {
        /* Vertex position */
        public int posX, posY, posZ;

        public Normal3(Decimal posX, Decimal posY, Decimal posZ)
        {
            this.posX = 0;
            this.posY = 0;
            this.posZ = 0;

            /* Convert decimals to precise ints */
            ToIntWithPrecision(posX, posY, posZ);
        }

        private void ToIntWithPrecision(Decimal posX, Decimal posY, Decimal posZ)
        {
            /* Are zero booleans */
            bool posXZero = (posX == 0);
            bool posYZero = (posY == 0);
            bool posZZero = (posZ == 0);

            Decimal XAbs = Math.Abs(posX);
            Decimal YAbs = Math.Abs(posY);
            Decimal ZAbs = Math.Abs(posZ);

            Decimal[] AbsArray = new Decimal[3] { XAbs, YAbs, ZAbs };

            if (posXZero & posYZero & posZZero)
            {
                /* Values already set to 0, return */
                return;
            }
            else
            {
                /* Get a ratio, and keep the sign by using the absolute value */
                Decimal MaxAbs = AbsArray.Max();
                Decimal Ratio = 127 / MaxAbs;

                /* Multiply all coordinates by the ratio and round. */
                this.posX = Convert.ToInt16(Math.Round(Ratio * posX));
                this.posY = Convert.ToInt16(Math.Round(Ratio * posY));
                this.posZ = Convert.ToInt16(Math.Round(Ratio * posZ));
            }
        }
    }

    public class Poly3
    {
        /* Vertex indices */
        public int[] verts;

        public Poly3(int[] verts)
        {
            this.verts = verts;
        }
    }

}
