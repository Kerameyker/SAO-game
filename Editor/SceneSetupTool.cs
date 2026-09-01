using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Автоматично будує тестову сцену: земля, персонаж (капсула) із контролером руху,
/// камера від третьої особи. Після імпорту цього файлу в Unity з'явиться пункт меню
/// "Вежа → Створити тестову сцену" — просто натисни його один раз.
/// ВАЖЛИВО: цей файл має лежати в папці Assets/Editor (саме так, назва папки важлива).
/// </summary>
public static class SceneSetupTool
{
    [MenuItem("Вежа/Створити тестову сцену")]
    public static void SetupScene()
    {
        // --- Земля ---
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 1f, 10f);

        // --- Персонаж ---
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);

        // капсула за замовчуванням має звичайний Collider — прибираємо його
        // і додаємо CharacterController, який потрібен нашому скрипту руху
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        player.AddComponent<CharacterController>();

        var playerController = player.AddComponent<PlayerController>();

        // --- Камера ---
        Camera mainCam = Camera.main;
        GameObject camObject;
        if (mainCam == null)
        {
            camObject = new GameObject("Main Camera");
            mainCam = camObject.AddComponent<Camera>();
            camObject.tag = "MainCamera";
            camObject.AddComponent<AudioListener>();
        }
        else
        {
            camObject = mainCam.gameObject;
        }

        GameObject rig = new GameObject("CameraRig");
        camObject.transform.SetParent(rig.transform);
        camObject.transform.localPosition = new Vector3(0f, 0f, -6f);
        camObject.transform.localRotation = Quaternion.identity;

        var cameraFollow = rig.AddComponent<CameraFollow>();
        cameraFollow.target = player.transform;

        playerController.cameraTransform = camObject.transform;

        // трохи світла, щоб сцена не була чорною
        if (Object.FindAnyObjectByType<Light>() == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        Selection.activeGameObject = player;
        Debug.Log("Готово! Сцену створено. Тепер просто натисни кнопку Play вгорі.");
    }

    [MenuItem("Вежа/Додати вежу і збільшити землю")]
    public static void AddTowerAndGround()
    {
        // --- Збільшуємо (або створюємо) землю ---
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
        }
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(60f, 1f, 60f);

        // --- Вежа по центру (проста заготовка з циліндрів, що звужуються догори) ---
        if (GameObject.Find("Tower") != null)
        {
            Debug.Log("Вежа вже є в сцені — нічого не додаю повторно.");
        }
        else
        {
            GameObject tower = new GameObject("Tower");

            float[] segmentHeights = { 6f, 5f, 4.5f, 4f, 3.5f };
            float[] segmentRadii   = { 5f, 4.2f, 3.5f, 2.8f, 2.2f };
            float y = 0f;
            for (int i = 0; i < segmentHeights.Length; i++)
            {
                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                seg.name = "TowerSegment_" + i;
                seg.transform.SetParent(tower.transform);
                float h = segmentHeights[i];
                float r = segmentRadii[i];
                // стандартний циліндр Unity має висоту 2 і радіус 0.5 при Scale(1,1,1)
                seg.transform.localScale = new Vector3(r, h / 2f, r);
                seg.transform.position = new Vector3(0f, y + h / 2f, 0f);
                y += h;
            }

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            roof.name = "TowerRoof";
            roof.transform.SetParent(tower.transform);
            roof.transform.localScale = new Vector3(0.6f, 2f, 0.6f);
            roof.transform.position = new Vector3(0f, y + 2f, 0f);

            Debug.Log("Вежу додано по центру сцени.");
        }

        // --- Відсуваємо гравця подалі від основи вежі, щоб не спавнитись всередині ---
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 1f, -20f);
            Debug.Log("Гравця переміщено подалі від вежі. Можеш підбігти ближче й роздивитись.");
        }
    }

    [MenuItem("Вежа/Портали/Додати портал до зали боса")]
    public static void AddPortalToBossChamber()
    {
        GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        portal.name = "PortalToBossChamber";
        portal.transform.position = new Vector3(0f, 1f, 3f); // біля основи вежі
        portal.transform.localScale = new Vector3(2f, 1f, 2f);
        Object.DestroyImmediate(portal.GetComponent<Collider>());
        BoxCollider col = portal.AddComponent<BoxCollider>();
        col.isTrigger = true;
        ScenePortal script = portal.AddComponent<ScenePortal>();
        script.targetSceneName = "BossChamber";
        Selection.activeGameObject = portal;
        Debug.Log("Портал до зали боса додано біля вежі. Перевір, що назва сцени в полі Target Scene Name = \"BossChamber\".");
    }

    static bool IsBossChamberGeneratedObject(string n)
    {
        string[] exact = { "BossFloor", "PortalBackToFloor", "BossSpawnPoint", "ThronePlatform", "SpawnPoint", "RuneCircle" };
        foreach (string e in exact) if (n == e) return true;
        string[] prefixes = { "Wall_", "Step_", "Crack_", "CornerCrystal", "CornerLight", "TorchPost", "Flame", "TorchLight", "Banner_", "WallCrystal_", "Rubble_" };
        foreach (string p in prefixes) if (n.StartsWith(p)) return true;
        return false;
    }

    static void ClearBossChamberObjects()
    {
        GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject go in all)
        {
            if (go != null && IsBossChamberGeneratedObject(go.name))
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    [MenuItem("Вежа/Портали/Створити залу боса в поточній сцені")]
    public static void SetupBossChamber()
    {
        ClearBossChamberObjects(); // прибираємо все зі старих запусків - інакше підлога/стіни накладаються одна на одну

        float roomSize = 60f; // удвічі більше
        float wallHeight = 20f; // вищі стіни

        Material darkFloorMat = MakeColorMat(new Color(0.16f, 0.14f, 0.17f));
        Material wallMat = MakeColorMat(new Color(0.2f, 0.18f, 0.22f));
        Material crystalMat = MakeColorMat(new Color(0.55f, 0.15f, 0.58f));
        Material crackMat = MakeColorMat(new Color(0.08f, 0.07f, 0.09f));

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "BossFloor";
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(roomSize / 10f, 1f, roomSize / 10f);
        floor.GetComponent<Renderer>().sharedMaterial = darkFloorMat;

        // тріщини на підлозі - прості темні плями для фактури
        System.Random arenaRng = new System.Random(444);
        for (int i = 0; i < 10; i++)
        {
            GameObject crack = new GameObject("Crack_" + i);
            crack.transform.position = new Vector3(
                ((float)arenaRng.NextDouble() - 0.5f) * roomSize * 0.8f,
                0.02f,
                ((float)arenaRng.NextDouble() - 0.5f) * roomSize * 0.8f
            );
            Mesh crackMesh = MeshBuilder.CreateBlob(0.8f + (float)arenaRng.NextDouble() * 1.5f, i + 5000);
            crack.AddComponent<MeshFilter>().mesh = crackMesh;
            crack.AddComponent<MeshRenderer>().sharedMaterial = crackMat;
            crack.transform.localScale = new Vector3(1f, 0.05f, 1f);
        }

        Vector3[] wallPositions = {
            new Vector3(0f, wallHeight / 2f, roomSize / 2f),
            new Vector3(0f, wallHeight / 2f, -roomSize / 2f),
            new Vector3(roomSize / 2f, wallHeight / 2f, 0f),
            new Vector3(-roomSize / 2f, wallHeight / 2f, 0f),
        };
        Vector3[] wallScales = {
            new Vector3(roomSize, wallHeight, 1f),
            new Vector3(roomSize, wallHeight, 1f),
            new Vector3(1f, wallHeight, roomSize),
            new Vector3(1f, wallHeight, roomSize),
        };
        for (int i = 0; i < 4; i++)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall_" + i;
            wall.transform.position = wallPositions[i];
            wall.transform.localScale = wallScales[i];
            wall.GetComponent<Renderer>().sharedMaterial = wallMat;
        }

        // кристалічні формації по кутах - у стилі боса, нормального розміру (не масштабуються батьком)
        Vector3[] cornerPositions = {
            new Vector3(roomSize * 0.4f, 0f, roomSize * 0.4f),
            new Vector3(-roomSize * 0.4f, 0f, roomSize * 0.4f),
            new Vector3(roomSize * 0.4f, 0f, -roomSize * 0.4f),
            new Vector3(-roomSize * 0.4f, 0f, -roomSize * 0.4f),
        };
        foreach (Vector3 cornerPos in cornerPositions)
        {
            for (int k = 0; k < 3; k++)
            {
                GameObject crystal = new GameObject("CornerCrystal");
                crystal.transform.position = cornerPos + new Vector3((k - 1) * 0.8f, 0f, (k % 2) * 0.6f);
                float h = 2.5f + k * 1.2f;
                Mesh crystalMesh = MeshBuilder.CreateTaperedCylinder(0.4f, 0.03f, h, 6);
                crystal.AddComponent<MeshFilter>().mesh = crystalMesh;
                crystal.AddComponent<MeshRenderer>().sharedMaterial = crystalMat;
            }

            // драматичне підсвічування біля кожного кута
            GameObject pointLightObj = new GameObject("CornerLight");
            pointLightObj.transform.position = cornerPos + Vector3.up * 3f;
            Light pl = pointLightObj.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = new Color(0.7f, 0.3f, 0.8f);
            pl.range = 12f;
            pl.intensity = 2.5f;
        }

        if (Object.FindAnyObjectByType<Light>() == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.4f; // приглушене освітлення - драматичніша атмосфера
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // --- Трон боса: підвищена платформа зі сходами ---
        const float platformSize = 13f;
        const float platformHeight = 3f;
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        platform.name = "ThronePlatform";
        platform.transform.position = new Vector3(0f, platformHeight / 2f, 0f);
        platform.transform.localScale = new Vector3(platformSize, platformHeight / 2f, platformSize);
        platform.GetComponent<Renderer>().sharedMaterial = wallMat;

        // сходи, що ведуть на платформу з боку входу (від порталу)
        int stepCount = 9;
        for (int s = 0; s < stepCount; s++)
        {
            GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = "Step_" + s;
            float stepHeight = platformHeight / stepCount;
            step.transform.position = new Vector3(0f, stepHeight * (s + 0.5f), platformSize / 2f + 1.8f - s * 1f);
            step.transform.localScale = new Vector3(6f, stepHeight, 1f);
            step.GetComponent<Renderer>().sharedMaterial = darkFloorMat;
        }

        // --- Факели на стінах - по 3 на кожній стіні ---
        Material torchPostMat = MakeColorMat(new Color(0.25f, 0.2f, 0.16f));
        Material flameMat = MakeColorMat(new Color(1f, 0.55f, 0.1f));
        Vector3[] wallInward = { new Vector3(0, 0, -1), new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector3(1, 0, 0) };
        for (int w = 0; w < 4; w++)
        {
            for (int t = -1; t <= 1; t++)
            {
                Vector3 basePos = wallPositions[w] + wallInward[w] * 0.6f;
                Vector3 along = (w < 2) ? Vector3.right : Vector3.forward;
                Vector3 torchPos = basePos + along * (t * roomSize * 0.28f);
                torchPos.y = wallHeight * 0.45f;

                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "TorchPost";
                post.transform.position = torchPos;
                post.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
                post.GetComponent<Renderer>().sharedMaterial = torchPostMat;

                GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flame.name = "Flame";
                Object.DestroyImmediate(flame.GetComponent<Collider>());
                flame.transform.position = torchPos + Vector3.up * 0.55f;
                flame.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
                flame.GetComponent<Renderer>().sharedMaterial = flameMat;

                GameObject torchLight = new GameObject("TorchLight");
                torchLight.transform.position = torchPos + Vector3.up * 0.55f;
                Light tl = torchLight.AddComponent<Light>();
                tl.type = LightType.Point;
                tl.color = new Color(1f, 0.6f, 0.2f);
                tl.range = 8f;
                tl.intensity = 1.5f;

                // прапор-гобелен між факелами
                if (t == 0)
                {
                    GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    banner.name = "Banner_" + w;
                    Object.DestroyImmediate(banner.GetComponent<Collider>());
                    banner.transform.position = basePos + Vector3.up * (wallHeight * 0.55f);
                    banner.transform.localScale = (w < 2) ? new Vector3(3f, 6f, 0.1f) : new Vector3(0.1f, 6f, 3f);
                    Material bannerMat = MakeColorMat(new Color(0.4f, 0.08f, 0.12f));
                    banner.GetComponent<Renderer>().sharedMaterial = bannerMat;
                }
            }

            // кристалічна формація посередині кожної стіни
            GameObject wallCrystal = new GameObject("WallCrystal_" + w);
            Vector3 wcPos = wallPositions[w] + wallInward[w] * 1.2f;
            wcPos.y = 0f;
            wallCrystal.transform.position = wcPos;
            for (int k = 0; k < 3; k++)
            {
                GameObject shard = new GameObject("Shard");
                shard.transform.SetParent(wallCrystal.transform, false);
                shard.transform.localPosition = new Vector3((k - 1) * 1f, 0f, 0f);
                float h = 3f + k * 1.5f;
                Mesh shardMesh = MeshBuilder.CreateTaperedCylinder(0.5f, 0.04f, h, 6);
                shard.AddComponent<MeshFilter>().mesh = shardMesh;
                shard.AddComponent<MeshRenderer>().sharedMaterial = crystalMat;
            }
        }

        // --- Уламки каміння розкидані по підлозі - додає занедбаної атмосфери ---
        System.Random rubbleRng = new System.Random(999);
        for (int i = 0; i < 14; i++)
        {
            GameObject rubble = new GameObject("Rubble_" + i);
            rubble.transform.position = new Vector3(
                ((float)rubbleRng.NextDouble() - 0.5f) * roomSize * 0.75f,
                0.15f,
                ((float)rubbleRng.NextDouble() - 0.5f) * roomSize * 0.75f
            );
            float rSize = 0.4f + (float)rubbleRng.NextDouble() * 0.7f;
            Mesh rubbleMesh = MeshBuilder.CreateBlob(rSize, i + 7000);
            rubble.AddComponent<MeshFilter>().mesh = rubbleMesh;
            rubble.AddComponent<MeshRenderer>().sharedMaterial = darkFloorMat;
        }

        // --- Магічне світне коло під троном ---
        GameObject rune = new GameObject("RuneCircle");
        rune.transform.position = new Vector3(0f, 0.03f, 0f);
        rune.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateAnnulus(platformSize * 0.55f, platformSize * 0.7f, 48);
        rune.AddComponent<MeshRenderer>().sharedMaterial = MakeTransparentMat(new Color(0.6f, 0.2f, 0.7f, 0.5f));

        // --- Точка спавну боса - на вершині трону ---
        GameObject bossSpawn = new GameObject("BossSpawnPoint");
        bossSpawn.transform.position = new Vector3(0f, platformHeight + 0.5f, 0f);

        // --- Круглий портал назад до вежі - в тому самому стилі, що й портал у вежі ---
        Vector3 portalPos = new Vector3(0f, 1.5f, roomSize / 2f - 3f);
        GameObject portalBack = new GameObject("PortalBackToFloor");
        portalBack.transform.position = portalPos;

        const float backPortalRadius = 2.5f;
        for (int p = 0; p < 12; p++)
        {
            float pAngle = 360f / 12 * p * Mathf.Deg2Rad;
            GameObject ringSeg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ringSeg.name = "RingSeg_" + p;
            ringSeg.transform.SetParent(portalBack.transform, false);
            ringSeg.transform.localScale = new Vector3(0.35f, 0.5f, 0.35f);
            ringSeg.transform.localPosition = new Vector3(Mathf.Cos(pAngle) * backPortalRadius, Mathf.Sin(pAngle) * backPortalRadius, 0f);
            ringSeg.transform.localRotation = Quaternion.Euler(0f, 0f, 90f - p * 30f);
            ringSeg.GetComponent<Renderer>().sharedMaterial = wallMat;
        }
        GameObject backGlow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        backGlow.name = "PortalGlow";
        Object.DestroyImmediate(backGlow.GetComponent<Collider>());
        backGlow.transform.SetParent(portalBack.transform, false);
        backGlow.transform.localScale = new Vector3(backPortalRadius * 1.9f, 0.05f, backPortalRadius * 1.9f);
        backGlow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        backGlow.GetComponent<Renderer>().sharedMaterial = MakeTransparentMat(new Color(0.4f, 0.7f, 0.9f, 0.6f));

        BoxCollider col = portalBack.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(backPortalRadius * 1.8f, backPortalRadius * 1.8f, 1.5f);
        ScenePortal script = portalBack.AddComponent<ScenePortal>();
        script.targetSceneName = "SampleScene";
        script.targetSpawnPointName = "TowerSpawnPoint";

        // --- Точка спавну гравця в залі - тепер біля підніжжя сходів, обличчям до трону/боса ---
        GameObject arenaSpawn = new GameObject("SpawnPoint");
        arenaSpawn.transform.position = new Vector3(0f, 0f, platformSize / 2f + 3.5f);
        arenaSpawn.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        Debug.Log("Епічну арену боса 30х30 створено: темний камінь, кристалічні формації по кутах, драматичне підсвітлення, тріщини на підлозі. НЕ ЗАБУДЬ: 1) Зберегти сцену під назвою \"BossChamber\", 2) Додати обидві сцени в Build Profiles → Scene List, 3) Створити боса командою нижче.");
    }

    [MenuItem("Вежа/Локації/Створити перший поверх (місто і природа)")]
    public static void SetupFirstFloorTownAndNature()
    {
        // --- Дуже велика земля ---
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
        }
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(20f, 1f, 20f); // площина 10x10 * 20 = 200x200

        if (GameObject.Find("Town") != null)
        {
            Debug.Log("Місто вже є в сцені — нічого не додаю повторно.");
            return;
        }

        // --- Кільце будиночків навколо вежі (вежа лишається головним монументом площі) ---
        GameObject townRoot = new GameObject("Town");
        int buildingCount = 8;
        float townRadius = 22f;
        for (int i = 0; i < buildingCount; i++)
        {
            float angle = (360f / buildingCount) * i * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * townRadius, 0f, Mathf.Sin(angle) * townRadius);

            GameObject building = new GameObject("Building_" + i);
            building.transform.SetParent(townRoot.transform);
            building.transform.position = pos;
            building.transform.LookAt(new Vector3(0f, pos.y, 0f)); // фасадом до центру площі

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(building.transform);
            body.transform.localPosition = new Vector3(0f, 2f, 0f);
            body.transform.localScale = new Vector3(6f, 4f, 6f);

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(building.transform);
            roof.transform.localPosition = new Vector3(0f, 4.5f, 0f);
            roof.transform.localScale = new Vector3(5f, 1f, 5f);
            roof.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        }

        // --- Дика природа далі назовні (майбутнє місце для мобів) ---
        GameObject wildRoot = new GameObject("Wilderness");
        int treeCount = 50;
        for (int i = 0; i < treeCount; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(townRadius + 15f, 90f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            GameObject tree = new GameObject("Tree_" + i);
            tree.transform.SetParent(wildRoot.transform);
            tree.transform.position = pos;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform);
            float trunkHeight = Random.Range(1.5f, 2.5f);
            trunk.transform.localScale = new Vector3(0.3f, trunkHeight, 0.3f);
            trunk.transform.localPosition = new Vector3(0f, trunkHeight, 0f);

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Canopy";
            canopy.transform.SetParent(tree.transform);
            float canopyScale = Random.Range(1.8f, 3f);
            canopy.transform.localScale = new Vector3(canopyScale, canopyScale, canopyScale);
            canopy.transform.localPosition = new Vector3(0f, trunkHeight * 2f + canopyScale * 0.3f, 0f);
        }

        Debug.Log("Перший поверх готовий: місто кільцем навколо вежі, дика природа з деревами розкидана далі назовні.");
    }

    [MenuItem("Вежа/Локації/Розширити світ і перенести вежу вбік")]
    public static void ExpandWorldAndRelocateTower()
    {
        // --- Земля стає ще у ~4 рази більшою ---
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            ground.transform.localScale = new Vector3(40f, 1f, 40f); // 10 * 40 = 400x400
        }

        // --- Прибираємо старе місто й ліс, будуємо заново під новий масштаб ---
        GameObject oldTown = GameObject.Find("Town");
        if (oldTown != null) Object.DestroyImmediate(oldTown);
        GameObject oldWild = GameObject.Find("Wilderness");
        if (oldWild != null) Object.DestroyImmediate(oldWild);

        float townRadius = 30f;
        GameObject townRoot = new GameObject("Town");
        int buildingCount = 10;
        for (int i = 0; i < buildingCount; i++)
        {
            float angle = (360f / buildingCount) * i * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * townRadius, 0f, Mathf.Sin(angle) * townRadius);

            GameObject building = new GameObject("Building_" + i);
            building.transform.SetParent(townRoot.transform);
            building.transform.position = pos;
            building.transform.LookAt(new Vector3(0f, pos.y, 0f));

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(building.transform);
            body.transform.localPosition = new Vector3(0f, 2f, 0f);
            body.transform.localScale = new Vector3(6f, 4f, 6f);

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(building.transform);
            roof.transform.localPosition = new Vector3(0f, 4.5f, 0f);
            roof.transform.localScale = new Vector3(5f, 1f, 5f);
            roof.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        }

        GameObject wildRoot = new GameObject("Wilderness");
        int treeCount = 140;
        for (int i = 0; i < treeCount; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(townRadius + 15f, 180f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            GameObject tree = new GameObject("Tree_" + i);
            tree.transform.SetParent(wildRoot.transform);
            tree.transform.position = pos;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform);
            float trunkHeight = Random.Range(1.5f, 2.5f);
            trunk.transform.localScale = new Vector3(0.3f, trunkHeight, 0.3f);
            trunk.transform.localPosition = new Vector3(0f, trunkHeight, 0f);

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Canopy";
            canopy.transform.SetParent(tree.transform);
            float canopyScale = Random.Range(1.8f, 3f);
            canopy.transform.localScale = new Vector3(canopyScale, canopyScale, canopyScale);
            canopy.transform.localPosition = new Vector3(0f, trunkHeight * 2f + canopyScale * 0.3f, 0f);
        }

        // --- Фонтан по центру площі (те, що бачить гравець одразу при спавні) ---
        if (GameObject.Find("Fountain") == null)
        {
            GameObject fountain = new GameObject("Fountain");
            fountain.transform.position = Vector3.zero;

            GameObject basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basin.name = "Basin";
            basin.transform.SetParent(fountain.transform);
            basin.transform.localScale = new Vector3(4f, 0.4f, 4f);
            basin.transform.localPosition = new Vector3(0f, 0.4f, 0f);

            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar";
            pillar.transform.SetParent(fountain.transform);
            pillar.transform.localScale = new Vector3(0.5f, 1.2f, 0.5f);
            pillar.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            top.name = "TopBowl";
            top.transform.SetParent(fountain.transform);
            top.transform.localScale = new Vector3(1.5f, 0.5f, 1.5f);
            top.transform.localPosition = new Vector3(0f, 3f, 0f);

            Debug.Log("Фонтан додано по центру площі.");
        }

        // --- Переносимо й збільшуємо вежу далеко в дику природу ---
        GameObject tower = GameObject.Find("Tower");
        Vector3 towerPos = new Vector3(150f, 0f, 150f); // далеко по діагоналі від площі
        if (tower != null)
        {
            tower.transform.position = towerPos;
            tower.transform.localScale = tower.transform.localScale * 2.5f;
            Debug.Log("Вежу перенесено вбік і збільшено — тепер це віддалена ціль, а не одразу видима будівля в центрі.");
        }

        GameObject portal = GameObject.Find("PortalToBossChamber");
        if (portal != null)
        {
            portal.transform.position = towerPos + new Vector3(6f, 1f, 6f);
        }

        Debug.Log("Світ розширено (~400x400). Площа з фонтаном лишається біля точки спавну, вежа тепер далека мета в дикій природі.");
    }

    [MenuItem("Вежа/Локації/Ще збільшити світ і вежу")]
    public static void ExpandWorldFurther()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            ground.transform.localScale = new Vector3(70f, 1f, 70f); // 700x700
        }

        GameObject oldTown = GameObject.Find("Town");
        if (oldTown != null) Object.DestroyImmediate(oldTown);
        GameObject oldWild = GameObject.Find("Wilderness");
        if (oldWild != null) Object.DestroyImmediate(oldWild);

        float townRadius = 45f;
        GameObject townRoot = new GameObject("Town");
        int buildingCount = 12;
        for (int i = 0; i < buildingCount; i++)
        {
            float angle = (360f / buildingCount) * i * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * townRadius, 0f, Mathf.Sin(angle) * townRadius);

            GameObject building = new GameObject("Building_" + i);
            building.transform.SetParent(townRoot.transform);
            building.transform.position = pos;
            building.transform.LookAt(new Vector3(0f, pos.y, 0f));

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(building.transform);
            body.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            body.transform.localScale = new Vector3(7f, 5f, 7f);

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(building.transform);
            roof.transform.localPosition = new Vector3(0f, 5.5f, 0f);
            roof.transform.localScale = new Vector3(6f, 1.2f, 6f);
            roof.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        }

        GameObject wildRoot = new GameObject("Wilderness");
        int treeCount = 200;
        for (int i = 0; i < treeCount; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(townRadius + 20f, 260f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            GameObject tree = new GameObject("Tree_" + i);
            tree.transform.SetParent(wildRoot.transform);
            tree.transform.position = pos;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform);
            float trunkHeight = Random.Range(1.8f, 3f);
            trunk.transform.localScale = new Vector3(0.35f, trunkHeight, 0.35f);
            trunk.transform.localPosition = new Vector3(0f, trunkHeight, 0f);

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Canopy";
            canopy.transform.SetParent(tree.transform);
            float canopyScale = Random.Range(2.2f, 3.6f);
            canopy.transform.localScale = new Vector3(canopyScale, canopyScale, canopyScale);
            canopy.transform.localPosition = new Vector3(0f, trunkHeight * 2f + canopyScale * 0.3f, 0f);
        }

        // фонтан трохи більший, під новий масштаб площі
        GameObject fountain = GameObject.Find("Fountain");
        if (fountain != null)
        {
            fountain.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
        }

        // вежа - ще далі і ще більша (абсолютні значення, не множення на попередній розмір)
        GameObject tower = GameObject.Find("Tower");
        Vector3 towerPos = new Vector3(220f, 0f, 220f);
        if (tower != null)
        {
            tower.transform.position = towerPos;
            tower.transform.localScale = new Vector3(6f, 6f, 6f);
        }

        GameObject portal = GameObject.Find("PortalToBossChamber");
        if (portal != null)
        {
            portal.transform.position = towerPos + new Vector3(10f, 1f, 10f);
        }

        Debug.Log("Світ розширено до ~700x700, вежа тепер за 220м від площі і вшестеро більша за початковий розмір — має бути видна здалеку як орієнтир.");
    }

    [MenuItem("Вежа/Локації/Збільшити вежу в 5 разів")]
    public static void ScaleTowerUp5x()
    {
        GameObject tower = GameObject.Find("Tower");
        if (tower == null)
        {
            Debug.LogWarning("Не знайшов об'єкт \"Tower\" у сцені.");
            return;
        }
        tower.transform.localScale = tower.transform.localScale * 5f;
        Debug.Log("Вежу збільшено в 5 разів. Новий масштаб: " + tower.transform.localScale);
    }

    static void CreateTagIfMissing(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tagName) return;
        }
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
    }

    [MenuItem("Вежа/Локації/Заселити дику природу мобами")]
    public static void PopulateWildlifeMobs()
    {
        CreateTagIfMissing("Enemy");

        if (GameObject.Find("Mobs") != null)
        {
            Debug.Log("Моби вже є в сцені — не додаю повторно.");
            return;
        }
        GameObject mobsRoot = new GameObject("Mobs");

        string[] mobNames = { "Лісовий вовк", "Дикий кабан", "Гігантський павук", "Лісовий розбійник", "Печерний тролль", "Отруйна змія" };

        int mobCount = 45;
        for (int i = 0; i < mobCount; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(60f, 240f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 1f, Mathf.Sin(angle) * radius);

            int lvl = Random.Range(1, 4);
            string chosenName = mobNames[Random.Range(0, mobNames.Length)];
            GameObject mob = BuildMobVisual(chosenName, new System.Random(i + 1));
            mob.name = chosenName + "_" + i;
            mob.transform.SetParent(mobsRoot.transform);
            mob.transform.position = pos;
            mob.tag = "Enemy";

            Health health = mob.AddComponent<Health>();
            health.maxHealth = 30f + lvl * 25f; // рівень 1 = 55, рівень 3 = 105 - бої тривають довше на вищих рівнях

            MobAI ai = mob.AddComponent<MobAI>();
            ai.mobLevel = lvl;
            ai.attackDamage = 3f + lvl * 2f;
            ai.xpReward = 10f * lvl;

            mob.AddComponent<HitFlash>();
            MobHealthBar hpBar = mob.AddComponent<MobHealthBar>();
            hpBar.mobLevel = lvl;

            MobNameTag tag = mob.AddComponent<MobNameTag>();
            tag.mobName = chosenName;
            tag.mobLevel = lvl;
        }

        // додаємо гравцю здоров'я і бойовий скрипт, якщо ще немає
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            if (player.GetComponent<Health>() == null)
            {
                Health playerHealth = player.AddComponent<Health>();
                playerHealth.maxHealth = 100f;
            }
            if (player.GetComponent<PlayerCombat>() == null)
            {
                player.AddComponent<PlayerCombat>();
            }
            if (player.GetComponent<PlayerStats>() == null)
            {
                player.AddComponent<PlayerStats>();
            }
            if (player.GetComponent<Inventory>() == null)
            {
                player.AddComponent<Inventory>();
            }
            if (player.GetComponent<PlayerRespawn>() == null)
            {
                player.AddComponent<PlayerRespawn>();
            }
            if (player.GetComponent<Gold>() == null)
            {
                player.AddComponent<Gold>();
            }
            if (player.GetComponent<Mana>() == null)
            {
                player.AddComponent<Mana>();
            }
            if (player.GetComponent<PlayerSkills>() == null)
            {
                player.AddComponent<PlayerSkills>();
            }
        }

        Debug.Log("Додано " + mobCount + " мобів у дику природу (червоні капсули). Гравець отримав Health, PlayerCombat і навички (Q - магічна хвиля, F - важка атака). Клікай лівою кнопкою миші поруч з мобом, щоб атакувати.");
    }

    [MenuItem("Вежа/UI/Створити HUD (HP гравця)")]
    public static void CreateHealthHUD()
    {
        if (GameObject.Find("HUD_Canvas") != null)
        {
            Debug.Log("HUD вже є в сцені.");
            return;
        }

        GameObject canvasObj = new GameObject("HUD_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject barBg = new GameObject("HealthBarBackground");
        barBg.transform.SetParent(canvasObj.transform);
        Image bgImage = barBg.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.5f);
        RectTransform bgRect = barBg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.pivot = new Vector2(0f, 1f);
        bgRect.anchoredPosition = new Vector2(20f, -20f);
        bgRect.sizeDelta = new Vector2(300f, 42f);

        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(barBg.transform);
        Slider slider = sliderObj.AddComponent<Slider>();
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = new Vector2(4f, 4f);
        sliderRect.offsetMax = new Vector2(-4f, -4f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.8f, 0.15f, 0.15f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 100f;
        slider.handleRect = null;
        slider.interactable = false;

        GameObject textObj = new GameObject("HealthLabel");
        textObj.transform.SetParent(barBg.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = "100 / 100";
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        PlayerHealthUI ui = barBg.AddComponent<PlayerHealthUI>();
        ui.slider = slider;
        ui.label = text;

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Health h = player.GetComponent<Health>();
            if (h == null) h = player.AddComponent<Health>();
            ui.targetHealth = h;
            if (player.GetComponent<HitFlash>() == null) player.AddComponent<HitFlash>();
            if (player.GetComponent<PlayerRespawn>() == null) player.AddComponent<PlayerRespawn>();
        }

        // --- Шкала мани, між HP і досвідом ---
        GameObject manaBg = new GameObject("ManaBarBackground");
        manaBg.transform.SetParent(canvasObj.transform);
        Image manaBgImage = manaBg.AddComponent<Image>();
        manaBgImage.color = new Color(0f, 0f, 0f, 0.5f);
        RectTransform manaBgRect = manaBg.GetComponent<RectTransform>();
        manaBgRect.anchorMin = new Vector2(0f, 1f);
        manaBgRect.anchorMax = new Vector2(0f, 1f);
        manaBgRect.pivot = new Vector2(0f, 1f);
        manaBgRect.anchoredPosition = new Vector2(20f, -66f);
        manaBgRect.sizeDelta = new Vector2(180f, 10f);

        GameObject manaSliderObj = new GameObject("ManaSlider");
        manaSliderObj.transform.SetParent(manaBg.transform);
        Slider manaSlider = manaSliderObj.AddComponent<Slider>();
        RectTransform manaSliderRect = manaSliderObj.GetComponent<RectTransform>();
        manaSliderRect.anchorMin = Vector2.zero;
        manaSliderRect.anchorMax = Vector2.one;
        manaSliderRect.offsetMin = new Vector2(4f, 4f);
        manaSliderRect.offsetMax = new Vector2(-4f, -4f);

        GameObject manaFillArea = new GameObject("Fill Area");
        manaFillArea.transform.SetParent(manaSliderObj.transform);
        RectTransform manaFillAreaRect = manaFillArea.AddComponent<RectTransform>();
        manaFillAreaRect.anchorMin = Vector2.zero;
        manaFillAreaRect.anchorMax = Vector2.one;
        manaFillAreaRect.offsetMin = Vector2.zero;
        manaFillAreaRect.offsetMax = Vector2.zero;

        GameObject manaFill = new GameObject("Fill");
        manaFill.transform.SetParent(manaFillArea.transform);
        Image manaFillImage = manaFill.AddComponent<Image>();
        manaFillImage.color = new Color(0.25f, 0.45f, 0.85f);
        RectTransform manaFillRect = manaFill.GetComponent<RectTransform>();
        manaFillRect.anchorMin = Vector2.zero;
        manaFillRect.anchorMax = Vector2.one;
        manaFillRect.offsetMin = Vector2.zero;
        manaFillRect.offsetMax = Vector2.zero;

        manaSlider.fillRect = manaFillRect;
        manaSlider.targetGraphic = manaFillImage;
        manaSlider.direction = Slider.Direction.LeftToRight;
        manaSlider.minValue = 0f;
        manaSlider.maxValue = 50f;
        manaSlider.value = 50f;
        manaSlider.handleRect = null;
        manaSlider.interactable = false;

        GameObject manaTextObj = new GameObject("ManaLabel");
        manaTextObj.transform.SetParent(manaBg.transform);
        Text manaText = manaTextObj.AddComponent<Text>();
        manaText.text = "50 / 50";
        manaText.alignment = TextAnchor.MiddleCenter;
        manaText.color = Color.white;
        manaText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        manaText.fontSize = 8;
        RectTransform manaTextRect = manaTextObj.GetComponent<RectTransform>();
        manaTextRect.anchorMin = Vector2.zero;
        manaTextRect.anchorMax = Vector2.one;
        manaTextRect.offsetMin = Vector2.zero;
        manaTextRect.offsetMax = Vector2.zero;

        ManaUI manaUi = manaBg.AddComponent<ManaUI>();
        manaUi.slider = manaSlider;
        manaUi.label = manaText;

        if (player != null)
        {
            Mana mana = player.GetComponent<Mana>();
            if (mana == null) mana = player.AddComponent<Mana>();
            manaUi.targetMana = mana;
        }

        // --- Рівень (маленький підпис зверху) і шкала досвіду з числами ---
        GameObject levelLabelObj = new GameObject("LevelLabel");
        levelLabelObj.transform.SetParent(canvasObj.transform);
        Text levelText = levelLabelObj.AddComponent<Text>();
        levelText.text = "Рівень 1 · Очки: 0";
        levelText.alignment = TextAnchor.MiddleLeft;
        levelText.color = new Color(0.9f, 0.85f, 0.6f);
        levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        levelText.fontSize = 14;
        RectTransform levelTextRect = levelLabelObj.GetComponent<RectTransform>();
        levelTextRect.anchorMin = new Vector2(0f, 1f);
        levelTextRect.anchorMax = new Vector2(0f, 1f);
        levelTextRect.pivot = new Vector2(0f, 1f);
        levelTextRect.anchoredPosition = new Vector2(22f, -80f);
        levelTextRect.sizeDelta = new Vector2(260f, 18f);

        GameObject xpBg = new GameObject("XpBarBackground");
        xpBg.transform.SetParent(canvasObj.transform);
        Image xpBgImage = xpBg.AddComponent<Image>();
        xpBgImage.color = new Color(0f, 0f, 0f, 0.5f);
        RectTransform xpBgRect = xpBg.GetComponent<RectTransform>();
        xpBgRect.anchorMin = new Vector2(0f, 1f);
        xpBgRect.anchorMax = new Vector2(0f, 1f);
        xpBgRect.pivot = new Vector2(0f, 1f);
        xpBgRect.anchoredPosition = new Vector2(20f, -98f);
        xpBgRect.sizeDelta = new Vector2(180f, 10f);

        GameObject xpSliderObj = new GameObject("XpSlider");
        xpSliderObj.transform.SetParent(xpBg.transform);
        Slider xpSlider = xpSliderObj.AddComponent<Slider>();
        RectTransform xpSliderRect = xpSliderObj.GetComponent<RectTransform>();
        xpSliderRect.anchorMin = Vector2.zero;
        xpSliderRect.anchorMax = Vector2.one;
        xpSliderRect.offsetMin = new Vector2(3f, 3f);
        xpSliderRect.offsetMax = new Vector2(-3f, -3f);

        GameObject xpFillArea = new GameObject("Fill Area");
        xpFillArea.transform.SetParent(xpSliderObj.transform);
        RectTransform xpFillAreaRect = xpFillArea.AddComponent<RectTransform>();
        xpFillAreaRect.anchorMin = Vector2.zero;
        xpFillAreaRect.anchorMax = Vector2.one;
        xpFillAreaRect.offsetMin = Vector2.zero;
        xpFillAreaRect.offsetMax = Vector2.zero;

        GameObject xpFill = new GameObject("Fill");
        xpFill.transform.SetParent(xpFillArea.transform);
        Image xpFillImage = xpFill.AddComponent<Image>();
        xpFillImage.color = new Color(0.55f, 0.75f, 0.25f);
        RectTransform xpFillRect = xpFill.GetComponent<RectTransform>();
        xpFillRect.anchorMin = Vector2.zero;
        xpFillRect.anchorMax = Vector2.one;
        xpFillRect.offsetMin = Vector2.zero;
        xpFillRect.offsetMax = Vector2.zero;

        xpSlider.fillRect = xpFillRect;
        xpSlider.targetGraphic = xpFillImage;
        xpSlider.direction = Slider.Direction.LeftToRight;
        xpSlider.minValue = 0f;
        xpSlider.maxValue = 50f;
        xpSlider.value = 0f;
        xpSlider.handleRect = null;
        xpSlider.interactable = false;

        GameObject xpNumbersObj = new GameObject("XpNumbers");
        xpNumbersObj.transform.SetParent(xpBg.transform);
        Text xpNumbersText = xpNumbersObj.AddComponent<Text>();
        xpNumbersText.text = "0 / 50";
        xpNumbersText.alignment = TextAnchor.MiddleCenter;
        xpNumbersText.color = Color.white;
        xpNumbersText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        xpNumbersText.fontSize = 8;
        RectTransform xpNumbersRect = xpNumbersObj.GetComponent<RectTransform>();
        xpNumbersRect.anchorMin = Vector2.zero;
        xpNumbersRect.anchorMax = Vector2.one;
        xpNumbersRect.offsetMin = Vector2.zero;
        xpNumbersRect.offsetMax = Vector2.zero;

        LevelUI levelUi = xpBg.AddComponent<LevelUI>();
        levelUi.xpSlider = xpSlider;
        levelUi.levelLabel = levelText;
        levelUi.xpLabel = xpNumbersText;

        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats == null) stats = player.AddComponent<PlayerStats>();
            levelUi.targetStats = stats;
        }

        // --- Золото ---
        GameObject goldLabelObj = new GameObject("GoldLabel");
        goldLabelObj.transform.SetParent(canvasObj.transform);
        Text goldText = goldLabelObj.AddComponent<Text>();
        goldText.text = "Золото: 0";
        goldText.alignment = TextAnchor.MiddleLeft;
        goldText.color = new Color(0.95f, 0.8f, 0.25f);
        goldText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        goldText.fontSize = 14;
        RectTransform goldTextRect = goldLabelObj.GetComponent<RectTransform>();
        goldTextRect.anchorMin = new Vector2(0f, 1f);
        goldTextRect.anchorMax = new Vector2(0f, 1f);
        goldTextRect.pivot = new Vector2(0f, 1f);
        goldTextRect.anchoredPosition = new Vector2(22f, -110f);
        goldTextRect.sizeDelta = new Vector2(260f, 20f);

        GoldUI goldUi = goldLabelObj.AddComponent<GoldUI>();
        goldUi.label = goldText;
        if (player != null)
        {
            Gold gold = player.GetComponent<Gold>();
            if (gold == null) gold = player.AddComponent<Gold>();
            goldUi.targetGold = gold;
        }

        Debug.Log("HUD зі шкалами здоров'я, мани, досвіду й золота створено у верхньому лівому куті екрана.");
    }

    static Button CreateMenuButton(Transform parent, string label, Vector2 anchoredPos, Vector2 size)
    {
        GameObject btnObj = new GameObject(label + "Button");
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.18f, 0.16f, 0.14f, 0.95f);
        Button btn = btnObj.AddComponent<Button>();
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 22;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        return btn;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
        Debug.Log("Додано EventSystem - без нього жодна UI-кнопка не реагує на клік.");
    }

    [MenuItem("Вежа/UI/Створити головне меню (в поточній сцені)")]
    public static void CreateMainMenu()
    {
        EnsureEventSystem();

        if (GameObject.Find("MenuCanvas") != null)
        {
            Debug.Log("Головне меню вже є в цій сцені.");
            return;
        }

        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.09f, 0.08f, 0.07f, 1f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        Text title = titleObj.AddComponent<Text>();
        title.text = "ВЕЖА";
        title.alignment = TextAnchor.MiddleCenter;
        title.color = new Color(0.91f, 0.64f, 0.24f);
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 64;
        title.fontStyle = FontStyle.Bold;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -100f);
        titleRect.sizeDelta = new Vector2(600f, 100f);

        GameObject menuPanel = new GameObject("MenuButtons");
        menuPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform menuRect = menuPanel.AddComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuRect.pivot = new Vector2(0.5f, 0.5f);
        menuRect.anchoredPosition = Vector2.zero;
        menuRect.sizeDelta = new Vector2(320f, 220f);

        Button playBtn = CreateMenuButton(menuPanel.transform, "Грати", new Vector2(0f, 70f), new Vector2(300f, 60f));
        Button settingsBtn = CreateMenuButton(menuPanel.transform, "Налаштування", new Vector2(0f, 0f), new Vector2(300f, 60f));
        Button quitBtn = CreateMenuButton(menuPanel.transform, "Вихід", new Vector2(0f, -70f), new Vector2(300f, 60f));

        // проста панель налаштувань (поки заглушка - наповнимо реальними опціями пізніше)
        GameObject settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvasObj.transform, false);
        Image settingsBg = settingsPanel.AddComponent<Image>();
        settingsBg.color = new Color(0.05f, 0.05f, 0.05f, 0.97f);
        RectTransform settingsRect = settingsPanel.GetComponent<RectTransform>();
        settingsRect.anchorMin = new Vector2(0.5f, 0.5f);
        settingsRect.anchorMax = new Vector2(0.5f, 0.5f);
        settingsRect.pivot = new Vector2(0.5f, 0.5f);
        settingsRect.sizeDelta = new Vector2(420f, 260f);
        settingsRect.anchoredPosition = Vector2.zero;

        GameObject settingsTitleObj = new GameObject("SettingsTitle");
        settingsTitleObj.transform.SetParent(settingsPanel.transform, false);
        Text settingsTitle = settingsTitleObj.AddComponent<Text>();
        settingsTitle.text = "Налаштування";
        settingsTitle.alignment = TextAnchor.MiddleCenter;
        settingsTitle.color = Color.white;
        settingsTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        settingsTitle.fontSize = 28;
        RectTransform settingsTitleRect = settingsTitleObj.GetComponent<RectTransform>();
        settingsTitleRect.anchorMin = new Vector2(0.5f, 1f);
        settingsTitleRect.anchorMax = new Vector2(0.5f, 1f);
        settingsTitleRect.pivot = new Vector2(0.5f, 1f);
        settingsTitleRect.anchoredPosition = new Vector2(0f, -20f);
        settingsTitleRect.sizeDelta = new Vector2(380f, 40f);

        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(settingsPanel.transform, false);
        Text placeholder = placeholderObj.AddComponent<Text>();
        placeholder.text = "Тут згодом з'являться реальні опції\n(звук, графіка тощо).";
        placeholder.alignment = TextAnchor.MiddleCenter;
        placeholder.color = new Color(0.7f, 0.7f, 0.7f);
        placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholder.fontSize = 16;
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = new Vector2(0.5f, 0.5f);
        placeholderRect.anchorMax = new Vector2(0.5f, 0.5f);
        placeholderRect.pivot = new Vector2(0.5f, 0.5f);
        placeholderRect.anchoredPosition = new Vector2(0f, 10f);
        placeholderRect.sizeDelta = new Vector2(360f, 60f);

        Button backBtn = CreateMenuButton(settingsPanel.transform, "Назад", new Vector2(0f, -90f), new Vector2(200f, 50f));
        settingsPanel.SetActive(false);

        MainMenuController controller = canvasObj.AddComponent<MainMenuController>();
        controller.settingsPanel = settingsPanel;
        controller.gameSceneName = "SampleScene";

        playBtn.onClick.AddListener(controller.OnPlay);
        settingsBtn.onClick.AddListener(controller.OnOpenSettings);
        backBtn.onClick.AddListener(controller.OnCloseSettings);
        quitBtn.onClick.AddListener(controller.OnQuit);

        Debug.Log("Головне меню створено. Не забудь: 1) зберегти цю сцену під назвою \"MainMenu\", 2) додати її в Build Profiles > Scene List ПЕРШОЮ (індекс 0), 3) перевірити, що controller.gameSceneName точно збігається з назвою твоєї ігрової сцени.");
    }

    [MenuItem("Вежа/UI/Створити інвентар (клавіша I)")]
    public static void CreateInventoryUI()
    {
        EnsureEventSystem();

        GameObject oldCanvas = GameObject.Find("InventoryCanvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        GameObject canvasObj = new GameObject("InventoryCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("InventoryPanel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.06f, 0.06f, 0.06f, 0.97f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(940f, 620f);
        panelRect.anchoredPosition = Vector2.zero;

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        Text title = titleObj.AddComponent<Text>();
        title.text = "Інвентар (I)";
        title.alignment = TextAnchor.MiddleCenter;
        title.color = new Color(0.9f, 0.65f, 0.25f);
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 24;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -14f);
        titleRect.sizeDelta = new Vector2(900f, 34f);

        // ===================== ЛІВА КОЛОНКА: екіпіровка + статистика =====================
        string[] slotLabelsLocal = { "Зброя", "Броня", "Шолом", "Плащ", "Взуття" };
        Text[] equippedTexts = new Text[5];
        Button[] unequipBtns = new Button[5];

        for (int i = 0; i < 5; i++)
        {
            GameObject row = new GameObject("EquipRow_" + i);
            row.transform.SetParent(panel.transform, false);
            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(0f, 1f);
            rowRect.pivot = new Vector2(0f, 1f);
            rowRect.anchoredPosition = new Vector2(20f, -60f - i * 36f);
            rowRect.sizeDelta = new Vector2(260f, 30f);

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            Text label = labelObj.AddComponent<Text>();
            label.text = slotLabelsLocal[i] + ": — порожньо —";
            label.alignment = TextAnchor.MiddleLeft;
            label.color = Color.gray;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 13;
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.68f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            equippedTexts[i] = label;

            GameObject btnObj = new GameObject("UnequipButton");
            btnObj.transform.SetParent(row.transform, false);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.25f, 0.2f, 0.18f);
            Button btn = btnObj.AddComponent<Button>();
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.7f, 0.1f);
            btnRect.anchorMax = new Vector2(1f, 0.9f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            Text btnText = btnTextObj.AddComponent<Text>();
            btnText.text = "Зняти";
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 11;
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            unequipBtns[i] = btn;
        }

        GameObject statsTitleObj = new GameObject("StatsTitle");
        statsTitleObj.transform.SetParent(panel.transform, false);
        Text statsTitle = statsTitleObj.AddComponent<Text>();
        statsTitle.text = "Характеристики";
        statsTitle.alignment = TextAnchor.MiddleLeft;
        statsTitle.color = new Color(0.9f, 0.65f, 0.25f);
        statsTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statsTitle.fontSize = 16;
        RectTransform statsTitleRect = statsTitleObj.GetComponent<RectTransform>();
        statsTitleRect.anchorMin = new Vector2(0f, 1f);
        statsTitleRect.anchorMax = new Vector2(0f, 1f);
        statsTitleRect.pivot = new Vector2(0f, 1f);
        statsTitleRect.anchoredPosition = new Vector2(20f, -252f);
        statsTitleRect.sizeDelta = new Vector2(260f, 24f);

        GameObject statsTextObj = new GameObject("StatsText");
        statsTextObj.transform.SetParent(panel.transform, false);
        Text statsText = statsTextObj.AddComponent<Text>();
        statsText.text = "Атака: 0\nЗахист: 0\nРеген. HP: 0/сек\nРеген. мани: 0/сек";
        statsText.alignment = TextAnchor.UpperLeft;
        statsText.color = new Color(0.85f, 0.85f, 0.85f);
        statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statsText.fontSize = 14;
        statsText.lineSpacing = 1.3f;
        RectTransform statsTextRect = statsTextObj.GetComponent<RectTransform>();
        statsTextRect.anchorMin = new Vector2(0f, 1f);
        statsTextRect.anchorMax = new Vector2(0f, 1f);
        statsTextRect.pivot = new Vector2(0f, 1f);
        statsTextRect.anchoredPosition = new Vector2(20f, -280f);
        statsTextRect.sizeDelta = new Vector2(260f, 100f);

        // Очки навичок + кнопки прокачки
        GameObject skillPointsObj = new GameObject("SkillPointsText");
        skillPointsObj.transform.SetParent(panel.transform, false);
        Text skillPointsText = skillPointsObj.AddComponent<Text>();
        skillPointsText.text = "Очки навичок: 0";
        skillPointsText.alignment = TextAnchor.MiddleLeft;
        skillPointsText.color = new Color(0.6f, 0.85f, 0.95f);
        skillPointsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        skillPointsText.fontSize = 14;
        RectTransform skillPointsRect = skillPointsObj.GetComponent<RectTransform>();
        skillPointsRect.anchorMin = new Vector2(0f, 1f);
        skillPointsRect.anchorMax = new Vector2(0f, 1f);
        skillPointsRect.pivot = new Vector2(0f, 1f);
        skillPointsRect.anchoredPosition = new Vector2(20f, -400f);
        skillPointsRect.sizeDelta = new Vector2(260f, 22f);

        Button upgradeHeavyBtn = CreateMenuButton(panel.transform, "Прокачати важку атаку", Vector2.zero, new Vector2(260f, 34f));
        RectTransform upgHeavyRect = upgradeHeavyBtn.GetComponent<RectTransform>();
        upgHeavyRect.anchorMin = new Vector2(0f, 1f);
        upgHeavyRect.anchorMax = new Vector2(0f, 1f);
        upgHeavyRect.pivot = new Vector2(0f, 1f);
        upgHeavyRect.anchoredPosition = new Vector2(20f, -428f);
        upgradeHeavyBtn.GetComponentInChildren<Text>().fontSize = 12;

        Button upgradeWaveBtn = CreateMenuButton(panel.transform, "Прокачати магічну хвилю", Vector2.zero, new Vector2(260f, 34f));
        RectTransform upgWaveRect = upgradeWaveBtn.GetComponent<RectTransform>();
        upgWaveRect.anchorMin = new Vector2(0f, 1f);
        upgWaveRect.anchorMax = new Vector2(0f, 1f);
        upgWaveRect.pivot = new Vector2(0f, 1f);
        upgWaveRect.anchoredPosition = new Vector2(20f, -466f);
        upgradeWaveBtn.GetComponentInChildren<Text>().fontSize = 12;

        // ===================== ЦЕНТР: золото зверху + силует персонажа =====================
        GameObject goldTextObj = new GameObject("GoldText");
        goldTextObj.transform.SetParent(panel.transform, false);
        Text goldText = goldTextObj.AddComponent<Text>();
        goldText.text = "Золото: 0";
        goldText.alignment = TextAnchor.MiddleCenter;
        goldText.color = new Color(0.95f, 0.8f, 0.25f);
        goldText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        goldText.fontSize = 20;
        RectTransform goldTextRect = goldTextObj.GetComponent<RectTransform>();
        goldTextRect.anchorMin = new Vector2(0.5f, 1f);
        goldTextRect.anchorMax = new Vector2(0.5f, 1f);
        goldTextRect.pivot = new Vector2(0.5f, 1f);
        goldTextRect.anchoredPosition = new Vector2(0f, -60f);
        goldTextRect.sizeDelta = new Vector2(260f, 30f);

        // силует персонажа-заглушка (справжня 3D-модель в UI - окрема велика задача,
        // тут проста форма, що показує де стоїть герой)
        GameObject heroSilhouette = new GameObject("HeroSilhouette");
        heroSilhouette.transform.SetParent(panel.transform, false);
        Image heroImg = heroSilhouette.AddComponent<Image>();
        heroImg.color = new Color(0.3f, 0.32f, 0.4f, 0.6f);
        RectTransform heroRect = heroSilhouette.GetComponent<RectTransform>();
        heroRect.anchorMin = new Vector2(0.5f, 0.5f);
        heroRect.anchorMax = new Vector2(0.5f, 0.5f);
        heroRect.pivot = new Vector2(0.5f, 0.5f);
        heroRect.anchoredPosition = new Vector2(0f, -20f);
        heroRect.sizeDelta = new Vector2(160f, 320f);

        // ===================== ПРАВА КОЛОНКА: прокручувана сумка =====================
        GameObject bagTitleObj = new GameObject("BagTitle");
        bagTitleObj.transform.SetParent(panel.transform, false);
        Text bagTitle = bagTitleObj.AddComponent<Text>();
        bagTitle.text = "Сумка";
        bagTitle.alignment = TextAnchor.MiddleLeft;
        bagTitle.color = new Color(0.9f, 0.65f, 0.25f);
        bagTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bagTitle.fontSize = 18;
        RectTransform bagTitleRect = bagTitleObj.GetComponent<RectTransform>();
        bagTitleRect.anchorMin = new Vector2(1f, 1f);
        bagTitleRect.anchorMax = new Vector2(1f, 1f);
        bagTitleRect.pivot = new Vector2(1f, 1f);
        bagTitleRect.anchoredPosition = new Vector2(-20f, -56f);
        bagTitleRect.sizeDelta = new Vector2(300f, 26f);

        GameObject scrollObj = new GameObject("BagScrollView");
        scrollObj.transform.SetParent(panel.transform, false);
        RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(1f, 0f);
        scrollRect.anchorMax = new Vector2(1f, 1f);
        scrollRect.pivot = new Vector2(1f, 1f);
        scrollRect.anchoredPosition = new Vector2(-20f, -88f);
        scrollRect.sizeDelta = new Vector2(300f, -128f);
        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = new Color(0f, 0f, 0f, 0.25f);
        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        Image viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = new Color(1f, 1f, 1f, 0.01f);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject bagContentObj = new GameObject("Content");
        bagContentObj.transform.SetParent(viewport.transform, false);
        RectTransform bagContentRect = bagContentObj.AddComponent<RectTransform>();
        bagContentRect.anchorMin = new Vector2(0f, 1f);
        bagContentRect.anchorMax = new Vector2(1f, 1f);
        bagContentRect.pivot = new Vector2(0.5f, 1f);
        bagContentRect.anchoredPosition = Vector2.zero;
        bagContentRect.sizeDelta = new Vector2(0f, 0f);
        VerticalLayoutGroup vlg = bagContentObj.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6f;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.padding = new RectOffset(4, 4, 4, 4);
        ContentSizeFitter fitter = bagContentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRect;
        scroll.content = bagContentRect;

        // ===================== Закрити =====================
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(panel.transform, false);
        Image closeBtnImg = closeBtnObj.AddComponent<Image>();
        closeBtnImg.color = new Color(0.18f, 0.16f, 0.14f, 0.95f);
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0f, 16f);
        closeRect.sizeDelta = new Vector2(160f, 40f);

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        Text closeText = closeTextObj.AddComponent<Text>();
        closeText.text = "Закрити";
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.white;
        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeText.fontSize = 16;
        RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;

        // InventoryUI живе на Canvas (завжди активний), а не на панелі, що ховається -
        // інакше клавіша I перестане реагувати після першого закриття
        InventoryUI invUi = canvasObj.AddComponent<InventoryUI>();
        invUi.panelRoot = panel;
        invUi.bagContent = bagContentObj.transform;
        invUi.equippedLabels = equippedTexts;
        invUi.unequipButtons = unequipBtns;
        invUi.statsText = statsText;
        invUi.goldText = goldText;
        invUi.skillPointsText = skillPointsText;

        for (int i = 0; i < 5; i++)
        {
            int idx = i;
            unequipBtns[i].onClick.AddListener(() => invUi.UnequipSlot(idx));
        }
        closeBtn.onClick.AddListener(invUi.ClosePanel);
        upgradeHeavyBtn.onClick.AddListener(invUi.UpgradeHeavy);
        upgradeWaveBtn.onClick.AddListener(invUi.UpgradeWave);

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Inventory inv = player.GetComponent<Inventory>();
            if (inv == null) inv = player.AddComponent<Inventory>();
            invUi.inventory = inv;
        }

        Debug.Log("Інвентар створено: статистика й екіпіровка ліворуч, золото й герой по центру, прокручувана сумка праворуч. Клавіша I - відкрити/закрити.");
    }

    static Material MakeColorMat(Color c)
    {
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.color = c;
        return m;
    }

    static GameObject BuildProceduralTree(int seed)
    {
        System.Random rng = new System.Random(seed);
        GameObject tree = new GameObject("ProcTree");

        float trunkHeight = 3f + (float)rng.NextDouble() * 4f; // 3-7 - помітна різниця у розмірах
        float trunkBaseRadius = 0.22f + (float)rng.NextDouble() * 0.18f;
        float bendX = ((float)rng.NextDouble() - 0.5f) * 0.7f;
        float bendZ = ((float)rng.NextDouble() - 0.5f) * 0.7f;

        GameObject trunk = new GameObject("Trunk");
        trunk.transform.SetParent(tree.transform, false);
        MeshFilter trunkMf = trunk.AddComponent<MeshFilter>();
        Mesh trunkMesh = MeshBuilder.CreateTaperedCylinder(trunkBaseRadius, trunkBaseRadius * 0.35f, trunkHeight, 6, bendX, bendZ);
        trunkMf.mesh = trunkMesh;
        MeshRenderer trunkMr = trunk.AddComponent<MeshRenderer>();
        trunkMr.sharedMaterial = MakeColorMat(new Color(0.32f, 0.22f, 0.14f));
        MeshCollider trunkCol = trunk.AddComponent<MeshCollider>();
        trunkCol.sharedMesh = trunkMesh;

        bool isLush = rng.NextDouble() < 0.35; // деякі дерева суттєво пишніші - більше гілок і листя
        int canopyClumps = isLush ? (7 + rng.Next(0, 5)) : (3 + rng.Next(0, 3));
        Vector3 topOfTrunk = new Vector3(bendX, trunkHeight, bendZ);
        // неперервна варіація кольору через HSV замість 3 фіксованих відтінків - значно більше різноманіття
        float baseHue = 0.24f + (float)rng.NextDouble() * 0.12f; // від жовтувато-зеленого до синювато-зеленого

        for (int i = 0; i < canopyClumps; i++)
        {
            GameObject clump = new GameObject("Canopy_" + i);
            clump.transform.SetParent(tree.transform, false);
            float clumpRadius = (1.1f + (float)rng.NextDouble() * 1.2f) * (trunkHeight / 5f);
            Vector3 offset = new Vector3(
                ((float)rng.NextDouble() - 0.5f) * clumpRadius * 1.3f,
                (float)rng.NextDouble() * clumpRadius * 0.6f,
                ((float)rng.NextDouble() - 0.5f) * clumpRadius * 1.3f
            );
            clump.transform.localPosition = topOfTrunk + offset;
            MeshFilter mf = clump.AddComponent<MeshFilter>();
            Mesh blobMesh = MeshBuilder.CreateBlob(clumpRadius, seed * 10 + i);
            mf.mesh = blobMesh;
            MeshRenderer mr = clump.AddComponent<MeshRenderer>();
            float clumpHue = baseHue + ((float)rng.NextDouble() - 0.5f) * 0.05f;
            float clumpSat = 0.55f + (float)rng.NextDouble() * 0.3f;
            float clumpVal = 0.28f + (float)rng.NextDouble() * 0.25f;
            mr.sharedMaterial = MakeColorMat(Color.HSVToRGB(clumpHue, clumpSat, clumpVal));
            MeshCollider col2 = clump.AddComponent<MeshCollider>();
            col2.sharedMesh = blobMesh;

            // зрідка магічна світлячок-іскра біля крони - для чарівної атмосфери
            if (rng.NextDouble() < 0.1)
            {
                GameObject firefly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                firefly.name = "Firefly";
                Object.DestroyImmediate(firefly.GetComponent<Collider>());
                firefly.transform.SetParent(tree.transform, false);
                firefly.transform.localScale = Vector3.one * 0.12f;
                firefly.transform.localPosition = topOfTrunk + offset + Vector3.up * 0.3f;
                firefly.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.7f, 1f, 0.5f));
            }

            // тонка гілка від верхівки стовбура до цієї крони - додає структури дереву
            GameObject branch = new GameObject("Branch_" + i);
            branch.transform.SetParent(tree.transform, false);
            MeshFilter branchMf = branch.AddComponent<MeshFilter>();
            Mesh branchMesh = MeshBuilder.CreateTaperedCylinder(trunkBaseRadius * 0.3f, trunkBaseRadius * 0.12f, offset.magnitude, 4);
            branchMf.mesh = branchMesh;
            branch.transform.localPosition = topOfTrunk;
            branch.transform.localRotation = Quaternion.FromToRotation(Vector3.up, offset.normalized);
            MeshRenderer branchMr = branch.AddComponent<MeshRenderer>();
            branchMr.sharedMaterial = trunkMr.sharedMaterial;
        }

        return tree;
    }

    static GameObject BuildProceduralBush(int seed)
    {
        System.Random rng = new System.Random(seed);
        GameObject bush = new GameObject("ProcBush");
        int clumps = 2 + rng.Next(0, 2);
        Color bushColor = new Color(0.22f + (float)rng.NextDouble() * 0.08f, 0.4f, 0.16f);

        for (int i = 0; i < clumps; i++)
        {
            GameObject clump = new GameObject("Clump_" + i);
            clump.transform.SetParent(bush.transform, false);
            float r = 0.5f + (float)rng.NextDouble() * 0.4f;
            clump.transform.localPosition = new Vector3(
                ((float)rng.NextDouble() - 0.5f) * 0.6f,
                r * 0.5f,
                ((float)rng.NextDouble() - 0.5f) * 0.6f
            );
            MeshFilter mf = clump.AddComponent<MeshFilter>();
            Mesh blobMesh = MeshBuilder.CreateBlob(r, seed * 10 + i);
            mf.mesh = blobMesh;
            MeshRenderer mr = clump.AddComponent<MeshRenderer>();
            mr.sharedMaterial = MakeColorMat(bushColor);
            MeshCollider col = clump.AddComponent<MeshCollider>();
            col.sharedMesh = blobMesh;
        }
        return bush;
    }

    static GameObject BuildProceduralPineTree(int seed)
    {
        System.Random rng = new System.Random(seed);
        GameObject tree = new GameObject("ProcPineTree");

        float trunkHeight = 4f + (float)rng.NextDouble() * 3f;
        float trunkRadius = 0.2f + (float)rng.NextDouble() * 0.1f;

        GameObject trunk = new GameObject("Trunk");
        trunk.transform.SetParent(tree.transform, false);
        Mesh trunkMesh = MeshBuilder.CreateTaperedCylinder(trunkRadius, trunkRadius * 0.5f, trunkHeight, 6);
        trunk.AddComponent<MeshFilter>().mesh = trunkMesh;
        Material barkMat = MakeColorMat(new Color(0.3f, 0.2f, 0.13f));
        trunk.AddComponent<MeshRenderer>().sharedMaterial = barkMat;
        trunk.AddComponent<MeshCollider>().sharedMesh = trunkMesh;

        // кілька конічних ярусів хвої, що звужуються догори - класична ялина
        int tiers = 4 + rng.Next(0, 3);
        Material needleMat = MakeColorMat(new Color(0.12f + (float)rng.NextDouble() * 0.05f, 0.3f, 0.16f));
        float tierZoneHeight = trunkHeight * 0.85f;
        for (int t = 0; t < tiers; t++)
        {
            float tFrac = (float)t / tiers;
            float tierY = trunkHeight * 0.3f + tFrac * tierZoneHeight;
            float tierRadius = (1f - tFrac * 0.75f) * (0.9f + (float)rng.NextDouble() * 0.3f);
            float tierHeight = tierZoneHeight / tiers * 1.4f;

            GameObject tier = new GameObject("Tier_" + t);
            tier.transform.SetParent(tree.transform, false);
            tier.transform.localPosition = new Vector3(0f, tierY, 0f);
            Mesh tierMesh = MeshBuilder.CreateTaperedCylinder(tierRadius, 0.03f, tierHeight, 8);
            tier.AddComponent<MeshFilter>().mesh = tierMesh;
            tier.AddComponent<MeshRenderer>().sharedMaterial = needleMat;
            tier.AddComponent<MeshCollider>().sharedMesh = tierMesh;
        }

        return tree;
    }

    static GameObject BuildProceduralGiantTree(int seed)
    {
        System.Random rng = new System.Random(seed);
        GameObject tree = new GameObject("ProcGiantTree");

        // справжній "герой-велет" лісу - у 3-4 рази більший за звичайне дерево (5м), а не просто трохи вищий
        float trunkHeight = 22f + (float)rng.NextDouble() * 12f;
        float trunkBaseRadius = 1.3f + (float)rng.NextDouble() * 0.6f;
        float bendX = ((float)rng.NextDouble() - 0.5f) * 1.5f;
        float bendZ = ((float)rng.NextDouble() - 0.5f) * 1.5f;

        GameObject trunk = new GameObject("Trunk");
        trunk.transform.SetParent(tree.transform, false);
        Mesh trunkMesh = MeshBuilder.CreateTaperedCylinder(trunkBaseRadius, trunkBaseRadius * 0.35f, trunkHeight, 10, bendX, bendZ);
        trunk.AddComponent<MeshFilter>().mesh = trunkMesh;
        Material barkMat = MakeColorMat(new Color(0.26f, 0.18f, 0.11f));
        trunk.AddComponent<MeshRenderer>().sharedMaterial = barkMat;
        trunk.AddComponent<MeshCollider>().sharedMesh = trunkMesh;

        // вузлувата кора - кілька грудкуватих наростів уздовж стовбура
        int knotCount = 8 + rng.Next(0, 6);
        for (int k = 0; k < knotCount; k++)
        {
            float t = (float)rng.NextDouble();
            float knotY = t * trunkHeight * 0.85f;
            float knotAngle = (float)rng.NextDouble() * 360f * Mathf.Deg2Rad;
            float radiusHere = Mathf.Lerp(trunkBaseRadius, trunkBaseRadius * 0.4f, t);
            GameObject knot = new GameObject("Knot_" + k);
            knot.transform.SetParent(trunk.transform, false);
            knot.transform.localPosition = new Vector3(Mathf.Cos(knotAngle) * radiusHere * 0.9f, knotY, Mathf.Sin(knotAngle) * radiusHere * 0.9f);
            float knotSize = 0.25f + (float)rng.NextDouble() * 0.35f;
            Mesh knotMesh = MeshBuilder.CreateBlob(knotSize, seed * 100 + k);
            knot.AddComponent<MeshFilter>().mesh = knotMesh;
            knot.AddComponent<MeshRenderer>().sharedMaterial = barkMat;
        }

        // коренева "спідниця" внизу стовбура - товсті виступаючі корені
        int rootCount = 5 + rng.Next(0, 3);
        for (int r = 0; r < rootCount; r++)
        {
            float rootAngle = (360f / rootCount * r + (float)rng.NextDouble() * 20f) * Mathf.Deg2Rad;
            GameObject root = new GameObject("Root_" + r);
            root.transform.SetParent(trunk.transform, false);
            Mesh rootMesh = MeshBuilder.CreateTaperedCylinder(trunkBaseRadius * 0.5f, trunkBaseRadius * 0.15f, trunkBaseRadius * 2.2f, 5);
            root.AddComponent<MeshFilter>().mesh = rootMesh;
            root.AddComponent<MeshRenderer>().sharedMaterial = barkMat;
            root.transform.localPosition = new Vector3(Mathf.Cos(rootAngle) * trunkBaseRadius * 0.5f, 0f, Mathf.Sin(rootAngle) * trunkBaseRadius * 0.5f);
            root.transform.localRotation = Quaternion.FromToRotation(Vector3.up, new Vector3(Mathf.Cos(rootAngle) * 0.6f, 1f, Mathf.Sin(rootAngle) * 0.6f).normalized);
        }

        // багато великих, пишних крон навколо верхівки - справжній велет лісу
        int canopyClumps = 8 + rng.Next(0, 5);
        Vector3 topOfTrunk = new Vector3(bendX, trunkHeight, bendZ);
        Color[] greenShades = {
            new Color(0.18f, 0.4f, 0.15f), new Color(0.24f, 0.46f, 0.19f), new Color(0.15f, 0.34f, 0.13f),
        };

        for (int i = 0; i < canopyClumps; i++)
        {
            GameObject clump = new GameObject("Canopy_" + i);
            clump.transform.SetParent(tree.transform, false);
            float clumpRadius = 4f + (float)rng.NextDouble() * 3.5f;
            Vector3 offset = new Vector3(
                ((float)rng.NextDouble() - 0.5f) * clumpRadius * 1.5f,
                (float)rng.NextDouble() * clumpRadius * 0.8f,
                ((float)rng.NextDouble() - 0.5f) * clumpRadius * 1.5f
            );
            clump.transform.localPosition = topOfTrunk + offset;
            Mesh blobMesh = MeshBuilder.CreateBlob(clumpRadius, seed * 10 + i);
            clump.AddComponent<MeshFilter>().mesh = blobMesh;
            clump.AddComponent<MeshRenderer>().sharedMaterial = MakeColorMat(greenShades[rng.Next(0, greenShades.Length)]);
            clump.AddComponent<MeshCollider>().sharedMesh = blobMesh;
        }

        return tree;
    }

    static GameObject BuildProceduralDeadTree(int seed)
    {
        System.Random rng = new System.Random(seed);
        GameObject tree = new GameObject("ProcDeadTree");
        Material barkMat = MakeColorMat(new Color(0.32f, 0.28f, 0.24f));

        float trunkHeight = 3f + (float)rng.NextDouble() * 3f;
        GameObject trunk = new GameObject("Trunk");
        trunk.transform.SetParent(tree.transform, false);
        Mesh trunkMesh = MeshBuilder.CreateTaperedCylinder(0.22f, 0.08f, trunkHeight, 6);
        trunk.AddComponent<MeshFilter>().mesh = trunkMesh;
        trunk.AddComponent<MeshRenderer>().sharedMaterial = barkMat;
        trunk.AddComponent<MeshCollider>().sharedMesh = trunkMesh;

        // голі гілки без крони - для похмурого урізноманітнення лісу
        int branchCount = 3 + rng.Next(0, 4);
        for (int i = 0; i < branchCount; i++)
        {
            float branchY = trunkHeight * (0.4f + (float)rng.NextDouble() * 0.5f);
            float branchLen = 0.8f + (float)rng.NextDouble() * 1.2f;
            Vector3 dir = new Vector3(((float)rng.NextDouble() - 0.5f) * 2f, 0.6f + (float)rng.NextDouble() * 0.6f, ((float)rng.NextDouble() - 0.5f) * 2f).normalized;

            GameObject branch = new GameObject("Branch_" + i);
            branch.transform.SetParent(tree.transform, false);
            Mesh branchMesh = MeshBuilder.CreateTaperedCylinder(0.06f, 0.02f, branchLen, 4);
            branch.AddComponent<MeshFilter>().mesh = branchMesh;
            branch.transform.localPosition = new Vector3(0f, branchY, 0f);
            branch.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
            branch.AddComponent<MeshRenderer>().sharedMaterial = barkMat;
        }

        return tree;
    }

    static GameObject BuildProceduralBlossomTree(int seed)
    {
        System.Random rng = new System.Random(seed);
        GameObject tree = new GameObject("ProcBlossomTree");

        float trunkHeight = 3f + (float)rng.NextDouble() * 2.5f;
        float trunkBaseRadius = 0.2f + (float)rng.NextDouble() * 0.12f;

        GameObject trunk = new GameObject("Trunk");
        trunk.transform.SetParent(tree.transform, false);
        Mesh trunkMesh = MeshBuilder.CreateTaperedCylinder(trunkBaseRadius, trunkBaseRadius * 0.4f, trunkHeight, 6);
        trunk.AddComponent<MeshFilter>().mesh = trunkMesh;
        trunk.AddComponent<MeshRenderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.22f, 0.16f));
        trunk.AddComponent<MeshCollider>().sharedMesh = trunkMesh;

        int canopyClumps = 3 + rng.Next(0, 3);
        Color[] blossomShades = {
            new Color(0.92f, 0.72f, 0.8f), new Color(0.95f, 0.82f, 0.85f), new Color(0.88f, 0.6f, 0.72f),
        };
        for (int i = 0; i < canopyClumps; i++)
        {
            GameObject clump = new GameObject("Blossom_" + i);
            clump.transform.SetParent(tree.transform, false);
            float clumpRadius = 0.9f + (float)rng.NextDouble() * 0.9f;
            Vector3 offset = new Vector3(
                ((float)rng.NextDouble() - 0.5f) * clumpRadius * 1.3f,
                trunkHeight * 0.15f + (float)rng.NextDouble() * clumpRadius * 0.6f,
                ((float)rng.NextDouble() - 0.5f) * clumpRadius * 1.3f
            );
            clump.transform.localPosition = new Vector3(0f, trunkHeight, 0f) + offset;
            Mesh blobMesh = MeshBuilder.CreateBlob(clumpRadius, seed * 10 + i);
            clump.AddComponent<MeshFilter>().mesh = blobMesh;
            clump.AddComponent<MeshRenderer>().sharedMaterial = MakeColorMat(blossomShades[rng.Next(0, blossomShades.Length)]);
            clump.AddComponent<MeshCollider>().sharedMesh = blobMesh;
        }

        return tree;
    }

    static GameObject BuildProceduralHouse(int seed, float width, float depth, float floorHeight, int floors)
    {
        System.Random rng = new System.Random(seed);
        floors = Mathf.Clamp(floors, 1, 3);
        GameObject house = new GameObject("ProcHouse");

        Color[] floorTints = {
            new Color(0.72f + (float)rng.NextDouble() * 0.1f, 0.66f, 0.54f),
            new Color(0.68f + (float)rng.NextDouble() * 0.1f, 0.6f, 0.5f),
            new Color(0.64f + (float)rng.NextDouble() * 0.1f, 0.56f, 0.47f),
        };

        for (int floor = 0; floor < floors; floor++)
        {
            float floorWidth = width - floor * 0.3f; // кожен наступний поверх трохи вужчий - виглядає природніше
            float floorY = floor * floorHeight;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body_Floor" + floor;
            body.transform.SetParent(house.transform, false);
            body.transform.localScale = new Vector3(floorWidth, floorHeight, depth - floor * 0.3f);
            body.transform.localPosition = new Vector3(0f, floorY + floorHeight / 2f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(floorTints[floor % floorTints.Length]);

            if (floor == 0)
            {
                GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
                door.name = "Door";
                Object.DestroyImmediate(door.GetComponent<Collider>());
                door.transform.SetParent(house.transform, false);
                door.transform.localScale = new Vector3(width * 0.18f, floorHeight * 0.75f, 0.05f);
                door.transform.localPosition = new Vector3(0f, floorHeight * 0.38f, depth / 2f + 0.03f);
                door.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.32f, 0.18f, 0.1f));

                // ліхтар біля дверей
                GameObject lanternPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lanternPost.name = "LanternPost";
                Object.DestroyImmediate(lanternPost.GetComponent<Collider>());
                lanternPost.transform.SetParent(house.transform, false);
                lanternPost.transform.localScale = new Vector3(0.04f, 0.35f, 0.04f);
                lanternPost.transform.localPosition = new Vector3(width * 0.18f, floorHeight * 0.5f, depth / 2f + 0.15f);
                lanternPost.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.2f, 0.18f, 0.16f));

                GameObject lanternGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lanternGlow.name = "LanternGlow";
                Object.DestroyImmediate(lanternGlow.GetComponent<Collider>());
                lanternGlow.transform.SetParent(house.transform, false);
                lanternGlow.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                lanternGlow.transform.localPosition = lanternPost.transform.localPosition + Vector3.up * 0.22f;
                lanternGlow.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(1f, 0.75f, 0.35f));
            }
            else
            {
                // невеликий балкон-виступ під вікнами верхніх поверхів
                GameObject balcony = GameObject.CreatePrimitive(PrimitiveType.Cube);
                balcony.name = "Balcony_" + floor;
                balcony.transform.SetParent(house.transform, false);
                balcony.transform.localScale = new Vector3(floorWidth * 0.5f, 0.08f, 0.5f);
                balcony.transform.localPosition = new Vector3(0f, floorY + floorHeight * 0.28f, depth / 2f + 0.25f);
                balcony.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.4f, 0.32f, 0.26f));
            }

            for (int side = -1; side <= 1; side += 2)
            {
                float winX = side * floorWidth * 0.28f;
                float winY = floorY + floorHeight * 0.55f;

                // рамка вікна (трохи більша темна підкладка позаду скла)
                GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frame.name = "WindowFrame_F" + floor;
                Object.DestroyImmediate(frame.GetComponent<Collider>());
                frame.transform.SetParent(house.transform, false);
                frame.transform.localScale = new Vector3(width * 0.17f, floorHeight * 0.46f, 0.04f);
                frame.transform.localPosition = new Vector3(winX, winY, depth / 2f + 0.025f);
                frame.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.35f, 0.24f, 0.16f));

                GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
                window.name = "Window_F" + floor;
                Object.DestroyImmediate(window.GetComponent<Collider>());
                window.transform.SetParent(house.transform, false);
                window.transform.localScale = new Vector3(width * 0.14f, floorHeight * 0.4f, 0.05f);
                window.transform.localPosition = new Vector3(winX, winY, depth / 2f + 0.03f);
                float glassHue = 0.5f + (float)rng.NextDouble() * 0.1f;
                window.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(glassHue, 0.72f + (float)rng.NextDouble() * 0.08f, 0.85f));

                // квітковий ящик під вікном - додає живості фасаду
                GameObject flowerBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flowerBox.name = "FlowerBox_F" + floor;
                Object.DestroyImmediate(flowerBox.GetComponent<Collider>());
                flowerBox.transform.SetParent(house.transform, false);
                flowerBox.transform.localScale = new Vector3(width * 0.16f, 0.08f, 0.12f);
                flowerBox.transform.localPosition = new Vector3(winX, winY - floorHeight * 0.22f, depth / 2f + 0.06f);
                flowerBox.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.35f, 0.26f, 0.18f));

                Color[] flowerColors = { new Color(0.85f, 0.2f, 0.3f), new Color(0.9f, 0.85f, 0.3f), new Color(0.8f, 0.3f, 0.75f) };
                for (int fl = 0; fl < 3; fl++)
                {
                    GameObject flower = new GameObject("Flower");
                    flower.transform.SetParent(house.transform, false);
                    flower.transform.localPosition = new Vector3(winX + (fl - 1) * width * 0.05f, winY - floorHeight * 0.16f, depth / 2f + 0.08f);
                    Mesh flowerMesh = MeshBuilder.CreateBlob(0.1f, seed * 50 + floor * 10 + fl);
                    flower.AddComponent<MeshFilter>().mesh = flowerMesh;
                    flower.AddComponent<MeshRenderer>().sharedMaterial = MakeColorMat(flowerColors[(seed + fl) % flowerColors.Length]);
                }

                // віконниці по обидва боки - симетрично
                for (int shSide = -1; shSide <= 1; shSide += 2)
                {
                    GameObject shutter = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    shutter.name = "Shutter_F" + floor;
                    Object.DestroyImmediate(shutter.GetComponent<Collider>());
                    shutter.transform.SetParent(house.transform, false);
                    shutter.transform.localScale = new Vector3(width * 0.045f, floorHeight * 0.44f, 0.04f);
                    shutter.transform.localPosition = new Vector3(winX + shSide * width * 0.11f, winY, depth / 2f + 0.025f);
                    shutter.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.42f, 0.28f));
                }
            }
        }

        // цоколь-фундамент по периметру основи - завершує вигляд знизу
        GameObject plinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plinth.name = "Plinth";
        plinth.transform.SetParent(house.transform, false);
        plinth.transform.localScale = new Vector3(width + 0.3f, 0.35f, depth + 0.3f);
        plinth.transform.localPosition = new Vector3(0f, 0.17f, 0f);
        plinth.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.42f, 0.4f, 0.36f));

        float totalHeight = floors * floorHeight;

        // облямівка-карниз на межі стін і даху - контрастний колір, додає деталізації фасаду
        GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim.name = "RoofTrim";
        Object.DestroyImmediate(trim.GetComponent<Collider>());
        trim.transform.SetParent(house.transform, false);
        trim.transform.localScale = new Vector3(width + 0.15f, 0.12f, depth + 0.15f);
        trim.transform.localPosition = new Vector3(0f, totalHeight, 0f);
        trim.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.9f, 0.85f, 0.7f));

        GameObject roof = new GameObject("Roof");
        roof.transform.SetParent(house.transform, false);
        roof.transform.localPosition = new Vector3(0f, totalHeight, 0f);
        MeshFilter roofMf = roof.AddComponent<MeshFilter>();
        Mesh roofMesh = MeshBuilder.CreateGableRoof(width * 1.15f, depth * 1.15f, floorHeight * 0.6f);
        roofMf.mesh = roofMesh;
        MeshRenderer roofMr = roof.AddComponent<MeshRenderer>();
        Color[] roofColors = { new Color(0.45f, 0.2f, 0.16f), new Color(0.35f, 0.24f, 0.2f), new Color(0.5f, 0.3f, 0.18f), new Color(0.3f, 0.28f, 0.32f) };
        roofMr.sharedMaterial = MakeColorMat(roofColors[seed % roofColors.Length]);
        MeshCollider roofCol = roof.AddComponent<MeshCollider>();
        roofCol.sharedMesh = roofMesh;

        GameObject chimney = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chimney.name = "Chimney";
        chimney.transform.SetParent(house.transform, false);
        chimney.transform.localScale = new Vector3(width * 0.12f, floorHeight * 0.5f, width * 0.12f);
        chimney.transform.localPosition = new Vector3(width * 0.28f, totalHeight + floorHeight * 0.25f, depth * 0.1f);
        chimney.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.5f, 0.42f, 0.36f));

        GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
        step.name = "DoorStep";
        step.transform.SetParent(house.transform, false);
        step.transform.localScale = new Vector3(width * 0.26f, 0.12f, 0.6f);
        step.transform.localPosition = new Vector3(0f, 0.06f, depth / 2f + 0.3f);
        step.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.55f, 0.52f, 0.46f));

        return house;
    }

    static GameObject BuildProceduralShop(int seed, float width, float depth, float wallHeight)
    {
        System.Random rng = new System.Random(seed);
        GameObject shop = new GameObject("ProcShop");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(shop.transform, false);
        body.transform.localScale = new Vector3(width, wallHeight, depth);
        body.transform.localPosition = new Vector3(0f, wallHeight / 2f, 0f);
        body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.68f, 0.6f, 0.5f + (float)rng.NextDouble() * 0.1f));

        // плаский дах (крамниці нижчі й простіші за житлові будинки)
        GameObject roofSlab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roofSlab.name = "RoofSlab";
        roofSlab.transform.SetParent(shop.transform, false);
        roofSlab.transform.localScale = new Vector3(width * 1.05f, 0.2f, depth * 1.05f);
        roofSlab.transform.localPosition = new Vector3(0f, wallHeight + 0.1f, 0f);
        roofSlab.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.4f, 0.32f, 0.26f));

        // яскравий навіс над входом
        GameObject awning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        awning.name = "Awning";
        Object.DestroyImmediate(awning.GetComponent<Collider>());
        awning.transform.SetParent(shop.transform, false);
        awning.transform.localScale = new Vector3(width * 0.9f, 0.08f, depth * 0.35f);
        awning.transform.localPosition = new Vector3(0f, wallHeight * 0.75f, depth / 2f + depth * 0.15f);
        awning.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
        Color[] awningColors = {
            new Color(0.75f, 0.25f, 0.2f), new Color(0.2f, 0.45f, 0.6f), new Color(0.85f, 0.6f, 0.15f)
        };
        awning.GetComponent<Renderer>().sharedMaterial = MakeColorMat(awningColors[rng.Next(0, awningColors.Length)]);

        // велика вітрина замість вузьких вікон
        GameObject display = GameObject.CreatePrimitive(PrimitiveType.Cube);
        display.name = "DisplayWindow";
        Object.DestroyImmediate(display.GetComponent<Collider>());
        display.transform.SetParent(shop.transform, false);
        display.transform.localScale = new Vector3(width * 0.55f, wallHeight * 0.45f, 0.05f);
        display.transform.localPosition = new Vector3(0f, wallHeight * 0.42f, depth / 2f + 0.03f);
        display.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.6f, 0.8f, 0.88f));

        return shop;
    }

    static GameObject BuildProceduralTower(int seed, float radius, float height)
    {
        System.Random rng = new System.Random(seed);
        GameObject tower = new GameObject("ProcTower");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(tower.transform, false);
        body.transform.localScale = new Vector3(radius * 2f, height / 2f, radius * 2f);
        body.transform.localPosition = new Vector3(0f, height / 2f, 0f);
        body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.62f, 0.58f, 0.52f));

        GameObject roof = new GameObject("ConeRoof");
        roof.transform.SetParent(tower.transform, false);
        roof.transform.localPosition = new Vector3(0f, height, 0f);
        MeshFilter roofMf = roof.AddComponent<MeshFilter>();
        Mesh roofMesh = MeshBuilder.CreateTaperedCylinder(radius * 1.15f, 0.02f, height * 0.55f, 10);
        roofMf.mesh = roofMesh;
        MeshRenderer roofMr = roof.AddComponent<MeshRenderer>();
        roofMr.sharedMaterial = MakeColorMat(new Color(0.32f, 0.22f, 0.4f));
        MeshCollider roofCol = roof.AddComponent<MeshCollider>();
        roofCol.sharedMesh = roofMesh;

        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
            window.name = "Window_" + i;
            Object.DestroyImmediate(window.GetComponent<Collider>());
            window.transform.SetParent(tower.transform, false);
            window.transform.localScale = new Vector3(radius * 0.3f, height * 0.15f, 0.05f);
            window.transform.localPosition = new Vector3(Mathf.Sin(angle) * (radius + 0.02f), height * 0.7f, Mathf.Cos(angle) * (radius + 0.02f));
            window.transform.localRotation = Quaternion.Euler(0f, i * 90f, 0f);
            window.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.55f, 0.75f, 0.85f));
        }

        return tower;
    }

    static GameObject SpawnBuilding(int seed, int floorsHint)
    {
        System.Random rng = new System.Random(seed);
        BuildingPrefabLibrary lib = BuildingPrefabLibrary.Instance;

        // якщо є реальні префаби гравця - віддаємо перевагу їм
        if (lib != null)
        {
            int totalPrefabs = (lib.housePrefabs?.Length ?? 0) + (lib.shopPrefabs?.Length ?? 0) + (lib.towerPrefabs?.Length ?? 0);
            if (totalPrefabs > 0)
            {
                int pick = rng.Next(0, totalPrefabs);
                GameObject prefab = null;
                if (lib.housePrefabs != null && pick < lib.housePrefabs.Length) prefab = lib.housePrefabs[pick];
                else if (lib.shopPrefabs != null && pick < (lib.housePrefabs?.Length ?? 0) + lib.shopPrefabs.Length)
                    prefab = lib.shopPrefabs[pick - (lib.housePrefabs?.Length ?? 0)];
                else if (lib.towerPrefabs != null)
                    prefab = lib.towerPrefabs[pick - (lib.housePrefabs?.Length ?? 0) - (lib.shopPrefabs?.Length ?? 0)];

                if (prefab != null)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    return instance;
                }
            }
        }

        // інакше - процедурні заготовки, тепер 15 різних типів
        int typeRoll = rng.Next(0, 15);
        int floors = floorsHint;
        if (typeRoll == 0) return BuildProceduralTower(seed, 2.6f, 8f + floors * 1.5f);
        if (typeRoll == 1) return BuildProceduralShop(seed, 6f, 6f, 3.2f);
        if (typeRoll == 2) return BuildProceduralDomedHouse(seed, 5.5f, 5.5f, 3.2f);
        if (typeRoll == 3) return BuildProceduralFlatRoofHouse(seed, 6f, 6f, 3f, floors);
        if (typeRoll == 4) return BuildProceduralLonghouse(seed, 4.5f, 6f, 3.4f);
        if (typeRoll == 5) return BuildProceduralStoneCottage(seed, 5.5f, 5.5f, 2.8f);
        if (typeRoll == 6) return BuildProceduralTudorHouse(seed, 6f, 6f, 3f, Mathf.Min(floors, 2));
        if (typeRoll == 7) return BuildProceduralWarehouse(seed, 6f, 6f, 4f);
        if (typeRoll == 8) return BuildProceduralChapel(seed, 5.5f, 6.5f, 3f);
        if (typeRoll == 9) return BuildProceduralWindmill(seed, 3f, 7f);
        if (typeRoll == 10) return BuildProceduralBlacksmith(seed, 6f, 6f, 3.2f);
        if (typeRoll == 11) return BuildProceduralLShapedHouse(seed, 7f, 7f, 3.2f);
        if (typeRoll == 12) return BuildProceduralInnWithSign(seed, 6.5f, 6.5f, 3.2f);
        if (typeRoll == 13) return BuildProceduralRoundWatchhouse(seed, 2.8f, 3.5f);
        return BuildProceduralHouse(seed, 6f, 6f, 3.2f, floors);
    }

    static GameObject BuildProceduralDomedHouse(int seed, float width, float depth, float wallHeight)
    {
        System.Random rng = new System.Random(seed);
        GameObject house = new GameObject("ProcDomedHouse");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(house.transform, false);
        body.transform.localScale = new Vector3(width, wallHeight / 2f, depth);
        body.transform.localPosition = new Vector3(0f, wallHeight / 2f, 0f);
        body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.78f + (float)rng.NextDouble() * 0.1f, 0.72f, 0.6f));

        GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dome.name = "Dome";
        dome.transform.SetParent(house.transform, false);
        dome.transform.localScale = new Vector3(width * 1.05f, width * 0.75f, width * 1.05f);
        dome.transform.localPosition = new Vector3(0f, wallHeight, 0f);
        dome.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.35f, 0.5f, 0.55f));

        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        Object.DestroyImmediate(door.GetComponent<Collider>());
        door.transform.SetParent(house.transform, false);
        door.transform.localScale = new Vector3(width * 0.28f, wallHeight * 0.7f, 0.06f);
        door.transform.localPosition = new Vector3(0f, wallHeight * 0.35f, depth / 2f + 0.03f);
        door.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.2f, 0.14f));

        return house;
    }

    static GameObject BuildProceduralFlatRoofHouse(int seed, float width, float depth, float floorHeight, int floors)
    {
        System.Random rng = new System.Random(seed);
        floors = Mathf.Clamp(floors, 1, 3);
        GameObject house = new GameObject("ProcFlatHouse");

        for (int floor = 0; floor < floors; floor++)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body_" + floor;
            body.transform.SetParent(house.transform, false);
            body.transform.localScale = new Vector3(width, floorHeight, depth);
            body.transform.localPosition = new Vector3(0f, floor * floorHeight + floorHeight / 2f, 0f);
            float tint = 0.55f + (float)rng.NextDouble() * 0.25f;
            body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(tint, tint * 0.95f, tint * 0.9f));

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
                window.name = "Window_F" + floor;
                Object.DestroyImmediate(window.GetComponent<Collider>());
                window.transform.SetParent(house.transform, false);
                window.transform.localScale = new Vector3(width * 0.3f, floorHeight * 0.5f, 0.05f);
                window.transform.localPosition = new Vector3(side * width * 0.24f, floor * floorHeight + floorHeight * 0.55f, depth / 2f + 0.03f);
                window.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.5f, 0.7f, 0.8f));
            }
        }

        GameObject roofSlab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roofSlab.name = "FlatRoof";
        roofSlab.transform.SetParent(house.transform, false);
        roofSlab.transform.localScale = new Vector3(width * 1.08f, 0.25f, depth * 1.08f);
        roofSlab.transform.localPosition = new Vector3(0f, floors * floorHeight + 0.12f, 0f);
        roofSlab.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.3f, 0.3f));

        return house;
    }

    static GameObject BuildProceduralLonghouse(int seed, float width, float depth, float wallHeight)
    {
        System.Random rng = new System.Random(seed);
        GameObject house = new GameObject("ProcLonghouse");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(house.transform, false);
        body.transform.localScale = new Vector3(width * 1.8f, wallHeight, depth * 0.75f); // помітно видовжений
        body.transform.localPosition = new Vector3(0f, wallHeight / 2f, 0f);
        body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.6f, 0.5f, 0.4f));

        Mesh roofMesh = MeshBuilder.CreateGableRoof(width * 1.9f, depth * 0.85f, wallHeight * 0.5f);
        GameObject roof = new GameObject("Roof");
        roof.transform.SetParent(house.transform, false);
        roof.transform.localPosition = new Vector3(0f, wallHeight, 0f);
        roof.AddComponent<MeshFilter>().mesh = roofMesh;
        MeshRenderer roofMr = roof.AddComponent<MeshRenderer>();
        roofMr.sharedMaterial = MakeColorMat(new Color(0.38f, 0.28f, 0.2f));
        MeshCollider roofCol = roof.AddComponent<MeshCollider>();
        roofCol.sharedMesh = roofMesh;

        // кілька дверей уздовж довгого фасаду
        for (int i = -1; i <= 1; i++)
        {
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door_" + i;
            Object.DestroyImmediate(door.GetComponent<Collider>());
            door.transform.SetParent(house.transform, false);
            door.transform.localScale = new Vector3(width * 0.22f, wallHeight * 0.6f, 0.05f);
            door.transform.localPosition = new Vector3(i * width * 0.55f, wallHeight * 0.3f, depth * 0.375f + 0.03f);
            door.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.2f, 0.14f));
        }

        return house;
    }

    static GameObject BuildProceduralStoneCottage(int seed, float width, float depth, float wallHeight)
    {
        System.Random rng = new System.Random(seed);
        GameObject house = new GameObject("ProcStoneCottage");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(house.transform, false);
        body.transform.localScale = new Vector3(width, wallHeight, depth);
        body.transform.localPosition = new Vector3(0f, wallHeight / 2f, 0f);
        body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.5f, 0.48f, 0.44f)); // сірий камінь

        // округла "солом'яна" крона-дах через блоб-меш замість гострого гребеня
        GameObject roof = new GameObject("ThatchRoof");
        roof.transform.SetParent(house.transform, false);
        roof.transform.localPosition = new Vector3(0f, wallHeight + width * 0.25f, 0f);
        Mesh roofMesh = MeshBuilder.CreateBlob(width * 0.75f, seed);
        roof.AddComponent<MeshFilter>().mesh = roofMesh;
        MeshRenderer roofMr = roof.AddComponent<MeshRenderer>();
        roofMr.sharedMaterial = MakeColorMat(new Color(0.62f, 0.5f, 0.26f)); // солом'яний жовто-коричневий
        MeshCollider roofCol = roof.AddComponent<MeshCollider>();
        roofCol.sharedMesh = roofMesh;
        roof.transform.localScale = new Vector3(1f, 0.6f, 1f); // сплюснутий, як купа соломи

        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        Object.DestroyImmediate(door.GetComponent<Collider>());
        door.transform.SetParent(house.transform, false);
        door.transform.localScale = new Vector3(width * 0.22f, wallHeight * 0.65f, 0.05f);
        door.transform.localPosition = new Vector3(0f, wallHeight * 0.32f, depth / 2f + 0.03f);
        door.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.2f, 0.14f));

        return house;
    }

    static GameObject BuildProceduralTudorHouse(int seed, float width, float depth, float floorHeight, int floors)
    {
        System.Random rng = new System.Random(seed);
        floors = Mathf.Clamp(floors, 1, 2);
        GameObject house = new GameObject("ProcTudorHouse");
        Material lightWall = MakeColorMat(new Color(0.88f, 0.84f, 0.74f));
        Material darkBeam = MakeColorMat(new Color(0.28f, 0.2f, 0.14f));

        for (int floor = 0; floor < floors; floor++)
        {
            float floorWidth = width + floor * 0.6f; // верхній поверх звисає ширше, як у справжніх тюдор-будинках
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body_" + floor;
            body.transform.SetParent(house.transform, false);
            body.transform.localScale = new Vector3(floorWidth, floorHeight, depth);
            body.transform.localPosition = new Vector3(0f, floor * floorHeight + floorHeight / 2f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = lightWall;

            // діагональні дерев'яні балки-прикраси
            for (int b = -1; b <= 1; b += 2)
            {
                GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
                beam.name = "Beam_" + floor + "_" + b;
                Object.DestroyImmediate(beam.GetComponent<Collider>());
                beam.transform.SetParent(house.transform, false);
                beam.transform.localScale = new Vector3(0.12f, floorHeight * 1.1f, 0.06f);
                beam.transform.localPosition = new Vector3(b * floorWidth * 0.25f, floor * floorHeight + floorHeight / 2f, depth / 2f + 0.03f);
                beam.transform.localRotation = Quaternion.Euler(0f, 0f, b * 25f);
                beam.GetComponent<Renderer>().sharedMaterial = darkBeam;
            }

            GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
            window.name = "Window_" + floor;
            Object.DestroyImmediate(window.GetComponent<Collider>());
            window.transform.SetParent(house.transform, false);
            window.transform.localScale = new Vector3(floorWidth * 0.22f, floorHeight * 0.4f, 0.05f);
            window.transform.localPosition = new Vector3(0f, floor * floorHeight + floorHeight * 0.55f, depth / 2f + 0.03f);
            window.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.5f, 0.7f, 0.8f));
        }

        float totalH = floors * floorHeight;
        Mesh roofMesh = MeshBuilder.CreateGableRoof(width * 1.2f, depth * 1.2f, floorHeight * 0.65f);
        GameObject roof = new GameObject("Roof");
        roof.transform.SetParent(house.transform, false);
        roof.transform.localPosition = new Vector3(0f, totalH, 0f);
        roof.AddComponent<MeshFilter>().mesh = roofMesh;
        roof.AddComponent<MeshRenderer>().sharedMaterial = MakeColorMat(new Color(0.35f, 0.16f, 0.14f));
        roof.AddComponent<MeshCollider>().sharedMesh = roofMesh;

        return house;
    }

    static GameObject BuildProceduralWarehouse(int seed, float width, float depth, float wallHeight)
    {
        System.Random rng = new System.Random(seed);
        GameObject house = new GameObject("ProcWarehouse");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(house.transform, false);
        body.transform.localScale = new Vector3(width * 1.4f, wallHeight, depth * 1.3f);
        body.transform.localPosition = new Vector3(0f, wallHeight / 2f, 0f);
        body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.55f, 0.5f, 0.42f));

        GameObject roofSlab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roofSlab.name = "RoofSlab";
        roofSlab.transform.SetParent(house.transform, false);
        roofSlab.transform.localScale = new Vector3(width * 1.5f, 0.2f, depth * 1.4f);
        roofSlab.transform.localPosition = new Vector3(0f, wallHeight + 0.1f, 0f);
        roofSlab.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.32f, 0.28f, 0.24f));

        // великі ворота замість вузьких дверей
        GameObject gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gate.name = "CargoGate";
        Object.DestroyImmediate(gate.GetComponent<Collider>());
        gate.transform.SetParent(house.transform, false);
        gate.transform.localScale = new Vector3(width * 0.6f, wallHeight * 0.75f, 0.06f);
        gate.transform.localPosition = new Vector3(0f, wallHeight * 0.375f, depth * 0.65f + 0.03f);
        gate.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.24f, 0.18f));

        return house;
    }

    static GameObject BuildProceduralChapel(int seed, float width, float depth, float wallHeight)
    {
        GameObject house = new GameObject("ProcChapel");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(house.transform, false);
        body.transform.localScale = new Vector3(width, wallHeight * 1.6f, depth * 1.3f); // вищі стіни, як у каплиці
        body.transform.localPosition = new Vector3(0f, wallHeight * 0.8f, 0f);
        body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.7f, 0.66f, 0.6f));

        Mesh roofMesh = MeshBuilder.CreateGableRoof(width * 1.15f, depth * 1.35f, wallHeight * 1.3f); // крутіший гребінь
        GameObject roof = new GameObject("SteepRoof");
        roof.transform.SetParent(house.transform, false);
        roof.transform.localPosition = new Vector3(0f, wallHeight * 1.6f, 0f);
        roof.AddComponent<MeshFilter>().mesh = roofMesh;
        roof.AddComponent<MeshRenderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.28f, 0.32f));
        roof.AddComponent<MeshCollider>().sharedMesh = roofMesh;

        GameObject crossV = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crossV.name = "CrossVertical";
        Object.DestroyImmediate(crossV.GetComponent<Collider>());
        crossV.transform.SetParent(house.transform, false);
        crossV.transform.localScale = new Vector3(0.15f, 1.2f, 0.15f);
        crossV.transform.localPosition = new Vector3(0f, wallHeight * 1.6f + wallHeight * 1.3f + 0.6f, 0f);
        crossV.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.25f, 0.22f, 0.2f));

        GameObject crossH = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crossH.name = "CrossHorizontal";
        Object.DestroyImmediate(crossH.GetComponent<Collider>());
        crossH.transform.SetParent(house.transform, false);
        crossH.transform.localScale = new Vector3(0.7f, 0.15f, 0.15f);
        crossH.transform.localPosition = crossV.transform.localPosition + new Vector3(0f, 0.25f, 0f);
        crossH.GetComponent<Renderer>().sharedMaterial = crossV.GetComponent<Renderer>().sharedMaterial;

        // велике арочне вікно на фасаді
        GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        window.name = "ArchWindow";
        Object.DestroyImmediate(window.GetComponent<Collider>());
        window.transform.SetParent(house.transform, false);
        window.transform.localScale = new Vector3(width * 0.25f, 0.05f, width * 0.25f);
        window.transform.localPosition = new Vector3(0f, wallHeight * 1.1f, depth * 0.65f + 0.03f);
        window.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        window.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.5f, 0.7f, 0.8f));

        return house;
    }

    static GameObject BuildProceduralWindmill(int seed, float radius, float height)
    {
        GameObject house = new GameObject("ProcWindmill");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(house.transform, false);
        body.transform.localScale = new Vector3(radius * 2f, height / 2f, radius * 2f);
        body.transform.localPosition = new Vector3(0f, height / 2f, 0f);
        body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.65f, 0.58f, 0.48f));

        Mesh roofMesh = MeshBuilder.CreateTaperedCylinder(radius * 1.1f, 0.02f, height * 0.4f, 8);
        GameObject roof = new GameObject("ConeRoof");
        roof.transform.SetParent(house.transform, false);
        roof.transform.localPosition = new Vector3(0f, height, 0f);
        roof.AddComponent<MeshFilter>().mesh = roofMesh;
        roof.AddComponent<MeshRenderer>().sharedMaterial = MakeColorMat(new Color(0.4f, 0.3f, 0.24f));
        roof.AddComponent<MeshCollider>().sharedMesh = roofMesh;

        // 4 лопаті
        GameObject hub = new GameObject("BladeHub");
        hub.transform.SetParent(house.transform, false);
        hub.transform.localPosition = new Vector3(0f, height * 0.75f, radius + 0.3f);
        for (int i = 0; i < 4; i++)
        {
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade_" + i;
            Object.DestroyImmediate(blade.GetComponent<Collider>());
            blade.transform.SetParent(hub.transform, false);
            blade.transform.localScale = new Vector3(0.15f, radius * 1.8f, 0.05f);
            blade.transform.localPosition = Vector3.zero;
            blade.transform.localRotation = Quaternion.Euler(0f, 0f, i * 45f);
            blade.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.75f, 0.7f, 0.6f));
        }

        return house;
    }

    static GameObject BuildProceduralBlacksmith(int seed, float width, float depth, float wallHeight)
    {
        GameObject house = new GameObject("ProcBlacksmith");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(house.transform, false);
        body.transform.localScale = new Vector3(width, wallHeight * 0.85f, depth);
        body.transform.localPosition = new Vector3(0f, wallHeight * 0.425f, 0f);
        body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.35f, 0.32f, 0.3f)); // темний закопчений камінь

        Mesh roofMesh = MeshBuilder.CreateGableRoof(width * 1.1f, depth * 1.1f, wallHeight * 0.4f);
        GameObject roof = new GameObject("Roof");
        roof.transform.SetParent(house.transform, false);
        roof.transform.localPosition = new Vector3(0f, wallHeight * 0.85f, 0f);
        roof.AddComponent<MeshFilter>().mesh = roofMesh;
        roof.AddComponent<MeshRenderer>().sharedMaterial = MakeColorMat(new Color(0.28f, 0.24f, 0.2f));
        roof.AddComponent<MeshCollider>().sharedMesh = roofMesh;

        // піч-горно з димарем
        GameObject furnace = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        furnace.name = "Furnace";
        furnace.transform.SetParent(house.transform, false);
        furnace.transform.localScale = new Vector3(width * 0.25f, wallHeight * 0.6f, width * 0.25f);
        furnace.transform.localPosition = new Vector3(width * 0.3f, wallHeight * 0.3f, depth * 0.3f);
        furnace.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.2f, 0.18f, 0.16f));

        GameObject chimney = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chimney.name = "Chimney";
        chimney.transform.SetParent(house.transform, false);
        chimney.transform.localScale = new Vector3(width * 0.15f, wallHeight * 0.9f, width * 0.15f);
        chimney.transform.localPosition = new Vector3(width * 0.3f, wallHeight * 1.1f, depth * 0.3f);
        chimney.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.2f, 0.16f));

        return house;
    }

    static GameObject BuildProceduralLShapedHouse(int seed, float width, float depth, float wallHeight)
    {
        System.Random rng = new System.Random(seed);
        GameObject house = new GameObject("ProcLShapedHouse");
        Material wallMat = MakeColorMat(new Color(0.74f + (float)rng.NextDouble() * 0.1f, 0.68f, 0.56f));

        GameObject wingA = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingA.name = "WingA";
        wingA.transform.SetParent(house.transform, false);
        wingA.transform.localScale = new Vector3(width, wallHeight, depth * 0.55f);
        wingA.transform.localPosition = new Vector3(0f, wallHeight / 2f, -depth * 0.22f);
        wingA.GetComponent<Renderer>().sharedMaterial = wallMat;

        GameObject wingB = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingB.name = "WingB";
        wingB.transform.SetParent(house.transform, false);
        wingB.transform.localScale = new Vector3(width * 0.5f, wallHeight, depth);
        wingB.transform.localPosition = new Vector3(width * 0.25f, wallHeight / 2f, depth * 0.225f);
        wingB.GetComponent<Renderer>().sharedMaterial = wallMat;

        Mesh roofA = MeshBuilder.CreateGableRoof(width * 1.1f, depth * 0.65f, wallHeight * 0.5f);
        GameObject roofObjA = new GameObject("RoofA");
        roofObjA.transform.SetParent(house.transform, false);
        roofObjA.transform.localPosition = new Vector3(0f, wallHeight, -depth * 0.22f);
        roofObjA.AddComponent<MeshFilter>().mesh = roofA;
        roofObjA.AddComponent<MeshRenderer>().sharedMaterial = MakeColorMat(new Color(0.42f, 0.2f, 0.16f));
        roofObjA.AddComponent<MeshCollider>().sharedMesh = roofA;

        Mesh roofB = MeshBuilder.CreateGableRoof(width * 0.6f, depth * 1.1f, wallHeight * 0.5f);
        GameObject roofObjB = new GameObject("RoofB");
        roofObjB.transform.SetParent(house.transform, false);
        roofObjB.transform.localPosition = new Vector3(width * 0.25f, wallHeight, depth * 0.225f);
        roofObjB.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        roofObjB.AddComponent<MeshFilter>().mesh = roofB;
        roofObjB.AddComponent<MeshRenderer>().sharedMaterial = MakeColorMat(new Color(0.42f, 0.2f, 0.16f));
        roofObjB.AddComponent<MeshCollider>().sharedMesh = roofB;

        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        Object.DestroyImmediate(door.GetComponent<Collider>());
        door.transform.SetParent(house.transform, false);
        door.transform.localScale = new Vector3(width * 0.16f, wallHeight * 0.6f, 0.05f);
        door.transform.localPosition = new Vector3(-width * 0.25f, wallHeight * 0.3f, -depth * 0.49f);
        door.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.2f, 0.14f));

        return house;
    }

    static GameObject BuildProceduralInnWithSign(int seed, float width, float depth, float wallHeight)
    {
        System.Random rng = new System.Random(seed);
        GameObject house = BuildProceduralHouse(seed, width, depth, wallHeight, 2);
        house.name = "ProcInn";

        GameObject signPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        signPost.name = "SignPost";
        signPost.transform.SetParent(house.transform, false);
        signPost.transform.localScale = new Vector3(0.08f, 0.9f, 0.08f);
        signPost.transform.localPosition = new Vector3(width * 0.5f + 0.3f, 1.6f, depth * 0.4f);
        signPost.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.22f, 0.16f));

        GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "SignBoard";
        sign.transform.SetParent(house.transform, false);
        sign.transform.localScale = new Vector3(0.05f, 0.7f, 1f);
        sign.transform.localPosition = signPost.transform.localPosition + new Vector3(0.1f, 0.3f, 0f);
        Color[] signColors = { new Color(0.7f,0.2f,0.15f), new Color(0.2f,0.4f,0.6f), new Color(0.6f,0.5f,0.15f) };
        sign.GetComponent<Renderer>().sharedMaterial = MakeColorMat(signColors[rng.Next(0, signColors.Length)]);

        return house;
    }

    static GameObject BuildProceduralRoundWatchhouse(int seed, float radius, float wallHeight)
    {
        GameObject house = new GameObject("ProcRoundWatchhouse");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(house.transform, false);
        body.transform.localScale = new Vector3(radius * 2f, wallHeight / 2f, radius * 2f);
        body.transform.localPosition = new Vector3(0f, wallHeight / 2f, 0f);
        body.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.58f, 0.55f, 0.5f));

        Mesh roofMesh = MeshBuilder.CreateTaperedCylinder(radius * 1.2f, 0.02f, wallHeight * 0.5f, 8);
        GameObject roof = new GameObject("ConeRoof");
        roof.transform.SetParent(house.transform, false);
        roof.transform.localPosition = new Vector3(0f, wallHeight, 0f);
        roof.AddComponent<MeshFilter>().mesh = roofMesh;
        roof.AddComponent<MeshRenderer>().sharedMaterial = MakeColorMat(new Color(0.35f, 0.3f, 0.28f));
        roof.AddComponent<MeshCollider>().sharedMesh = roofMesh;

        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        Object.DestroyImmediate(door.GetComponent<Collider>());
        door.transform.SetParent(house.transform, false);
        door.transform.localScale = new Vector3(radius * 0.6f, wallHeight * 0.6f, 0.05f);
        door.transform.localPosition = new Vector3(0f, wallHeight * 0.3f, radius + 0.03f);
        door.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.2f, 0.14f));

        return house;
    }

    [MenuItem("Вежа/Локації/Замінити дерева й будинки на кращі процедурні моделі")]
    static GameObject SpawnTreeOrBushVaried(int seed, bool forceGiant)
    {
        System.Random pickRng = new System.Random(seed * 31 + 7);
        if (forceGiant) return BuildProceduralGiantTree(seed);

        int roll = pickRng.Next(0, 100);
        GameObject obj;
        if (roll < 5) obj = BuildProceduralGiantTree(seed);        // 5% - шанс на велетня навіть поза примусовим розподілом
        else if (roll < 20) obj = BuildProceduralBush(seed);
        else if (roll < 45) obj = BuildProceduralPineTree(seed);
        else if (roll < 55) obj = BuildProceduralDeadTree(seed);
        else if (roll < 65) obj = BuildProceduralBlossomTree(seed);
        else obj = BuildProceduralTree(seed);

        if (roll >= 20) obj.transform.localScale = Vector3.one * 2.5f; // "менші" типи дерев у 2.5 раза більші (крім велетня й куща)
        return obj;
    }

    public static void UpgradeNatureAndBuildings()
    {
        GameObject wild = GameObject.Find("Wilderness");
        if (wild != null)
        {
            List<Vector3> positions = new List<Vector3>();
            foreach (Transform child in wild.transform) positions.Add(child.position);
            for (int i = wild.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(wild.transform.GetChild(i).gameObject);

            int giantEvery = 9; // приблизно кожне 9-те дерево - гарантований велетень, для рівномірнішого розподілу
            for (int i = 0; i < positions.Count; i++)
            {
                GameObject newObj = SpawnTreeOrBushVaried(i + 1, i % giantEvery == 0);
                newObj.transform.SetParent(wild.transform);
                newObj.transform.position = positions[i];
            }
        }

        GameObject town = GameObject.Find("Town");
        if (town != null)
        {
            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();
            foreach (Transform child in town.transform)
            {
                positions.Add(child.position);
                rotations.Add(child.rotation);
            }
            for (int i = town.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(town.transform.GetChild(i).gameObject);

            for (int i = 0; i < positions.Count; i++)
            {
                int floors = 1 + (i % 3); // 1-3 поверхи упереміш
                GameObject house = SpawnBuilding(i + 1000, floors);
                house.transform.SetParent(town.transform);
                house.transform.position = positions[i];
                house.transform.rotation = rotations[i];
            }
        }

        Debug.Log("Дерева, кущі й будинки замінено на кращі процедурні моделі (справжня геометрія, не примітиви).");
    }

    static Material MakeTransparentMat(Color c)
    {
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetFloat("_Surface", 1f); // Transparent
        m.SetFloat("_Blend", 0f);   // Alpha blend
        m.SetOverrideTag("RenderType", "Transparent");
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); // ключове слово, яке URP Lit насправді перевіряє
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        m.color = c;
        return m;
    }

    [MenuItem("Вежа/Локації/Побудувати вулиці, дороги, річку і кращий фонтан")]
    public static void BuildStreetsRoadsRiverFountain()
    {
        Material roadMat = MakeColorMat(new Color(0.5f, 0.47f, 0.42f));
        Material waterMat = MakeTransparentMat(new Color(0.2f, 0.45f, 0.7f, 0.72f));
        Material rockMat = MakeColorMat(new Color(0.52f, 0.5f, 0.47f));

        // --- Місто заново: СІТКА прямих вулиць з перехрестями, а не кільця ---
        GameObject oldTown = GameObject.Find("Town");
        if (oldTown != null) Object.DestroyImmediate(oldTown);
        GameObject townRoot = new GameObject("Town");

        GameObject oldRoads = GameObject.Find("Roads");
        if (oldRoads != null) Object.DestroyImmediate(oldRoads);
        GameObject roadsRoot = new GameObject("Roads");

        const float blockSize = 34f;   // відстань між лініями вулиць - простіше й ширше, ніж було
        const float streetWidth = 10f; // помітно ширші вулиці
        const int gridExtent = 4;      // ліній вулиць в кожен бік від центру
        const float clearRadius = 20f; // тут стоїть площа з фонтаном - будинків і доріг тут нема
        const float townRadius = 145f; // МІСТО ОКРУГЛЕ по контуру (не квадратне!) - саме це усуває
                                        // перетин з річкою на кутах, і виглядає органічніше за шахову сітку
        System.Random roadRng = new System.Random(4242);

        for (int i = -gridExtent; i <= gridExtent; i++)
        {
            float linePos = i * blockSize;
            float halfChordSq = townRadius * townRadius - linePos * linePos;
            if (halfChordSq <= 4f) continue; // ця лінія цілком за межею округлого міста
            float halfChord = Mathf.Sqrt(halfChordSq);

            GameObject roadX = new GameObject("Road_X_" + i);
            roadX.transform.SetParent(roadsRoot.transform);
            roadX.transform.position = new Vector3(0f, 0.18f, 0f);
            List<Vector3> ptsX = JitterLine(new Vector3(-halfChord, 0f, linePos), new Vector3(halfChord, 0f, linePos), 6, 2.5f, roadRng);
            roadX.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateRibbon(ptsX, streetWidth);
            roadX.AddComponent<MeshRenderer>().sharedMaterial = roadMat;

            GameObject roadZ = new GameObject("Road_Z_" + i);
            roadZ.transform.SetParent(roadsRoot.transform);
            roadZ.transform.position = new Vector3(0f, 0.18f, 0f);
            List<Vector3> ptsZ = JitterLine(new Vector3(linePos, 0f, -halfChord), new Vector3(linePos, 0f, halfChord), 6, 2.5f, roadRng);
            roadZ.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateRibbon(ptsZ, streetWidth);
            roadZ.AddComponent<MeshRenderer>().sharedMaterial = roadMat;
        }

        for (int bi = -gridExtent; bi < gridExtent; bi++)
        {
            for (int bj = -gridExtent; bj < gridExtent; bj++)
            {
                float cx = (bi + 0.5f) * blockSize;
                float cz = (bj + 0.5f) * blockSize;
                float distFromCenter = new Vector2(cx, cz).magnitude;
                if (distFromCenter < clearRadius) continue;      // площа лишається вільною
                if (distFromCenter > townRadius - 6f) continue;  // за округлою межею міста - тут вже дика природа

                // "район" через шум Перліна - десь щільна забудова, десь відкриті майданчики/парки
                float district = Mathf.PerlinNoise((bi + gridExtent) * 0.35f + 13.7f, (bj + gridExtent) * 0.35f + 4.2f);
                if (district < 0.22f) continue; // порожній квартал - невеликий майданчик/парк

                int seed = (bi + 100) * 1000 + (bj + 100);
                System.Random rng = new System.Random(seed);
                int buildingsInBlock = district > 0.65f ? 3 : (district > 0.4f ? 2 : 1);

                for (int k = 0; k < buildingsInBlock; k++)
                {
                    float offsetX;
                    if (buildingsInBlock == 3) offsetX = (k - 1) * 8f;
                    else if (buildingsInBlock == 2) offsetX = (k == 0 ? -7f : 7f);
                    else offsetX = 0f;
                    float offsetZ = buildingsInBlock == 3 && k == 1 ? 6f : 0f;
                    Vector3 pos = new Vector3(cx + offsetX, 0f, cz + offsetZ);

                    GameObject building;
                    int typeRoll = rng.Next(0, 6);
                    int floors = 1 + rng.Next(0, 3);
                    if (typeRoll == 0) building = BuildProceduralTower(seed * 10 + k, 2.6f, 8f + floors * 1.5f);
                    else if (typeRoll == 1) building = BuildProceduralShop(seed * 10 + k, 6.5f, 6f, 3.2f);
                    else building = BuildProceduralHouse(seed * 10 + k, 6f, 6f, 3.2f, floors);

                    building.transform.SetParent(townRoot.transform);
                    building.transform.position = pos;
                    building.transform.rotation = Quaternion.Euler(0f, rng.Next(0, 4) * 90f, 0f);
                }
            }
        }

        // --- Гравець спавниться біля фонтану, не серед будинків ---
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerObj.transform.position = new Vector3(0f, 1f, -12f);
        }

        // --- Каміння вздовж доріг і розкидане по всій локації ---
        GameObject oldRocks = GameObject.Find("Rocks");
        if (oldRocks != null) Object.DestroyImmediate(oldRocks);
        GameObject rocksRoot = new GameObject("Rocks");
        System.Random rockRng = new System.Random(555);

        // каміння біля узбіч вулиць
        for (int i = -gridExtent; i <= gridExtent; i++)
        {
            float linePos = i * blockSize;
            for (int k = 0; k < 3; k++)
            {
                float t = (float)rockRng.NextDouble();
                float alongLine = Mathf.Lerp(-townRadius, townRadius, t);
                bool alongX = rockRng.NextDouble() < 0.5;
                Vector3 pos = alongX
                    ? new Vector3(alongLine, 0f, linePos + streetWidth / 2f + 1f + (float)rockRng.NextDouble())
                    : new Vector3(linePos + streetWidth / 2f + 1f + (float)rockRng.NextDouble(), 0f, alongLine);
                CreateRock(rocksRoot.transform, pos, rockMat, rockRng, 0.3f, 0.7f);
            }
        }

        // каміння розкидане по всій локації (за межами скупчення будинків теж)
        for (int i = 0; i < 70; i++)
        {
            float angle = (float)rockRng.NextDouble() * 360f;
            float radius = 15f + (float)rockRng.NextDouble() * 300f;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
            CreateRock(rocksRoot.transform, pos, rockMat, rockRng, 0.4f, 1.6f);
        }

        // --- Річка - за межею округлого міста, з реальною видимою глибиною (дно нижче поверхні) ---
        GameObject oldRiver = GameObject.Find("River");
        if (oldRiver != null) Object.DestroyImmediate(oldRiver);
        List<Vector3> riverPts = new List<Vector3>();
        const float riverAngleStart = 20f, riverAngleEnd = 75f; // компактна дуга - НЕ навколо всього міста
        const int riverSegs = 18;
        for (int i = 0; i <= riverSegs; i++)
        {
            float t = (float)i / riverSegs;
            float ang = Mathf.Lerp(riverAngleStart, riverAngleEnd, t) * Mathf.Deg2Rad;
            float rad = Mathf.Lerp(175f, 215f, t) + Mathf.Sin(t * 4f) * 10f; // м'які вигини, не спіраль
            riverPts.Add(new Vector3(Mathf.Cos(ang) * rad, 0f, Mathf.Sin(ang) * rad));
        }
        BuildWaterBody("River", riverPts, 20f, waterMat, new System.Random(9001));

        // --- Ще дві водойми в ІНШИХ, чітко відокремлених напрямках - з великими проміжками сухої землі між усіма трьома ---
        List<Vector3> lake2Pts = new List<Vector3>();
        for (int i = 0; i <= 10; i++)
        {
            float t = (float)i / 10;
            float ang = Mathf.Lerp(160f, 205f, t) * Mathf.Deg2Rad;
            float rad = Mathf.Lerp(185f, 225f, t) + Mathf.Sin(t * 4f) * 10f;
            lake2Pts.Add(new Vector3(Mathf.Cos(ang) * rad, 0f, Mathf.Sin(ang) * rad));
        }
        BuildWaterBody("Lake_West", lake2Pts, 24f, waterMat, new System.Random(9002));

        List<Vector3> lake3Pts = new List<Vector3>();
        for (int i = 0; i <= 8; i++)
        {
            float t = (float)i / 8;
            float ang = Mathf.Lerp(275f, 315f, t) * Mathf.Deg2Rad;
            float rad = Mathf.Lerp(170f, 210f, t) + Mathf.Sin(t * 4f) * 8f;
            lake3Pts.Add(new Vector3(Mathf.Cos(ang) * rad, 0f, Mathf.Sin(ang) * rad));
        }
        BuildWaterBody("Lake_North", lake3Pts, 20f, waterMat, new System.Random(9003));
        // --- Кругла бруківка-майданчик навколо фонтану (площа) ---
        GameObject oldPlaza = GameObject.Find("PlazaPavement");
        if (oldPlaza != null) Object.DestroyImmediate(oldPlaza);
        GameObject plaza = new GameObject("PlazaPavement");
        plaza.transform.position = new Vector3(0f, 0.15f, 0f);
        Material plazaMat = MakeColorMat(new Color(0.56f, 0.53f, 0.48f));
        plaza.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateAnnulus(0f, clearRadius + 4f, 48);
        plaza.AddComponent<MeshRenderer>().sharedMaterial = plazaMat;

        // --- Багатоярусний фонтан ---
        GameObject oldFountain = GameObject.Find("Fountain");
        if (oldFountain != null) Object.DestroyImmediate(oldFountain);
        GameObject fountain = new GameObject("Fountain");
        Material stoneMat = MakeColorMat(new Color(0.65f, 0.63f, 0.58f));

        GameObject bigBasin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bigBasin.name = "BigBasin";
        bigBasin.transform.SetParent(fountain.transform, false);
        bigBasin.transform.localScale = new Vector3(6f, 0.4f, 6f);
        bigBasin.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        bigBasin.GetComponent<Renderer>().sharedMaterial = stoneMat;

        GameObject water1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water1.name = "Water1";
        Object.DestroyImmediate(water1.GetComponent<Collider>());
        water1.transform.SetParent(fountain.transform, false);
        water1.transform.localScale = new Vector3(5.6f, 0.05f, 5.6f);
        water1.transform.localPosition = new Vector3(0f, 0.62f, 0f);
        water1.GetComponent<Renderer>().sharedMaterial = waterMat;

        GameObject midPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        midPillar.name = "MidPillar";
        midPillar.transform.SetParent(fountain.transform, false);
        midPillar.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
        midPillar.transform.localPosition = new Vector3(0f, 1.4f, 0f);
        midPillar.GetComponent<Renderer>().sharedMaterial = stoneMat;

        GameObject smallBasin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        smallBasin.name = "SmallBasin";
        smallBasin.transform.SetParent(fountain.transform, false);
        smallBasin.transform.localScale = new Vector3(2.6f, 0.25f, 2.6f);
        smallBasin.transform.localPosition = new Vector3(0f, 2.3f, 0f);
        smallBasin.GetComponent<Renderer>().sharedMaterial = stoneMat;

        GameObject water2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water2.name = "Water2";
        Object.DestroyImmediate(water2.GetComponent<Collider>());
        water2.transform.SetParent(fountain.transform, false);
        water2.transform.localScale = new Vector3(2.3f, 0.05f, 2.3f);
        water2.transform.localPosition = new Vector3(0f, 2.42f, 0f);
        water2.GetComponent<Renderer>().sharedMaterial = waterMat;

        GameObject pillar2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar2.name = "TopPillar";
        pillar2.transform.SetParent(fountain.transform, false);
        pillar2.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);
        pillar2.transform.localPosition = new Vector3(0f, 3f, 0f);
        pillar2.GetComponent<Renderer>().sharedMaterial = stoneMat;

        GameObject spout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spout.name = "SpoutTop";
        spout.transform.SetParent(fountain.transform, false);
        spout.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        spout.transform.localPosition = new Vector3(0f, 3.9f, 0f);
        spout.GetComponent<Renderer>().sharedMaterial = waterMat;

        for (int i = 0; i < 4; i++)
        {
            float ang = i * 90f * Mathf.Deg2Rad;
            GameObject edgePillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            edgePillar.name = "EdgePillar_" + i;
            edgePillar.transform.SetParent(fountain.transform, false);
            edgePillar.transform.localScale = new Vector3(0.3f, 0.6f, 0.3f);
            edgePillar.transform.localPosition = new Vector3(Mathf.Cos(ang) * 5.2f, 0.6f, Mathf.Sin(ang) * 5.2f);
            edgePillar.GetComponent<Renderer>().sharedMaterial = stoneMat;
        }

        Debug.Log("Вулиці-сітка з перехрестями, ширші проміжки, каміння, річка (напрямок ~20-130° від площі, за 165-215м, одразу за межею міста) і новий фонтан готові.");
    }

    static void BuildWaterBody(string name, List<Vector3> centerPoints, float width, Material surfaceMat, System.Random rng)
    {
        GameObject oldOne = GameObject.Find(name);
        if (oldOne != null) Object.DestroyImmediate(oldOne);

        GameObject waterRoot = new GameObject(name);
        waterRoot.transform.position = new Vector3(0f, 0.2f, 0f);

        GameObject surface = new GameObject("Surface");
        surface.transform.SetParent(waterRoot.transform, false);
        Mesh surfaceMesh = MeshBuilder.CreateRibbon(centerPoints, width);
        surface.AddComponent<MeshFilter>().mesh = surfaceMesh;
        surface.AddComponent<MeshRenderer>().sharedMaterial = surfaceMat;

        // тригери плавання ПО СЕГМЕНТАХ - точно повторюють вигин річки,
        // а не один величезний прямокутник навколо всієї звивистої лінії
        GameObject triggersRoot = new GameObject("SwimTriggers");
        triggersRoot.transform.SetParent(waterRoot.transform, false);
        for (int i = 0; i < centerPoints.Count - 1; i++)
        {
            Vector3 a = centerPoints[i];
            Vector3 bPt = centerPoints[i + 1];
            Vector3 mid = (a + bPt) / 2f;
            float segLength = Vector3.Distance(a, bPt);
            Vector3 dir = (bPt - a).normalized;

            GameObject segTrigger = new GameObject("SwimSeg_" + i);
            segTrigger.transform.SetParent(triggersRoot.transform, false);
            segTrigger.transform.localPosition = mid + new Vector3(0f, -3f, 0f);
            segTrigger.transform.localRotation = Quaternion.LookRotation(dir);
            BoxCollider segCol = segTrigger.AddComponent<BoxCollider>();
            segCol.isTrigger = true;
            segCol.size = new Vector3(width, 8f, segLength + 1f); // Z вздовж напрямку сегмента
            segTrigger.AddComponent<WaterVolume>();
        }

        // справжнє дно - нижче поверхні й вужче (береги лишаються мілководними, середина глибша)
        GameObject bed = new GameObject("Lakebed");
        bed.transform.SetParent(waterRoot.transform, false);
        bed.transform.localPosition = new Vector3(0f, -5.2f, 0f);
        Material bedMat = MakeColorMat(new Color(0.33f, 0.3f, 0.22f));
        bed.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateRibbon(centerPoints, width * 0.55f);
        bed.AddComponent<MeshRenderer>().sharedMaterial = bedMat;

        GameObject shallowBed = new GameObject("ShallowBed");
        shallowBed.transform.SetParent(waterRoot.transform, false);
        shallowBed.transform.localPosition = new Vector3(0f, -1.6f, 0f);
        Material shallowMat = MakeColorMat(new Color(0.4f, 0.36f, 0.27f));
        shallowBed.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateRibbon(centerPoints, width * 0.85f);
        shallowBed.AddComponent<MeshRenderer>().sharedMaterial = shallowMat;

        // водорості й папороті на дні
        GameObject plantsRoot = new GameObject("Plants");
        plantsRoot.transform.SetParent(waterRoot.transform, false);
        int plantCount = Mathf.Max(8, centerPoints.Count * 3);
        for (int i = 0; i < plantCount; i++)
        {
            Vector3 basePt = centerPoints[rng.Next(0, centerPoints.Count)];
            Vector3 jitter = new Vector3(((float)rng.NextDouble() - 0.5f) * width * 0.5f, 0f, ((float)rng.NextDouble() - 0.5f) * width * 0.5f);
            GameObject plant = new GameObject("Plant_" + i);
            plant.transform.SetParent(plantsRoot.transform, false);
            plant.transform.localPosition = basePt + jitter + new Vector3(0f, -4.8f, 0f);
            float scale = 0.4f + (float)rng.NextDouble() * 0.6f;
            MeshFilter mf = plant.AddComponent<MeshFilter>();
            mf.mesh = MeshBuilder.CreateBlob(scale, rng.Next(0, 100000));
            MeshRenderer mr = plant.AddComponent<MeshRenderer>();
            mr.sharedMaterial = MakeColorMat(new Color(0.15f + (float)rng.NextDouble() * 0.1f, 0.32f, 0.14f));
            plant.transform.localScale = new Vector3(0.6f, 1.5f, 0.6f); // витягнуті вертикально, як водорості
        }
    }

    static List<Vector3> JitterLine(Vector3 a, Vector3 b, int segments, float jitterAmount, System.Random rng)
    {
        List<Vector3> pts = new List<Vector3>();
        Vector3 dir = (b - a).normalized;
        Vector3 perp = new Vector3(-dir.z, 0f, dir.x);
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 basePos = Vector3.Lerp(a, b, t);
            float jitter = (i == 0 || i == segments) ? 0f : ((float)rng.NextDouble() - 0.5f) * 2f * jitterAmount;
            pts.Add(basePos + perp * jitter);
        }
        return pts;
    }

    static Color JitterColor(Color baseColor, System.Random rng, float amount)
    {
        float r = Mathf.Clamp01(baseColor.r + ((float)rng.NextDouble() - 0.5f) * amount);
        float g = Mathf.Clamp01(baseColor.g + ((float)rng.NextDouble() - 0.5f) * amount);
        float b = Mathf.Clamp01(baseColor.b + ((float)rng.NextDouble() - 0.5f) * amount);
        return new Color(r, g, b);
    }

    static GameObject BuildMobVisual(string mobName, System.Random rng)
    {
        GameObject root;
        switch (mobName)
        {
            case "Гігантський павук":
            {
                root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                root.transform.localScale = Vector3.one * 1.1f;
                Material spiderMat = MakeColorMat(JitterColor(new Color(0.08f, 0.08f, 0.08f), rng, 0.04f));
                root.GetComponent<Renderer>().sharedMaterial = spiderMat;
                for (int i = 0; i < 8; i++)
                {
                    float angleDeg = i * 45f;
                    float rad = angleDeg * Mathf.Deg2Rad;
                    GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leg.name = "Leg_" + i;
                    Object.DestroyImmediate(leg.GetComponent<Collider>());
                    leg.transform.SetParent(root.transform, false);
                    leg.transform.localScale = new Vector3(0.06f, 0.5f, 0.06f);
                    leg.transform.localPosition = new Vector3(Mathf.Cos(rad) * 0.5f, -0.3f, Mathf.Sin(rad) * 0.5f);
                    leg.transform.localRotation = Quaternion.Euler(0f, angleDeg, 60f);
                    leg.GetComponent<Renderer>().sharedMaterial = spiderMat;
                }
                // маленька грудка-черевце ззаду
                GameObject abdomen = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                abdomen.name = "Abdomen";
                Object.DestroyImmediate(abdomen.GetComponent<Collider>());
                abdomen.transform.SetParent(root.transform, false);
                abdomen.transform.localScale = new Vector3(1.2f, 1f, 1.4f);
                abdomen.transform.localPosition = new Vector3(0f, -0.05f, -0.65f);
                abdomen.GetComponent<Renderer>().sharedMaterial = spiderMat;
                // світні червоні очі
                for (int e = -1; e <= 1; e += 2)
                {
                    GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    eye.name = "Eye";
                    Object.DestroyImmediate(eye.GetComponent<Collider>());
                    eye.transform.SetParent(root.transform, false);
                    eye.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                    eye.transform.localPosition = new Vector3(e * 0.22f, 0.15f, 0.42f);
                    Material eyeMat = MakeColorMat(new Color(0.9f, 0.1f, 0.05f));
                    eye.GetComponent<Renderer>().sharedMaterial = eyeMat;
                }
                break;
            }
            case "Отруйна змія":
            {
                root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                root.transform.localScale = new Vector3(0.6f, 1.6f, 0.6f);
                root.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                root.GetComponent<Renderer>().sharedMaterial = MakeColorMat(JitterColor(new Color(0.2f, 0.5f, 0.18f), rng, 0.15f));
                break;
            }
            case "Дикий кабан":
            {
                root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                root.transform.localScale = new Vector3(0.9f, 0.6f, 0.9f);
                Material boarMat = MakeColorMat(JitterColor(new Color(0.4f, 0.28f, 0.18f), rng, 0.1f));
                root.GetComponent<Renderer>().sharedMaterial = boarMat;
                for (int t = -1; t <= 1; t += 2)
                {
                    GameObject tusk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    tusk.name = "Tusk";
                    Object.DestroyImmediate(tusk.GetComponent<Collider>());
                    tusk.transform.SetParent(root.transform, false);
                    tusk.transform.localScale = new Vector3(0.08f, 0.2f, 0.08f);
                    tusk.transform.localPosition = new Vector3(t * 0.25f, -0.3f, 0.4f);
                    tusk.transform.localRotation = Quaternion.Euler(70f, 0f, 0f);
                    tusk.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.9f, 0.88f, 0.8f));
                }
                break;
            }
            case "Лісовий вовк":
            {
                root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                root.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
                Material wolfMat = MakeColorMat(JitterColor(new Color(0.42f, 0.4f, 0.38f), rng, 0.12f));
                root.GetComponent<Renderer>().sharedMaterial = wolfMat;
                for (int e = -1; e <= 1; e += 2)
                {
                    GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    ear.name = "Ear";
                    Object.DestroyImmediate(ear.GetComponent<Collider>());
                    ear.transform.SetParent(root.transform, false);
                    ear.transform.localScale = new Vector3(0.12f, 0.18f, 0.12f);
                    ear.transform.localPosition = new Vector3(e * 0.2f, 0.55f, 0f);
                    ear.GetComponent<Renderer>().sharedMaterial = wolfMat;
                }
                // пухнастий хвіст
                GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tail.name = "Tail";
                Object.DestroyImmediate(tail.GetComponent<Collider>());
                tail.transform.SetParent(root.transform, false);
                tail.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);
                tail.transform.localPosition = new Vector3(0f, -0.1f, -0.55f);
                tail.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
                tail.GetComponent<Renderer>().sharedMaterial = wolfMat;
                break;
            }
            case "Печерний тролль":
            {
                root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                root.transform.localScale = new Vector3(1.3f, 1.5f, 1.3f);
                Material trollMat = MakeColorMat(JitterColor(new Color(0.35f, 0.42f, 0.3f), rng, 0.12f));
                root.GetComponent<Renderer>().sharedMaterial = trollMat;
                // кам'янисті нарости-бородавки на шкірі
                for (int b = 0; b < 5; b++)
                {
                    GameObject bump = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    bump.name = "Bump_" + b;
                    Object.DestroyImmediate(bump.GetComponent<Collider>());
                    bump.transform.SetParent(root.transform, false);
                    float bumpScale = 0.15f + (float)rng.NextDouble() * 0.15f;
                    bump.transform.localScale = new Vector3(bumpScale, bumpScale, bumpScale);
                    float ang = (float)rng.NextDouble() * 360f * Mathf.Deg2Rad;
                    float h = -0.2f + (float)rng.NextDouble() * 0.6f;
                    bump.transform.localPosition = new Vector3(Mathf.Cos(ang) * 0.48f, h, Mathf.Sin(ang) * 0.48f);
                    bump.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.36f, 0.26f));
                }
                // невеликі бивні знизу
                for (int t = -1; t <= 1; t += 2)
                {
                    GameObject tusk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    tusk.name = "Tusk";
                    Object.DestroyImmediate(tusk.GetComponent<Collider>());
                    tusk.transform.SetParent(root.transform, false);
                    tusk.transform.localScale = new Vector3(0.06f, 0.18f, 0.06f);
                    tusk.transform.localPosition = new Vector3(t * 0.15f, -0.42f, 0.35f);
                    tusk.transform.localRotation = Quaternion.Euler(150f, 0f, 0f);
                    tusk.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.85f, 0.82f, 0.75f));
                }
                break;
            }
            default: // Лісовий розбійник і будь-які інші
            {
                root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Material banditMat = MakeColorMat(JitterColor(new Color(0.25f, 0.22f, 0.2f), rng, 0.08f));
                root.GetComponent<Renderer>().sharedMaterial = banditMat;
                GameObject hood = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                hood.name = "Hood";
                Object.DestroyImmediate(hood.GetComponent<Collider>());
                hood.transform.SetParent(root.transform, false);
                hood.transform.localScale = new Vector3(0.35f, 0.15f, 0.35f);
                hood.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                hood.GetComponent<Renderer>().sharedMaterial = banditMat;
                // плащ ззаду
                GameObject cape = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cape.name = "Cape";
                Object.DestroyImmediate(cape.GetComponent<Collider>());
                cape.transform.SetParent(root.transform, false);
                cape.transform.localScale = new Vector3(0.55f, 0.9f, 0.08f);
                cape.transform.localPosition = new Vector3(0f, -0.05f, -0.28f);
                cape.GetComponent<Renderer>().sharedMaterial = MakeColorMat(new Color(0.3f, 0.12f, 0.1f));
                break;
            }
        }
        return root;
    }

    static void CreateRock(Transform parent, Vector3 pos, Material mat, System.Random rng, float minSize, float maxSize)
    {
        GameObject rock = new GameObject("Rock");
        rock.transform.SetParent(parent);
        rock.transform.position = pos;
        float size = minSize + (float)rng.NextDouble() * (maxSize - minSize);
        MeshFilter mf = rock.AddComponent<MeshFilter>();
        Mesh rockMesh = MeshBuilder.CreateBlob(size, rng.Next(0, 100000));
        mf.mesh = rockMesh;
        MeshRenderer mr = rock.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        MeshCollider col = rock.AddComponent<MeshCollider>();
        col.sharedMesh = rockMesh;
        rock.transform.localScale = new Vector3(1f, 0.55f + (float)rng.NextDouble() * 0.35f, 1f); // трохи сплюснуті, як справжнє каміння
        rock.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
    }

    class MountainPeak
    {
        public Vector2 center;
        public float radiusX, radiusZ, rotationDeg, height;
        public MountainPeak(Vector2 c, float rx, float rz, float rot, float h)
        {
            center = c; radiusX = rx; radiusZ = rz; rotationDeg = rot; height = h;
        }
    }

    [MenuItem("Вежа/Локації/Додати пагорби навколо міста")]
    public static void AddHills()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogWarning("Не знайдено активний Terrain у сцені.");
            return;
        }
        TerrainData data = terrain.terrainData;

        const float maxHillHeight = 75f;
        data.size = new Vector3(data.size.x, maxHillHeight, data.size.z);

        // Місто-сітка сягає кутами приблизно 216м від центру - зона рівної землі має бути ще більшою.
        const float flatRadius = 280f;
        const float blendRadius = 55f;
        Vector3 terrainPos = terrain.transform.position;

        // Гори тепер овальні, різного розміру й повернуті під різними кутами - не симетричні конуси.
        // radiusX/radiusZ різні = витягнутий хребет; кут повороту довільний; height - відносна висота піку.
        MountainPeak[] peaks = {
            new MountainPeak(new Vector2(300f, 60f),   150f, 85f,   15f, 1.0f),
            new MountainPeak(new Vector2(-280f, 170f),  95f, 190f,  70f, 0.9f),  // видовжений хребет
            new MountainPeak(new Vector2(110f, -320f), 170f, 100f, -35f, 0.95f),
            new MountainPeak(new Vector2(-230f, -260f), 80f, 80f,   0f,  0.55f), // менша, кругліша гора
            new MountainPeak(new Vector2(20f, 330f),   190f, 110f,  45f, 1.0f),
            new MountainPeak(new Vector2(255f, -230f),  60f, 140f, 100f, 0.5f),  // низька довга гряда
        };

        float ComputeHeight(float worldX, float worldZ)
        {
            Vector2 worldXZ = new Vector2(worldX, worldZ);
            float distFromCenter = worldXZ.magnitude;

            // кілька октав шуму замість одної - помітно більш нерівний, "живий" рельєф
            float n1 = Mathf.PerlinNoise(worldX * 0.008f, worldZ * 0.008f);
            float n2 = Mathf.PerlinNoise(worldX * 0.025f + 100f, worldZ * 0.025f + 100f) * 0.5f;
            float n3 = Mathf.PerlinNoise(worldX * 0.06f + 500f, worldZ * 0.06f + 500f) * 0.25f;
            float baseNoise = (n1 + n2 + n3) / 1.75f;

            float blend = Mathf.InverseLerp(flatRadius, flatRadius + blendRadius, distFromCenter);
            float h = baseNoise * 0.35f * blend;

            foreach (MountainPeak peak in peaks)
            {
                float dx = worldX - peak.center.x;
                float dz = worldZ - peak.center.y;
                float rad = -peak.rotationDeg * Mathf.Deg2Rad;
                float rx = dx * Mathf.Cos(rad) - dz * Mathf.Sin(rad);
                float rz = dx * Mathf.Sin(rad) + dz * Mathf.Cos(rad);
                float normDist = Mathf.Sqrt((rx * rx) / (peak.radiusX * peak.radiusX) + (rz * rz) / (peak.radiusZ * peak.radiusZ));
                if (normDist < 1f)
                {
                    // два різних масштаби шуму на схилі - і крупні складки, і дрібна кам'яниста нерівність
                    float ridgeNoise = Mathf.PerlinNoise(worldX * 0.02f, worldZ * 0.02f) * 0.3f + 0.6f;
                    float fineNoise = Mathf.PerlinNoise(worldX * 0.08f + 777f, worldZ * 0.08f + 777f) * 0.15f;
                    float factor = (1f - normDist) * (ridgeNoise + fineNoise);
                    h = Mathf.Max(h, factor * factor * peak.height);
                }
            }
            return h;
        }

        int res = data.heightmapResolution;
        float[,] heights = new float[res, res];
        for (int zi = 0; zi < res; zi++)
        {
            for (int xi = 0; xi < res; xi++)
            {
                float worldX = terrainPos.x + (float)xi / (res - 1) * data.size.x;
                float worldZ = terrainPos.z + (float)zi / (res - 1) * data.size.z;
                heights[zi, xi] = ComputeHeight(worldX, worldZ);
            }
        }
        data.SetHeights(0, 0, heights);

        // --- Темна кам'яниста текстура на високих/крутих ділянках - трава лишається тільки на рівнині ---
        // Збережено як файли-асети (не лише в пам'яті) - інакше посилання губиться після
        // перезапуску Unity, і на горах з'являються "клітинки" замість кольору.
        string genDir = "Assets/GeneratedTerrainAssets";
        if (!AssetDatabase.IsValidFolder(genDir))
        {
            AssetDatabase.CreateFolder("Assets", "GeneratedTerrainAssets");
        }
        string texPath = genDir + "/ProcRockTexture.asset";
        string layerPath = genDir + "/ProcRockLayer.asset";

        Texture2D rockTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (rockTex == null)
        {
            rockTex = new Texture2D(4, 4);
            Color rockColor = new Color(0.3f, 0.28f, 0.26f); // темніший, драматичніший камінь
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    rockTex.SetPixel(x, y, rockColor);
            rockTex.Apply();
            AssetDatabase.CreateAsset(rockTex, texPath);
        }

        TerrainLayer rockLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
        if (rockLayer == null)
        {
            rockLayer = new TerrainLayer();
            rockLayer.diffuseTexture = rockTex;
            rockLayer.tileSize = new Vector2(10f, 10f);
            AssetDatabase.CreateAsset(rockLayer, layerPath);
        }
        else
        {
            rockLayer.diffuseTexture = rockTex; // про всяк випадок - оновлюємо посилання, якщо воно було втрачене
        }

        bool layerAlreadyInTerrain = false;
        foreach (TerrainLayer l in data.terrainLayers)
        {
            if (l == rockLayer) { layerAlreadyInTerrain = true; break; }
        }
        if (!layerAlreadyInTerrain)
        {
            TerrainLayer[] newLayers = new TerrainLayer[data.terrainLayers.Length + 1];
            System.Array.Copy(data.terrainLayers, newLayers, data.terrainLayers.Length);
            newLayers[newLayers.Length - 1] = rockLayer;
            data.terrainLayers = newLayers;
        }
        int rockLayerIndex = System.Array.IndexOf(data.terrainLayers, rockLayer);

        int alphaRes = data.alphamapResolution;
        int layerCount = data.terrainLayers.Length;
        float[,,] alphamaps = data.GetAlphamaps(0, 0, alphaRes, alphaRes);
        for (int zi = 0; zi < alphaRes; zi++)
        {
            for (int xi = 0; xi < alphaRes; xi++)
            {
                float worldX = terrainPos.x + (float)xi / (alphaRes - 1) * data.size.x;
                float worldZ = terrainPos.z + (float)zi / (alphaRes - 1) * data.size.z;
                float h = ComputeHeight(worldX, worldZ);
                float rockWeight = Mathf.Clamp01((h - 0.15f) / 0.35f);

                float nonRockSum = 0f;
                for (int l = 0; l < layerCount; l++)
                {
                    if (l != rockLayerIndex) nonRockSum += alphamaps[zi, xi, l];
                }
                if (nonRockSum < 0.0001f)
                {
                    // цю ділянку ніколи не фарбували вручну пензлем - без цієї заглушки
                    // вона лишалась би БЕЗ жодної текстури (0 на всіх шарах), звідси й "працює
                    // лише там, де я малював". Тепер вважаємо її базовою травою (перший не-кам'яний шар).
                    int baseLayer = (rockLayerIndex == 0 && layerCount > 1) ? 1 : 0;
                    alphamaps[zi, xi, baseLayer] = 1f;
                    nonRockSum = 1f;
                }

                for (int l = 0; l < layerCount; l++)
                {
                    if (l == rockLayerIndex) alphamaps[zi, xi, l] = rockWeight;
                    else alphamaps[zi, xi, l] = (alphamaps[zi, xi, l] / nonRockSum) * (1f - rockWeight);
                }
            }
        }
        data.SetAlphamaps(0, 0, alphamaps);

        Debug.Log("Гори додано: різної форми, розміру й повороту, з кам'янисто-сірою текстурою на висотах. У радіусі " + flatRadius + "м навколо площі рельєф лишається рівним. Тепер запусти \"Підняти дерева й мобів на рівень нового рельєфу\".");
    }

    [MenuItem("Вежа/Локації/Підняти дерева й мобів на рівень нового рельєфу")]
    public static void SnapNatureToTerrain()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogWarning("Не знайдено активний Terrain у сцені.");
            return;
        }

        GameObject wild = GameObject.Find("Wilderness");
        if (wild != null)
        {
            foreach (Transform child in wild.transform)
            {
                float h = terrain.SampleHeight(child.position) + terrain.transform.position.y;
                Vector3 p = child.position;
                p.y = h;
                child.position = p;
            }
        }

        GameObject rocks = GameObject.Find("Rocks");
        if (rocks != null)
        {
            foreach (Transform child in rocks.transform)
            {
                float h = terrain.SampleHeight(child.position) + terrain.transform.position.y;
                Vector3 p = child.position;
                p.y = h;
                child.position = p;
            }
        }

        GameObject mobs = GameObject.Find("Mobs");
        if (mobs != null)
        {
            foreach (Transform child in mobs.transform)
            {
                float h = terrain.SampleHeight(child.position) + terrain.transform.position.y;
                Vector3 p = child.position;
                p.y = h + 1f;
                child.position = p;
            }
        }

        Debug.Log("Дерева, каміння й моби піднято/опущено на рівень нового рельєфу.");
    }

    static bool IsInWaterBand(float worldX, float worldZ, float angleStart, float angleEnd, float radiusInner, float radiusOuter, float width)
    {
        float dist = new Vector2(worldX, worldZ).magnitude;
        float halfWidth = width / 2f + 8f; // запас з обох боків
        if (dist < radiusInner - halfWidth || dist > radiusOuter + halfWidth) return false;
        float angle = Mathf.Atan2(worldZ, worldX) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;

        if (angleEnd <= 360f)
        {
            return angle >= angleStart - 6f && angle <= angleEnd + 6f;
        }
        // діапазон проходить через 0°/360° (напр. 280..400) - перевіряємо обидві половини
        float wrappedEnd = angleEnd - 360f;
        return angle >= angleStart - 6f || angle <= wrappedEnd + 6f;
    }

    static List<Vector3> GetMainRiverPoints()
    {
        const float cityRadius = 150f;
        const float riverAngleA = 70f, riverAngleB = 230f;
        Vector2 riverA = new Vector2(Mathf.Cos(riverAngleA * Mathf.Deg2Rad), Mathf.Sin(riverAngleA * Mathf.Deg2Rad)) * (cityRadius + 55f);
        Vector2 riverB = new Vector2(Mathf.Cos(riverAngleB * Mathf.Deg2Rad), Mathf.Sin(riverAngleB * Mathf.Deg2Rad)) * (cityRadius + 55f);
        Vector2 riverDir = (riverB - riverA).normalized;
        Vector2 riverPerp = new Vector2(-riverDir.y, riverDir.x);
        List<Vector3> pts = new List<Vector3>();
        const int riverSegs = 20;
        for (int i = 0; i <= riverSegs; i++)
        {
            float t = (float)i / riverSegs;
            Vector2 p = Vector2.Lerp(riverA, riverB, t) + riverPerp * (Mathf.Sin(t * Mathf.PI * 1.4f) * 9f);
            pts.Add(new Vector3(p.x, 0f, p.y));
        }
        return pts;
    }

    static bool IsInAnyWaterBody(float worldX, float worldZ)
    {
        if (IsInWaterBand(worldX, worldZ, 20f, 75f, 175f, 215f, 20f)) return true;    // River (стара дугова версія - для сітчастого міста)
        if (IsInWaterBand(worldX, worldZ, 160f, 205f, 185f, 225f, 24f)) return true;  // Lake_West
        if (IsInWaterBand(worldX, worldZ, 275f, 315f, 170f, 210f, 20f)) return true;  // Lake_North
        if (DistanceToPolyline(new Vector2(worldX, worldZ), GetMainRiverPoints()) < 12f) return true; // Річка радіального міста
        return false;
    }

    [MenuItem("Вежа/Локації/Заповнити всю карту деревами і мобами")]
    public static void FillMapWithNatureAndMobs()
    {
        // Місто-сітка сягає кутами приблизно 216м від центру - дерева/моби починаються ще далі,
        // щоб точно не проростати крізь будинки.
        const float townClearRadius = 175f;
        const float worldEdge = 520f; // під новий, більший розмір карти (бар'єр десь на ~525м)

        GameObject oldWild = GameObject.Find("Wilderness");
        if (oldWild != null) Object.DestroyImmediate(oldWild);
        GameObject wildRoot = new GameObject("Wilderness");

        Terrain terrain = Terrain.activeTerrain;
        int treeCount = 1100;
        List<Vector2> placedTreePositions = new List<Vector2>();
        int giantTreeBudget = Mathf.RoundToInt(treeCount * 0.22f); // ~22% - у 4-5 разів частіше, ніж було (5%)
        int giantPlaced = 0;

        for (int i = 0; i < treeCount; i++)
        {
            bool wantGiant = giantPlaced < giantTreeBudget && (i % 4 == 0);
            float minSpacing = wantGiant ? 15f : 5f; // велетні - з помітним відступом одне від одного

            Vector3 pos = Vector3.zero;
            bool foundSpot = false;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                float angle = UnityEngine.Random.Range(0f, 360f);
                float radius = UnityEngine.Random.Range(townClearRadius, worldEdge);
                float rad = angle * Mathf.Deg2Rad;
                Vector3 candidate = new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
                if (IsInAnyWaterBody(candidate.x, candidate.z)) continue;

                Vector2 c2 = new Vector2(candidate.x, candidate.z);
                bool tooClose = false;
                foreach (Vector2 existing in placedTreePositions)
                {
                    if (Vector2.Distance(c2, existing) < minSpacing) { tooClose = true; break; }
                }
                if (tooClose) continue;

                pos = candidate;
                foundSpot = true;
                break;
            }
            if (!foundSpot) continue;
            placedTreePositions.Add(new Vector2(pos.x, pos.z));

            GameObject newObj = SpawnTreeOrBushVaried(i + 1, wantGiant);
            if (wantGiant) giantPlaced++;

            // одразу підганяємо під рельєф Terrain (якщо вже є пагорби) - без окремого кроку вирівнювання
            if (terrain != null)
            {
                pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;
            }

            newObj.transform.SetParent(wildRoot.transform);
            newObj.transform.position = pos;
        }

        // --- Квіткові кущики розкидані по всій траві - для чарівної, живої атмосфери ---
        GameObject oldFlowers = GameObject.Find("GroundFlowers");
        if (oldFlowers != null) Object.DestroyImmediate(oldFlowers);
        GameObject flowersRoot = new GameObject("GroundFlowers");
        Color[] fieldFlowerColors = {
            new Color(0.85f, 0.2f, 0.3f), new Color(0.9f, 0.85f, 0.25f),
            new Color(0.8f, 0.3f, 0.75f), new Color(0.95f, 0.95f, 0.95f), new Color(0.4f, 0.55f, 0.95f)
        };
        int flowerClusterCount = 400;
        for (int i = 0; i < flowerClusterCount; i++)
        {
            float angle = UnityEngine.Random.Range(0f, 360f);
            float radius = UnityEngine.Random.Range(townClearRadius * 0.5f, worldEdge);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
            if (IsInAnyWaterBody(pos.x, pos.z)) continue;
            if (terrain != null) pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

            GameObject cluster = new GameObject("FlowerCluster");
            cluster.transform.SetParent(flowersRoot.transform);
            cluster.transform.position = pos;
            int petalCount = 2 + UnityEngine.Random.Range(0, 3);
            Color col = fieldFlowerColors[UnityEngine.Random.Range(0, fieldFlowerColors.Length)];
            for (int p = 0; p < petalCount; p++)
            {
                GameObject petal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                petal.name = "Petal";
                Object.DestroyImmediate(petal.GetComponent<Collider>());
                petal.transform.SetParent(cluster.transform, false);
                petal.transform.localScale = Vector3.one * (0.12f + (float)UnityEngine.Random.value * 0.08f);
                petal.transform.localPosition = new Vector3(
                    (UnityEngine.Random.value - 0.5f) * 0.4f,
                    petal.transform.localScale.y * 0.5f,
                    (UnityEngine.Random.value - 0.5f) * 0.4f
                );
                petal.GetComponent<Renderer>().sharedMaterial = MakeColorMat(col);
            }
        }

        GameObject oldMobs = GameObject.Find("Mobs");
        if (oldMobs != null) Object.DestroyImmediate(oldMobs);
        GameObject mobsRoot = new GameObject("Mobs");
        string[] mobNames = { "Лісовий вовк", "Дикий кабан", "Гігантський павук", "Лісовий розбійник", "Печерний тролль", "Отруйна змія" };

        int mobCount = 260;
        for (int i = 0; i < mobCount; i++)
        {
            float angle = UnityEngine.Random.Range(0f, 360f);
            float radius = UnityEngine.Random.Range(townClearRadius, worldEdge);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(rad) * radius, 1f, Mathf.Sin(rad) * radius);
            if (IsInAnyWaterBody(pos.x, pos.z)) continue; // не саджаємо мобів просто у воду
            if (terrain != null) pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + 1f;

            int lvl = 1 + UnityEngine.Random.Range(0, 3);
            string chosenName = mobNames[UnityEngine.Random.Range(0, mobNames.Length)];
            GameObject mob = BuildMobVisual(chosenName, new System.Random(i + 1));
            mob.name = chosenName + "_" + i;
            mob.transform.SetParent(mobsRoot.transform);
            mob.transform.position = pos;
            mob.tag = "Enemy";

            Health health = mob.AddComponent<Health>();
            health.maxHealth = 30f + lvl * 25f;

            MobAI ai = mob.AddComponent<MobAI>();
            ai.mobLevel = lvl;
            ai.attackDamage = 3f + lvl * 2f;
            ai.xpReward = 10f * lvl;

            mob.AddComponent<HitFlash>();
            MobHealthBar hpBar = mob.AddComponent<MobHealthBar>();
            hpBar.mobLevel = lvl;
            MobNameTag tag = mob.AddComponent<MobNameTag>();
            tag.mobName = chosenName;
            tag.mobLevel = lvl;
        }

        Debug.Log("Карту заповнено: " + treeCount + " дерев/кущів і " + mobCount + " мобів, від краю міста (~" + townClearRadius + "м) до країв світу (~" + worldEdge + "м). Якщо додавав(ла) пагорби - запусти після цього ще раз \"Підняти дерева й мобів на рівень нового рельєфу\".");
    }

    [MenuItem("Вежа/Локації/Прибрати траву з доріг і річки")]
    public static void ClearGrassFromRoadsAndRiver()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogWarning("Не знайдено активний Terrain у сцені.");
            return;
        }
        TerrainData data = terrain.terrainData;
        if (data.detailPrototypes.Length == 0)
        {
            Debug.LogWarning("На Terrain немає жодного шару трави (Detail) - нічого прибирати.");
            return;
        }
        Vector3 terrainPos = terrain.transform.position;

        const float blockSize = 34f;
        const float streetWidth = 10f;
        const int gridExtent = 4;
        const float townRadius = 145f;
        float halfStreet = streetWidth / 2f + 2.5f;

        bool IsOnRoad(float worldX, float worldZ)
        {
            if (Mathf.Abs(worldX) <= townRadius + halfStreet)
            {
                for (int i = -gridExtent; i <= gridExtent; i++)
                {
                    if (Mathf.Abs(worldZ - i * blockSize) < halfStreet) return true;
                }
            }
            if (Mathf.Abs(worldZ) <= townRadius + halfStreet)
            {
                for (int i = -gridExtent; i <= gridExtent; i++)
                {
                    if (Mathf.Abs(worldX - i * blockSize) < halfStreet) return true;
                }
            }
            return false;
        }

        int detailRes = data.detailResolution;
        int layerCount = data.detailPrototypes.Length;
        int cleared = 0;

        for (int layer = 0; layer < layerCount; layer++)
        {
            int[,] map = data.GetDetailLayer(0, 0, detailRes, detailRes, layer);
            for (int zi = 0; zi < detailRes; zi++)
            {
                for (int xi = 0; xi < detailRes; xi++)
                {
                    float worldX = terrainPos.x + (float)xi / (detailRes - 1) * data.size.x;
                    float worldZ = terrainPos.z + (float)zi / (detailRes - 1) * data.size.z;

                    if (IsOnRoad(worldX, worldZ) || IsInAnyWaterBody(worldX, worldZ))
                    {
                        if (map[zi, xi] != 0) { map[zi, xi] = 0; cleared++; }
                    }
                }
            }
            data.SetDetailLayer(0, 0, layer, map);
        }

        Debug.Log("Траву прибрано з ділянок під дорогами й річкою (" + cleared + " точок деталей очищено). Дороги й річка тепер мають бути видні крізь траву.");
    }

    [MenuItem("Вежа/Локації/Автоматично зафарбувати травою всю карту")]
    public static void AutoPaintGrassEverywhere()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogWarning("Не знайдено активний Terrain у сцені.");
            return;
        }
        TerrainData data = terrain.terrainData;
        if (data.detailPrototypes.Length == 0)
        {
            Debug.LogWarning("На Terrain немає жодного шару трави (Detail) - спочатку додай хоча б один через Paint Details → Edit Details.");
            return;
        }
        Vector3 terrainPos = terrain.transform.position;
        int detailRes = data.detailResolution;
        int layerCount = data.detailPrototypes.Length;

        for (int layer = 0; layer < layerCount; layer++)
        {
            int[,] map = new int[detailRes, detailRes];
            for (int zi = 0; zi < detailRes; zi++)
            {
                for (int xi = 0; xi < detailRes; xi++)
                {
                    float worldX = terrainPos.x + (float)xi / (detailRes - 1) * data.size.x;
                    float worldZ = terrainPos.z + (float)zi / (detailRes - 1) * data.size.z;

                    bool onWater = IsInAnyWaterBody(worldX, worldZ);
                    float distFromCenter = new Vector2(worldX, worldZ).magnitude;
                    bool onTownCore = distFromCenter < 20f; // площа з фонтаном лишається без трави

                    map[zi, xi] = (onWater || onTownCore || layer != 0) ? 0 : 9; // трава лише на першому шарі, скрізь окрім води й самої площі
                }
            }
            data.SetDetailLayer(0, 0, layer, map);
        }

        Debug.Log("Траву перефарбовано програмно по всій карті (окрім водойм і самої площі) - ручний пензлик більше не потрібен для рівномірного покриття.");
    }

    static bool IsNearRoadLine(Vector3 pos, float[] ringRadii, int spokeCount)
    {
        float r = new Vector2(pos.x, pos.z).magnitude;
        float thetaDeg = Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg;

        foreach (float ringR in ringRadii)
        {
            if (Mathf.Abs(r - ringR) < 3f + 5f) return true; // половина ширини кільцевої дороги + запас
        }

        for (int s = 0; s < spokeCount; s++)
        {
            float spokeThetaDeg = 360f / spokeCount * s;
            float angDiffRad = Mathf.DeltaAngle(thetaDeg, spokeThetaDeg) * Mathf.Deg2Rad;
            float perpDist = r * Mathf.Abs(Mathf.Sin(angDiffRad));
            float spokeHalfWidth = (s == 0) ? 7f : 4f; // головна брама ширша за звичайні спиці
            if (perpDist < spokeHalfWidth + 5f) return true;
        }
        return false;
    }

    static float DistanceToPolyline(Vector2 p, List<Vector3> polyline)
    {
        float minDist = float.MaxValue;
        for (int i = 0; i < polyline.Count - 1; i++)
        {
            Vector2 a = new Vector2(polyline[i].x, polyline[i].z);
            Vector2 b = new Vector2(polyline[i + 1].x, polyline[i + 1].z);
            Vector2 ab = b - a;
            float t = Vector2.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude);
            t = Mathf.Clamp01(t);
            Vector2 closest = a + ab * t;
            float d = Vector2.Distance(p, closest);
            if (d < minDist) minDist = d;
        }
        return minDist;
    }

    [MenuItem("Вежа/Локації/Побудувати кругле радіальне місто (за макетом)")]
    public static void BuildRadialCity()
    {
        Material roadMat = MakeColorMat(new Color(0.5f, 0.47f, 0.42f));
        Material waterMat = MakeTransparentMat(new Color(0.2f, 0.45f, 0.7f, 0.72f));
        Material wallMat = MakeColorMat(new Color(0.45f, 0.42f, 0.38f));
        Material plazaMat = MakeColorMat(new Color(0.56f, 0.53f, 0.48f));

        const float cityRadius = 150f;
        const float clearRadius = 18f;
        const int spokeCount = 8;
        float[] ringRadii = { 55f, 100f, 150f };

        foreach (string n in new[] { "Town", "Roads", "Wall", "River", "PlazaPavement", "Fountain" })
        {
            GameObject old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }

        GameObject townRoot = new GameObject("Town");
        GameObject roadsRoot = new GameObject("Roads");

        // --- Річка, що ріже місто наскрізь (як на референсі), не проходячи точно через центр ---
        List<Vector3> riverPts = GetMainRiverPoints();
        BuildWaterBody("River", riverPts, 14f, waterMat, new System.Random(5555));

        // --- Кільцеві вулиці ---
        foreach (float r in ringRadii)
        {
            GameObject ring = new GameObject("RingRoad_" + r);
            ring.transform.SetParent(roadsRoot.transform);
            ring.transform.position = new Vector3(0f, 0.18f, 0f);
            ring.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateAnnulus(r - 3f, r + 3f, 64);
            ring.AddComponent<MeshRenderer>().sharedMaterial = roadMat;
        }

        // --- Радіальні вулиці-спиці від центру до стіни ---
        for (int i = 0; i < spokeCount; i++)
        {
            float angle = 360f / spokeCount * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));

            GameObject spoke = new GameObject("Spoke_" + i);
            spoke.transform.SetParent(roadsRoot.transform);
            spoke.transform.position = new Vector3(0f, 0.18f, 0f);
            List<Vector3> pts = new List<Vector3> { dir * clearRadius, dir * cityRadius };
            float w = (i == 0) ? 14f : 8f; // перша спиця ширша - головна брама, як на референсі
            spoke.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateRibbon(pts, w);
            spoke.AddComponent<MeshRenderer>().sharedMaterial = roadMat;

            if (i == 0)
            {
                GameObject gateRoad = new GameObject("GateRoad");
                gateRoad.transform.SetParent(roadsRoot.transform);
                gateRoad.transform.position = new Vector3(0f, 0.18f, 0f);
                List<Vector3> gatePts = new List<Vector3> { dir * cityRadius, dir * (cityRadius + 80f) };
                gateRoad.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateRibbon(gatePts, 10f);
                gateRoad.AddComponent<MeshRenderer>().sharedMaterial = roadMat;
            }
        }

        // --- Стіна з розривами-брамами (на кожній спиці й там, де річку перетинає межа) ---
        List<float> gateAngles = new List<float>();
        for (int i = 0; i < spokeCount; i++) gateAngles.Add(360f / spokeCount * i); // брама на кожній вулиці-спиці

        List<float> riverCrossingAngles = new List<float>();
        for (int i = 0; i < riverPts.Count - 1; i++)
        {
            float d0 = new Vector2(riverPts[i].x, riverPts[i].z).magnitude;
            float d1 = new Vector2(riverPts[i + 1].x, riverPts[i + 1].z).magnitude;
            if ((d0 < cityRadius) != (d1 < cityRadius))
            {
                Vector3 mid = (riverPts[i] + riverPts[i + 1]) / 2f;
                float crossAngle = Mathf.Atan2(mid.z, mid.x) * Mathf.Rad2Deg;
                if (crossAngle < 0f) crossAngle += 360f;
                riverCrossingAngles.Add(crossAngle);
            }
        }
        gateAngles.AddRange(riverCrossingAngles);

        const float gateHalfWidthDeg = 9f;
        bool IsNearGate(float angleDeg)
        {
            foreach (float g in gateAngles)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(angleDeg, g)) < gateHalfWidthDeg) return true;
            }
            return false;
        }

        GameObject wallRoot = new GameObject("Wall");
        int wallSegCount = 72;
        for (int i = 0; i < wallSegCount; i++)
        {
            float angle = 360f / wallSegCount * i;
            if (IsNearGate(angle)) continue; // тут розрив - брама або міст через річку
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(rad) * cityRadius, 0f, Mathf.Sin(rad) * cityRadius);
            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = "WallSeg_" + i;
            seg.transform.SetParent(wallRoot.transform);
            seg.transform.position = pos + new Vector3(0f, 3f, 0f);
            float segLen = 2f * Mathf.PI * cityRadius / wallSegCount + 1f;
            seg.transform.localScale = new Vector3(segLen, 6f, 3f);
            seg.transform.LookAt(seg.transform.position + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)));
            seg.GetComponent<Renderer>().sharedMaterial = wallMat;
        }

        // --- Мости там, де річка перетинає стіну ---
        Material bridgeMat = MakeColorMat(new Color(0.42f, 0.32f, 0.22f));
        foreach (float crossAngle in riverCrossingAngles)
        {
            float rad = crossAngle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
            GameObject bridge = new GameObject("Bridge_" + crossAngle);
            bridge.transform.position = dir * cityRadius + new Vector3(0f, 0.5f, 0f);
            List<Vector3> bridgePts = new List<Vector3> { dir * -20f, dir * 20f };
            bridge.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateRibbon(bridgePts, 8f);
            bridge.AddComponent<MeshRenderer>().sharedMaterial = bridgeMat;
        }

        // --- Мости там, де річка перетинає дороги ВСЕРЕДИНІ міста (кільцеві й спиці) ---
        for (int i = 0; i < riverPts.Count; i++)
        {
            Vector3 p = riverPts[i];
            if (!IsNearRoadLine(p, ringRadii, spokeCount)) continue;

            Vector3 dir = (i < riverPts.Count - 1)
                ? (riverPts[i + 1] - p).normalized
                : (p - riverPts[i - 1]).normalized;
            Vector3 perp = new Vector3(-dir.z, 0f, dir.x);

            GameObject bridge = new GameObject("InnerBridge_" + i);
            bridge.transform.position = p + new Vector3(0f, 0.5f, 0f);
            List<Vector3> bridgePts = new List<Vector3> { perp * -9f, perp * 9f };
            bridge.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateRibbon(bridgePts, 10f);
            bridge.AddComponent<MeshRenderer>().sharedMaterial = bridgeMat;
        }

        // --- Кругла бруківка навколо фонтану ---
        GameObject plaza = new GameObject("PlazaPavement");
        plaza.transform.position = new Vector3(0f, 0.15f, 0f);
        plaza.AddComponent<MeshFilter>().mesh = MeshBuilder.CreateAnnulus(0f, clearRadius + 4f, 48);
        plaza.AddComponent<MeshRenderer>().sharedMaterial = plazaMat;

        // --- Будинки заповнюють "клітинки" між спицями й кільцями - не прямокутна сітка ---
        System.Random rng = new System.Random(321);
        float[] ringBoundaries = new float[ringRadii.Length + 1];
        ringBoundaries[0] = clearRadius;
        for (int i = 0; i < ringRadii.Length; i++) ringBoundaries[i + 1] = ringRadii[i];

        for (int ringIdx = 0; ringIdx < ringBoundaries.Length - 1; ringIdx++)
        {
            float rInner = ringBoundaries[ringIdx] + 4f;
            float rOuter = ringBoundaries[ringIdx + 1] - 4f;
            if (rOuter <= rInner) continue;

            for (int spokeIdx = 0; spokeIdx < spokeCount; spokeIdx++)
            {
                float aStart = 360f / spokeCount * spokeIdx + 4f;
                float aEnd = 360f / spokeCount * (spokeIdx + 1) - 4f;

                // кількість будинків масштабується від ПЛОЩІ клітинки - приблизно у 8 разів густіше, ніж раніше
                float cellAreaSqM = (aEnd - aStart) * Mathf.Deg2Rad / 2f * (rOuter * rOuter - rInner * rInner);
                int buildingsInCell = Mathf.Max(2, Mathf.RoundToInt(cellAreaSqM / 24f));

                List<Vector2> placedInCell = new List<Vector2>();
                int attempts = 0;
                int placed = 0;
                while (placed < buildingsInCell && attempts < buildingsInCell * 6)
                {
                    attempts++;
                    float a = Mathf.Lerp(aStart, aEnd, (float)rng.NextDouble());
                    float r = Mathf.Lerp(rInner, rOuter, (float)rng.NextDouble());
                    float rad = a * Mathf.Deg2Rad;
                    Vector3 pos = new Vector3(Mathf.Cos(rad) * r, 0f, Mathf.Sin(rad) * r);
                    Vector2 pos2 = new Vector2(pos.x, pos.z);

                    if (DistanceToPolyline(pos2, riverPts) < 14f) continue; // не будувати просто в річці
                    if (IsNearRoadLine(pos, ringRadii, spokeCount)) continue; // не будувати просто на дорозі

                    bool tooClose = false;
                    foreach (Vector2 existing in placedInCell)
                    {
                        if (Vector2.Distance(pos2, existing) < 7.5f) { tooClose = true; break; }
                    }
                    if (tooClose) continue;

                    placedInCell.Add(pos2);
                    placed++;

                    int seed = ringIdx * 10000 + spokeIdx * 100 + placed;
                    GameObject building = SpawnBuilding(seed, floorsHint: 1 + rng.Next(0, 3));

                    building.transform.SetParent(townRoot.transform);
                    building.transform.position = pos;
                    building.transform.LookAt(Vector3.zero); // фасадом до центру площі
                }
            }
        }

        // --- Гравець спавниться біля фонтану ---
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null) playerObj.transform.position = new Vector3(0f, 1f, -12f);

        // --- Багатоярусний фонтан ---
        GameObject fountain = new GameObject("Fountain");
        Material stoneMat = MakeColorMat(new Color(0.65f, 0.63f, 0.58f));

        GameObject bigBasin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bigBasin.name = "BigBasin";
        bigBasin.transform.SetParent(fountain.transform, false);
        bigBasin.transform.localScale = new Vector3(6f, 0.4f, 6f);
        bigBasin.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        bigBasin.GetComponent<Renderer>().sharedMaterial = stoneMat;

        GameObject water1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water1.name = "Water1";
        Object.DestroyImmediate(water1.GetComponent<Collider>());
        water1.transform.SetParent(fountain.transform, false);
        water1.transform.localScale = new Vector3(5.6f, 0.05f, 5.6f);
        water1.transform.localPosition = new Vector3(0f, 0.62f, 0f);
        water1.GetComponent<Renderer>().sharedMaterial = waterMat;

        GameObject midPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        midPillar.name = "MidPillar";
        midPillar.transform.SetParent(fountain.transform, false);
        midPillar.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
        midPillar.transform.localPosition = new Vector3(0f, 1.4f, 0f);
        midPillar.GetComponent<Renderer>().sharedMaterial = stoneMat;

        GameObject smallBasin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        smallBasin.name = "SmallBasin";
        smallBasin.transform.SetParent(fountain.transform, false);
        smallBasin.transform.localScale = new Vector3(2.6f, 0.25f, 2.6f);
        smallBasin.transform.localPosition = new Vector3(0f, 2.3f, 0f);
        smallBasin.GetComponent<Renderer>().sharedMaterial = stoneMat;

        GameObject water2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water2.name = "Water2";
        Object.DestroyImmediate(water2.GetComponent<Collider>());
        water2.transform.SetParent(fountain.transform, false);
        water2.transform.localScale = new Vector3(2.3f, 0.05f, 2.3f);
        water2.transform.localPosition = new Vector3(0f, 2.42f, 0f);
        water2.GetComponent<Renderer>().sharedMaterial = waterMat;

        GameObject pillar2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar2.name = "TopPillar";
        pillar2.transform.SetParent(fountain.transform, false);
        pillar2.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);
        pillar2.transform.localPosition = new Vector3(0f, 3f, 0f);
        pillar2.GetComponent<Renderer>().sharedMaterial = stoneMat;

        GameObject spout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spout.name = "SpoutTop";
        spout.transform.SetParent(fountain.transform, false);
        spout.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        spout.transform.localPosition = new Vector3(0f, 3.9f, 0f);
        spout.GetComponent<Renderer>().sharedMaterial = waterMat;

        for (int i = 0; i < 4; i++)
        {
            float ang = i * 90f * Mathf.Deg2Rad;
            GameObject edgePillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            edgePillar.name = "EdgePillar_" + i;
            edgePillar.transform.SetParent(fountain.transform, false);
            edgePillar.transform.localScale = new Vector3(0.3f, 0.6f, 0.3f);
            edgePillar.transform.localPosition = new Vector3(Mathf.Cos(ang) * 5.2f, 0.6f, Mathf.Sin(ang) * 5.2f);
            edgePillar.GetComponent<Renderer>().sharedMaterial = stoneMat;
        }

        Debug.Log("Кругле радіальне місто побудовано за референсом: стіна по периметру, " + spokeCount + " вулиць-спиць, " + ringRadii.Length + " кільцевих доріг, річка навскіс через місто, головна брама, будинки в клітинках між дорогами.");
    }

    [MenuItem("Вежа/Локації/Розширити карту, зробити круглою, додати бар'єр і небо")]
    public static void ExpandMapCircularWithBarrierAndSky()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogWarning("Не знайдено активний Terrain у сцені.");
            return;
        }
        TerrainData data = terrain.terrainData;

        float newSize = 1100f; // приблизно у 1.55 раза більше за попередній розмір (~700)
        data.size = new Vector3(newSize, data.size.y, newSize);
        terrain.transform.position = new Vector3(-newSize / 2f, terrain.transform.position.y, -newSize / 2f);

        float barrierRadius = newSize / 2f - 25f;

        GameObject oldBarrier = GameObject.Find("WorldBarrier");
        if (oldBarrier != null) Object.DestroyImmediate(oldBarrier);
        GameObject barrierRoot = new GameObject("WorldBarrier");

        Material barrierMat = MakeTransparentMat(new Color(0.65f, 0.82f, 0.95f, 0.22f));
        int segCount = 96;
        float barrierHeight = 70f;
        for (int i = 0; i < segCount; i++)
        {
            float angle = 360f / segCount * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(rad) * barrierRadius, barrierHeight / 2f, Mathf.Sin(rad) * barrierRadius);
            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = "BarrierSeg_" + i;
            seg.transform.SetParent(barrierRoot.transform);
            seg.transform.position = pos;
            float segLen = 2f * Mathf.PI * barrierRadius / segCount + 1.5f;
            seg.transform.localScale = new Vector3(segLen, barrierHeight, 1f);
            seg.transform.LookAt(seg.transform.position + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)));
            seg.GetComponent<Renderer>().sharedMaterial = barrierMat;
            // колайдер лишається твердим (не тригер) - фізично блокує гравця від виходу за межі
        }

        // --- Хмари в небі, розкидані по всій території і трохи за бар'єром теж (щоб не було порожнечі) ---
        GameObject oldClouds = GameObject.Find("Clouds");
        if (oldClouds != null) Object.DestroyImmediate(oldClouds);
        GameObject cloudsRoot = new GameObject("Clouds");
        Material cloudMat = MakeColorMat(new Color(0.97f, 0.97f, 0.99f));
        System.Random cloudRng = new System.Random(777);
        int cloudCount = 70;
        for (int i = 0; i < cloudCount; i++)
        {
            float angle = (float)cloudRng.NextDouble() * 360f;
            float radius = (float)cloudRng.NextDouble() * (barrierRadius + 60f);
            float rad = angle * Mathf.Deg2Rad;
            float height = 90f + (float)cloudRng.NextDouble() * 50f;
            Vector3 pos = new Vector3(Mathf.Cos(rad) * radius, height, Mathf.Sin(rad) * radius);

            GameObject cloud = new GameObject("Cloud_" + i);
            cloud.transform.SetParent(cloudsRoot.transform);
            cloud.transform.position = pos;
            int puffCount = 3 + cloudRng.Next(0, 3);
            for (int p = 0; p < puffCount; p++)
            {
                GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                puff.name = "Puff_" + p;
                Object.DestroyImmediate(puff.GetComponent<Collider>());
                puff.transform.SetParent(cloud.transform, false);
                float puffScale = 6f + (float)cloudRng.NextDouble() * 6f;
                puff.transform.localScale = new Vector3(puffScale, puffScale * 0.5f, puffScale);
                puff.transform.localPosition = new Vector3(
                    ((float)cloudRng.NextDouble() - 0.5f) * puffScale,
                    0f,
                    ((float)cloudRng.NextDouble() - 0.5f) * puffScale
                );
                puff.GetComponent<Renderer>().sharedMaterial = cloudMat;
            }
        }

        Debug.Log("Карту розширено до " + newSize + "x" + newSize + " (~1.5-1.6 раза більше). Напівпрозорий бар'єр по колу на радіусі " + barrierRadius + "м. Хмари додані в небі. Не забудь запустити \"Заповнити всю карту деревами і мобами\" ще раз, щоб покрити нову територію.");
    }

    static GameObject BuildBossVisual(int seed)
    {
        System.Random rng = new System.Random(seed);
        GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        boss.name = "BossBody";
        boss.transform.localScale = new Vector3(3.4f, 4.6f, 3.4f);
        Material bodyMat = MakeColorMat(new Color(0.22f, 0.22f, 0.25f));
        boss.GetComponent<Renderer>().sharedMaterial = bodyMat;
        Material darkMat = MakeColorMat(new Color(0.14f, 0.14f, 0.16f));
        Material crystalMat = MakeColorMat(new Color(0.55f, 0.15f, 0.58f));
        Material eyeMat = MakeColorMat(new Color(1f, 0.25f, 0.05f));

        for (int s = -1; s <= 1; s += 2)
        {
            GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shoulder.name = "Shoulder";
            Object.DestroyImmediate(shoulder.GetComponent<Collider>());
            shoulder.transform.SetParent(boss.transform, false);
            shoulder.transform.localScale = new Vector3(0.55f, 0.4f, 0.5f);
            shoulder.transform.localPosition = new Vector3(s * 0.55f, 0.75f, 0f);
            shoulder.GetComponent<Renderer>().sharedMaterial = bodyMat;

            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.name = "Arm";
            Object.DestroyImmediate(arm.GetComponent<Collider>());
            arm.transform.SetParent(boss.transform, false);
            arm.transform.localScale = new Vector3(0.22f, 0.7f, 0.22f);
            arm.transform.localPosition = new Vector3(s * 0.6f, 0.1f, 0f);
            arm.GetComponent<Renderer>().sharedMaterial = bodyMat;

            GameObject fist = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fist.name = "Fist";
            Object.DestroyImmediate(fist.GetComponent<Collider>());
            fist.transform.SetParent(boss.transform, false);
            fist.transform.localScale = new Vector3(0.32f, 0.32f, 0.32f);
            fist.transform.localPosition = new Vector3(s * 0.6f, -0.35f, 0f);
            fist.GetComponent<Renderer>().sharedMaterial = darkMat;
        }

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        Object.DestroyImmediate(head.GetComponent<Collider>());
        head.transform.SetParent(boss.transform, false);
        head.transform.localScale = new Vector3(0.5f, 0.45f, 0.45f);
        head.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        head.GetComponent<Renderer>().sharedMaterial = bodyMat;

        for (int e = -1; e <= 1; e += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            Object.DestroyImmediate(eye.GetComponent<Collider>());
            eye.transform.SetParent(head.transform, false);
            eye.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            eye.transform.localPosition = new Vector3(e * 0.28f, 0f, 0.55f);
            eye.GetComponent<Renderer>().sharedMaterial = eyeMat;
        }

        // кристалічні шипи на плечах/спині - додає загрозливого силуету
        for (int c = 0; c < 5; c++)
        {
            GameObject shard = new GameObject("Shard_" + c);
            shard.transform.SetParent(boss.transform, false);
            float shardHeight = 0.6f + (float)rng.NextDouble() * 0.6f;
            Mesh shardMesh = MeshBuilder.CreateTaperedCylinder(0.12f, 0.01f, shardHeight, 5);
            shard.AddComponent<MeshFilter>().mesh = shardMesh;
            shard.AddComponent<MeshRenderer>().sharedMaterial = crystalMat;
            float ang = (c - 2) * 20f * Mathf.Deg2Rad;
            shard.transform.localPosition = new Vector3(Mathf.Sin(ang) * 0.4f, 0.9f, -0.3f + Mathf.Cos(ang) * 0.1f);
            shard.transform.localRotation = Quaternion.Euler(-20f, 0f, 0f);
        }

        return boss;
    }

    [MenuItem("Вежа/Локації/Створити боса в поточній сцені (зала боса)")]
    public static void CreateBossInChamber()
    {
        GameObject oldBoss = GameObject.Find("Boss");
        if (oldBoss != null) Object.DestroyImmediate(oldBoss);

        GameObject boss = BuildBossVisual(999);
        boss.name = "Boss";
        GameObject bossSpawnMarker = GameObject.Find("BossSpawnPoint");
        boss.transform.position = bossSpawnMarker != null ? bossSpawnMarker.transform.position : new Vector3(0f, 1.5f, 0f);
        boss.tag = "Enemy";

        Health health = boss.AddComponent<Health>();
        health.maxHealth = 800f;

        BossAI ai = boss.AddComponent<BossAI>();
        ai.bossLevel = 10;

        boss.AddComponent<HitFlash>();
        MobHealthBar bossHpBar = boss.AddComponent<MobHealthBar>();
        bossHpBar.mobLevel = ai.bossLevel;

        MobNameTag tag = boss.AddComponent<MobNameTag>();
        tag.mobName = "Страж Вежі";
        tag.mobLevel = ai.bossLevel;

        Debug.Log("Боса створено (\"Страж Вежі\", 800 HP). Перевір позицію вручну - вона мала б бути по центру зали боса.");
    }

    [MenuItem("Вежа/Локації/Перебудувати вежу епічно (вхід з боку міста)")]
    public static void RebuildEpicTower()
    {
        foreach (string n in new[] { "Tower", "PortalToBossChamber" })
        {
            GameObject old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }

        const float towerAngleDeg = 45f; // назад у той бік, де вежа була раніше
        const float towerRadius = 480f; // біля самого бар'єру, на краю карти
        const float scaleFactor = 7.5f; // колосальна - це ж головна вежа всієї гри
        float towerAngleRad = towerAngleDeg * Mathf.Deg2Rad;
        Vector3 towerPos = new Vector3(Mathf.Cos(towerAngleRad) * towerRadius, 0f, Mathf.Sin(towerAngleRad) * towerRadius);
        Vector3 towardCity = -new Vector3(Mathf.Cos(towerAngleRad), 0f, Mathf.Sin(towerAngleRad));

        // підганяємо висоту під реальний рельєф (гори) - інакше вежа "тоне" в піднятій землі
        Terrain terrainForTower = Terrain.activeTerrain;
        if (terrainForTower != null)
        {
            towerPos.y = terrainForTower.SampleHeight(towerPos) + terrainForTower.transform.position.y;
        }

        GameObject tower = new GameObject("Tower");
        tower.transform.position = towerPos;

        Material stoneMat = MakeColorMat(new Color(0.34f, 0.32f, 0.36f));
        Material darkStoneMat = MakeColorMat(new Color(0.22f, 0.2f, 0.24f));
        Material glowMat = MakeColorMat(new Color(0.55f, 0.85f, 0.95f));
        Material crystalMat = MakeColorMat(new Color(0.6f, 0.2f, 0.65f));
        Material ledgeMat = MakeColorMat(new Color(0.28f, 0.26f, 0.3f));

        float baseRadius = 9f;
        float currentRadius = baseRadius;
        float currentY = 0f;
        int tierCount = 12; // удвічі більше поверхів, ніж було
        for (int t = 0; t < tierCount; t++)
        {
            float tierHeight = 13f - t * 0.7f;
            float nextRadius = currentRadius * 0.8f; // повільніше звуження - природніше на такій кількості ярусів

            GameObject tier = new GameObject("Tier_" + t);
            tier.transform.SetParent(tower.transform, false);
            tier.transform.localPosition = new Vector3(0f, currentY, 0f);
            Mesh tierMesh = MeshBuilder.CreateTaperedCylinder(currentRadius, nextRadius, tierHeight, 12);
            tier.AddComponent<MeshFilter>().mesh = tierMesh;
            tier.AddComponent<MeshRenderer>().sharedMaterial = (t % 2 == 0) ? stoneMat : darkStoneMat;
            tier.AddComponent<MeshCollider>().sharedMesh = tierMesh;

            int windowCount = 6;
            for (int w = 0; w < windowCount; w++)
            {
                float wAngle = 360f / windowCount * w * Mathf.Deg2Rad;
                GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
                window.name = "Window";
                Object.DestroyImmediate(window.GetComponent<Collider>());
                window.transform.SetParent(tier.transform, false);
                window.transform.localScale = new Vector3(0.4f, tierHeight * 0.4f, 0.15f);
                float wRadius = (currentRadius + nextRadius) / 2f;
                window.transform.localPosition = new Vector3(Mathf.Cos(wAngle) * wRadius, tierHeight * 0.5f, Mathf.Sin(wAngle) * wRadius);
                window.transform.localRotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(wAngle), 0f, Mathf.Sin(wAngle)));
                window.GetComponent<Renderer>().sharedMaterial = glowMat;
            }

            currentY += tierHeight;
            currentRadius = nextRadius;

            // виступаюча "оглядова" площадка-карниз через кожні 3 яруси
            if (t % 3 == 2)
            {
                GameObject ledge = new GameObject("Ledge_" + t);
                ledge.transform.SetParent(tower.transform, false);
                ledge.transform.localPosition = new Vector3(0f, currentY, 0f);
                Mesh ledgeMesh = MeshBuilder.CreateAnnulus(currentRadius, currentRadius + 1.8f, 16);
                ledge.AddComponent<MeshFilter>().mesh = ledgeMesh;
                ledge.AddComponent<MeshRenderer>().sharedMaterial = ledgeMat;
            }
        }

        GameObject spire = new GameObject("Spire");
        spire.transform.SetParent(tower.transform, false);
        spire.transform.localPosition = new Vector3(0f, currentY, 0f);
        Mesh spireMesh = MeshBuilder.CreateTaperedCylinder(currentRadius * 1.3f, 0.05f, 18f, 10);
        spire.AddComponent<MeshFilter>().mesh = spireMesh;
        spire.AddComponent<MeshRenderer>().sharedMaterial = darkStoneMat;
        spire.AddComponent<MeshCollider>().sharedMesh = spireMesh;

        // кристалічні прикраси біля основи - контр-масштабовані, щоб лишатись розумного
        // розміру НЕЗАЛЕЖНО від загального масштабу вежі (інакше стають велетенськими шпилями)
        for (int c = 0; c < 8; c++)
        {
            float cAngle = 360f / 8 * c * Mathf.Deg2Rad;
            GameObject crystal = new GameObject("Crystal_" + c);
            crystal.transform.SetParent(tower.transform, false);
            Mesh crystalMesh = MeshBuilder.CreateTaperedCylinder(1.2f, 0.05f, 6f + (c % 3) * 2f, 6);
            crystal.AddComponent<MeshFilter>().mesh = crystalMesh;
            crystal.AddComponent<MeshRenderer>().sharedMaterial = crystalMat;
            crystal.transform.localPosition = new Vector3(Mathf.Cos(cAngle) * (baseRadius + 3f), 0f, Mathf.Sin(cAngle) * (baseRadius + 3f));
            crystal.transform.localScale = Vector3.one / scaleFactor; // компенсує масштаб батька

        }

        // масштабуємо ВСЮ вежу разом - вона має бути величезною порівняно з деревами й будинками
        tower.transform.localScale = Vector3.one * scaleFactor;

        // --- Портал трохи ПОВЕРХ поверхні вежі (не глибоко всередині) - з боку міста ---
        GameObject portal = new GameObject("PortalToBossChamber");
        portal.transform.position = towerPos + towardCity * (baseRadius * scaleFactor + 6f) + Vector3.up * 1.5f;

        // кругла кам'яна рама-кільце замість прямокутної арки
        const float portalRingRadius = 9f;    // ще в 3 рази більше за попереднє
        const int portalRingSegments = 14;
        for (int p = 0; p < portalRingSegments; p++)
        {
            float pAngle = 360f / portalRingSegments * p * Mathf.Deg2Rad;
            GameObject ringSeg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ringSeg.name = "RingSeg_" + p;
            ringSeg.transform.SetParent(portal.transform, false);
            ringSeg.transform.localScale = new Vector3(portalRingRadius * 0.17f, portalRingRadius * 0.25f, portalRingRadius * 0.17f);
            ringSeg.transform.localPosition = new Vector3(Mathf.Cos(pAngle) * portalRingRadius, Mathf.Sin(pAngle) * portalRingRadius, 0f);
            ringSeg.transform.localRotation = Quaternion.Euler(0f, 0f, 90f - p * (360f / portalRingSegments));
            ringSeg.GetComponent<Renderer>().sharedMaterial = darkStoneMat;
        }

        // велике кругле мерехтливе "вікно" порталу всередині кільця
        GameObject portalPlane = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        portalPlane.name = "PortalGlow";
        Object.DestroyImmediate(portalPlane.GetComponent<Collider>());
        portalPlane.transform.SetParent(portal.transform, false);
        portalPlane.transform.localScale = new Vector3(portalRingRadius * 1.9f, 0.05f, portalRingRadius * 1.9f);
        portalPlane.transform.localPosition = Vector3.zero;
        portalPlane.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Material portalGlowMat = MakeTransparentMat(new Color(0.4f, 0.7f, 0.9f, 0.6f));
        portalPlane.GetComponent<Renderer>().sharedMaterial = portalGlowMat;

        // підганяємо портал під реальну висоту рельєфу так само, як і вежу
        if (terrainForTower != null)
        {
            float portalGroundY = terrainForTower.SampleHeight(portal.transform.position) + terrainForTower.transform.position.y;
            portal.transform.position = new Vector3(portal.transform.position.x, portalGroundY + portalRingRadius, portal.transform.position.z);
        }

        BoxCollider trigger = portal.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(portalRingRadius * 1.8f, portalRingRadius * 2.6f, 4f);
        trigger.center = new Vector3(0f, -portalRingRadius * 0.75f, 0f); // опускаємо зону ближче до землі, де реально ходить гравець
        ScenePortal scenePortal = portal.AddComponent<ScenePortal>();
        scenePortal.targetSceneName = "BossChamber";
        scenePortal.targetSpawnPointName = "SpawnPoint";

        // --- Точка, куди повертається гравець із зали боса ---
        GameObject oldTowerSpawn = GameObject.Find("TowerSpawnPoint");
        if (oldTowerSpawn != null) Object.DestroyImmediate(oldTowerSpawn);
        GameObject towerSpawn = new GameObject("TowerSpawnPoint");
        towerSpawn.transform.position = portal.transform.position + towardCity * 5f;
        towerSpawn.transform.rotation = Quaternion.LookRotation(-towardCity);

        Debug.Log("Вежу перебудовано епічно (багатоярусний шпиль " + tierCount + " ярусів, кристали біля основи, без сірих контрфорсів). Портал тепер трохи поверх стіни вежі з боку міста, позиція: " + portal.transform.position + ".");
    }

    [MenuItem("Вежа/Гравець/Перевірити й додати всі компоненти гравця")]
    public static void EnsureAllPlayerComponents()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("Не знайдено об'єкт \"Player\" у сцені.");
            return;
        }

        if (player.GetComponent<Health>() == null)
        {
            Health h = player.AddComponent<Health>();
            h.maxHealth = 100f;
        }
        if (player.GetComponent<Mana>() == null) player.AddComponent<Mana>();
        if (player.GetComponent<PlayerCombat>() == null) player.AddComponent<PlayerCombat>();
        if (player.GetComponent<PlayerStats>() == null) player.AddComponent<PlayerStats>();
        if (player.GetComponent<Inventory>() == null) player.AddComponent<Inventory>();
        if (player.GetComponent<PlayerRespawn>() == null) player.AddComponent<PlayerRespawn>();
        if (player.GetComponent<Gold>() == null) player.AddComponent<Gold>();
        if (player.GetComponent<PlayerSkills>() == null) player.AddComponent<PlayerSkills>();
        if (player.GetComponent<PlayerAppearance>() == null) player.AddComponent<PlayerAppearance>();
        if (player.GetComponent<HitFlash>() == null) player.AddComponent<HitFlash>();

        Debug.Log("Перевірено: Health, Mana, PlayerCombat, PlayerStats, Inventory, PlayerRespawn, Gold, PlayerSkills, PlayerAppearance, HitFlash - усі присутні на Player. Q і F тепер мають працювати.");
    }
}
