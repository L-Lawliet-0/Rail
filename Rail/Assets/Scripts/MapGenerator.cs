using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{
    public bool RUN;
    public bool DELETE;

    public GameObject LinePrefab;
    private LineRenderer CurrentLine;
    private List<Vector3> Positions;

    void Update()
    {
        
        if (RUN)
        {
            using (StreamReader file = new StreamReader(Application.dataPath + "/Data/gadm41_CHN_0.json"))
            {
                while (!file.EndOfStream)
                {
                    char character = (char)file.Read();
                    if (character.Equals('[') && ((char)file.Peek()).Equals('['))
                    {
                        // we enter a new multipolygon
                        // start will be 3 [[[
                        // and this ends with 3 ]]]

                        // starts new shape with 2 [[
                        // ends a shape with 2 ]]

                    NEWPOLYGON:     
                        file.Read();
                        file.Read();
                        CurrentLine = Instantiate(LinePrefab, transform).GetComponent<LineRenderer>();
                        CurrentLine.transform.localPosition = Vector3.zero;
                        Positions = new List<Vector3>();


                    READCOORD:
                        // starts read coord
                        ReadCoord(file);

                        character = (char)file.Read();
                        if (character.Equals(','))
                            goto READCOORD;
                        else
                        {
                            // this is either a ]] or a ]]]
                            file.Read();
                            // a polygon is done, add it to the line renderer
                            Vector3[] positions = Positions.ToArray();
                            CurrentLine.positionCount = positions.Length;
                            CurrentLine.SetPositions(positions);

                            if (((char)file.Peek()).Equals(']'))
                            {
                                file.Read(); // start a new read
                            }
                            else
                            {
                                file.Read(); // read the ,
                                goto NEWPOLYGON;
                            }
                        }
                    }
                }


            }

            RUN = false;
        }   
        
        if (DELETE)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            DELETE = false;
        }
    }

    private void ReadCoord(StreamReader file)
    {
        file.Read(); // the [
        string v2 = "";
        while (!((char)file.Peek()).Equals(']'))
            v2 += (char)file.Read(); // append the vector
        string s_x = v2.Split(',')[0];
        string s_y = v2.Split(',')[1];

        float x = float.Parse(s_x);
        float y = float.Parse(s_y);
        file.Read(); // the ]

        Positions.Add(new Vector3(x, y, 0));
    }
}
