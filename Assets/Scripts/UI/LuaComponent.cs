#region Macro

#define _LUA_UI_OPT_LOW

#if _LUA_UI_OPT_SUPER
#undef _LUA_UI_OPT_LOW
#undef _LUA_UI_OPT_MID
#undef _LUA_UI_OPT_LOW
#undef _LUA_UI_OPT_HIGH
#endif

#if !UNITY_EDITOR
#define _FORCE_AB
#endif

#endregion

//#define _OPT_CALL
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

using UnityEngine;
using UnityEngine.UI;


public static class StringEx
{
    public static float ToFloat(this string value)
    {
        float result;
        value = value.Trim();
        if (value == "")
            value = "0";
        if (float.TryParse(value, out result))
            return result;
        Debug.LogError("string2float error=" + value);
        return 0;
    }

    public static int ToInt(this string value)
    {
        int result;
        value = value.Trim();
        if (value == "")
            value = "0";
        if (int.TryParse(value, out result))
            return result;
        Debug.LogError("string2float error=" + value);
        return 0;
    }

    //配置变长字符串(字段)取出其中一个, 比如 "dashi,0.3,6" 2 那就返回6
    public static int SplitInt(this string value, char splitStr, int idx)
    {
        string[] subs = value.Split(splitStr);
        if (subs.Length > idx)
            return subs[idx].ToInt();
        return -1;
    }

    //配置变长字符串(字段)取出其中一个, 比如 "dashi,0.3,6" 1 那就返回0.3
    public static float SplitFloat(this string value, char splitStr, int idx)
    {
        string[] subs = value.Split(splitStr);
        if (subs.Length > idx)
            return subs[idx].ToFloat();
        return -1;
    }

    //配置变长字符串(字段)取出其中一个, 比如 "dashi,0.3,6" 0 那就返回dashi
    public static string SplitStr(this string value, char splitStr, int idx)
    {
        string[] subs = value.Split(splitStr);
        if (subs.Length > idx)
            return subs[idx];
        return "-1";
    }

    public static bool IsValid(this string value)
    {
        return value!=null && value!="" && value!="-1";
    }
}

public interface ILua
{
    object[] Call(string funname, params object[] args);
}

public sealed class LuaUINode
{
    public string LuaName = "";

    /** 后期从lua中获取配置 */
    public bool EnableUpdate = false;
    public bool EnableLateUpdate = false;
    
    private LuaState luaState { get { return LuaComponent.luaState; } }
    private const string LuaNameFormat = "{0}.{1}";
    private const string LuaNameFormat1 = "{0}:{1}";

    private IEnumerator m_updateCoroutine = null;
    private IEnumerator m_lateUpdateCoroutine = null;

    private LuaBaseRef[] Refs;
    public LuaBaseRef LuaInstance { get { return Refs[0]; } }

    private GameObject _node;
    public GameObject Node { get { return _node;} }

    private LuaComponent looper;

    private Dictionary<string, Component> cachedComponent;

    private int parentNodeId;
    public int ParentNodeId { get { return parentNodeId; } }

    public bool HasParent { get { return parentNodeId != 0; } }

    private List<int> _childNodeId;

    public List<int> ChildNodeId
    {
        get { return  _childNodeId = _childNodeId ?? new List<int>();}
    }

#if _LUA_UI_OPT_MID
    public LuaUINode(LuaComponent l, GameObject node, string luaName, LuaBaseRef luaInstance, int parentNodeId = 0)
    {
        this.looper = l;
        this._node = node;
        this.LuaName = luaName;
        this.parentNodeId = parentNodeId;
        this.Refs = new[] {luaInstance};
    }
#else
     public LuaUINode(LuaComponent l, GameObject node, string luaName, int parentNodeId = 0)
    {
        this.looper = l;
        this._node = node;
        this.LuaName = luaName;
        this.parentNodeId = parentNodeId;
    }
#endif

    public void AddChildNode(int id)
    {
        _childNodeId = ChildNodeId ?? new List<int>();
        if (!_childNodeId.Contains(id))
            _childNodeId.Add(id);
        else
            Debug.LogWarning("$$ same id ...");
    }

    public Component GetComponent(string path, string type)
    {
        string key = string.Concat(path, "$", type);
        cachedComponent = cachedComponent ?? new Dictionary<string, Component>();
        if (!cachedComponent.ContainsKey(key))
        {
            Component com = _node.transform.Find(path).GetComponent(type);
            cachedComponent[key] = com;
        }
        return cachedComponent[key];
    }

   
    Dictionary<string, LuaFunction> cachedLuaFunction = new Dictionary<string, LuaFunction>();

