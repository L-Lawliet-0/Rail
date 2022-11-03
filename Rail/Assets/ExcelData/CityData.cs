using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is the import data from internet
[System.Serializable]
public class CityData
{
    public string CityName;
    public float Population, GDP;

    public int ResidentPopulation; // the home residence population
    public Dictionary<int, int> VisitorPopulation; // the population that comes here, and will go back

    public Vector3 posV3 { get { return new Vector3(x, y); } }
    public float x, y;
}
