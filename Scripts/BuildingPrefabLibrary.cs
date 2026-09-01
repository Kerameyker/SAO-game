using UnityEngine;

/// <summary>
/// Створи порожній об'єкт у сцені, додай цей компонент, і перетягни свої імпортовані
/// префаби будинків у відповідні списки в Inspector. Генератор міста автоматично
/// використає їх замість процедурних будівель, коли вони є.
/// </summary>
public class BuildingPrefabLibrary : MonoBehaviour
{
    [Header("Перетягни сюди свої префаби будинків (Asset Store / знайдені паки)")]
    public GameObject[] housePrefabs;
    public GameObject[] shopPrefabs;
    public GameObject[] towerPrefabs;

    private static BuildingPrefabLibrary instance;
    public static BuildingPrefabLibrary Instance
    {
        get
        {
            if (instance == null)
            {
#if UNITY_2023_1_OR_NEWER
                instance = Object.FindFirstObjectByType<BuildingPrefabLibrary>();
#else
                instance = Object.FindObjectOfType<BuildingPrefabLibrary>();
#endif
            }
            return instance;
        }
    }
}
