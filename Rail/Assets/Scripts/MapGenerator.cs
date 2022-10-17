using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{
    public bool RUN;
    public bool DELETE;

    public bool READSECONDLEVEL;

    public string FILENAME = "/Data/gadm41_CHN_0.json";

    public GameObject LinePrefab;
    private LineRenderer CurrentLine;
    private List<Vector3> Positions;

    void Update()
    {
        
        if (RUN)
        {
            using (StreamReader file = new StreamReader(Application.dataPath + FILENAME))
            {
                while (!file.EndOfStream)
                {
                    string polygonName = "Line";
                    char character = (char)file.Read();
                    if (READSECONDLEVEL && character.Equals('N') && PeekCompare(file, 'L'))
                    {
                        polygonName = "";
                        bool findProvince = true;
                        // if read second level, read NL_NAME_1 and NL_NAME_2
                        // this represent the provinance and city
                        // find NL_NAME_1 
                        FINDNAME:
                        for (int i = 0; i < 11; i++)
                            file.Read();

                        string value = "";
                        while (!PeekCompare(file, '"'))
                        {
                            value += (char)file.Read();
                        }

                        string[] splits = value.Split('|');

                        if (splits.Length > 1)
                            polygonName += splits[1];
                        else
                            polygonName += value;
                        polygonName += ",";

                        if (findProvince)
                        {
                            findProvince = false;
                            while (!character.Equals('N') || !PeekCompare(file, 'L'))
                                character = (char)file.Read();
                            goto FINDNAME;
                        }
                        else
                        {
                            while (!character.Equals('[') || !PeekCompare(file, '['))
                                character = (char)file.Read();
                        }
                       
                    }

                    
                    if (character.Equals('[') && ((char)file.Peek()).Equals('['))
                    {
                        // we enter a new multipolygon
                        // start will be 3 [[[
                        // and this ends with 3 ]]]

                        // starts new shape with 2 [[
                        // ends a shape with 2 ]]

                    NEWPOLYGON:
                        if (PeekCompare(file, '['))
                            file.Read();
                        if (PeekCompare(file, '['))
                            file.Read();
                        CurrentLine = Instantiate(LinePrefab, transform).GetComponent<LineRenderer>();
                        CurrentLine.transform.name = polygonName;
                        CurrentLine.transform.localPosition = Vector3.zero;
                        Positions = new List<Vector3>();
                        

                    READCOORD:
                        // starts read coord
                        ReadCoord(file);

                        // three conditions, ], ]], ]]]
                        character = (char)file.Read();
                        if (character.Equals(','))
                            goto READCOORD;
                        else
                        {
                            Vector3[] positions = Positions.ToArray();
                            Vector2[] polyPos = new Vector2[positions.Length];
                            for (int i = 0; i < positions.Length; i++)
                            {
                                polyPos[i] = positions[i];
                            }
                            CurrentLine.positionCount = positions.Length;
                            CurrentLine.SetPositions(positions);

                            // add a polygon collider to the line renderer
                            if (READSECONDLEVEL)
                            {
                                PolygonCollider2D polygon = CurrentLine.gameObject.AddComponent<PolygonCollider2D>();
                                polygon.pathCount = 1;
                                polygon.SetPath(0, polyPos);
                            }
                            // this is either a ]] or a ]]]
                            if (PeekCompare(file, ']'))
                                file.Read();
                            else
                            {
                                file.Read();
                                goto NEWPOLYGON;
                            }

                            // a polygon is done, add it to the line renderer
                            

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

                file.Close();
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

    private bool PeekCompare(StreamReader file, char compare)
    {
        return ((char)file.Peek()).Equals(compare);
    }

    private const float smRadius = 6371f;
    private void ReadCoord(StreamReader file)
    {
        float smRange = smRadius * Mathf.PI * 2;
        float smLonToX = smRange / 360f;
        float smRadiansOverDegrees = Mathf.PI / 180f;

        if (PeekCompare(file, '['))
            file.Read(); // the [
        string v2 = "";
        while (!((char)file.Peek()).Equals(']'))
            v2 += (char)file.Read(); // append the vector
        string s_x = v2.Split(',')[0];
        string s_y = v2.Split(',')[1];

        float x = float.Parse(s_x);
        float y = float.Parse(s_y);

        float f_x = x * smLonToX;
        float f_y = y * smRadiansOverDegrees;
        f_y = Mathf.Log(Mathf.Tan(f_y) + 1f / Mathf.Cos(f_y), (float)Math.E);
        f_y *= smRadius;

        /*
        float lon = x * Mathf.Deg2Rad;
        float lat = y * Mathf.Deg2Rad;

        float earth_R = 6371f;
        float f_x = earth_R * lon;
        float f_y = earth_R * Mathf.Log(Mathf.Tan(Mathf.PI * .25f + .5f * lat));
        */


        file.Read(); // the ]

        Positions.Add(new Vector3(f_x, f_y, 0));
    }
}
