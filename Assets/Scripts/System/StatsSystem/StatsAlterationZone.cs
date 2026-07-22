using System;
using UnityEngine;

/// <summary>
/// These are placeable zones that apply some form of status effect while some entity is within them.
/// </summary>
public class StatsAlterationZone : MonoBehaviour
{
    
    //————— Customization Variables —————//
    [Header("Zone Settings")]
    [SerializeField] private Color editorGizmoColor = new Color(0f, 1f, 0f, 0.4f);

    //————— Internal Variables —————//
    private BoxCollider boxCollider;


    private void Start()
    {
        if(boxCollider == null){ boxCollider = GetComponent<BoxCollider>(); boxCollider.isTrigger = true; }
    }
    
    
    //————— Collision Functions —————//
    private void OnTriggerEnter(Collider other)
    {

    }
    
    private void OnTriggerStay(Collider other)
    {
        
    }
    
    private void OnTriggerExit(Collider other)
    {
        
    }
    
    
    
    //————— Gizmos Functions —————//
    
}
