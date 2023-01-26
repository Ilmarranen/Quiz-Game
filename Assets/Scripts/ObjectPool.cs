using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Class for pooling objects for reuse - not really necessary for predefined number of questions and for small app, but why not
public class ObjectPool : MonoBehaviour
{
    //Object to pool
    [SerializeField] GameObject prefab;
    //Already created instances not in use
    private Stack<GameObject> inactiveInstances = new Stack<GameObject>();

    //Get object from the pool
    public GameObject GetObject()
    {
        GameObject spawnedGameObject;

        //If exist - get instance from stack of unused
        if (inactiveInstances.Count > 0)
        {
            spawnedGameObject = inactiveInstances.Pop();
        }
        //If not - create
        else
        {
            spawnedGameObject = GameObject.Instantiate(prefab);

            //Created object belongs to this pool
            PooledObject pooledObject = spawnedGameObject.AddComponent<PooledObject>();
            pooledObject.pool = this;
        }

        //And we use the object - why else would we need it
        spawnedGameObject.SetActive(true);

        return spawnedGameObject;
    }

    //Used it - return it for future use. Function puts unneeded object in stack 
    public void ReturnObject(GameObject toReturn)
    {
        //Search for component which specifies pool object is from
        PooledObject pooledObject = toReturn.GetComponent<PooledObject>();

        //From this pool - disable and put on shelf
        if(pooledObject != null && pooledObject.pool == this)
        {
            toReturn.SetActive(false);

            inactiveInstances.Push(toReturn);
        }
        //Not from this pool - just don't need it
        else
        {
            Debug.LogWarning(toReturn.name + " was returned to a pool it wasn't spawned from! Destroying.");
            Destroy(toReturn);
        }
    }

}

//Class for verifying pool of the object
public class PooledObject : MonoBehaviour
{
    public ObjectPool pool;
}
