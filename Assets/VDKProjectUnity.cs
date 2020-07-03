using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Vault;

public class VDKProjectUnity : MonoBehaviour
{
  public string path = @"C:/git/vaultsdkunity/Assets/VDK/project_cloud_located.json";
  public bool isLoaded = false;
  private string geoJSON;
  VDKProject proj;

    // Start is called before the first frame update
    void Start()
    {
        
    }
  void LoadFromFile(string path)
  {
    geoJSON = System.IO.File.ReadAllText(path);
    proj = new VDKProject(geoJSON);
    this.path = path;
    print("loaded node!");
  }

    // Update is called once per frame
    void Update()
    {
    if (!isLoaded)
    {
      try
      {
        LoadFromFile(path);
        GameObject rootNodeGO = new GameObject();
        rootNodeGO.transform.parent = transform;
        var pn = rootNodeGO.AddComponent<VDKProjectNodeUnity>();
        pn.LoadTree(proj.pRootNode);
        //pn.projectNode = proj.rootNode;

      }
      catch(Exception e) {
        Debug.LogError("caught exception loading project: " + e.ToString());
      }
      isLoaded = true;

    }

    }
}
