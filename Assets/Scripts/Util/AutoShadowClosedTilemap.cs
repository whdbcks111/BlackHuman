using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(CompositeCollider2D))]
public class AutoShadowClosedTilemap : MonoBehaviour
{

    [Space]
    [SerializeField]
    private bool selfShadows = true;

    private CompositeCollider2D tilemapCollider;
    private List<Vector2> unionVertices = new();


    static readonly FieldInfo meshField = typeof(ShadowCaster2D).GetField("m_Mesh", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly MethodInfo generateShadowMeshMethod = typeof(ShadowCaster2D)
            .Assembly
            .GetType("UnityEngine.Rendering.Universal.ShadowUtility")
            .GetMethod("GenerateShadowMesh", BindingFlags.Public | BindingFlags.Static);
    public void Generate()
    {
        tilemapCollider = GetComponent<CompositeCollider2D>();

        if(tilemapCollider.pathCount != 2) {
            print("Shadow must be used in one closed tiles. Please erase the other tiles to other Tilemap.");
            return;
        }

        unionVertices.Clear();
        DestroyAllChildren();


        Vector2[] pathVertices = new Vector2[tilemapCollider.GetPathPointCount(0)];
        tilemapCollider.GetPath(0, unionVertices);
        tilemapCollider.GetPath(1, pathVertices);
        unionVertices.Add(unionVertices[0]);

        var index = 0;
        var endPath = unionVertices[unionVertices.Count - 1];
        var length = Vector2.Distance(pathVertices[0], endPath);

        for(var i = 1; i < pathVertices.Length; i++) {
            var path = pathVertices[i];
            var curLen = Vector2.Distance(endPath, path);
            if(curLen < length) {
                length = curLen;
                index = i;
            }
        }

        for(var i = 0; i < pathVertices.Length + 1; i++) {
            var path = pathVertices[(i + index) % pathVertices.Length];
            unionVertices.Add(path);
        }
        
        var shadowCaster = new GameObject("ShadowCaster");
        shadowCaster.transform.parent = gameObject.transform;
        shadowCaster.transform.position = Vector3.zero;
        ShadowCaster2D shadowCasterComponent = shadowCaster.AddComponent<ShadowCaster2D>();
        shadowCasterComponent.selfShadows = this.selfShadows;

        unionVertices.Add(unionVertices[0]);

        var testPath = new Vector3[unionVertices.Count];
        var j = 0;
        foreach (var path in unionVertices)
        {
            testPath[j++] = path;
        }

        shapePathField.SetValue(shadowCasterComponent, testPath);
        meshField.SetValue(shadowCasterComponent, new Mesh());
        generateShadowMeshMethod.Invoke(shadowCasterComponent, new object[] { meshField.GetValue(shadowCasterComponent), shapePathField.GetValue(shadowCasterComponent) });

        Debug.Log("Shadow Generated");

    }
    public void DestroyAllChildren()
    {

        var tempList = transform.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            DestroyImmediate(child.gameObject);
        }

    }

}
