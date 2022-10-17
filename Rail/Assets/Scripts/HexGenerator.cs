using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HexGenerator : MonoBehaviour
{
    public bool GENERATE;
    public bool CLEARALL;
    public float OuterRadius; // this the raidus of the outer circle
    private float InnerRadius;
    public GameObject LinePrefab;

    void Update()
    {
        if (GENERATE)
        {
            // generate hexagons in the center, depends on the square length

            // calculate inner radius 
            InnerRadius = OuterRadius * Mathf.Sqrt(3) / 2f;

            int xCount = (int)(5200f / InnerRadius / 2);
            int yCount = (int)(7200f / OuterRadius / 1.5f);

            // create some hexagon just for test purpose
            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    GameObject obj = Instantiate(LinePrefab);
                    obj.transform.parent = transform;
                    float y = 2 * InnerRadius * i; // the y offset
                    float x = 1.5f * OuterRadius * j; // the x offset
                    y += InnerRadius * (j % 2);

                    obj.transform.position = new Vector3(x, y) + transform.position;

                    // add a line renderer and set a hexagon shape
                    LineRenderer line = obj.GetComponent<LineRenderer>();
                    line.positionCount = 6;
                    Vector3[] positions = new Vector3[]
                    {
                        obj.transform.position - Vector3.right * .5f * OuterRadius + Vector3.up * InnerRadius,
                        obj.transform.position + Vector3.right * .5f * OuterRadius + Vector3.up * InnerRadius,
                        obj.transform.position + Vector3.right * OuterRadius,
                        obj.transform.position + Vector3.right * .5f * OuterRadius - Vector3.up * InnerRadius,
                        obj.transform.position - Vector3.right * .5f * OuterRadius - Vector3.up * InnerRadius,
                        obj.transform.position - Vector3.right * OuterRadius
                    };
                    line.SetPositions(positions);
                    line.loop = true;
                }
            }

            GENERATE = false;
        }

        if (CLEARALL)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            CLEARALL = false;
        }
    }
}
