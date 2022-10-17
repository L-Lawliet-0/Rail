using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BorderDrawer : MonoBehaviour
{
    public bool DRAW;
    public bool CLEAR;

    public GameObject LinePrefab;

    public Transform HexagonParent;

    public bool Border, ProvinceBorder, CityBorder;

    void Update()
    {
        if (DRAW)
        {
            float outerRadius = 5.7735f;
            float innerRadius = 5f;
            float distance = 10;
            float edgeDistance = 10 * Mathf.Sin(Mathf.PI / 4);
            float sin45 = innerRadius * Mathf.Sin(Mathf.PI / 4);
            int xLength = 831;
            int yLength = 520;

            HashSet<Line> lines = new HashSet<Line>();

            Vector3[] offsets = new Vector3[]
            {
                -Vector3.right * .5f * outerRadius + Vector3.up * innerRadius,
                Vector3.right * .5f * outerRadius + Vector3.up * innerRadius,
                Vector3.right * outerRadius,
                Vector3.right * .5f * outerRadius - Vector3.up * innerRadius,
                -Vector3.right * .5f * outerRadius - Vector3.up * innerRadius,
                -Vector3.right * outerRadius
            };

            float extentBase = 4;
            if (ProvinceBorder)
                extentBase = 2;
            if (CityBorder)
                extentBase = 1;

            for (int i = 0; i < HexagonParent.childCount; i++)
            {
                Transform tran = HexagonParent.GetChild(i);
                if (!tran.name.Equals("sea")) // only draw edge on non-sea hexagon
                {
                    int x = i / xLength;
                    int y = i % xLength;

                    // get the six hex index
                    int[] indexs = new int[6];
                    indexs[0] = (x + 1) * xLength + y;
                    indexs[1] = (x - 1) * xLength + y;
                    if (y % 2 == 0)
                    {
                        indexs[2] = x * xLength + y + 1;
                        indexs[3] = (x - 1) * xLength + y + 1;
                        indexs[4] = x * xLength + y - 1;
                        indexs[5] = (x - 1) * xLength + y - 1;
                    }
                    else
                    {
                        indexs[2] = (x + 1) * xLength + y + 1;
                        indexs[3] = x * xLength + y + 1;
                        indexs[4] = (x + 1) * xLength + y - 1;
                        indexs[5] = x * xLength + y - 1;
                    }

                    Vector3[] vertices = new Vector3[offsets.Length];
                    for (int j = 0; j < vertices.Length; j++)
                        vertices[j] = tran.position + offsets[j];

                    bool isEdge = false;
                    string s1 = tran.name;
                    for (int j = 0; j < indexs.Length; j++)
                    {
                        string s2 = HexagonParent.GetChild(indexs[j]).name;
                        if ((Border && CheckBorder(s1, s2)) || (CityBorder && CheckCityBorder(s1,s2)) || (ProvinceBorder && CheckProvinceBorder(s1,s2))) 
                        {
                            Line line;
                            // calculate edge position
                            if (j == 0)
                                line = new Line(vertices[0], vertices[1], extentBase);
                            else if (j == 1)
                                line = new Line(vertices[4], vertices[3], extentBase);
                            else if (j == 2)
                                line = new Line(vertices[1], vertices[2], extentBase);
                            else if (j == 3)
                                line = new Line(vertices[2], vertices[3], extentBase);
                            else if (j == 4)
                                line = new Line(vertices[0], vertices[5], extentBase);
                            else
                                line = new Line(vertices[5], vertices[4], extentBase);

                            lines.Add(line);
                        }
                    }

                    MeshRenderer mr = tran.GetComponent<MeshRenderer>();
                    if (mr)
                        mr.enabled = false;
                }
            }

            foreach (Line line in lines)
            {
                LineRenderer lr = Instantiate(LinePrefab, transform).GetComponent<LineRenderer>();
                lr.transform.position = line.v1;
                lr.positionCount = 2;
                lr.SetPositions(new Vector3[] { line.v1, line.v2 });
            }

            DRAW = false;
        }

        if (CLEAR)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            CLEAR = false;
        }
    }

    private class Line
    {
        public Vector3 v1, v2;
        public Line(Vector3 v1, Vector3 v2, float extentBase)
        {
            float extent = extentBase / 2 / Mathf.Sqrt(3) * 2;
            this.v1 = v1;
            this.v2 = v2;

            this.v1 += (v1 - v2).normalized * extent;
            this.v2 += (v2 - v1).normalized * extent;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Line))
                return false;
            Line other = (Line)obj;
            return (v1.Equals(other.v1) && v2.Equals(other.v2)) || (v1.Equals(other.v2) && v2.Equals(other.v1));
        }

        public override int GetHashCode()
        {
            return (int)v1.magnitude + (int)v2.magnitude;
        }
    }

    public bool CheckBorder(string s1, string s2)
    {
        return s2.Equals("sea");
    }

    public bool CheckProvinceBorder(string s1, string s2)
    {
        // dont over draw previous border
        return !s2.Equals("sea") && !s1.Split(',')[0].Equals(s2.Split(',')[0]);
    }

    public bool CheckCityBorder(string s1, string s2)
    {
        return !s2.Equals("sea") /*&& s1.Split(',')[0].Equals(s2.Split(',')[0])*/ && !s1.Split(',')[1].Equals(s2.Split(',')[1]);
    }
}
