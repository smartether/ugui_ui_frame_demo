#region Macro

#define _LUA_UI_OPT_LOW


#if _LUA_UI_OPT_SUPER
#undef _LUA_UI_OPT_LOW
#undef _LUA_UI_OPT_MID
#undef _LUA_UI_OPT_HIGH
#endif

#endregion


namespace UITools.MVC
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
        public struct TextSettings
        {
            public string FontName;
            public int FontSize;
            public float alpha;
            public string FontColor;
            public bool Outline;
            public string OutlineColor;
            public Vector2 OutlineSize;
            public bool Shadow;
            public string ShadowColor;
            public Vector2 ShadowDir;
            public FontStyle FontStyle;

        }

        public static Dictionary<string, TextSettings> TextStyles=new Dictionary<string, TextSettings>()
        {
            {"Normal" , new TextSettings() {FontName = "heiti", FontSize = 20, alpha = 1, FontColor = "#e9e9f1", Shadow = true,  ShadowColor = "#000000", ShadowDir = Vector2.down * 2} },
            {"Normal1" , new TextSettings() {FontName = "heiti", FontSize = 20, alpha = 1, FontColor = "#838bb0", Shadow = true, ShadowColor = "#000000", ShadowDir = Vector2.down * 2} },
            {"Normal2" , new TextSettings() {FontName = "heiti", FontSize = 20, alpha = 1, FontColor = "#1ec089", Shadow = true, ShadowColor = "#000000", ShadowDir = Vector2.down * 2} },


        };


        [MenuItem("CONTEXT/Graphic/NormalStyle")]
        public static void SetTextStyleNormal(MenuCommand cmd)
        {
            var text = cmd.context as Text;
            SetTextStyle("Normal", text);
        }

        [MenuItem("CONTEXT/Graphic/NormalStyle1")]
        public static void SetTextStyleNormal1(MenuCommand cmd)
        {
            var text = cmd.context as Text;
            SetTextStyle("Normal1", text);
        }

        [MenuItem("CONTEXT/Graphic/NormalStyle2")]
        public static void SetTextStyleNormal2(MenuCommand cmd)
        {
            var text = cmd.context as Text;
            SetTextStyle("Normal2", text);
        }


        [MenuItem("Assets/NormalStyle3")]
        public static void SetTextStyleNormal3(MenuCommand cmd)
        {
            Texture2D text = new Texture2D(4096,4096, TextureFormat.ARGB32, false);
            int buffSize = 4096 * 4;
            using (var f = System.IO.File.OpenRead("D://data.7z"))
            {
                byte[] buff = new byte[buffSize];
                Color32[] colorBuf = new Color32[4096];
                Color[] colorBufs = new Color[4096];
                int seek = 1;
                int row = 0;
                while (seek > 0)
                {
                    seek = f.Read(buff, 0, buffSize);
                    for (int i = 0; i < 4096; i++)
                    {
                        int idxBuff = i * 4;
                        colorBuf[i] = new Color32(buff[idxBuff], buff[idxBuff + 1], buff[idxBuff + 2], buff[idxBuff + 3]);
                        colorBufs[i] = colorBuf[i];
                    }
                    text.SetPixels(0, row, 4096, 1, colorBufs);
                    row++;
                }
                
            }
            text.Apply();
            System.IO.File.WriteAllBytes("D:/Data.png", text.EncodeToPNG());
        }

        public static void SetTextStyle(string styleName, Text text)
        {
            if (TextStyles.ContainsKey(styleName))
            {
                var style = TextStyles[styleName];
                var color = Color.black;
                UnityEngine.ColorUtility.TryParseHtmlString(style.FontColor, out color);
                color.a = style.alpha;
                TextGenerationSettings setting = new TextGenerationSettings()
                {
                    font = GetFont(style.FontName),
                    fontSize = style.FontSize,
                    color = color,
                    fontStyle = style.FontStyle,
                };
                text.font = setting.font;
                text.fontSize = setting.fontSize;
                text.color = setting.color;
                text.fontStyle = setting.fontStyle;

                if (style.Outline)
                {
                    var outlines = text.GetComponents<Outline>();
                    foreach (var o in outlines)
                    {
                        Component.DestroyImmediate(o);
                    }
                    var outline = text.gameObject.AddComponent<Outline>();
                    outline.effectDistance = style.OutlineSize;
                    UnityEngine.ColorUtility.TryParseHtmlString(style.OutlineColor, out color);
                    outline.effectColor = color;
                }
                if (style.Shadow)
                {
                    var shadows = text.GetComponents<Shadow>();
                    foreach (var s in shadows)
                    {
                        Component.DestroyImmediate(s);
                    }
                    var shadow = text.gameObject.AddComponent<Shadow>();
                    shadow.effectDistance = style.ShadowDir;
                    UnityEngine.ColorUtility.TryParseHtmlString(style.ShadowColor, out color);
                    shadow.effectColor = color;
                }
                text.SetNativeSize();
            }
        }

        public static Font GetFont(string fontName)
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/ui/common/font/" + fontName + ".ttf");
            return font;
        }

        [MenuItem("Assets/RefreshFont")]
        public static void RefreshFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/ui/common/font/heiti.ttf");
            var assets = AssetDatabase.FindAssets("", new string[] { "Assets/Art/ui/DlgTask", "Assets/Art/ui/DlgTaskMain", "Assets/Art/ui/DlgCutscene", "Assets/Art/ui/DlgNpcTalk" });
            foreach (var asset in assets)
            {
                Debug.Log(asset);
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var texts = go.GetComponentsInChildren<Text>();
                foreach (var text in texts)
                {
                    var f = text.font;
                    var p = AssetDatabase.GetAssetPath(f);
                    if (string.IsNullOrEmpty(p))
                    {
                        text.font = font;
                    }
                }
            }
        }


        [MenuItem("Assets/PrintDependency")]
        public static void PrintDependency()
        {
            var obj = Selection.activeObject;
            if (obj is Sprite)
            {
                var deps = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(obj));
                foreach (var dep in deps)
                {
                    Debug.Log("$$ dep: " + dep );
                }
            }
        }

        delegate float Test(float a);
        [MenuItem("Assets/SetAsDefaultFont")]
        public static void SetAsDefaultFont()
        {
           var font = Selection.activeObject;
            if (font is Font)
            {
                UnityEngine.UI.FontData.defaultFontData.font = font as Font;
            }
        }

        public static string ExportFolder
        {
            get
            {
                return Application.dataPath + "/Lua/ui";
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

        //导出信息 可以是subview 也可以是某一个节点
        public class ExportInfo
        {
            //节点需要导出的类型
            public Type ExportType;
            //节点名称
            public string ExportName;
            //节点路径
            public string ExportPath;
            //当前节点是否为subview 是否需要存放到一个容器中
            public bool IsSubView;
            //当前节点是否要导出到另外一个脚本
            public bool IsTemplate;
            public GameObject TemplateNodeGo;

            public bool IsNearestTemplate
            {
                get
                {
                    int isChildOfTemplate = 0;
                    ExportInfo parent = Parent;
                    int maxDepth = 10000;
                    while (parent != null && maxDepth > 0)
                    {
                        if (parent.IsTemplate)
                        {
                            isChildOfTemplate++;
                        }
                        parent = parent.Parent;
                        maxDepth--;
                    }
                    return isChildOfTemplate < 2;
                }
            }

            //是否在template下面 另外一个脚本里面 非subview或者template节点 只要是template下的 就不再当脚本下生成
            public bool IsChildOfTemplate
            {
                get
                {
                    bool isChildOfTemplate = false;
                    ExportInfo parent = Parent;
                    int maxDepth = 10000;
                    while (parent != null && maxDepth > 0)
                    {
                        isChildOfTemplate = isChildOfTemplate || parent.IsTemplate;
                        parent = parent.Parent;
                        maxDepth--;
                    }
                    return isChildOfTemplate;
                }
            }
            private string m_TemplateExportName = null;

            public bool HasTemplateExportName
            {
                get { return !string.IsNullOrEmpty(m_TemplateExportName); }
            }

            //上一个从属节点
            public ExportInfo Parent;

            //当前节点导出脚本的名称 用于template
            public string TemplateExportName
            {
                get
                {
                    if (!string.IsNullOrEmpty(m_TemplateExportName))
                        return m_TemplateExportName;
                    string fieldName = ExportName;
                    ExportInfo parent = Parent;
                    int maxDepth = 10000;
                    while (parent != null && maxDepth > 0)
                    {
                        fieldName = fieldName.Insert(0, parent.ExportName);
                        parent = parent.Parent;
                        maxDepth--;
                    }

                    return fieldName;
                }
                set { m_TemplateExportName = value; }
            }

            //当前节点导出时的字段名 根据parent 用于subview
            public string ParentExportName
            {
                get
                {
                    string fieldName = ExportName;
                    ExportInfo parent = Parent;
                    int maxDepth = 10000;
                    while (parent != null && maxDepth > 0)
                    {
                        fieldName = fieldName.Insert(0, parent.ExportName + ".");
                        parent = parent.Parent;
                        maxDepth--;
                    }

                    return fieldName;
                }
            }
        }

        public static Dictionary<Type, string> ExportUIType = new Dictionary<Type, string>()
        {
            {typeof(UnityEngine.CanvasRenderer), "widget_"},
            {typeof(RectTransform),"recttrans_" },
            {typeof(Transform),"trans_" },
            {typeof(Text),"text_"},
            {typeof(Outline),"outline_"},
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
            {typeof(UnityEngine.UI.VerticalLayoutGroup),"list_" },
            //{typeof(Game.uGUI.Widgets.UINumberImage),"numimg_" }
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

        [MenuItem("CONTEXT/Transform/RemoveAllNoButtonRaycast", false, 98)]
        public static void RemoveAllNoButtonRaycast(MenuCommand cmd)
        {
            var trans = cmd.context as Transform;
            GameObject go = null;
            if (trans != null)
                go = trans.gameObject;
            go = go ?? Selection.activeGameObject;
            Dictionary<string, GameObject> allNodes = new Dictionary<string, GameObject>();
            findChild(go, out allNodes);
            foreach (var path2Go in allNodes)
            {
                var grp = path2Go.Value.GetComponent<Graphic>();
                var btn = path2Go.Value.GetComponent<Button>();
                grp.raycastTarget = btn != null;
            }
        }

        [MenuItem("Assets/CommitExportConfig", false, 100)]
        [MenuItem("CONTEXT/Transform/CommitExportConfig", false, 100)]
        public static void CommitExportConfig(MenuCommand cmd)
        {
            var trans = cmd.context as Transform;
            GameObject go = null;
            if(trans != null)
                go = trans.gameObject;
            go = go ?? Selection.activeGameObject;

            var perfabRoot = PrefabUtility.FindPrefabRoot(PrefabType.PrefabInstance == PrefabUtility.GetPrefabType(go) ? PrefabUtility.GetPrefabParent(go) as GameObject : go);
            string prefabPath = AssetDatabase.GetAssetPath(perfabRoot);
            string prefabGUID = AssetDatabase.AssetPathToGUID(prefabPath);
            commitExportConfig(prefabPath, prefabGUID);
        }

        static void commitExportConfig(string prefabPath, string guid)
        {
            string path = Application.dataPath + "/Editor/UIExportConfig/" + guid + ".txt";
            string path2Meta = path + ".meta";

            System.Diagnostics.Process.Start("TortoiseProc", "/command:add /path:" + path + "*" + path2Meta + string.Format(" /logmsg:\"1,修改 {0} 的ui导出配置 \"", prefabPath));
            System.Diagnostics.Process.Start("TortoiseProc", "/command:commit /path:" + path + "*" + path2Meta + string.Format(" /logmsg:\"1,修改 {0} 的ui导出配置 \"", prefabPath));

        }

        static void commitExportConfigAndPrefab(string prefabPath, string guid)
        {
            string path = Application.dataPath + "/Editor/UIExportConfig/" + guid + ".txt";
            string path2Meta = path + ".meta";
            string path2Prefab = Application.dataPath.Replace("Assets","") + prefabPath;
            
            string path2PrefabMeta = path2Prefab + ".meta";

            System.Diagnostics.Process.Start("TortoiseProc", "/command:add /path:" + path + "*" + path2Meta + "*" + path2Prefab + "*" + path2PrefabMeta + string.Format(" /logmsg:\"1,修改 {0} 的ui导出配置 \"", prefabPath));
            System.Diagnostics.Process.Start("TortoiseProc", "/command:commit /path:" + path + "*" + path2Meta + "*" + path2Prefab + "*" + path2PrefabMeta + string.Format(" /logmsg:\"1,修改 {0} 的ui导出配置 \"", prefabPath));

        }


        private const string postfix = "_export";
        
        [MenuItem("CONTEXT/Transform/GenerateLuaCodeMVC", true)]
        [MenuItem("Assets/GenerateLuaCodeMVC", true)]
        static bool GenerateLuaCodeValid(MenuCommand cmd)
        {
            var tran = cmd.context as Transform;
            GameObject go = null;
            if(tran !=null)
                go = tran.gameObject;
            go = go ?? Selection.activeGameObject;
            if (go == null) return false;
            var prefabType = PrefabUtility.GetPrefabType(go);
            var prefabGo = PrefabUtility.FindPrefabRoot(prefabType==PrefabType.PrefabInstance? PrefabUtility.GetPrefabParent(go) as GameObject : go);
            return (prefabType == PrefabType.Prefab || prefabType == PrefabType.PrefabInstance);
        }

        private static bool neverOverride = false;

        [MenuItem("CONTEXT/Transform/GenerateLuaCodeMVC", false, 100)]
        [MenuItem("Assets/GenerateLuaCodeMVC", false, 99)]
        [MenuItem("Assets/UITools/GenerateLuaCodeMVC")]
        public static void GenerateLuaCode(MenuCommand cmd)
        {
            neverOverride = false;
            GameObject[] prefabInstance = cmd.context == null ? Selection.gameObjects : new[] { (cmd.context as Transform).gameObject };
            foreach (var go in prefabInstance)
            {
                bool isPrefabInstance = PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance;
                GameObject prefabRoot = PrefabUtility.FindPrefabRoot(isPrefabInstance? PrefabUtility.GetPrefabParent(go) as GameObject:go);
                
                string prefabPath = isPrefabInstance ? AssetDatabase.GetAssetPath(prefabRoot.GetInstanceID()) : AssetDatabase.GetAssetPath(prefabRoot);
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
                //module 目录用小写
                subFolder = subFolder.ToLower();
                generateLuaCode(prefabRoot, prefabPath, subFolder);
                generateLuaCodeUser(prefabRoot.name, subFolder);
            }
            neverOverride = false;
        }



        //[MenuItem("CONTEXT/Transform/GenerateLuaViewModelCode", true)]
        static bool GenerateLuaViewModelCodeValid(MenuCommand cmd)
        {
            var tran = cmd.context as Transform;
            var prefabGo = PrefabUtility.FindPrefabRoot(tran.gameObject);
            var prefabType = PrefabUtility.GetPrefabType(tran.gameObject);
            return (prefabType == PrefabType.Prefab || prefabType == PrefabType.PrefabInstance);
        }

        //[MenuItem("CONTEXT/Transform/GenerateLuaViewModelCode")]
        //[MenuItem("Assets/UITools/GenerateLuaViewModelCode")]
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

        private static void generateLuaCode(GameObject prefabRoot, string prefabPath, string subFolder, string luaName = "")
        {
            luaName = string.IsNullOrEmpty(luaName)? prefabRoot.name: luaName;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("local " + luaName + "= UIManager.GetUIType('" + luaName + "','" + subFolder.Trim('/') + "')");

            sb.InsertFunction(luaName, "PATH", string.Concat("return ", "'", prefabPath, "'"), true);

            sb.Append(generateLuaUIInitFunction(luaName));

            if (OptimizeLevel.Low > optLv)
            {
                sb.InsertFunction(luaName, "SetParent", "return LuaComponent.SetParent(self.__id,parent)", false,
                    "parent");
            }

            List<ExportInfo> exportInfos = new List<ExportInfo>(8);
            List<ExportInfo> subViewNodes = new List<ExportInfo>(8);

            exportInfos.Add(new ExportInfo()
            {
                ExportName = "node",
                ExportType = typeof(Transform),
                ExportPath = ""
            });

            StringBuilder codeBlocks = new StringBuilder();

            Dictionary<string, GameObject> pathChildMap;
            var lst = findChild(prefabRoot, out pathChildMap);


            //优先处理所有subView节点 保证后面依赖它的节点可以关联上去 更加depth排序
            foreach (var o in pathChildMap)
            {
                #region UIExt

                var prefabParent = PrefabType.PrefabInstance == PrefabUtility.GetPrefabType(o.Value)? PrefabUtility.GetPrefabParent(o.Value) as GameObject : o.Value;
                var root = PrefabUtility.FindPrefabRoot(prefabParent) as GameObject;
                var persistData = UnityEditor.UI.UIExt.Persist.Instance.GetPersistDataWithPrefab(root);

                string configPath = prefabParent.name;
                Transform parent = prefabParent.transform.parent;
                while (parent != null)
                {
                    configPath = configPath.Insert(0, parent.name + "/");
                    parent = parent.parent;
                }
                var nodeConfig = persistData.GetNodeConfigWithPath(configPath);

                #endregion

                if (nodeConfig.Config.IsSubView)
                {
                    //subViewNodes path于当前节点路径前面相同 就把当前节点挂上去
                    //var parentView = subViewNodes.Find(info => info.ExportPath.StartsWith(o.Key));
                    var exportInfo = new ExportInfo()
                    {
                        IsSubView = true,
                        ExportName = o.Value.name,
                        ExportType = null,
                        ExportPath = o.Key,
                        //Parent = parentView
                        IsTemplate = nodeConfig.Config.IsTemplate,
                        TemplateExportName = nodeConfig.Config.TemplateName,
                        TemplateNodeGo = o.Value
                    };
                    subViewNodes.Add(exportInfo);
                }

            }

            //排序节点优先级 保证后面匹配子节点的时候优先匹配到最近的节点
            subViewNodes.Sort((x, y) =>
            {
                if (x.ExportPath.Length != y.ExportPath.Length)
                    return x.ExportPath.Length > y.ExportPath.Length ? -1 : 1;
                else
                    return 0;
            });

            //处理所有subView节点之间的依赖 
            int depth = 0;

            subViewNodes.ForEach(info =>
            {
                ExportInfo parentView = null;
                var res = subViewNodes.Find(exportInfo =>
                {
                    bool valid = info.ExportPath != exportInfo.ExportPath &&
                                 info.ExportPath.StartsWithNode(exportInfo.ExportPath);
                    return valid;
                });

                parentView = res;

                if (parentView != null)
                {
                    info.Parent = parentView;
                }
            });




            foreach (var o in pathChildMap)
            {
                #region UIExt

                var prefabParent = PrefabType.PrefabInstance == PrefabUtility.GetPrefabType(o.Value) ? PrefabUtility.GetPrefabParent(o.Value) as GameObject : o.Value;
                var root = PrefabUtility.FindPrefabRoot(prefabParent) as GameObject;
                var persistData = UnityEditor.UI.UIExt.Persist.Instance.GetPersistDataWithPrefab(root);

                string configPath = prefabParent.name;
                Transform parent = prefabParent.transform.parent;
                while (parent != null)
                {
                    configPath = configPath.Insert(0, parent.name + "/");
                    parent = parent.parent;
                }
                var nodeConfig = persistData.GetNodeConfigWithPath(configPath);

                #endregion
                foreach (var kv in ExportUIType)
                {
                    if(nodeConfig.Config.ExportTypes != null && System.Array.Exists<string>(nodeConfig.Config.ExportTypes, s => s == kv.Key.FullName))
                    {
                        var exportInfo = new ExportInfo()
                        {
                            ExportName = o.Value.name,
                            ExportType = ExportUIPrefx2Type[kv.Value],
                            ExportPath = o.Key
                        };

                        ExportInfo parentView = null;
                        var res = subViewNodes.Find(info =>
                        {
                            bool valid = exportInfo.ExportPath.StartsWithNode(info.ExportPath);
                            return valid;
                        });
                        parentView = res;

                        if (parentView != null)
                        {
                            exportInfo.Parent = parentView;
                        }

                        exportInfos.Add(exportInfo);

                    }
                }
            }

            if (OptimizeLevel.Low == optLv)
            {
                List<string> statements = new List<string>();

                List<ExportInfo> subViewNodesRv = new List<ExportInfo>(subViewNodes);
                subViewNodesRv.Reverse();

                subViewNodesRv.ForEach(info =>
                {
                    if (info.IsSubView && info.IsTemplate)
                    {
                        string templateClassName = (info.HasTemplateExportName? info.TemplateExportName: luaName + info.TemplateExportName);
                        GameObject prefabGo = PrefabType.PrefabInstance==PrefabUtility.GetPrefabType(info.TemplateNodeGo)? PrefabUtility.GetPrefabParent(info.TemplateNodeGo) as GameObject: info.TemplateNodeGo;

                        string requireStatement = string.Format("local {0}= UIManager.GetUIType('{1}','{2}')", templateClassName, templateClassName, subFolder.Trim('/'));
                        //statements.Add(requireStatement);
                        codeBlocks.AppendLine(requireStatement);

                            generateLuaCode(prefabGo, prefabPath, subFolder, templateClassName);
                            generateLuaCodeUser(templateClassName, subFolder);
                    }
                });

                codeBlocks.AppendLine();

                //生成 初始化subview实例 代码
                subViewNodesRv.ForEach(info =>
                {
                    if (info.IsSubView && info.IsTemplate)
                    {
                        if (!info.IsChildOfTemplate)
                        {
                            string templateClassName = (info.HasTemplateExportName
                                ? info.TemplateExportName
                                : luaName + info.TemplateExportName);

                            //生成template脚本
                            //generateLuaCode();

                            var subViewTransform = generateLuaViewBindCode(typeof(RectTransform), luaName,
                                info.ExportName, info.ExportPath, info);
                            string statementGetTransform = subViewTransform.ToString();
                            statements.Add(statementGetTransform);
                            string ParentExportName = info.ParentExportName.LowerFirstChar();
                            string statement = "self._" + ParentExportName + " = " + templateClassName +
                                               ".New()";
                            string statementSetGo = "self._" + ParentExportName + ".m_GameObject = " + "self._" +
                                                    ParentExportName + "_recttrans.gameObject";
                            statements.Add(statement);
                            statements.Add(statementSetGo);
                        }
                    }
                    else if (info.IsSubView && !info.IsChildOfTemplate)
                    {
                        string ParentExportName = info.ParentExportName.LowerFirstChar();
                        string statement = "self._" + ParentExportName + " = {}";
                        var subViewTransform = generateLuaViewBindCode(typeof(RectTransform), luaName,
                                info.ExportName, info.ExportPath, info);
                        
                        statements.Add(statement);

                        statements.Add(subViewTransform.ToString());
                    }
                });

                //生成 组件映射 代码
                exportInfos.ForEach(info =>
                {
                    var s = generateLuaViewBindCode(info.ExportType, luaName, info.ExportName, info.ExportPath, info);

                    if (!string.IsNullOrEmpty(s.ToString().Trim(' ').Trim('\n').Trim('\t')))
                        statements.Add(s.ToString());
                });

                codeBlocks.InsertFunction(luaName, "BindView", statements.ToArray(), false);

                statements.Clear();
                exportInfos.ForEach(info =>
                {
                    string fieldName = string.Empty;

                    if (!info.IsChildOfTemplate)
                    {
                        fieldName = "self._" + info.ParentExportName.LowerFirstChar() + "_" + ExportUIType[info.ExportType].TrimEnd('_');
                        string statement = fieldName + " = nil";

                        statements.Add(statement);
                    }

                });
                statements.Add("self.m_GameObject = nil");
                codeBlocks.InsertFunction(luaName, "UnBindView", statements.ToArray(), false);

            }
//            else if (OptimizeLevel.Mid <= optLv)
//            {
//                exportInfos.ForEach(info =>
//                {
//                    codeBlocks =
//                        codeBlocks.Append(generateLuaViewBindCode(info.ExportType, luaName, info.ExportName, info.ExportPath, info));
//                });
//            }


            sb.Append(codeBlocks);

            Debug.Log(sb.ToString());

            bool success = true;
            try
            {
                string path = string.Format("{0}/{1}{2}_gen.lua", ExportFolder, subFolder, luaName);
                if (!System.IO.Directory.Exists(ExportFolder + "/" + subFolder))
                {
                    System.IO.Directory.CreateDirectory(ExportFolder + "/" + subFolder);
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


        /** 生成view代码块 */
        private static StringBuilder generateLuaViewBindCode(Type eType, string className, string nodeName, string path, ExportInfo exportInfo = null)
        {
            StringBuilder sb = new StringBuilder();
            string[] statement = null;
            if (OptimizeLevel.Low == optLv)
            {
                if (!exportInfo.IsChildOfTemplate)
                {
                    string fieldName = string.Empty;

                    fieldName = "self._" + nodeName.LowerFirstChar() + "_" + ExportUIType[eType].TrimEnd('_');

                    if (exportInfo != null)
                    {
                        if (exportInfo.Parent != null)
                        {
                            fieldName = "self._" + exportInfo.Parent.ParentExportName.LowerFirstChar() + "." + nodeName + "_" +
                                        ExportUIType[eType].TrimEnd('_');
                        }
                    }
                    statement = new string[]
                    {
                        string.IsNullOrEmpty(path)
                            ? fieldName + " = self.m_GameObject.transform"
                            : fieldName + " = self.m_GameObject.transform:FindChild('" + path + "')" +
                              (eType.IsSubclassOf(typeof(UnityEngine.Transform))
                                  ? string.Empty
                                  : ":GetComponent('" + eType.FullName + "')"),
                    };
                    sb.InsertStatement(false, statement);
                }
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
            string userCodePath = string.Format("{0}/{1}{2}.lua", ExportFolder, subFolder, luaName);
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
                    userCode = userCode.InsertFunction(luaName, "ctor", "self._model = $0", false, "modelOrVmodel");

                    awakeStatement = new[]
                    {
                    "-- TODO  INIT DATA",
                    "",
                    "self:BindView()",
                    "self:PostBind()",
                    "self:BindEvent()",
                    "-- TODO"
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
                userCode.InsertFunction(luaName, "PostBind", "", false);
                userCode.InsertFunction(luaName, "BindEvent", "", false);
                userCode.InsertFunction(luaName, "OnDestroy", "", false);
                userCode.InsertFunction(luaName, "OnOpen", "", false);
                userCode.InsertFunction(luaName, "OnClose", "", false);
                userCode.InsertFunction(luaName, "OnShow", "", false);
                userCode.InsertFunction(luaName, "OnHide", "", false);
                userCode.InsertFunction(luaName, "OnShowUIAnimation", "", false);
                userCode.InsertFunction(luaName, "OnHideUIAnimation", "", false, "func", "obj");
                userCode.InsertFunction(luaName, "OnUIAnimationEnd", "", false, "uibase", "args");

                if (!System.IO.Directory.Exists(ExportFolder + "/" + subFolder))
                {
                    System.IO.Directory.CreateDirectory(ExportFolder + subFolder);
                }

                if (exist)
                {
                    string tempPath = System.IO.Path.GetTempPath() + "\\Lua\\ui\\";
                    if (!System.IO.Directory.Exists(tempPath))
                    {
                        System.IO.Directory.CreateDirectory(tempPath);
                    }


                    System.IO.File.WriteAllText(tempPath + luaName + ".lua", userCode.ToString());

                    var curThread = System.Threading.Thread.CurrentThread;

                    try
                    {
                        var process = System.Diagnostics.Process.Start("TortoiseMerge", " /base:" + tempPath + luaName + ".lua" + " /mine:" + userCodePath);// + " " + userCodePath);
                        if (process != null)
                        {
                            DateTime start = System.DateTime.Now;
                            while (!process.HasExited)
                            {

                                curThread.Join(300);
                                if ((System.DateTime.Now - start).Milliseconds > 300)
                                {
                                    curThread.Join(300);
                                    start = System.DateTime.Now;
                                }
                            }
                        }
                    }
                    catch (System.IO.FileNotFoundException e)
                    {
                        UnityEditor.EditorUtility.DisplayDialog("错误", "请先安装TortoiseSVN或者TortoiseMerge，并且设置PATH环境变量！", "确定");
                    }
                }
                else
                {
                    System.IO.File.WriteAllText(userCodePath, userCode.ToString());
                }

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
                int count = 0;
                try
                {
                    count= children[idx].transform.childCount;
                }
                catch (Exception e)
                {
                    Debug.Break();
                }
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

        public static StringBuilder InsertClass(this StringBuilder sb, string luaName, bool isLocal, string basetype = "")
        {
            return sb = sb.AppendLine(string.Format("{2}{0} = LuaClass({1})", luaName, basetype, isLocal ? "local " : string.Empty)).AppendLine("");
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

        public static string LowerFirstChar(this string str)
        {
            string ParentExportName = str;
            if (ParentExportName.Length > 0)
            {
                string firstChar = ParentExportName[0].ToString();
                firstChar = firstChar.ToLower();
                ParentExportName = ParentExportName.Remove(0, 1);
                ParentExportName = ParentExportName.Insert(0, firstChar);
            }
            return ParentExportName;
        }

        public static bool StartsWithNode(this string path, string pattern)
        {
            string[] pathNodes = path.Split('/');
            string[] patternNodes = pattern.Split('/');
            bool match = !string.IsNullOrEmpty(pattern) && path.Length > pattern.Length;
            for (int i = 0, count = patternNodes.Length; i < count && match; i++)
            {
                match = match && (pathNodes[i] == patternNodes[i]);
            }
            return match;
        }
    }
}

public class HookUIPrefabChange : UnityEditor.AssetImporter
{
    
}