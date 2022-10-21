using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HexTrimmer : MonoBehaviour
{
    public bool TRIM;
    public bool LOGCOUNT;
    public bool LOGPROVINCE;
    public bool LOGCITY;
    public bool ASSIGNHEXDATA;
    public bool EnableLine;
    public bool DisableLine;

    public Sprite HexagonSprite;
    public Material Mat;

    private static Color[] CityColors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.white,
        Color.black,
        Color.gray,
        Color.yellow,
        Color.cyan,
        Color.magenta
    };
    private int ColorPointer = 0;

    void Update()
    {
        if (TRIM)
        {
            // trim hexagon that not overlap with the chinese map
            float radius = GetComponent<HexGenerator>().OuterRadius;
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Vector3 origin = transform.GetChild(i).position;
                // do the overlap check
                Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, radius);
                if (colliders == null || colliders.Length == 0)
                {
                    //DestroyImmediate(transform.GetChild(i).gameObject);
                    //transform.GetChild(i).name = "sea";
                }
                else
                {
                    float minDis = float.MaxValue;
                    Collider2D finalC = colliders[0];
                    foreach (Collider2D collider in colliders)
                    {
                        float distance = Vector3.Distance(collider.ClosestPoint(origin), origin);
                        if (distance < minDis)
                        {
                            minDis = distance;
                            finalC = collider;
                        }
                    }
                    transform.GetChild(i).name = finalC.name;
                }
            }

            TRIM = false;
        }
        
        if (LOGCOUNT)
        {
            Debug.LogError(transform.childCount);
            LOGCOUNT = false;
        }

        if (LOGPROVINCE)
        {
            LogTrimNames(0);
            LOGPROVINCE = false;
        }

        if (LOGCITY)
        {
            LogTrimNames(1);
            LOGCITY = false;
        }

        if (ASSIGNHEXDATA)
        {
            float outerRadius = GetComponent<HexGenerator>().OuterRadius;
            float innerRadius = outerRadius * Mathf.Sqrt(3) / 2f;

            // create mesh
            Mesh mesh = GlobalDataTypes.GetHexagonMesh();

            /*
            Vector3[] normals = new Vector3[vertices.Length];
            for (int x = 0; x < normals.Length; x++)
            {
                normals[x] = Vector3.forward;
            }
            mesh.normals = normals;
            */

            mesh.RecalculateNormals();

            Dictionary<string, Color> CityColors = new Dictionary<string, Color>();
            ColorPointer = 0;

            for (int i = 0; i < transform.childCount; i++)
            {
                HexData hex = transform.GetChild(i).GetComponent<HexData>();
                if (!hex)
                    hex = transform.GetChild(i).gameObject.AddComponent<HexData>();

                SpriteRenderer sr = transform.GetChild(i).GetComponent<SpriteRenderer>();
                if (sr)
                    DestroyImmediate(sr);
                // transform.GetChild(i).GetComponent<LineRenderer>().enabled = false;

                MeshFilter meshFilter = transform.GetChild(i).GetComponent<MeshFilter>();
                if (!meshFilter)
                    meshFilter = transform.GetChild(i).gameObject.AddComponent<MeshFilter>();

                MeshRenderer mr = transform.GetChild(i).GetComponent<MeshRenderer>();
                if (!mr)
                    mr = transform.GetChild(i).gameObject.AddComponent<MeshRenderer>();

                bool findCity = false;
                foreach ((GlobalDataTypes.Province, GlobalDataTypes.ProvinceData) tuple in GlobalDataTypes.ProvinceDatas)
                {
                    if (transform.GetChild(i).name.Split(',')[0].Equals(tuple.Item2.Name))
                    {
                        string city = transform.GetChild(i).name.Split(',')[1];
                        if (!CityColors.ContainsKey(city))
                        {
                            CityColors.Add(city, HexTrimmer.CityColors[ColorPointer]);
                            ColorPointer++;
                            if (ColorPointer >= HexTrimmer.CityColors.Length)
                                ColorPointer = 0;
                        }

                        hex.Province = tuple.Item1;

                        meshFilter.mesh = mesh;
                       
                        Material mat = new Material(Mat);
                        mat.SetColor("_BaseColor", Color.black);
                        mr.material = mat;

                        findCity = true;
                        break;
                    }
                }

                mr.enabled = findCity;
            }
            ASSIGNHEXDATA = false;
        }

        if (EnableLine)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).GetComponent<LineRenderer>().enabled = true;
            EnableLine = false;
        }

        if (DisableLine)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).GetComponent<LineRenderer>().enabled = false;
            DisableLine = false;
        }
    }

    private void LogTrimNames(int index)
    {
        HashSet<string> Names = new HashSet<string>();
        for (int i = 0; i < transform.childCount; i++)
        {
            string name = transform.GetChild(i).name.Split(',')[index];
            Names.Add(name);
        }

        foreach (string name in Names)
            Debug.LogError(name);
        Debug.LogError("Length : " + Names.Count);
    }
}
