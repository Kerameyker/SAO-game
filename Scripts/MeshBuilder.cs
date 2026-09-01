using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Генерує власну геометрію (вершини й трикутники) для природи й будівель:
/// звужені стовбури, нерівні "грудкуваті" крони дерев, двосхилі дахи.
/// Це справжні згенеровані меші, а не комбінації GameObject.CreatePrimitive.
/// </summary>
public static class MeshBuilder
{
    public static Mesh CreateTaperedCylinder(float bottomRadius, float topRadius, float height, int segments, float bendX = 0f, float bendZ = 0f)
    {
        Mesh mesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        Vector3 bottomCenter = Vector3.zero;
        Vector3 topCenter = new Vector3(bendX, height, bendZ);

        int bottomStart = verts.Count;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            verts.Add(bottomCenter + offset * bottomRadius);
        }
        int topStart = verts.Count;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            verts.Add(topCenter + offset * topRadius);
        }

        for (int i = 0; i < segments; i++)
        {
            int b0 = bottomStart + i;
            int b1 = bottomStart + i + 1;
            int t0 = topStart + i;
            int t1 = topStart + i + 1;
            tris.Add(b0); tris.Add(t0); tris.Add(b1);
            tris.Add(b1); tris.Add(t0); tris.Add(t1);
        }

        int bottomCapCenter = verts.Count;
        verts.Add(bottomCenter);
        for (int i = 0; i < segments; i++)
        {
            tris.Add(bottomCapCenter);
            tris.Add(bottomStart + i + 1);
            tris.Add(bottomStart + i);
        }

        int topCapCenter = verts.Count;
        verts.Add(topCenter);
        for (int i = 0; i < segments; i++)
        {
            tris.Add(topCapCenter);
            tris.Add(topStart + i);
            tris.Add(topStart + i + 1);
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    /// <summary>Нерівна "грудкувата" крона - підрозділений октаедр із випадковим зміщенням вершин.</summary>
    public static Mesh CreateBlob(float radius, int seed)
    {
        System.Random rng = new System.Random(seed);
        Mesh mesh = new Mesh();

        Vector3[] baseVerts = {
            Vector3.up, Vector3.down,
            Vector3.left, Vector3.right,
            Vector3.forward, Vector3.back
        };
        int[] baseTris = {
            0,4,3, 0,3,5, 0,5,2, 0,2,4,
            1,3,4, 1,5,3, 1,2,5, 1,4,2
        };

        List<Vector3> verts = new List<Vector3>(baseVerts);
        List<int> tris = new List<int>();
        Dictionary<long, int> midpointCache = new Dictionary<long, int>();

        int GetMidpoint(int a, int b)
        {
            long key = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
            if (midpointCache.TryGetValue(key, out int existing)) return existing;
            Vector3 mid = ((verts[a] + verts[b]) * 0.5f).normalized;
            verts.Add(mid);
            int idx = verts.Count - 1;
            midpointCache[key] = idx;
            return idx;
        }

        for (int i = 0; i < baseTris.Length; i += 3)
        {
            int a = baseTris[i], b = baseTris[i + 1], c = baseTris[i + 2];
            int ab = GetMidpoint(a, b);
            int bc = GetMidpoint(b, c);
            int ca = GetMidpoint(c, a);
            tris.Add(a); tris.Add(ab); tris.Add(ca);
            tris.Add(b); tris.Add(bc); tris.Add(ab);
            tris.Add(c); tris.Add(ca); tris.Add(bc);
            tris.Add(ab); tris.Add(bc); tris.Add(ca);
        }

        for (int i = 0; i < verts.Count; i++)
        {
            float jitter = 0.75f + (float)rng.NextDouble() * 0.5f;
            verts[i] = verts[i].normalized * radius * jitter;
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    /// <summary>Справжній двосхилий дах (трикутна призма з фронтонами), а не плаский повернутий куб.</summary>
    public static Mesh CreateGableRoof(float width, float depth, float ridgeHeight)
    {
        Vector3[] baseVerts = {
            new Vector3(-width/2f, 0f, -depth/2f), // 0
            new Vector3( width/2f, 0f, -depth/2f), // 1
            new Vector3( width/2f, 0f,  depth/2f), // 2
            new Vector3(-width/2f, 0f,  depth/2f), // 3
            new Vector3(0f, ridgeHeight, -depth/2f), // 4 ковзан ззаду
            new Vector3(0f, ridgeHeight,  depth/2f), // 5 ковзан спереду
        };
        // дублюємо вершини для зворотної сторони - так кожна сторона отримує СВОЮ нормаль
        // (спільні вершини для обох сторін призводять до "погашення" нормалей і зникнення даху під кутом)
        Vector3[] verts = new Vector3[12];
        for (int i = 0; i < 6; i++) { verts[i] = baseVerts[i]; verts[i + 6] = baseVerts[i]; }

        int[] frontTris = {
            0,4,3, 3,4,5, // лівий схил
            1,2,4, 2,5,4, // правий схил
            0,1,4,        // задній фронтон
            3,5,2,        // передній фронтон
        };
        int[] backSrc = {
            0,3,4, 3,5,4,
            1,4,2, 2,4,5,
            0,4,1,
            3,2,5,
        };
        int[] tris = new int[frontTris.Length + backSrc.Length];
        System.Array.Copy(frontTris, tris, frontTris.Length);
        for (int i = 0; i < backSrc.Length; i++) tris[frontTris.Length + i] = backSrc[i] + 6;

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    /// <summary>Плоске кільце (кругова дорога навколо площі).</summary>
    public static Mesh CreateAnnulus(float innerRadius, float outerRadius, int segments)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float cx = Mathf.Cos(angle), cz = Mathf.Sin(angle);
            verts.Add(new Vector3(cx * innerRadius, 0f, cz * innerRadius));
            verts.Add(new Vector3(cx * outerRadius, 0f, cz * outerRadius));
        }
        for (int i = 0; i < segments; i++)
        {
            int i0 = i * 2, i1 = i * 2 + 1, i2 = i * 2 + 2, i3 = i * 2 + 3;
            tris.Add(i0); tris.Add(i2); tris.Add(i1);
            tris.Add(i1); tris.Add(i2); tris.Add(i3);
            // дзеркальна сторона - гарантує видимість незалежно від напрямку трикутників
            tris.Add(i1); tris.Add(i2); tris.Add(i0);
            tris.Add(i3); tris.Add(i2); tris.Add(i1);
        }
        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        Vector3[] normalsUp = new Vector3[verts.Count];
        for (int i = 0; i < normalsUp.Length; i++) normalsUp[i] = Vector3.up;
        mesh.normals = normalsUp;
        mesh.RecalculateBounds();
        return mesh;
    }

    /// <summary>Стрічка вздовж довільної ламаної лінії - використовується і для прямих доріг (2 точки), і для звивистої річки (багато точок). Точки - у ЛОКАЛЬНИХ координатах відносно об'єкта.</summary>
    public static Mesh CreateRibbon(List<Vector3> centerPoints, float width)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        for (int i = 0; i < centerPoints.Count; i++)
        {
            Vector3 dir;
            if (i == 0) dir = (centerPoints[1] - centerPoints[0]).normalized;
            else if (i == centerPoints.Count - 1) dir = (centerPoints[i] - centerPoints[i - 1]).normalized;
            else dir = (centerPoints[i + 1] - centerPoints[i - 1]).normalized;
            Vector3 perp = new Vector3(-dir.z, 0f, dir.x) * (width / 2f);
            verts.Add(centerPoints[i] - perp);
            verts.Add(centerPoints[i] + perp);
        }
        for (int i = 0; i < centerPoints.Count - 1; i++)
        {
            int i0 = i * 2, i1 = i * 2 + 1, i2 = i * 2 + 2, i3 = i * 2 + 3;
            tris.Add(i0); tris.Add(i2); tris.Add(i1);
            tris.Add(i1); tris.Add(i2); tris.Add(i3);
            // дзеркальна сторона - гарантує видимість незалежно від напрямку трикутників
            tris.Add(i1); tris.Add(i2); tris.Add(i0);
            tris.Add(i3); tris.Add(i2); tris.Add(i1);
        }
        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        Vector3[] normalsUp = new Vector3[verts.Count];
        for (int i = 0; i < normalsUp.Length; i++) normalsUp[i] = Vector3.up;
        mesh.normals = normalsUp;
        mesh.RecalculateBounds();
        return mesh;
    }
}
