using System.Collections;
using System.Collections.Generic;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.UI;

public static class LuaCodeTemplate
{

    public class BindDataTemplate
    {
        /** 需要绑定的组件属性名称 */
        public string PropertyName { get; set; }
        /** 生成的绑定函数后缀 */
        public string FunctionNamePostfix { get; set; }
        /** 生成的绑定函数参数列表 */
        public string[] CodeBlockParams { get; set; }
        /** 生成的函数体模板 */
        /**
         *  模板中的变量
         *  $nodeName 组件节点名称
         *  $fix 函数后缀
         */
        public string[] CodeBlockTemplate { get; set; }
    }

    public static List<BindDataTemplate> CommonBindDataTemplate = new List<BindDataTemplate>()
    {
                      new BindDataTemplate()
                    {
                        PropertyName   = "gameObject",
                        FunctionNamePostfix = "gameObject",
                        CodeBlockParams =  new []{"active"},
                        CodeBlockTemplate = new string[]
                        {
                            "self.m_GameObject:SetActive($0)"
                        }
                    },
    };

    //
    public static Dictionary<System.Type, Dictionary<string, BindDataTemplate>> UIBindDataTemplateMap = new Dictionary<System.Type, Dictionary<string, BindDataTemplate>>(){

           {typeof(UnityEngine.CanvasRenderer), new Dictionary<string, BindDataTemplate>()
            {
                {"widget", new BindDataTemplate()
                {
                        PropertyName   = "widget",
                        FunctionNamePostfix = "widget",
                        CodeBlockParams =  new []{"active"},
                        CodeBlockTemplate = new string[]
                        {
                            "if(string.match(tostring($0),'Subject') or string.match(tostring($0),'Observable')) then",
                            "local dispose = $0:subscribe(function(v)",
                            "self.m_$nodeName.gameObject:SetActive(v)",
                            "end)",
                            "self:PushSub(dispose)",
                            "else",
                            "self.m_$nodeName.gameObject:SetActive($0)",
                            "end",
                        }
                }},
            } },

            {typeof(UnityEngine.Transform), new Dictionary<string, BindDataTemplate>()
            {
                {"transform", new BindDataTemplate()
                {
                        PropertyName   = "gameObject",
                        FunctionNamePostfix = "gameObject",
                        CodeBlockParams =  new []{"active"},
                        CodeBlockTemplate = new string[]
                        {
                            "if(string.match(tostring($0),'Subject') or string.match(tostring($0),'Observable')) then",
                            "local dispose = $0:subscribe(function(v)",
                            "self.m_$nodeName.gameObject:SetActive(v)",
                            "end)",
                            "self:PushSub(dispose)",
                            "else",
                            "self.m_$nodeName.gameObject:SetActive($0)",
                            "end",
                        }
                }},
            } },


            {typeof(UnityEngine.RectTransform), new Dictionary<string, BindDataTemplate>()
            {
                {"transform", new BindDataTemplate()
                {
                        PropertyName   = "gameObject",
                        FunctionNamePostfix = "gameObject",
                        CodeBlockParams =  new []{"active"},
                        CodeBlockTemplate = new string[]
                        {
                            "if(string.match(tostring($0),'Subject') or string.match(tostring($0),'Observable')) then",
                            "local dispose = $0:subscribe(function(v)",
                            "self.m_$nodeName.gameObject:SetActive(v)",
                            "end)",
                            "self:PushSub(dispose)",
                            "else",
                            "self.m_$nodeName.gameObject:SetActive($0)",
                            "end",
                        }
                }},
            } },


            {typeof(UnityEngine.UI.Text), new Dictionary<string,BindDataTemplate>(){
                    {"text", new BindDataTemplate()
                    {
                        PropertyName   = "text",
                        FunctionNamePostfix = "text",
                        CodeBlockParams = new []{"s"},
                        CodeBlockTemplate = new []{
                            "if(string.match(tostring($0),'Subject') or string.match(tostring($0),'Observable')) then",
                            "local dispose = $0:subscribe(function(v)",
                            "if(self.m_$nodeName.$fix ~= v) then ",
                            "self.m_$nodeName.$fix = v",
                            "end",
                            "end)",
                            "self:PushSub(dispose)",
                            "else",
                            "self.m_$nodeName.$fix = $0",
                            "end",
                      },
                    }},

                }
            },

            {typeof(UnityEngine.UI.Image), new Dictionary<string,BindDataTemplate>(){
                    {"sprite",new BindDataTemplate()
                    {
                        PropertyName   = "sprite",
                        FunctionNamePostfix = "sprite",
                        CodeBlockParams = new string[] {"spritePath"},
                        CodeBlockTemplate = new string[]
                        {
                            "if(string.match(tostring($0),'Subject') or string.match(tostring($0),'Observable')) then",
                            "local dispose = $0:subscribe(function(v)",
                            "self.m_$nodeName.enabled = (v ~=nil)",
                            "if(v==nil) then return end",
                            "LuaComponent.LoadSprite(v, function(sprite)",
                            "self.m_$nodeName.sprite = sprite",
                            "end)",
                            "end)",
                            "self:PushSub(dispose)",
                            "else",
                            "self.m_$nodeName.enabled = (v ~=nil)",
                            "if(v==nil) then return end",
                            "LuaComponent.LoadSprite($0, function(sprite)",
                            "self.m_$nodeName.sprite = sprite",
                            "end)",
                            "end",
                        }
                    }},
                {"fillAmount",new BindDataTemplate()
                {
                   PropertyName = "fillAmount",
                   FunctionNamePostfix = "fillAmount",
                   CodeBlockParams = new string[] {"f"},
                   CodeBlockTemplate = new string[]
                   {
                       "local dispose = $0:subscribe(function(v)",
                       "self.m_$nodeName.fillAmount = v",
                       "end)",
                       "self:PushSub(dispose)"
                   }
                }},
                {"color",new BindDataTemplate()
                {
                   PropertyName = "color",
                   FunctionNamePostfix = "color",
                   CodeBlockParams = new string[] {"color"},
                   CodeBlockTemplate = new string[]
                   {
                       "local dispose = $0:subscribe(function(v)",
                       "self.m_$nodeName.color = v",
                       "end)",
                       "self:PushSub(dispose)"
                   }
                }},
                }
            },

            {typeof(UnityEngine.UI.RawImage), new Dictionary<string,BindDataTemplate>(){
                    {"sprite",new BindDataTemplate()
                    {
                        PropertyName   = "sprite",
                        FunctionNamePostfix = "sprite",
                        CodeBlockParams = new string[] {"spritePath"},
                        CodeBlockTemplate = new string[]
                        {
                            "if(string.match(tostring($0),'Subject') or string.match(tostring($0),'Observable')) then",
                            "local dispose = $0:subscribe(function(v)",
                            "LuaComponent.LoadTexture(v, function(tex)",
                            "self.m_$nodeName.texture = tex",
                            "end)",
                            "end)",
                            "self:PushSub(dispose)",
                            "else",
                            "LuaComponent.LoadSprite($0, function(tex)",
                            "self.m_$nodeName.texture = tex",
                            "end)",
                            "end",
                        }
                    }},

                }
            },

            {typeof(UnityEngine.UI.InputField), new Dictionary<string,BindDataTemplate>(){
                    {"text", new BindDataTemplate()
                        {
                        PropertyName   = "text",
                        FunctionNamePostfix = "text",
                        CodeBlockParams = new []{"s"},
                        CodeBlockTemplate = new []{
                            "if(string.match(tostring($0), 'Subject') or string.match(tostring($0),'Observable')) then",
                            "local dispose = $0:subscribe(function(v)",
                        "if(self.m_$nodeName.$fix ~= v) then ",
                        "self.m_$nodeName.$fix = v",
                        "end",
                        "end)",
                        "self:PushSub(dispose)",
                        "else",
                        "self.m_$nodeName.$fix = $0",
                        "end",
                       },
                    }
                },
                }
            },

            {typeof(UnityEngine.UI.Button), new Dictionary<string,BindDataTemplate>(){
                    {"sprite",new BindDataTemplate()
                    {
                        PropertyName   = "sprite",
                        FunctionNamePostfix = "sprite",
                        CodeBlockParams = new string[] {"spritePath"},
                        CodeBlockTemplate = new string[]
                        {
                            "if(string.match(tostring($0), 'Subject') or string.match(tostring($0),'Observable')) then",
                            "local dispose = $0:subscribe(function(v)",
                            "LuaComponent.LoadSprite(v, function(sprite)",
                            "self.m_$nodeName.image.sprite = sprite",
                            "end)",
                            "end)",
                            "self:PushSub(dispose)",
                            "else",
                            "LuaComponent.LoadSprite($0, function(sprite)",
                            "self.m_$nodeName.image.sprite = sprite",
                            "end)",
                            "end",
                        }
                    }},
                {
                    "interactable", new BindDataTemplate()
                    {
                        PropertyName = "active",
                        FunctionNamePostfix = "active",
                        CodeBlockParams = new string[] {"b"},
                        CodeBlockTemplate = new string[]
                        {
                            "local dispose = $0:subscribe(function(v)",
                            "self.m_$nodeName.interactable = v",
                            "end)",
                            "self:PushSub(dispose)",
                        }
                    }
                },

                {"color",new BindDataTemplate()
                {
                   PropertyName = "color",
                   FunctionNamePostfix = "color",
                   CodeBlockParams = new string[] {"color"},
                   CodeBlockTemplate = new string[]
                   {
                       "local dispose = $0:subscribe(function(v)",
                       "self.m_$nodeName.color = v",
                       "end)",
                       "self:PushSub(dispose)"
                   }
                }},
                }},

            {typeof(UnityEngine.UI.Dropdown), new Dictionary<string, BindDataTemplate>()
            {
                {
                    "Options",new BindDataTemplate()
                    {
                        PropertyName = "Options",
                        FunctionNamePostfix = "optionsData",
                        CodeBlockParams = new string[] { "options", "select"},
                        CodeBlockTemplate = new []
                        {
                            //"for i,v in ipairs(options) do","self.m_dropDown_Dropdown.options:Add(v)",
                            //"end",
                            //"self.m_dropDown_Dropdown:RefreshShownValue()",

                            "local dispose1 = options:ObserveAdd(function(idx, item)",
                            "  self.m_$nodeName.options:Add(item)",
                            "  self.m_$nodeName:RefreshShownValue()",
                            "end)",
                            "local dispose2 = options:ObserveRemove(function(idx, item)",
                            "  self.m_$nodeName.options:RemoveAt(idx - 1)",
                            "  self.m_$nodeName:RefreshShownValue()",
                            "end)" ,
                            "self:PushSub(dispose1)",
                            "self:PushSub(dispose2)",
                            "if($1 ~= nil) then",
                            "$1:subscribe(function(i)",
                            "if(self.m_$nodeName.value ~= i) then",
                            "self.m_$nodeName.value = i",
                            "end",
                            "end)",
                            "end",
                        }
                    }
                },
            }},
            {typeof(UnityEngine.UI.Toggle), new Dictionary<string, BindDataTemplate>()
            {
                {
                    "isOn",new BindDataTemplate()
                    {
                        PropertyName = "isOn",
                        FunctionNamePostfix = "isOn",
                        CodeBlockParams = new string[] { "isOn"},
                        CodeBlockTemplate = new []
                        {
                         "if(string.match(tostring($0), 'Subject') or string.match(tostring($0),'Observable')) then",
                         "local dispose = $0:subscribe(function(v)",
                        "if(self.m_$nodeName.$fix ~= v) then ",
                        "self.m_$nodeName.$fix = v",
                        "end",
                        "end)",
                        "self:PushSub(dispose)",
                        "else",
                        "self.m_$nodeName.$fix = $0",
                        "end",
                       },
                    }
                },
            }},
        {typeof(UnityEngine.UI.GridLayoutGroup),new Dictionary<string, BindDataTemplate>(){
          {"", new BindDataTemplate()
          {
              FunctionNamePostfix = "_items",
              CodeBlockParams = new string[] {"collection", "onAdded", "onRemoved"},
              CodeBlockTemplate = new string[]
              {
                  "self.m_$nodeName_data = {}",
                  "local dispose1 = collection:ObserveAdd(function(idx, item)",
                  "self:CreateTemplate(item.Template, function(child)",
                  "self.m_$nodeName_data[item] = child",
                  "child:SetParent(self.m_$nodeName.transform)",
                  "if(onAdded ~= nil) then onAdded() end",
                  "end, item)",
                  "end)",
                  "local dispose2 = collection:ObserveRemove(function(idx, item)",
                  "self.m_$nodeName_data[item]:DestroyUI()",
                  "self.m_$nodeName_data[item] = nil",
                  "if(onRemoved ~= nil) then onRemoved() end",
                  "end)",
                  "self:PushSub(dispose1)",
                  "self:PushSub(dispose2)",
              }
          }},

        }},
          {typeof(UnityEngine.UI.VerticalLayoutGroup),new Dictionary<string, BindDataTemplate>(){
          {"vlayout", new BindDataTemplate()
          {
              FunctionNamePostfix = "_items",
              CodeBlockParams = new string[] {"collection", "onAdded", "onRemoved"},
              CodeBlockTemplate = new string[]
              {
                  "self.m_$nodeName_data = {}",
                  "local dispose1 = collection:ObserveAdd(function(idx, item)",
                  "self:CreateTemplate(item.Template, function(child)",
                  "self.m_$nodeName_data[item] = child",
                  "child:SetParent(self.m_$nodeName.transform)",
                  "if(onAdded ~= nil) then onAdded() end",
                  "end, item)",
                  "end)",
                  "local dispose2 = collection:ObserveRemove(function(idx, item)",
                  "self.m_$nodeName_data[item]:DestroyUI()",
                  "self.m_$nodeName_data[item] = nil",
                  "if(onRemoved ~= nil) then onRemoved() end",
                  "end)",
                  "self:PushSub(dispose1)",
                  "self:PushSub(dispose2)",
              }
          }},

        }},
           {typeof(UnityEngine.UI.HorizontalLayoutGroup),new Dictionary<string, BindDataTemplate>(){
          {"hlayout", new BindDataTemplate()
          {
              FunctionNamePostfix = "_items",
              CodeBlockParams = new string[] {"collection", "onAdded", "onRemoved"},
              CodeBlockTemplate = new string[]
              {
                  "self.m_$nodeName_data = {}",
                  "local dispose1 = collection:ObserveAdd(function(idx, item)",
                  "self:CreateTemplate(item.Template, function(child)",
                  "self.m_$nodeName_data[item] = child",
                  "child:SetParent(self.m_$nodeName.transform)",
                  "if(onAdded ~= nil) then onAdded() end",
                  "end, item)",
                  "end)",
                  "local dispose2 = collection:ObserveRemove(function(idx, item)",
                  "self.m_$nodeName_data[item]:DestroyUI()",
                  "self.m_$nodeName_data[item] = nil",
                  "if(onRemoved ~= nil) then onRemoved() end",
                  "end)",
                  "self:PushSub(dispose1)",
                  "self:PushSub(dispose2)",
              }
          }},

        }},

        };



