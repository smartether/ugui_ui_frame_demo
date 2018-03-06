#region Macro

#define _LUA_UI_OPT_LOW


#if _LUA_UI_OPT_SUPER
#undef _LUA_UI_OPT_LOW
#undef _LUA_UI_OPT_MID
#undef _LUA_UI_OPT_HIGH
#endif

#endregion


namespace UITools
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using Object = UnityEngine.Object;
    using System.IO;
    public class UIBuilder : Editor
    {

        public static string ExportFolder
        {
            get
            {
                return Application.dataPath + "/Lua/UI";
            }
        }

        private static OptimizeLevel optLv
        {
            get
            {
                OptimizeLevel l = OptimizeLevel.Super;

#if _LUA_UI_OPT_SUPER
                l = OptimizeLevel.Super;
#elif _LUA_UI_OPT_HIGH
                l = OptimizeLevel.High;
#elif _LUA_UI_OPT_MID
                l = OptimizeLevel.Mid;
#elif _LUA_UI_OPT_LOW
                l = OptimizeLevel.Low;
#endif
                return l;

            }
        }


        enum OptimizeLevel
        {
            Low = 1,
            Mid = 2,
            High = 3,
            Super = 4
        }

        public struct ExportInfo
        {
            public Type ExportType;
            public string ExportName;
            public string ExportPath;
        }

        public static Dictionary<Type, string> ExportUIType = new Dictionary<Type, string>()
        {
            {typeof(UnityEngine.CanvasRenderer), "widget_"},
            {typeof(RectTransform),"recttrans_" },
            {typeof(Transform),"trans_" },
            {typeof(Text),"text_"},
            {typeof(Image),"image_"},
            {typeof(RawImage),"rawImage_"},
            {typeof(UnityEngine.UI.Button),"button_"},
            {typeof(UnityEngine.UI.InputField), "input_" },
            {typeof(UnityEngine.UI.Toggle), "toggle_" },
            {typeof(UnityEngine.UI.Scrollbar), "scrollBar_" },
            {typeof(UnityEngine.UI.ScrollRect), "scrollRect_" },
            {typeof(UnityEngine.UI.Slider), "slider_" },
            {typeof(UnityEngine.UI.Dropdown),"dropDown_" },
            {typeof(UnityEngine.UI.GridLayoutGroup),"grid_" },
            {typeof(UnityEngine.UI.HorizontalLayoutGroup),"page_" },
            {typeof(UnityEngine.UI.VerticalLayoutGroup),"list_" }
        };

        private static Dictionary<string, Type> _ExportUIPrefx2Type;
        public static Dictionary<string, Type> ExportUIPrefx2Type
        {
            get
            {
                if (_ExportUIPrefx2Type == null)
                {
                    var exchange = new Dictionary<string, Type>();
                    foreach (var kv in ExportUIType)
                    {
                        exchange[kv.Value] = kv.Key;
                    }
                    _ExportUIPrefx2Type = exchange;
                }
                return _ExportUIPrefx2Type;
            }
        }


        static bool TagValid(MenuCommand cmd)
        {
            return ExportUIType.ContainsKey(cmd.context.GetType()) && !cmd.context.name.StartsWith(ExportUIType[cmd.context.GetType()]);
        }

        static bool TagValidUndo(MenuCommand cmd)
        {
            return ExportUIType.ContainsKey(cmd.context.GetType()) && cmd.context.name.StartsWith(ExportUIType[cmd.context.GetType()]);
        }


        static string nameGenerate(MenuCommand cmd)
        {
            if (ExportUIType.ContainsKey(cmd.context.GetType()))
            {
                return string.Format("{1}{0}", cmd.context.name, ExportUIType[cmd.context.GetType()]);
            }
            return cmd.context.name;
        }

        private const string postfix = "_export";

        [MenuItem("CONTEXT/Component/Tag2Export")]
        public static void Tag2Export(MenuCommand cmd)
        {
            cmd.context.name = nameGenerate(cmd);
        }

        [MenuItem("CONTEXT/Component/Tag2Export", true)]
        public static bool Tag2ExportValid(MenuCommand cmd)
        {
            var type = cmd.context.GetType();
            bool exportable = type.FullName.StartsWith("UnityEngine");
            return exportable && TagValid(cmd);
        }


        [MenuItem("CONTEXT/Component/UndoExport")]
        public static void UndoExport(MenuCommand cmd)
        {
            Object t = cmd.context;
            if (TagValidUndo(cmd))
            {
                t.name = t.name.Remove(0, ExportUIType[cmd.context.GetType()].Length);
                // t.name.Remove(t.name.Length - postfix.Length, postfix.Length);
            }
        }

        [MenuItem("CONTEXT/Component/UndoExport", true)]
        public static bool UndoExportValid(MenuCommand cmd)
        {
            return TagValidUndo(cmd);
        }


        [MenuItem("CONTEXT/Transform/GenerateLuaCode", true)]
        static bool GenerateLuaCodeValid(MenuCommand cmd)
        {
            var tran = cmd.context as Transform;
            var prefabGo = PrefabUtility.FindPrefabRoot(tran.gameObject);
            var prefabType = PrefabUtility.GetPrefabType(tran.gameObject);
            return (prefabType == PrefabType.Prefab || prefabType == PrefabType.PrefabInstance) && prefabGo.name == tran.name;
        }

        private static bool neverOverride = false;

        [MenuItem("CONTEXT/Transform/GenerateLuaCode")]
        [MenuItem("Assets/GenerateLuaCode")]
        [MenuItem("Assets/UITools/GenerateLuaCode")]
        public static void GenerateLuaCode(MenuCommand cmd)
        {
            neverOverride = false;
            GameObject[] prefabInstance = cmd.context == null ? Selection.gameObjects : new[] { (cmd.context as Transform).gameObject };
            foreach (var go in prefabInstance)
            {
                GameObject prefabRoot = PrefabUtility.FindPrefabRoot(go);
                bool isPrefabInstance = PrefabUtility.GetPrefabType(prefabRoot) == PrefabType.PrefabInstance;
                string prefabPath = isPrefabInstance ? AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(prefabRoot).GetInstanceID()) : AssetDatabase.GetAssetPath(prefabRoot);
                const string strClip = "Assets/Art/";
                const string strClip2 = ".prefab";
                if (prefabPath.StartsWith(strClip))
                {
                    prefabPath = prefabPath.Replace(strClip, string.Empty);
                }
                if (prefabPath.EndsWith(strClip2))
                {
                    prefabPath = prefabPath.Replace(strClip2, string.Empty);
                }

                string subFolder = prefabPath.Remove(prefabPath.Length - System.IO.Path.GetFileName(prefabPath).Length, System.IO.Path.GetFileName(prefabPath).Length);
                subFolder = subFolder.Remove(0, "ui".Length);
                generateLuaCode(prefabRoot, prefabPath, subFolder);
                generateLuaCodeUser(prefabRoot.name, subFolder);
            }
            neverOverride = false;
        }



        [MenuItem("CONTEXT/Transform/GenerateLuaViewModelCode", true)]
        static bool GenerateLuaViewModelCodeValid(MenuCommand cmd)
        {
            var tran = cmd.context as Transform;
            var prefabGo = PrefabUtility.FindPrefabRoot(tran.gameObject);
            var prefabType = PrefabUtility.GetPrefabType(tran.gameObject);
            return (prefabType == PrefabType.Prefab || prefabType == PrefabType.PrefabInstance) && prefabGo.name == tran.name;
        }

        [MenuItem("CONTEXT/Transform/GenerateLuaViewModelCode")]
        [MenuItem("Assets/UITools/GenerateLuaViewModelCode")]
        public static void GenerateLuaViewModelCode(MenuCommand cmd)
        {
            GameObject[] prefabInstance = cmd.context == null ? Selection.gameObjects : new[] { (cmd.context as Transform).gameObject };
            foreach (var go in prefabInstance)
            {
                GameObject prefabRoot = PrefabUtility.FindPrefabRoot(go);
                bool isPrefabInstance = PrefabUtility.GetPrefabType(prefabRoot) == PrefabType.PrefabInstance;
                string prefabPath = isPrefabInstance ? AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(prefabRoot).GetInstanceID()) : AssetDatabase.GetAssetPath(prefabRoot);
                const string strClip = "Assets/Art/";
                const string strClip2 = ".prefab";
                if (prefabPath.StartsWith(strClip))
                {
                    prefabPath = prefabPath.Replace(strClip, string.Empty);
                }
                if (prefabPath.EndsWith(strClip2))
                {
                    prefabPath = prefabPath.Replace(strClip2, string.Empty);
                }

                string subFolder = prefabPath.Remove(prefabPath.Length - System.IO.Path.GetFileName(prefabPath).Length, System.IO.Path.GetFileName(prefabPath).Length);
                subFolder = subFolder.Remove(0, "ui".Length);
                generateLuaViewModelCodeUser(prefabRoot.name, subFolder);
            }
        }

        private static void generateLuaCode(GameObject prefabRoot, string prefabPath, string subFolder)
        {
            string luaName = prefabRoot.name;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("local " + luaName + "= UIManager.GetUIType('" + luaName+"','" + subFolder.Trim('/') + "')");

            sb.InsertFunction(luaName, "PATH", string.Concat("return ", "'", prefabPath, "'"), true);

            sb.Append(generateLuaUIInitFunction(luaName));

            if (OptimizeLevel.Low > optLv)
            {
                sb.InsertFunction(luaName, "SetParent", "return LuaComponent.SetParent(self.__id,parent)", false,
                    "parent");
            }

            List<ExportInfo> exportInfos = new List<ExportInfo>(8);

            exportInfos.Add(new ExportInfo()
            {
                ExportName = "node",
                ExportType = typeof(Transform),
                ExportPath = ""
            });

            StringBuilder codeBlocks = new StringBuilder();

            Dictionary<string, GameObject> pathChildMap;
            var lst = findChild(prefabRoot, out pathChildMap);
            foreach (var o in pathChildMap)
            {
                foreach (var kv in ExportUIType)
                {
                    if (o.Value.name.StartsWith(kv.Value))
                    {
                        //codeBlocks = codeBlocks.Append(generateLuaViewBindCode(ExportUIPrefx2Type[kv.Value], luaName, o.Value.name, o.Key));
                        if (exportInfos.Exists(info => info.ExportName == o.Value.name))
                        {
                            if (EditorUtility.DisplayDialog("UI错误", "UI命名有重复", "找到有问题的命名"))
                            {
                                Selection.activeGameObject = o.Value;
                                return;
                            }
                        }

                       

                        exportInfos.Add(new ExportInfo() { ExportName = o.Value.name, ExportType = ExportUIPrefx2Type[kv.Value], ExportPath = o.Key });

                        //如果导出的是transform 同时当初其他已经定义的组件
                        if (ExportUIPrefx2Type[kv.Value] == typeof(RectTransform) || ExportUIPrefx2Type[kv.Value] == typeof(Transform))
                        {
                            var comps = o.Value.GetComponents<Component>();
                            foreach (var com in comps)
                            {
                                if (ExportUIType.ContainsKey(com.GetType()) && com.GetType()!=typeof(Transform) && com.GetType()!=typeof(RectTransform))
                                {
                                    string exportName = ExportUIType[com.GetType()] + o.Value.name.Replace(ExportUIType[kv.Key], string.Empty);
                                    exportInfos.Add(new ExportInfo() { ExportName = exportName, ExportType = com.GetType(), ExportPath = o.Key });
                                }
                            }

                        }
                    }
                }
            }

            if (OptimizeLevel.Low == optLv)
            {
                List<string> statements = new List<string>();
                exportInfos.ForEach(info =>
                {
                    var s = generateLuaViewBindCode(info.ExportType, luaName, info.ExportName, info.ExportPath);
                    statements.Add(s.ToString());
                });

                codeBlocks.InsertFunction(luaName, "BindView", statements.ToArray(), false);

                statements.Clear();
                exportInfos.ForEach(info =>
                {
                    string statement = "self.m_" + info.ExportName + " = nil";
                    statements.Add(statement);
                });
                statements.Add("self.m_GameObject = nil");
                codeBlocks.InsertFunction(luaName, "UnBindView", statements.ToArray(), false);
                

                exportInfos.ForEach(info =>{
                    var bindEventFunction = generateLuaViewEventBindCode(info.ExportType, luaName, info.ExportName);
                    codeBlocks.Append(bindEventFunction);
                });

                //generate common bindData function for root node



                exportInfos.ForEach(info =>
                {
                    var bindDataFunction = generateLuaViewDataBindCode(info.ExportType, luaName, info.ExportName);
                    codeBlocks.Append(bindDataFunction);
                });
            }
            else if (OptimizeLevel.Mid <= optLv)
            {
                exportInfos.ForEach(info =>
                {
                    codeBlocks =
                        codeBlocks.Append(generateLuaViewBindCode(info.ExportType, luaName, info.ExportName, info.ExportPath));
                });
            }


            sb.Append(codeBlocks);

            Debug.Log(sb.ToString());

            bool success = true;
            try
            {
                string path = string.Format("{0}/gen/{1}{2}_gen.lua", ExportFolder, subFolder, luaName);
                if (!System.IO.Directory.Exists(ExportFolder + "/" + "gen/" + subFolder))
                {
                    System.IO.Directory.CreateDirectory(ExportFolder + "/"+ "gen/" + subFolder);
                }
                System.IO.File.WriteAllText(path, sb.ToString());
            }
            catch (Exception e)
            {
                success = false;
                EditorUtility.DisplayDialog("异常", e.Message, "ok");
            }
            finally
            {
                if (success)
                {
                    Debug.Log("$$ UI代码生成成功!");
                }
            }


        }

        private static StringBuilder generateLuaUIInitFunction(string className)
        {
            string[] statements;
            StringBuilder sb = new StringBuilder();
            if (OptimizeLevel.Low == optLv)
            {
                statements = new string[]
                {
                    "local go = LuaComponent.Create($this.PATH())",
                    "local self = $this.New()",
                    "self.m_GameObject = go",
                    "self:Init($0) ",
                    "return self"
                };


                //sb.InsertFunction(className, "Create", statements, true, "...");

            }
            else if (OptimizeLevel.Mid <= optLv)
            {
                statements = new string[]
                {
                    "t = LuaComponent.Create('$this',$this.PATH(), 0)",
                    "t:Init($0) ",
                    "return t"
                };

                //sb.InsertFunction(className, "Create", statements, true, "...");

                statements = new string[]
                {
                    "t = LuaComponent.Create('$this',$this.PATH(), $0.__id)",
                    "t:Init(...)",
                    "return t",
                };
                //sb.InsertFunction(className, "CreateWithParent", statements, true, "parent", "...");
            }

            if (OptimizeLevel.Low == optLv)
            {
                statements = new string[]
                {
                    "UnityEngine.GameObject.Destroy(self.m_GameObject)",
                    "self:OnDestroy()",
                    "self:UnBindView()"
                };


            }
            else if (OptimizeLevel.Mid <= optLv)
            {
                statements = new string[]
                {
                    "LuaComponent.DestroyUI(self.__id)",
                    "self:UnBindView()"
                };
                sb.InsertFunction(className, "DestroyUI", statements, false);
            }
            else
            {
                statements = new string[]
                {
                    "LuaComponent.DestroyUI(self.__id)",

                };

                sb.InsertFunction(className, "DestroyUI", statements, false);
            }


            return sb;
        }

        // public struct BindData{
        //     public string BindProperty;
        // }
        private static readonly Dictionary<System.Type, Dictionary<string, string>> UIBindEventMap = LuaCodeTemplate.UIBindEventMap;


        /** 生成view事件绑定语法糖 */
        private static StringBuilder generateLuaViewEventBindCode(Type eType, string className, string nodeName)
        {
            StringBuilder sb = new StringBuilder();
            if(UIBindEventMap.ContainsKey(eType)){
                foreach(var kv in UIBindEventMap[eType]){
                    string funName = string.Format("Bind_{0}_{1}", nodeName, kv.Value);
                    string statement = string.Format("self.m_{0}.{1}(self:On($0,$1,$2))", nodeName, kv.Key);
                    sb.InsertFunction(className, funName, statement, false, "func", "onfinished","onfailed");
                }
            }
            return sb;
        }

        /* 生成数据绑定语法糖 */
        private static StringBuilder generateLuaViewDataBindCode(Type eType, string className, string nodeName)
        {
            Debug.Log("$$ generateLua " + nodeName);
            Regex regexNodeName = new Regex("\\$nodeName");
            Regex regexFix = new Regex("\\$fix");
            StringBuilder sb = new StringBuilder();
            if (LuaCodeTemplate.UIBindDataTemplateMap.ContainsKey(eType)){
                foreach(var kv in LuaCodeTemplate.UIBindDataTemplateMap[eType]){
                    
                    string funName = string.Format("BindData_{0}_{1}", nodeName, kv.Value.FunctionNamePostfix);
                    string[] statements = new string[ kv.Value.CodeBlockTemplate.Length];
                    System.Array.Copy(kv.Value.CodeBlockTemplate, 0, statements, 0, kv.Value.CodeBlockTemplate.Length);

                    for (int i = 0; i < statements.Length; i++)
                    {
                        while (regexNodeName.IsMatch(statements[i]))
                        {
                            var match = regexNodeName.Match(statements[i]);
                            statements[i] = statements[i].Remove(match.Index, match.Length);
                            statements[i] = statements[i].Insert(match.Index, nodeName);
                        }
                        while (regexFix.IsMatch(statements[i]))
                        {
                            var match = regexFix.Match(statements[i]);
                            statements[i] = statements[i].Remove(match.Index, match.Length);
                            statements[i] = statements[i].Insert(match.Index,kv.Value.FunctionNamePostfix);
                        }
                    }
                    
                    sb.InsertFunction(className, funName, statements, false, kv.Value.CodeBlockParams);
                }
                
            }
            return sb;
        }

        /** 生成view代码块 */
        private static StringBuilder generateLuaViewBindCode(Type eType, string className, string nodeName, string path)
        {
            GameObject go;
            StringBuilder sb = new StringBuilder();
            //string statement = string.Format("return self.root.transform:FindChild('{0}'):GetComponent('{1}')",path, eType.FullName);
            //string statement = statement = string.Format("return LuaComponent.GetUIElement('{0}','{1}',{2})", path, eType.FullName, "self.root");
            string[] statement = null;
            if (OptimizeLevel.Low == optLv)
            {
                statement = new string[]
                {
                  string.IsNullOrEmpty(path)?"self.m_" +nodeName + " = self.m_GameObject" : "self.m_"+nodeName+" = self.m_GameObject.transform:FindChild('"+path+"')"+(eType.IsSubclassOf(typeof(UnityEngine.Transform))?string.Empty:":GetComponent('"+eType.FullName+"')"),
                };
                sb.InsertStatement(false, statement);
            }
            else if (OptimizeLevel.Mid == optLv)
            {
                statement = new string[]
                {
                "if self.m_"+nodeName+" == nil then",
                "self.m_"+nodeName+" = self.m_GameObject.transform:FindChild('"+path+"'):GetComponent('"+eType.FullName+"')",
                "end",
                "return self.m_" + nodeName,
                };
                sb.InsertFunction(className, nodeName, statement, false);
            }
            else if (OptimizeLevel.High <= optLv)
            {
                statement = new string[] { string.Format("return LuaComponent.GetUIElement({0},'{1}','{2}')", "self.__id", path, eType.FullName) };
                sb.InsertFunction(className, nodeName, statement, false);
            }
            return sb;
        }

        private static StringBuilder generateLuaCodeUser(string luaName, string subFolder)
        {
            string userCodePath = string.Format("{0}/manual/{1}{2}.lua", ExportFolder, subFolder, luaName);
            bool exist = System.IO.File.Exists(userCodePath);
            bool isOverride = exist && !neverOverride && EditorUtility.DisplayDialog("提示", string.Format("用户代码{0}已经存在 覆盖吗？", luaName), "覆盖", "不覆盖");

            var userCode = new StringBuilder();
            if (!exist || System.IO.File.Exists(userCodePath) && isOverride)
            {

                //userCode = userCode.InsertClass(luaName, true);
                userCode = userCode.AppendLine("local " + luaName + " = " + string.Format("UIManager.GenUIType('{0}','{1}')", luaName, subFolder.Trim('/')));
                userCode = userCode.AppendLine("local this=" + luaName);



                string[] awakeStatement = null;
                if (OptimizeLevel.Low == optLv)
                {
                    userCode = userCode.InsertFunction(luaName, "ctor", "self.model = $0", false, "modelOrVmodel");

                    awakeStatement = new[]
                    {
                    "self:BindView()",
                    //"self.model = $0"
                    };
                    userCode.InsertFunction(luaName, "Init", awakeStatement, false);  //, "model");
                }
                else if (OptimizeLevel.Mid <= optLv)
                {
                    userCode = userCode.InsertFunction(luaName, "ctor", "self.__id = $0", false, "id");

                    awakeStatement = new[] { "self.m_GameObject = go" };
                    userCode.InsertFunction(luaName, "Init", "", false, "model");
                    userCode.InsertFunction(luaName, "Awake", awakeStatement, false, "go");
                }


                //userCode.InsertFunction(luaName, "Start", "", false, "go");
                userCode.InsertFunction(luaName, "OnDestroy", "", false);
                userCode.InsertFunction(luaName, "OnOpen", "", false);
                userCode.InsertFunction(luaName, "OnClose", "", false);
                userCode.InsertFunction(luaName, "OnShow", "", false);
                userCode.InsertFunction(luaName, "OnHide", "", false);
                userCode.InsertFunction(luaName, "OnShowUIAnimation", "", false);
                userCode.InsertFunction(luaName, "OnHideUIAnimation", "", false, "func", "obj");
                userCode.InsertFunction(luaName, "OnUIAnimationEnd", "", false, "uibase", "args");

                if (!System.IO.Directory.Exists(ExportFolder + "/" + "manual/" + subFolder))
                {
                    System.IO.Directory.CreateDirectory(ExportFolder + "/" + "manual/" + subFolder);
                }
                System.IO.File.WriteAllText(userCodePath, userCode.ToString());

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                var select = AssetDatabase.FindAssets(luaName, new[] { ExportFolder });
                if (select.Length > 0)
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(select[0]));
            }
            if (exist && !isOverride && !neverOverride)
                neverOverride = EditorUtility.DisplayDialog("提示", "之后都不覆盖？", "是", "不是");

            return userCode;
        }


        private static StringBuilder generateLuaViewModelCodeUser(string luaName, string subFolder)
        {
            string userCodePath = string.Format("{0}/manual/{1}{2}ViewModel.lua", ExportFolder, subFolder, luaName);
            string TemplateName = luaName;
            luaName = luaName + "ViewModel";
            bool exist = System.IO.File.Exists(userCodePath);
            bool isOverride = exist && EditorUtility.DisplayDialog("提示", string.Format("用户代码{0}已经存在 覆盖吗？", luaName), "覆盖", "不覆盖");

            var userCode = new StringBuilder();
            if (!exist || System.IO.File.Exists(userCodePath) && isOverride)
            {

                //userCode = userCode.InsertClass(luaName, true);
                userCode = userCode.AppendLine("local " + luaName + " = " + string.Format("UIManager.GenViewModelType('{0}','{1}')", luaName, subFolder.Trim('/')));
                userCode = userCode.AppendLine("local this=" + luaName);

                //local DlgBagViewModel = UIManager.GenViewModelType('DlgBagViewModel')


                string[] awakeStatement = null;
                if (OptimizeLevel.Low == optLv)
                {
                    userCode = userCode.InsertFunction(luaName, "ctor", "self.Template = '" + TemplateName + "'", false, "model", "...");
                    /*
                    awakeStatement = new[]
                    {
                    "self:BindView()",
                    "self.model = $0"
                    };
                    userCode.InsertFunction(luaName, "Init", awakeStatement, false, "model");
                    */
                }
                else if (OptimizeLevel.Mid <= optLv)
                {
 
                }



                if (!System.IO.Directory.Exists(ExportFolder + "/" + "manual/" + subFolder))
                {
                    System.IO.Directory.CreateDirectory(ExportFolder + "/" + "manual/" + subFolder);
                }
                System.IO.File.WriteAllText(userCodePath, userCode.ToString());

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                var select = AssetDatabase.FindAssets(luaName, new[] { ExportFolder });
                if (select.Length > 0)
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(select[0]));
            }

            return userCode;
        }


        public static List<GameObject> findChild(GameObject parent, out Dictionary<string, GameObject> pathChildMap)
        {

            List<GameObject> children = new List<GameObject> { parent };
            pathChildMap = new Dictionary<string, GameObject>();
            int idx = 0;
            while (children.Count > idx)
            {
                int count = children[idx].transform.childCount;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        GameObject child = children[idx].transform.GetChild(i).gameObject;
                        children.Add(child);

                        Transform tranTmp = child.transform;
                        string childPath = tranTmp.gameObject.name;
                        StringBuilder sb = new StringBuilder(childPath);
                        int max = 0;
                        while (tranTmp.parent != null && tranTmp.parent != parent.transform)
                        {
                            tranTmp = tranTmp.parent;
                            sb.Insert(0, "/");
                            sb.Insert(0, tranTmp.name);
                            max++;
                            if (max > 100)
                                break;
                        }
                        pathChildMap[sb.ToString()] = child;
                    }
                }
                idx++;
            }

            return children;
        }
    }

    /** 后期仿造codedom实现lua的代码生成 替代字符串拼接 */
    public class LuaCodeBuilder
    {
        public struct LuaCodeFunction
        {
            
        }

        public struct LuaCodeField
        {
            
        }

        public struct LuaCodeStatement
        {
            
        }

    }

    public static class StringBuild_CodeBuilder
    {

        public static StringBuilder InsertTab(this StringBuilder sb, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                sb.Append("\t");
            }
            return sb;
        }

        public static StringBuilder InsertEqual(this StringBuilder sb)
        {
            sb.Append(" = ");
            return sb;
        }
        public static StringBuilder InsertEnd(this StringBuilder sb)
        {
            sb.AppendLine("end");
            return sb;
        }

        public static StringBuilder InsertClass(this StringBuilder sb, string luaName,bool isLocal, string basetype = "")
        {
            return sb = sb.AppendLine(string.Format("{2}{0} = LuaClass({1})", luaName, basetype, isLocal?"local ":string.Empty)).AppendLine("");
        }
        public static StringBuilder InsertTable(this StringBuilder sb, string tableName)
        {
            return sb = sb.AppendLine(string.Format("local {0} = {1}", tableName, "{}")).AppendLine("");
        }

        public static StringBuilder InsertFunction(this StringBuilder sb, string className, string funName, string statement, bool isStatic, params string[] args)
        {
            return InsertFunction(sb, className, funName, new[] { statement }, isStatic, args);
        }

        /** match args in statement */
        private static Regex regexArg = new Regex("\\$\\d+");
        private static Regex regexThis = new Regex("\\$this");

        public static StringBuilder InsertFunction(this StringBuilder sb, string className, string funName, string[] statement, bool isStatic, params string[] args)
        {
            sb.AppendLine(string.Format("--  /** function {0} */", funName));
            sb.AppendLine(string.Format("function {0}{1}{2}({3})", className, isStatic ? "." : ":", funName, string.Join(",", args)));
            for (int i = 0, c = statement.Length; i < c; i++)
            {
                if (regexThis.IsMatch(statement[i]))
                {
                    statement[i] = regexThis.Replace(statement[i], className);
                    //statement[i] = statement[i].Replace("$this", className);
                }
                int step = 1000;
                while (step > 0 && regexArg.IsMatch(statement[i]))
                {
                    step--;
                    var match = regexArg.Match(statement[i]);

                    string argIdxStr = match.Value.Trim('$');
                    int argIdx = 0;
                    int.TryParse(argIdxStr, out argIdx);
                    if (args.Length > argIdx)
                    {
                        statement[i] = statement[i].Remove(match.Index, match.Length);
                        statement[i] = statement[i].Insert(match.Index, args[argIdx]);
                    }

                }
                sb.InsertTab();
                sb.AppendLine(statement[i]);
            }
            sb.InsertEnd();
            sb.AppendLine("");
            return sb;
        }

        private static Regex regexNewLine = new Regex("\n\\w+\n");
        private static Regex regexN = new Regex("\n\\w+\n");
        public static StringBuilder InsertFunction(this StringBuilder sb, string className, string funName, StringBuilder statements, bool isStatic, params string[] args)
        {
            List<string> statementsLst = new List<string>();
            string statementsStr = statements.ToString();
            if (regexNewLine.IsMatch(statementsStr))
            {
                foreach (Match match in regexNewLine.Matches(statementsStr))
                {
                    string statement = match.Value;
                    if (regexN.IsMatch(statement))
                    {
                        statement = regexN.Replace(statement, "");
                    }
                    statementsLst.Add(statement);
                }
            }

            return sb.InsertFunction(className, funName, statementsLst.ToArray(), isStatic, args);
        }

        public static StringBuilder InsertField(this StringBuilder sb, string className, string fieldName, string initStatement, bool isStatic)
        {
            sb.AppendLine(string.Format("--  /** field {0} */", fieldName));
            sb =
                sb.Append("self.")
                    .Append(className)
                    .Append(isStatic ? "." : ":")
                    .Append(fieldName)
                    .InsertEqual()
                    .Append(initStatement);
            return sb;
        }

        public static StringBuilder InsertStatement(this StringBuilder sb, bool AutoLine, params string[] statements)
        {
            foreach (var statement in statements)
            {
                if (AutoLine)
                    sb.AppendLine(statement);
                else
                    sb.Append(statement);
            }
            return sb;
        }
    }
}