    LuaFunction FindLuaFunction(string funName)
    {
        LuaFunction luaFun = null;
        if (!cachedLuaFunction.ContainsKey(funName))
        {
            luaFun = LuaClient.GetMainState().GetFunction(funName);
            cachedLuaFunction[funName] = luaFun;
        }
        else
        {
            luaFun = cachedLuaFunction[funName];
        }
        
        return luaFun;
    }

#if _OPT_CALL
    object CallFunction(string funName, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
    {
        LuaFunction func = FindLuaFunction(string.Concat(LuaName,'.', funName));
        object res = null;
        if (func != null)
        {
            func.BeginPCall();
            if (arg1 != null)
                func.Push(arg1);
            if (arg2 != null)
                func.Push(arg2);
            if (arg3 != null)
                func.Push(arg3);
            if (arg4 != null)
                func.Push(arg4);
            if (arg5 != null)
                func.Push(arg5);
            if (arg6 != null)
                func.Push(arg6);
            func.PCall();
            //int num = (int)func.CheckNumber();
            res = func.CheckObject(typeof(object));
            func.EndPCall();
        }
        return res;
    }
#else
    
    object CallFunction(string funName, bool isSelf = false, params object[] args)
    {
        LuaFunction func = FindLuaFunction(string.Concat(LuaName, isSelf?":":".", funName));
        object[] res = func.Invoke<object[],object[]>(args);
        if (res != null && res.Length > 0)
            return res[0];
        return 0;
    }
    object CallFunction(string funName, params object[] args)
    {
        LuaFunction func = FindLuaFunction(string.Concat(LuaName, ".", funName));
        object[] res = func.Invoke<object[], object[]>(args);
        if (res != null && res.Length > 0)
            return res[0];
        return 0;
    }
#endif



    GameObject injectRef()
    {
        return _node;
    }


    public void InitLua()
    {
        if (string.IsNullOrEmpty(LuaName))
        {
            LuaName = _node.name;
        }

#if _LUA_UI_OPT_LOW

#else

        object res = CallFunction("New",false, _node.GetInstanceID());
        Refs = new LuaBaseRef[1];
        Refs[0] = res as LuaBaseRef;
#endif

        /** option */
        //CallFunction("SetRootNode", Refs[0], injectRef());
        CallFunction("Awake", Refs[0], injectRef());
    }

    // Use this for initialization
    public void Start()
    {
        CallFunction("Start", Refs[0], injectRef());
        if (EnableUpdate)
        {
            m_updateCoroutine = doUpdate(() =>
            {
                CallFunction("Update", Refs[0], injectRef(), Time.deltaTime);
            });
            looper.StartCoroutine(m_updateCoroutine);
        }
        if (EnableLateUpdate)
        {
            m_lateUpdateCoroutine = doLateUpdate(() =>
            {
                CallFunction("LateUpdate", Refs[0], injectRef());
            });
            looper.StartCoroutine(m_lateUpdateCoroutine);
        }
    }

    public void OnEnable()
    {
        CallFunction("OnEnable", Refs[0], injectRef());
        if (m_updateCoroutine != null)
        {
            looper.StartCoroutine(m_updateCoroutine);
        }
        if (m_lateUpdateCoroutine != null)
        {
            looper.StartCoroutine(m_lateUpdateCoroutine);
        }
    }

    public void OnDisable()
    {
        CallFunction("OnDisable", Refs[0], injectRef());
    }

    private bool isDestroy = false;

    public void OnDestroy()
    {
#if UNITY_EDITOR

#endif
        CallFunction("OnDestroy", Refs[0], injectRef());
        try
        {
            foreach (var r in Refs)
            {
                r.Dispose();
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        finally
        {
            Refs = null;
        }
    }

    IEnumerator commonLoop(IEnumerator targetCoroutine)
    {
        while (true)
        {
            targetCoroutine.MoveNext();
            if (targetCoroutine.Current != null)
                yield return targetCoroutine.Current;
            else
                yield return null;
        }
    }

    IEnumerator doUpdate(System.Action onUpdate)
    {
        while (true)
        {
            onUpdate();
            yield return null;
        }
    }

    IEnumerator doLateUpdate(System.Action onLateUpdate)
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            onLateUpdate();
        }
    }
}

public class LuaComponent : MonoBehaviour
{

    public LuaComponent()
    {
        _instance = this;
    }

    private static LuaComponent _instance;

    public static LuaComponent Instance
    {
        get { return _instance; }
        set { _instance = value; }
    }
    
    public static LuaState luaState { get { return LuaClient.GetMainState(); } }