    //导出XXX组件的XXX属性绑定语法糖 函数名后缀text
    public static Dictionary<System.Type, Dictionary<string, string>> UIBindEventMap = new Dictionary<System.Type, Dictionary<string, string>>(){

           /*{ typeof(UnityEngine.UI.Text), new Dictionary<string,string>(){
                    {"onValueChanged:AddListener", "changed"}
                }
            }, 
           
            {typeof(UnityEngine.UI.Image), new Dictionary<string,string>(){{"sprite","sprite"}}},
             */

            {typeof(UnityEngine.UI.InputField), new Dictionary<string,string>(){
                    {"onValueChanged:AddListener", "changed"}
                }
            },

            {typeof(UnityEngine.UI.Button), new Dictionary<string,string>(){
                    {"onClick:AddListener", "click"}
                }
            },
            {typeof(UnityEngine.UI.Dropdown), new Dictionary<string, string>()
            {
                {"onValueChanged:AddListener", "changed"}
            }},
            {typeof(UnityEngine.UI.Toggle), new Dictionary<string, string>()
            {
                {"onValueChanged:AddListener", "changed" }
            } },

        };

    public static void Test()
    {
        UnityEngine.UI.Button btn;
        UnityEngine.UI.Text txt;
        UnityEngine.UI.ScrollRect s;
        UnityEngine.UI.ScrollRect rect;
        UnityEngine.UI.GridLayoutGroup grid;
        //grid.flexibleWidth
        //grid.preferredHeight 
        UnityEngine.UI.RawImage rawImage;
        //rawImage.texture = null;
        UnityEngine.UI.Toggle toggle;
        UnityEngine.Canvas canvas;
        //UnityEngine.Profiling.Profiler.BeginSample("");
        //        rect.Rebuild(UnityEngine.UI.CanvasUpdate); 
    }
}
