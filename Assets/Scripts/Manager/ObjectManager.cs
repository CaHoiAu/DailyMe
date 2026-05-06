using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public List<DragObject> objects;

    private void Awake()
    {
        objects.AddRange(FindObjectsOfType<DragObject>());
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
