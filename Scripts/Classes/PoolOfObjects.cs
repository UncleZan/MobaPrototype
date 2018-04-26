using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Takes care of all the unused GameObjects in the scene that might be useful later in the lifetime of the program.
/// Avoids useless GC for GameObjects that needs to be created and destroyed often.
/// Activates and disables objects in the pools accordingly to the instructions.
/// </summary>
public class PoolOfObjects
{
    /// <summary>
    /// The parent Transform for the new GameObjects to be instantiated or unused GameObjects to be activated.
    /// </summary>
    public Transform pool;

    GameObject newObject;

    GameObject prefab;

    public PoolOfObjects(Transform objectsPool, GameObject objPrefab)
    {
        this.pool = objectsPool;
        //this.manager = objectsManager;
        this.prefab = objPrefab;
    }

    public GameObject GetObject()
    {
        int lastElement = pool.childCount - 1;

        foreach (Transform trns in pool)
        {
            newObject = trns.gameObject;
            if (!newObject.activeSelf)
            {
                newObject.SetActive(true);
                return newObject;
            }
        }

        newObject = (GameObject.Instantiate(prefab, pool));
        newObject.name = prefab.name;

        return newObject;
    }

    public void DisableObject(GameObject objToDisable)
    {
        objToDisable.SetActive(false);
    }

    public bool IsAnObjectAvailable()
    {
        foreach (Transform trns in pool)
        {
            if (!trns.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    public void EmptyPool()
    {
        foreach (Transform trns in pool)
        {
            Debug.Log(trns.GetSiblingIndex());
            trns.gameObject.SetActive(false);
        }
    }

}
