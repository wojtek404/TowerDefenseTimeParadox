using UnityEngine;
using System.Collections.Generic;

public class SkinnedMeshCombiner : MonoBehaviour 
{
 	Material baseMat;
	SkinnedMeshRenderer newSkin;
	List<SkinnedMeshRenderer> smRenderers;
	
    void Start() 
	{        
		smRenderers = new List<SkinnedMeshRenderer>();
        List<Transform> bones = new List<Transform>();        
        List<BoneWeight> boneWeights = new List<BoneWeight>();        
        List<CombineInstance> combineInstances = new List<CombineInstance>();
		SkinnedMeshRenderer[] allRenderers =GetComponentsInChildren<SkinnedMeshRenderer>();
		print (allRenderers.Length);
		foreach(SkinnedMeshRenderer smr in allRenderers)
		{
			if(smr.enabled ==true)
			{
				if(baseMat==null)
				{
					print (smr.name);
					baseMat = smr.sharedMaterial;
				}
				
				if(smr.sharedMaterial == baseMat)
				{
					print (smr.name);
					smRenderers.Add(smr);
				}
			}
		}
		
        int numSubs = 0;
        foreach(SkinnedMeshRenderer smr in smRenderers) numSubs += smr.sharedMesh.subMeshCount;
 		
        int[] meshIndex = new int[numSubs];
        for( int s = 0; s < smRenderers.Count; s++ ) 
		{
            SkinnedMeshRenderer smr = smRenderers[s];
            foreach( Transform bone in smr.bones )
			{
				if(!bones.Contains(bone))
				{
					bones.Add( bone );
				}
			}
            BoneWeight[] meshBoneweights = smr.sharedMesh.boneWeights;
            foreach( BoneWeight bw in meshBoneweights ) 
			{
                BoneWeight bWeight = bw;
				bWeight.boneIndex0 = bones.IndexOf(smr.bones[bw.boneIndex0]); 
                bWeight.boneIndex1 = bones.IndexOf(smr.bones[bw.boneIndex1]);
				bWeight.boneIndex2 = bones.IndexOf(smr.bones[bw.boneIndex2]);
				bWeight.boneIndex3 = bones.IndexOf(smr.bones[bw.boneIndex3]);
                boneWeights.Add( bWeight );
            }
 
            CombineInstance ci = new CombineInstance();
			ci.transform = smr.transform.localToWorldMatrix;
            ci.mesh = smr.sharedMesh;
            meshIndex[s] = ci.mesh.vertexCount;
            combineInstances.Add( ci );
			smr.enabled = false;
        }
 
        List<Matrix4x4> bindposes = new List<Matrix4x4>();
         for( int b = 0; b < bones.Count; b++ ) 
		{
            bindposes.Add( bones[b].worldToLocalMatrix);
        }
		
		
		newSkin = gameObject.AddComponent<SkinnedMeshRenderer>();
        newSkin.sharedMesh = new Mesh();
        newSkin.sharedMesh.CombineMeshes( combineInstances.ToArray(), true, true );
        newSkin.bones = bones.ToArray();
		newSkin.material = baseMat;
        newSkin.sharedMesh.boneWeights = boneWeights.ToArray();
        newSkin.sharedMesh.bindposes = bindposes.ToArray();
        newSkin.sharedMesh.RecalculateBounds();
    }
}
