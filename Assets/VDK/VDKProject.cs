using System;
using System.Runtime.InteropServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Vault
{
public enum vdkProjectGeometryType { 
    //These are the geometry types for nodes 
    vdkPGT_None, //!<There is no geometry associated with this node.
    vdkPGT_Point, //!<pCoordinates is a single 3D position
    vdkPGT_MultiPoint, //!<Array of vdkPGT_Point, pCoordinates is an array of 3D positions.
    vdkPGT_LineString, //!<pCoordinates is an array of 3D positions forming an open line
    vdkPGT_MultiLineString, //!<Array of vdkPGT_LineString; pCoordinates is NULL and children will be present.
    vdkPGT_Polygon, //!<pCoordinates will be a closed linear ring (the outside), there MAY be children that are interior as pChildren vdkPGT_MultiLineString items, these should be counted as islands of the external ring.
    vdkPGT_MultiPolygon, //!<pCoordinates is null, children will be vdkPGT_Polygon (which still may have internal islands)
    vdkPGT_GeometryCollection, //!<Array of geometries; pCoordinates is NULL and children may be present of any type.
    vdkPGT_Count, //!<Total number of geometry types. Used internally but can be used as an iterator max when displaying different type modes.
    vdkPGT_Internal, //!<Used internally when calculating other types. Do not use.
	}
 public enum vdkProjectNodeType
  {
    /*
This represents the type of data stored in the node.

Note

    The itemtypeStr in the vdkProjectNode is a string version. This enum serves to simplify the reading of standard types. The the string in brackets at the end of the comment is the string.
     */
    vdkPNT_Custom, //!<Need to check the itemtypeStr string manually.
    vdkPNT_PointCloud, //!<A Euclideon Unlimited Detail Point Cloud file (“UDS”)
    vdkPNT_PointOfInterest, //!<A point, line or region describing a location of interest (“POI”)
    vdkPNT_Folder, //!<A folder of other nodes (“Folder”)
    vdkPNT_LiveFeed, //!<A Euclideon Vault live feed container (“IOT”)
    vdkPNT_Media, //!<An Image, Movie, Audio file or other media object (“Media”)
    vdkPNT_Viewpoint, //!<An Camera Location & Orientation (“Camera”)
    vdkPNT_VisualisationSettings, //!<Visualisation settings (itensity, map height etc) (“VizSet”)
    vdkPNT_Count, //!<Total number of node types. Used internally but can be used as an iterator max when displaying different type modes.
  }

    [StructLayout(LayoutKind.Sequential)]
  public struct vdkProjectNode
  {
    /*
    Stores the state of a node.

    Warning

        This struct is exposed to avoid having a huge API of accessor functions but it should be treated as read only with the exception of pUserData. Making changes to the internal data will cause issues syncronising data

  */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
    public readonly char[] UUID; //!<Unique identifier for this node “id”.

    public readonly double lastUpdate; //!<The last time this node was updated in UTC.

    public readonly vdkProjectNodeType itemtype; //!<The type of this node, see vdkProjectNodeType for more information.

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly char[] itemtypeStr; //!<The string representing the type of node. If its a known type during node creation itemtype will be set to something other than vdkPNT_Custom.

    public readonly IntPtr pName; //!<Human readable name of the item.

    public readonly IntPtr pURI; //!<The address or filename that the resource can be found.

    public readonly bool hasBoundingBox; //!<Set to true if this nodes boundingBox item is filled out.

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public readonly double[] boundingBox; //!<The bounds of this model, ordered as [West, South, Floor, East, North, Ceiling].

    public readonly vdkProjectGeometryType geomtype; //!<Indicates what geometry can be found in this model. See the vdkProjectGeometryType documentation for more information.

    public readonly int geomCount; //!<How many geometry items can be found on this model.

    public readonly IntPtr pCoordinates; //!<The positions of the geometry of this node (NULL this this node doesn’t have points). The format is [X0,Y0,Z0,…Xn,Yn,Zn].

    public readonly IntPtr pNextSibling; //!<This is the next item in the project (NULL if no further siblings)

    public readonly IntPtr pFirstChild; //!<Some types (“folder”, “collection”, “polygon”…) have children nodes, NULL if there are no children.

    IntPtr pUserData; //!<This is application specific user data. The application should traverse the tree to release these before releasing the vdkProject.

    public readonly IntPtr pInternalData; //!<Internal VaultSDK specific state for this node.

  }
	public class VDKProject
	{
    IntPtr pVdkProject;
    public IntPtr pRootNode;

		public VDKProject(string geoJSON)
		{
      vdkError err = vdkProject_LoadFromMemory(ref pVdkProject, geoJSON);

      if(err != vdkError.vE_Success)
        throw new Exception("project load failed: "+ err.ToString());

      pRootNode = IntPtr.Zero;
      vdkProject_GetProjectRoot(pVdkProject, ref pRootNode);
		}

    ~VDKProject()
    {
      vdkProject_Release(ref pVdkProject);
    }


  //Create an empty, local only, instance of vdkProject.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProject_CreateLocal(ref IntPtr ppProject, string pName);
  //Create a local only instance of vdkProject filled in with the contents of a GeoJSON string.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProject_LoadFromMemory(ref IntPtr ppProject, string pGeoJSON);
    //Destroy the instance of the project.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProject_Release(ref IntPtr ppProject);
    //Export a project to a GeoJSON string in memory.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProject_WriteToMemory(IntPtr pProject, ref IntPtr ppMemory);
    //Get the project root node.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProject_GetProjectRoot(IntPtr pProject, ref IntPtr ppRootNode);
    //Get the state of unsaved local changes
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProject_HasUnsavedChanges(IntPtr pProject);
    [DllImport(VaultSDKLibrary.name)]
  private static extern string vdkProject_GetTypeName(vdkProjectNodeType itemtype);


	}

  public class VDKProjectNode
  {
    public IntPtr pNode;
    public vdkProjectNode nodeData;
    public VDKProjectNode(IntPtr nodeAddr)
    {
      pNode = nodeAddr;
      this.nodeData = (vdkProjectNode) Marshal.PtrToStructure(nodeAddr, typeof(vdkProjectNode));
    }

    public void RemoveChild() 
    {
    
    }

    //Create a node in a project
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_Create(IntPtr pProject, IntPtr ppNode, ref vdkProjectNode pParent, string pType, string pName, string pURI, IntPtr pUserData);
    //Move a node to reorder within the current parent or move to a different parent.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_MoveChild(IntPtr pProject, ref vdkProjectNode pCurrentParent, ref vdkProjectNode pNewParent, ref vdkProjectNode pNode, ref vdkProjectNode pInsertBeforeChild);
    //Remove a node from the project.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_RemoveChild(IntPtr pProject, ref vdkProjectNode pParentNode, ref vdkProjectNode pNode);
    //Set the human readable name of a node.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_SetName(IntPtr pProject, ref vdkProjectNode pNode, string pNodeName);
    //Set the URI of a node.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_SetURI(IntPtr pProject, ref vdkProjectNode pNode, string pNodeURI);
    //Set the new geometry of a node.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_SetGeometry(IntPtr pProject, ref vdkProjectNode pNode, vdkProjectGeometryType nodeType, int geometryCount, ref double pCoordinates);
    //Get a metadata item of a node as an integer.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_GetMetadataInt(ref vdkProjectNode pNode, string pMetadataKey, ref Int32 pInt, Int32 defaultValue);
    //Set a metadata item of a node from an integer.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_SetMetadataInt(ref vdkProjectNode pNode, string pMetadataKey, Int32 iValue);
    //Get a metadata item of a node as an unsigned integer.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_GetMetadataUint(ref vdkProjectNode pNode, string pMetadataKey, ref UInt32 pInt, UInt32 defaultValue);
    //Set a metadata item of a node from an unsigned integer.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_SetMetadataUint(ref vdkProjectNode pNode, string pMetadataKey, UInt32 iValue);
    //Get a metadata item of a node as a 64 bit integer.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_GetMetadataInt64(ref vdkProjectNode pNode, string pMetadataKey, ref Int64 pInt64, Int64 defaultValue);
    //Set a metadata item of a node from a 64 bit integer.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_SetMetadataInt64(ref vdkProjectNode pNode, string pMetadataKey, Int64 i64Value);
    //Get a metadata item of a node as a double.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_GetMetadataDouble(ref vdkProjectNode pNode, string pMetadataKey, ref double pDouble, double defaultValue);
    //Set a metadata item of a node from a double.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_SetMetadataDouble(ref vdkProjectNode pNode, string pMetadataKey, double doubleValue);
    //Get a metadata item of a node as an boolean.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_GetMetadataBool(ref vdkProjectNode pNode, string pMetadataKey, ref bool pBool, bool defaultValue);
    //Set a metadata item of a node from an boolean.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_SetMetadataBool(ref vdkProjectNode pNode, string pMetadataKey, bool boolValue);
    //Get a metadata item of a node as a string.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_GetMetadataString(ref vdkProjectNode pNode, string pMetadataKey, ref string ppString, string pDefaultValue);
    //Set a metadata item of a node from a string.
    [DllImport(VaultSDKLibrary.name)]
  private static extern vdkError vdkProjectNode_SetMetadataString(ref vdkProjectNode pNode, string pMetadataKey, string pString);
    //Get the standard type string name for an itemtype
  }
}
