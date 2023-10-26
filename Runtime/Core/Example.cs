using Packages.Estenis.ComponentGroups_;
using Packages.Estenis.UnityExts_;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    [SerializeField] private ObservableList<int> _components = new();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"UPDATE from {this.name}.{nameof(Example)}");
    }
}