    //public static readonly Dictionary<string, List< LuaUINode>> UINodes = new Dictionary<string, List<LuaUINode>>();
    public static readonly Dictionary<int, LuaUINode> Id2UINodes = new Dictionary<int, LuaUINode>();


    public void Start()
    {
#if UNITY_EDITOR        
        luaState.AddSearchPath(string.Concat(Application.dataPath, "/Lua/"));

#endif
    }

    public static void LoadUILuaFile()
    {
        string manual = Application.dataPath + "/Lua/UI/manual";
        string gen = Application.dataPath + "/Lua/UI/gen";

        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {

            manual = Application.dataPath + "../data/lua/UI/manual";
            gen = Application.dataPath + "../data/lua/UI/gen";

        }

        System.Array.ForEach<string>(System.IO.Directory.GetDirectories(manual), (path)=>{
            System.Array.ForEach<string>(System.IO.Directory.GetFiles(path), (file)=>{
                if(!file.EndsWith(".meta"))
                luaState.LuaDoFile(file);
            });
        });

        System.Array.ForEach<string>(System.IO.Directory.GetDirectories(gen), (path)=>{
            System.Array.ForEach<string>(System.IO.Directory.GetFiles(path), (file)=>{
                if(!file.EndsWith(".meta"))
                luaState.LuaDoFile(file);
            });
        });
    }

    public static void LoadLuaFile(string subFolder, string name)
    {
        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            luaState.LuaDoFile(string.Concat(Application.dataPath, "/../data/lua/", subFolder, "/", name, ".lua"));
        }
        else
        {
            luaState.LuaDoFile(string.Concat(Application.dataPath, "/Lua/", subFolder, "/", name, ".lua"));
        }
    }

    /** 常用lua调用c#的代码 静态方法绑定性能好*/
    public static GameObject LoadUIPrefab(string path)
    {
        return Resources.Load<GameObject>(path);
    }

#if _LUA_UI_OPT_LOW

    public static void Create(string path, LuaFunction onLoaded)
    {

#if UNITY_EDITOR && !_FORCE_AB
        try
        {
            UnityEngine.Object prefab = null;
            prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Art/" + path + ".prefab");
            var go = GameObject.Instantiate(prefab) as GameObject;
            onLoaded.Call(go);
        }
        finally
        {
            onLoaded.Dispose();
        }

#else

        //string[] nodes = path.Split('/');
        //path = string.Concat(nodes[0], "/", nodes[nodes.Length - 1]);
        //ResourceManager.Instance.LoadBundle(path.ToLower(), obj =>
        //{
        //    try
        //    {
        //        UnityEngine.Object prefab = obj as UnityEngine.GameObject;
        //        var go = GameObject.Instantiate(prefab) as GameObject;
        //        onLoaded.Call(go);
        //    }
        //    finally
        //    {
        //        onLoaded.Dispose();
        //    }
        //});

#endif
    }

    public static void CreateWithEmit(string path, LuaTable emitter)
    {

#if UNITY_EDITOR && !_FORCE_AB
        try
        {
            UnityEngine.Object prefab = null;
            prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Art/" + path + ".prefab");
            var go = GameObject.Instantiate(prefab) as GameObject;
            GameObject.DontDestroyOnLoad(go);
            LuaFunction emitFunc = emitter.GetLuaFunction("Emit");
            emitFunc.BeginPCall();
            emitFunc.Push(emitter);
            emitFunc.Push(go);
            emitFunc.PCall();
            emitFunc.EndPCall();
            emitFunc.Dispose();
        }
        finally
        {
            emitter.Dispose();
        }

        
        
#else
        string[] nodes = path.Split('/');
        path = string.Concat(nodes[0], "/", nodes[nodes.Length - 1]);

        var resMgr = AppFacade.Instance.GetManager<LuaFramework.ResourceManager>(LuaFramework.ManagerName.Resource);
        resMgr.LoadPrefab("", nodes[nodes.Length - 1], (objs)=>
        {
            try
            {
                UnityEngine.Object prefab = objs[0] as UnityEngine.GameObject;
                var go = GameObject.Instantiate(prefab) as GameObject;
                GameObject.DontDestroyOnLoad(go);
                LuaFunction emitFunc = emitter.GetLuaFunction("Emit");
                emitFunc.BeginPCall();
                emitFunc.Push(emitter);
                emitFunc.Push(go);
                emitFunc.PCall();
                emitFunc.EndPCall();
                emitFunc.Dispose();
            }
            finally
            {
                emitter.Dispose();
            }
        });

        //ResourceManager.Instance.LoadBundle(path.ToLower(), obj =>
        //{
        //    try
        //    {
        //        UnityEngine.Object prefab = obj as UnityEngine.GameObject;
        //        var go = GameObject.Instantiate(prefab) as GameObject;
        //        GameObject.DontDestroyOnLoad(go);
        //        LuaFunction emitFunc = emitter.GetLuaFunction("Emit");
        //        emitFunc.BeginPCall();
        //        emitFunc.Push(emitter);
        //        emitFunc.Push(go);
        //        emitFunc.PCall();
        //        emitFunc.EndPCall();
        //        emitFunc.Dispose();
        //    }
        //    finally
        //    {
        //        emitter.Dispose();
        //    }
        //});

#endif
    }

