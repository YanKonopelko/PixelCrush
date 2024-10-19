using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pool : MonoBehaviour
{

    private Dictionary<GameObject, List<GameObject>> freeObjects = new Dictionary<GameObject, List<GameObject>>();
    private Dictionary<GameObject, List<GameObject>> usingObjects = new Dictionary<GameObject, List<GameObject>>();


    public GameObject GetFromPool(GameObject Object)
    {
        return this.GetOrCreate(Object);
    }


    private GameObject GetOrCreate(GameObject key)
    {
        GameObject objectToReturn = null;

        List<GameObject> freeObjectsArray = null;

        if (freeObjects.ContainsKey(key) && freeObjects[key].Count > 0)
        {
            freeObjectsArray = freeObjects[key];
            
        }

        if (freeObjectsArray is not null)
        {
            for (int i = 0; i < freeObjectsArray?.Count; i++)
            {
                GameObject element = freeObjectsArray[i];
                if (!element.activeInHierarchy)
                {

                    objectToReturn = element;
                    freeObjectsArray.RemoveRange(freeObjectsArray.IndexOf(objectToReturn), 1);
                    this.AddToUsingMap(key, objectToReturn);
                    break;
                }
            }
            // objectToReturn = Instantiate(key, transform);
            AddToUsingMap(key, objectToReturn);
        }
        else
        {
            objectToReturn = Instantiate(key);
            this.AddToUsingMap(key, objectToReturn);
        }
        objectToReturn.SetActive(true);
        return objectToReturn;
    }

    private void AddToUsingMap(GameObject key, GameObject Object)
    {
        List<GameObject> usingObjectsArray = null;
        if (usingObjects.ContainsKey(key))
        {
            usingObjectsArray = usingObjects[key];
        }


        if (usingObjectsArray == null)
        {
            usingObjectsArray = new List<GameObject>
            {
                Object
            };
            this.usingObjects[key] = usingObjectsArray;
        }
        else
        {
            usingObjectsArray.Add(Object);
        }
    }

    public void Release(GameObject key, GameObject Object)
    {
        List<GameObject> usingObjectsArray = null;
        var a = this.usingObjects.ContainsKey(key);
        var b = this.usingObjects[key];
        if ( a && b.Count > 0)
        {
            usingObjectsArray = usingObjects[key];
        }
        if (usingObjectsArray == null)
        {
            Debug.LogError("There're no objects with" + key + "key in using pool!");
            return;
        }

        for (int i = 0; i < usingObjectsArray.Count; i++)
        {
            GameObject element = usingObjectsArray[i];
            if (element == Object)
            {
                usingObjectsArray.RemoveRange(usingObjectsArray.IndexOf(element), 1);

                List<GameObject> freeObjectsArray = null;
                if (this.freeObjects.ContainsKey(key) && this.freeObjects[key].Count > 0)
                {
                    freeObjectsArray = freeObjects[key];
                }
                if (freeObjectsArray == null)
                {
                    freeObjects[key] = new List<GameObject>();
                    freeObjectsArray = freeObjects[key];
                    // Debug.LogError("There're no objects with" + key + "key in free pool! Missed GetFromPool method call!");
                    // return;
                }
                freeObjectsArray.Add(element);
                element.SetActive(false);
            }
        }

    }

    public void PreparePool(GameObject key, int countOfObjectsToPrewarm)
    {
        // for (int i = 0; i < keys.Length; i++)
        // {
            // if (this.freeObjects.ContainsKey(key)) continue;
            List<GameObject> newArray = new List<GameObject>();
            for (int o = 0; o < countOfObjectsToPrewarm; o++)
            {
                GameObject newElement = Instantiate(key, transform);
                newElement.SetActive(false);
                newArray.Add(newElement);
            }
            this.freeObjects[key] = newArray;
        // }

    }
}
