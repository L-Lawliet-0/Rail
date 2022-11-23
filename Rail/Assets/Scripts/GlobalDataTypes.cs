using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDataTypes : MonoBehaviour
{
    private static GlobalDataTypes m_Instance;
    public static GlobalDataTypes Instance { get { return m_Instance; } }

    public Material TestHexMaterial;

    public static float GDPtoMult(float gdp)
    {
        return 1;
    }

    public const int StartBudget = 1000000;

    public const int MaxLevel = 4;

    // the unit will be in 00000 shi wan, but will be calculated in thousand
    // spend is in acutal cost
    public const int CrossBuildPrice = 100;
    public const int CrossToStationPrice = 100;

    public static int[] StationPrices = { 300, 350, 400, 450, 500 }; // 5
    public static int[] StationUpgradePrices = { 60, 60, 60, 60}; // 4
    public static int[] StationCapacity = { 1000, 2000, 3000, 4000, 5000 };
    public static int[] StationDailySpend = { 10000, 10500, 11000, 11500, 12000 };

    public static int[] TrackPrices = { 30, 35, 40, 45, 50};
    public static int[] TrackUpgradePrices = { 6, 6, 6, 6};
    public static int[] TrackDailySpend = { 3, 4, 5, 6, 7 };

    public static int[] TrainPrices = { 50, 75, 100, 125, 150 };
    public static int[] TrainUpgradePrices = { 50, 50, 50, 50 };
    public static int[] TrainCapacity = { 500, 750, 1000, 1250, 1500 };
    public static int[] TrainDailySpend = { 10, 20, 30, 40, 50 };

    public static int[] Speeds = { 100, 150, 200, 250, 300 };

    public static Color[] RarityColors = { Color.white, new Color(30 / 255f, 255 / 255f, 0), new Color(0, 112 / 255f, 221 / 255f), new Color(163 / 255f, 53 / 255f, 238 / 255f), new Color(255 / 255f, 128 / 255f, 0) };
    public static Color[] TrackRarityColors = { Color.black, new Color(30 / 255f, 255 / 255f, 0), new Color(0, 112 / 255f, 221 / 255f), new Color(163 / 255f, 53 / 255f, 238 / 255f), new Color(255 / 255f, 128 / 255f, 0) };

    public static float MinTrainPrice = .1f;
    public static float MaxTrainPrice = .5f;

    private void Awake()
    {
        m_Instance = this;
    }

    public const float HexDistance = 10f; // the distance between two hex, 2 * inner radius
    public const float Xdistance = 5.7735f * 1.5f; // the horizontal distance between two hex grid on the same row
    public const int xCount = 831; // how many grids in one row
    public const int yCount = 520; // how many grids in one column

    public static int GetIndexXY(int x, int y)
    {
        // because we generated it right, up order
        return y * xCount + x;
    }

    public static Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return p1 + Mathf.Pow(1 - t, 2) * (p0 - p1) + Mathf.Pow(t, 2) * (p2 - p1);
    }


    // return the six neightbors of this grid
    public static int[] FindNeighbors(GridData.GridSave grid)
    {
        int[] indexs = new int[6];

        int x = grid.Index / xCount;
        int y = grid.Index % xCount;

        // get the six hex index
        indexs[0] = (x + 1) * xCount + y;
        indexs[1] = (x - 1) * xCount + y;
        if (y % 2 == 0)
        {
            indexs[2] = x * xCount + y + 1;
            indexs[3] = (x - 1) * xCount + y + 1;
            indexs[4] = x * xCount + y - 1;
            indexs[5] = (x - 1) * xCount + y - 1;
        }
        else
        {
            indexs[2] = (x + 1) * xCount + y + 1;
            indexs[3] = x * xCount + y + 1;
            indexs[4] = (x + 1) * xCount + y - 1;
            indexs[5] = x * xCount + y - 1;
        }

        return indexs;
    }

    // This section declares different datas used in the game

    // ?
    public enum Province
    {
        HaiNan,
        GuangDong,
        GuangXi,
        YunNan,
        XiangGang,
        AoMen,
        FuJian,
        JiangXi,
        GuiZhou,
        HuNan,
        SiChuan,
        ZheJiang,
        XiZang,
        ChongQing,
        HuBei,
        AnHui,
        ShangHai,
        JiangSu,
        HeNan,
        QingHai,
        ShanXi,
        GanSu,
        XinJiang,
        ShanDong,
        ShanXi1,
        NingXia,
        HeBei,
        NeiMengGu,
        TianJin,
        LiaoNing,
        BeiJing,
        JiLin,
        HeiLongJiang,
        TaiWan
    }

    public class ProvinceData
    {
        public string Name;
        public Color Color;
        public ProvinceData(string name, Color color)
        {
            Name = name;
            Color = color;
        }
    }

    public static (Province, ProvinceData)[] ProvinceDatas = new (Province, ProvinceData)[]
    {
        (Province.HaiNan, new ProvinceData("海南", new Color(196f/255f, 214f/255f, 223f/255f))),
        (Province.GuangDong, new ProvinceData("广东", new Color(247f/255f, 123f/255f, 38f/255f))),
        (Province.GuangXi, new ProvinceData("广西壮族自治区", new Color(240f/255f, 205f/255f, 20f/255f))),
        (Province.YunNan, new ProvinceData("云南", new Color(21f/255f, 168f/255f, 155f/255f))),
        (Province.XiangGang, new ProvinceData("香港", new Color(147f/255f, 177f/255f, 168f/255f))),
        (Province.AoMen, new ProvinceData("澳门", new Color(235f/255f, 175f/255f, 13f/255f))),
        (Province.FuJian, new ProvinceData("福建", new Color(239f/255f, 60f/255f, 78f/255f))),
        (Province.JiangXi, new ProvinceData("江西", new Color(166f/255f, 82f/255f, 87f/255f))),
        (Province.GuiZhou, new ProvinceData("贵州", new Color(186f/255f, 164f/255f, 147f/255f))),
        (Province.HuNan, new ProvinceData("湖南", new Color(246f/255f, 117f/255f, 57f/255f))),
        (Province.SiChuan, new ProvinceData("四川", new Color(254f/255f, 153f/255f, 108f/255f))),
        (Province.ZheJiang, new ProvinceData("浙江", new Color(0f/255f, 63f/255f, 136f/255f))),
        (Province.XiZang, new ProvinceData("西藏自治区", new Color(250f/255f, 242f/255f, 239f/255f))),
        (Province.ChongQing, new ProvinceData("重庆", new Color(205f/255f, 79f/255f, 65f/255f))),
        (Province.HuBei, new ProvinceData("湖北", new Color(255f/255f, 164f/255f, 58f/255f))),
        (Province.AnHui, new ProvinceData("安徽", new Color(180f/255f, 185f/255f, 188f/255f))),
        (Province.ShangHai, new ProvinceData("上海", new Color(37f/255f, 117f/255f, 185f/255f))),
        (Province.JiangSu, new ProvinceData("江苏", new Color(254f/255f, 200f/255f, 42f/255f))),
        (Province.HeNan, new ProvinceData("河南", new Color(236f/255f, 163f/255f, 163f/255f))),
        (Province.HeNan, new ProvinceData("青海", new Color(137f/255f, 187f/255f, 208f/255f))),
        (Province.ShanXi, new ProvinceData("陕西", new Color(229f/255f, 181f/255f, 83f/255f))),
        (Province.GanSu, new ProvinceData("甘肃", new Color(207f/255f, 96f/255f, 39f/255f))),
        (Province.XinJiang, new ProvinceData("新疆维吾尔自治区", new Color(247f/255f, 142f/255f, 21f/255f))),
        (Province.ShanDong, new ProvinceData("山东", new Color(65f/255f, 158f/255f, 114f/255f))),
        (Province.ShanXi1, new ProvinceData("山西", new Color(136f/255f, 125f/255f, 116f/255f))),
        (Province.NingXia, new ProvinceData("宁夏回族自治区", new Color(228f/255f, 176f/255f, 69f/255f))),
        (Province.HeBei, new ProvinceData("河北", new Color(193f/255f, 44f/255f, 56f/255f))),
        (Province.NeiMengGu, new ProvinceData("内蒙古自治区", new Color(157f/255f, 188f/255f, 176f/255f))),
        (Province.TianJin, new ProvinceData("天津", new Color(241f/255f, 87f/255f, 70f/255f))),
        (Province.LiaoNing, new ProvinceData("辽宁", new Color(201f/255f, 40f/255f, 29f/255f))),
        (Province.BeiJing, new ProvinceData("北京", new Color(188f/255f, 38f/255f, 40f/255f))),
        (Province.JiLin, new ProvinceData("吉林", new Color(218f/255f, 225f/255f, 226f/255f))),
        (Province.HeiLongJiang, new ProvinceData("黑龍江省", new Color(240f/255f, 235f/255f, 234f/255f))),
        (Province.TaiWan, new ProvinceData("台湾", new Color(240f/255f, 235f/255f, 234f/255f))),
    };

    public static Mesh GetHexagonMesh(float outerRadius = 5.7735f, float innerRadius = 5f)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[7]; // there are six vertices in a hexagon
        vertices[0] = -Vector3.right * .5f * outerRadius + Vector3.up * innerRadius;
        vertices[1] = Vector3.right * .5f * outerRadius + Vector3.up * innerRadius;
        vertices[2] = Vector3.right * outerRadius;
        vertices[3] = Vector3.right * .5f * outerRadius - Vector3.up * innerRadius;
        vertices[4] = -Vector3.right * .5f * outerRadius - Vector3.up * innerRadius;
        vertices[5] = -Vector3.right * outerRadius;
        vertices[6] = Vector3.zero;
        mesh.vertices = vertices;


        Vector2[] uvs = new Vector2[]
        {
                new Vector2(.25f, 1),
                new Vector2(.75f, 1),
                new Vector2(1, .5f),
                new Vector2(.75f, 0),
                new Vector2(.25f, 0),
                new Vector2(0, .5f),
                new Vector2(.5f, .5f)
        };
        mesh.uv = uvs;


        int[] triangles = new int[]
        {
                6, 5, 0,
                6, 0, 1,
                6, 1, 2,
                6, 2, 3,
                6, 3, 4,
                6, 4, 5
        };
        mesh.triangles = triangles;

        return mesh;
    }
}
