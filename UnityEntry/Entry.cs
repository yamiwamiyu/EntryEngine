using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.IO;

namespace EntryEngine.Unity
{
    public class Entry : MonoBehaviour
    {
        public Material GLMaterial;

        void LoadResources(string resource, Action<TextAsset> load)
        {
            string name = Path.GetFileNameWithoutExtension(resource);

            var asset = Resources.Load<TextAsset>(name);
            try
            {
                load(asset);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Load resources[{0}] error! ex={1}", resource, ex.Message));
            }
            finally
            {
                Resources.UnloadAsset(asset);
            }
        }
        // Use this for initialization
        void Start()
        {
            Type entryType = Type.GetType("EntryEngine.Entry");
            if (entryType == null)
            {
                try
                {
                    // 读取dll二进制流，或许还能通过可写文件夹动态加载
                    LoadResources("UnityRuntime.bytes",
                        asset =>
                        {
                            // 加载EntryEngine
                            Assembly assembly = AppDomain.CurrentDomain.Load(asset.bytes, null);
                            // 调用dll自解析
                            Type type = assembly.GetType("Program");
                            MethodInfo method = type.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
                            method.Invoke(null, new object[1] { null });
                            Debug.Log("Load 成功");
                        });

                    LoadResources("__filelist.txt",
                        asset =>
                        {
                            string[] list = asset.text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < list.Length; i++)
                            {
                                LoadResources(list[i], dll => AppDomain.CurrentDomain.Load(dll.bytes, null));
                                Debug.Log(string.Format("加载{0}完成", list[i]));
                            }
                        });
                }
                catch (Exception ex)
                {
                    Debug.Log(string.Format("加载解析dll异常:{0}", ex.Message));
                }
            }

            // 初始化游戏开始过程
            StartCoroutine_Auto(StartCoroutine());
        }

        IEnumerator StartCoroutine()
        {
            // 创建Unity入口
            Assembly unity = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Unity");
            Type gateType = unity.GetType("EntryEngine.Unity.UnityGate");
            var gate = gameObject.AddComponent(gateType);

            // 设置同名属性
            //Debug.Log(gateType.GetProperty("Gate", BindingFlags.Public | BindingFlags.Static).GetValue(null, new object[0]));  // 测试是否已经执行了Start: 没有执行
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
                gateType.GetField(fields[i].Name, BindingFlags.Public | BindingFlags.Static).SetValue(null, fields[i].GetValue(this));

            // 等待帧结束，让入口Start调用完毕
            yield return new WaitForEndOfFrame();

            try
            {
                // 加载游戏入口场景
                Assembly client = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Client");
                // 使用入口展示场景实例 entry.ShowMainScene<T>();
                PropertyInfo entry = gateType.GetProperty("Entry");
                entry.PropertyType.GetMethod("ShowMainScene", Type.EmptyTypes).
                    MakeGenericMethod(client.GetType("MAIN")).
                    Invoke(entry.GetValue(gate, null),
                    new object[] { });
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + "\r\n" + ex.StackTrace);
            }

            // 移除加载程序
            DestroyImmediate(this);
        }
    }
}
