using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class EnemySetup : EditorWindow
{
    public GameObject enemyModel;
    public GameObject hudBar;
    public GameObject shadow;
    public enum ColliderType
    {
        boxCollider,
        sphereCollider,
        capsuleCollider,
    }
    public ColliderType colliderType = ColliderType.capsuleCollider;
    public string tag = "Ground";
    public int layer = LayerMask.NameToLayer("Enemies");
    public bool attachTMove = true;
    public bool attachProperties = true;
    public bool attachRigidbody = true;
    Bounds totalBounds = new Bounds();
    private Renderer[] renderers;

    [MenuItem("Window/TD Starter Kit/Enemy Setup")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(EnemySetup));
    }

	void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Enemy Model:");
        enemyModel = (GameObject)EditorGUILayout.ObjectField(enemyModel, typeof(GameObject), false);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("HUD Bar Prefab:");
        hudBar = (GameObject)EditorGUILayout.ObjectField(hudBar, typeof(GameObject), false);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shadow Prefab:");
        shadow = (GameObject)EditorGUILayout.ObjectField(shadow, typeof(GameObject), false);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Collider Type:");
        colliderType = (ColliderType)EditorGUILayout.EnumPopup(colliderType);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Enemy Tag:");
        tag = EditorGUILayout.TagField(tag);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Enemy Layer:");
        layer = EditorGUILayout.LayerField(layer);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Attach TweenMove:");
        attachTMove = EditorGUILayout.Toggle(attachTMove);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Attach Properties:");
        attachProperties = EditorGUILayout.Toggle(attachProperties);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Attach Rigidbody:");
        attachRigidbody = EditorGUILayout.Toggle(attachRigidbody);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("By clicking on 'Apply!' all chosen components are added and a prefab will be created next to your enemy model.", MessageType.Info);
        EditorGUILayout.Space();
        if (GUILayout.Button("Apply!"))
        {
            if (enemyModel == null)
            {
                Debug.LogWarning("No enemy model chosen. Aborting Enemy Setup execution.");
                return;
            }
            string assetPath = AssetDatabase.GetAssetPath(enemyModel.GetInstanceID());
            string[] folders = assetPath.Split('/');
            assetPath = assetPath.Replace(folders[folders.Length-1], enemyModel.name + ".prefab");
            ProcessModel();
            if (attachTMove)
                enemyModel.AddComponent<TweenMove>();

            Properties properties = null;
            if (attachProperties)
            {
                properties = enemyModel.AddComponent<Properties>();
            }
            if (hudBar)
            {
                hudBar = (GameObject)Instantiate(hudBar);
                hudBar.name = hudBar.name.Replace("(Clone)", "");
                hudBar.transform.parent = enemyModel.transform;
                hudBar.transform.position = enemyModel.transform.position;

                if(!properties) return;
                Transform healthbarTrans = hudBar.transform.FindChild("healthbar");
                if (healthbarTrans)
                {
                    Slider healthbar = healthbarTrans.GetComponent<Slider>();
                    properties.healthbar = healthbar;
                }
                Transform shieldbarTrans = hudBar.transform.FindChild("shieldbar");
                if (shieldbarTrans)
                {
                    Slider shieldbar = shieldbarTrans.GetComponent<Slider>();
                    properties.shield = new Shield();
                    properties.shield.bar = shieldbar;
                    properties.shield.enabled = true;
                }
            }
            if (shadow)
            {
                shadow = (GameObject)Instantiate(shadow);
                shadow.name = shadow.name.Replace("(Clone)", "");
                shadow.transform.parent = enemyModel.transform;
                shadow.transform.position = enemyModel.transform.position;
            }
            if (attachRigidbody)
            {
                Rigidbody rigidbody = enemyModel.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = false;
            }
            GameObject prefab = null;
            if (AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)))
            {
                if (EditorUtility.DisplayDialog("Are you sure?",
                "The prefab already exists. Do you want to overwrite it?",
                "Yes",
                "No"))
                {
                    prefab = PrefabUtility.CreatePrefab(assetPath, enemyModel);
                }
            }
            else
                prefab = PrefabUtility.CreatePrefab(assetPath, enemyModel);
            DestroyImmediate(enemyModel);
            if (prefab)
            {
                Selection.activeGameObject = prefab;
                this.Close();
            }
        }
	}


    void ProcessModel()
    {
        enemyModel = (GameObject)Instantiate(enemyModel);
        enemyModel.transform.position = Vector3.zero;
        enemyModel.name = enemyModel.name.Replace("(Clone)", "");
        UnityEngine.GameObject empty = new GameObject(enemyModel.name);
        empty.transform.position = totalBounds.center;
        enemyModel.transform.parent = empty.transform;
        enemyModel = empty;
        AddCollider();
        enemyModel.tag = tag;
        enemyModel.layer = layer;
        Animation anim = enemyModel.GetComponentInChildren<Animation>();
        if(anim != null)
            anim.playAutomatically = false;
    }


    void AddCollider()
    {
        switch (colliderType)
        {
            case ColliderType.boxCollider:
                BoxCollider boxCol = enemyModel.AddComponent<BoxCollider>();
                boxCol.isTrigger = true;
                break;
            case ColliderType.sphereCollider:
                SphereCollider sphereCol = enemyModel.AddComponent<SphereCollider>();
                sphereCol.isTrigger = true;
                break;
            case ColliderType.capsuleCollider:
                CapsuleCollider capsuleCol = enemyModel.AddComponent<CapsuleCollider>();
                capsuleCol.isTrigger = true;
                break;
        }
    }
}