    public static void LoadTexture(string path, LuaFunction onLoaded)
    {
#if UNITY_EDITOR && !_FORCE_AB
        try
        {
            UnityEngine.Texture2D tex = null;
            tex = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Art/" + path + ".png");
            tex = tex ?? AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Art/" + path + ".bmg");
            tex = tex ?? AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Art/" + path + ".jpg");
            tex = tex ?? AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Art/" + path + ".tga");
            tex = tex ?? AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Art/" + path + ".psd");
            UnityEngine.Assertions.Assert.IsNotNull(tex, "## Texture " + path + " is not exist");
            onLoaded.Call(tex);
        }
        finally
        {
            onLoaded.Dispose();
        }
#else

        //string[] nodes = path.Split('/');
        //path = string.Concat(nodes[0], "/", nodes[nodes.Length - 1]);
        //ResourceManager.Instance.LoadBundle(path.ToLower(), obj =>
        //{
        //    UnityEngine.Texture2D tex = obj as UnityEngine.Texture2D;
        //    try
        //    {
        //        onLoaded.Call(tex);
        //    }
        //    finally
        //    {
        //        onLoaded.Dispose();
        //    }
        //});
#endif
    }

    public static void LoadSprite(string path, LuaFunction onLoaded)
    {

#if UNITY_EDITOR && !_FORCE_AB
        try
        {
            UnityEngine.Texture2D tex = null;
            tex = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Art/" + path + ".png");
            tex = tex ?? AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Art/" + path + ".bmg");
            tex = tex ?? AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Art/" + path + ".jpg");
            tex = tex ?? AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Art/" + path + ".tga");
            tex = tex ?? AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Art/" + path + ".psd");
            UnityEngine.Assertions.Assert.IsNotNull(tex, "## Texture " + path + " is not exist");
            if (tex != null)
            {
                UnityEngine.Sprite sprite = UnityEngine.Sprite.Create(tex, Rect.MinMaxRect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100f);
                onLoaded.Call(sprite);
            }
        }
        finally
        {
            onLoaded.Dispose();
        }
#else

        //string[] nodes = path.Split('/');
        //path = string.Concat(nodes[0], "/", nodes[nodes.Length - 1]);
        //ResourceManager.Instance.LoadBundle(path.ToLower(), obj =>
        //{
        //    UnityEngine.Texture2D tex = obj as UnityEngine.Texture2D;
        //    try
        //    {
        //        UnityEngine.Sprite sprite = UnityEngine.Sprite.Create(tex, Rect.MinMaxRect(0, 0, tex.width, tex.height),
        //            new Vector2(0.5f, 0.5f), 100f);
        //        onLoaded.Call(sprite);
        //    }
        //    finally
        //    {
        //        onLoaded.Dispose();
        //    }
        //});
#endif



    }
#else
    
    public static LuaTable Create(string luaName, string prefabPath, int parentId)
    {
        var prefab = Resources.Load<GameObject>(prefabPath);
        var go = GameObject.Instantiate(prefab);
        int instanceId = go.GetInstanceID();
        string luaClass = string.IsNullOrEmpty(luaName) ? prefab.name : luaName;


        LuaUINode node = new LuaUINode(Instance, go, luaClass, parentId);

        //UINodes[path].Add(node);
        Id2UINodes[go.GetInstanceID()] = node;

        if (node.HasParent && Id2UINodes.ContainsKey(node.ParentNodeId))
        {
            Id2UINodes[node.ParentNodeId].AddChildNode(go.GetInstanceID());
        }

        node.InitLua();
#if UNITY_EDITOR
        Debug.Log("$$ createUI Id:"+ instanceId);
#endif
        return node.LuaInstance as LuaTable;
    }
#endif


    public static bool DestroyUI(int instanceId)
    {
        if (Id2UINodes.ContainsKey(instanceId))
        {
            LuaUINode target = Id2UINodes[instanceId];
            if (Id2UINodes[instanceId].HasParent)
            {
                Id2UINodes[Id2UINodes[instanceId].ParentNodeId].ChildNodeId.Remove(instanceId);
            }
            DeattachChild(instanceId);

            GameObject.Destroy(target.Node);
        }

        return true;
    }

