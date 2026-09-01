using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Повісь окремо на Player і окремо на CameraRig (з різними uniqueId!).
/// Такий об'єкт "виживає" при переході через портал в іншу сцену,
/// замість того щоб зникнути разом зі старою сценою.
/// Якщо при поверненні в сцену там випадково опиниться ще один такий самий
/// об'єкт (з тим самим uniqueId) - зайвий буде автоматично видалено.
/// </summary>
public class PersistAcrossScenes : MonoBehaviour
{
    [Tooltip("Постав унікальне ім'я, напр. \"Player\" або \"CameraRig\"")]
    public string uniqueId = "Player";

    private static HashSet<string> livingIds = new HashSet<string>();

    void Awake()
    {
        if (livingIds.Contains(uniqueId))
        {
            Destroy(gameObject);
            return;
        }
        livingIds.Add(uniqueId);
        DontDestroyOnLoad(gameObject);
    }
}
