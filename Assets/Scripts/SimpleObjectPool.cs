using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPool : MonoBehaviour
{
    public static SimpleObjectPool Instance { get; private set; }

    [SerializeField] private GameObject objectToPool;
    [SerializeField] private int poolSize = 30;

    private Queue<GameObject> pooledObjects;

    private void Awake()
    {
        // Bu havuz genellikle UI canvas'ýnýn bir parçasý olacaðý için Singleton
        // ve DontDestroyOnLoad burada gerekli olmayabilir. Ama isterseniz ekleyebilirsiniz.
        Instance = this;
    }

    void Start()
    {
        pooledObjects = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(objectToPool);
            obj.transform.SetParent(this.transform); // Havuz objesinin altýna taþý
            obj.SetActive(false); // Baþlangýçta pasif yap
            pooledObjects.Enqueue(obj); // Kuyruða ekle
        }
    }

    /// <summary>
    /// Havuzdan bir obje alýr.
    /// </summary>
    public GameObject GetPooledObject()
    {
        if (pooledObjects.Count > 0)
        {
            GameObject obj = pooledObjects.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Eðer havuz boþalýrsa (ihtiyaçtan fazla obje gerekirse) yeni bir tane oluþturur.
            // Bu bir güvenlik önlemidir.
            GameObject obj = Instantiate(objectToPool);
            obj.transform.SetParent(this.transform);
            return obj;
        }
    }

    /// <summary>
    /// Kullanýlan bir objeyi havuza geri döndürür.
    /// </summary>
    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(this.transform); // Tekrar havuzun altýna taþý
        pooledObjects.Enqueue(obj);
    }
}