    public static void DeattachChild(int instanceId)
    {
        for(int i=0,c = Id2UINodes[instanceId].ChildNodeId.Count; i<c; i++)
        {
            DeattachChild(Id2UINodes[instanceId].ChildNodeId[i]);
        }
        Id2UINodes[instanceId].OnDestroy();
        if (Id2UINodes.ContainsKey(instanceId))
            Id2UINodes.Remove(instanceId);
    }


    public static Component GetUIElement(int nodeId, string path, string type)
    {
        return Id2UINodes[nodeId].GetComponent(path, type);
    }

    public static bool SetSortOrder(GameObject canvasObj, int sortOrder)
    {
        var canvas = canvasObj.GetComponent<UnityEngine.Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = sortOrder;
            canvas.worldCamera = GameObject.Find("/GameManager/UIManager/UICamera").GetComponent<Camera>();
            return true;
        }
        return false;
    }

    public static void SetParent(int nodeId, Transform parent)
    {
        Transform child = Id2UINodes[nodeId].Node.transform;
        child.SetParent( parent as RectTransform);
        child.localScale = Vector3.one;
    }

    public static void LogWithTag(string tag, string msg)
    {
        //Debug.logger.Log(LogType.Log, tag, msg);
    }
    
    //查找某个物体 递归搜索所有子节点物体
    public static Transform FindTr(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t != null)
            return t;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform o = FindTr(parent.GetChild(i), name);
            if (o != null)
                return o;
        }
        return null;
    }

    //根据名字查找物体，在所有子中物体递归搜索
    public static GameObject FindObj(GameObject objRoot, string name)
    {
        GameObject obj = null;
        foreach (Transform it in objRoot.transform)
        {
            if (it.name == name)
                return it.gameObject;

            if (it.childCount == 0)
                continue;

            obj = FindObj(it.gameObject, name);
            if (obj != null)
                break;
        }
        return obj;
    }

    //根据名字查找物体，如果具体路径不知道(懒得去知道)，只知道物体名字和顶层物体名字，则用这个查找
    public static GameObject FindObj2(string root, string name)
    {
        GameObject objRoot = GameObject.Find(root);
        if (objRoot == null)
            return null;
        return FindObj(objRoot, name);
    }
    
    //设置某个物体是否启用 
    public static void SetObjActive2(string root, string name, bool bActive)
    {
        GameObject go = FindObj2(root, name);
        if (go != null)
            go.SetActive(bActive);
    }

    //设置某个物体是否启用 
    public static GameObject SetObjActive(GameObject root, string name, bool bActive)
    {
        if (root == null)
            return null;
        GameObject go = FindObj(root, name);
        if (go != null)
            go.SetActive(bActive);
        return go;
    }

    //递归遍历某物体下所有子物体节点, 然后对这些进行某种操作
    public static void ForChild(Transform parent, System.Action<GameObject, object[]> callback, params object[] args)
    {
        foreach (Transform child in parent)
        {
            ForChild(child, callback, args);
        }

        if (callback != null)
            callback(parent.gameObject, args);
    }

    //根据名字找子物体中的Component
    public static UnityEngine.Object FindComponent(GameObject objRoot, string name,string componentName)
    {
        GameObject obj = FindObj(objRoot, name);
        if (obj != null)
            return obj.GetComponent(componentName);
        return null;
    }

    //根据名字找子物体中的Component
    public static T FindComponent2<T>(GameObject objRoot, string name)
    {
        GameObject obj = FindObj(objRoot, name);
        if (obj != null)
            return obj.GetComponent<T>();
        return default(T);
    }

    //所有子物体中的指定Component
    public static List<T> FindComponents<T>(GameObject objRoot)
    {
        List<T> components = new List<T>();
        ForChild(objRoot.transform, delegate (GameObject obj, object[] parms)
        {
            if (obj != null)
                components.AddRange(obj.GetComponents<T>());
        });
        return components;
    }

    //取得根下某个物体(没则创建)
    public static Transform GetGoRoot(string name, Transform trParent = null)
    {
        GameObject tmp = trParent == null ? GameObject.Find(name) : FindObj(trParent.gameObject, name);
        if (tmp == null)
        {
            tmp = new GameObject(name);
            tmp.transform.SetParent(trParent);
            tmp.transform.rotation = Quaternion.identity;
            tmp.transform.localScale = Vector3.one;
            tmp.transform.position = Vector3.zero;
        }
        return tmp == null ? null : tmp.transform;
    }
    
}
