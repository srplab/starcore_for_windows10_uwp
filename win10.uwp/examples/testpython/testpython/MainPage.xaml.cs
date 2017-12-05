using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Libstarcore;
using Star_csharp;
using System.Diagnostics;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace testpython
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Host;
        public StarSrvGroupClass SrvGroup;

        public MainPage()
        {
            this.InitializeComponent();
            Host = this;

            StarCoreFactoryInit.Init(this);
            StarCoreFactory starcore = StarCoreFactory.GetFactory();
            starcore._RegMsgCallBack_P(new StarMsgCallBackInterface(delegate (int ServiceGroupID, int uMes, object wParam, object lParam)
            {
                if (uMes == starcore._Getint("MSG_VSDISPMSG") || uMes == starcore._Getint("MSG_VSDISPLUAMSG") || uMes == starcore._Getint("MSG_DISPMSG") || uMes == starcore._Getint("MSG_DISPLUAMSG"))
                {
                    Debug.WriteLine((string)wParam);
                }
                return null;
            }));
            StarServiceClass Service = (StarServiceClass)starcore._InitSimple("test", "123", 0, 0, null);
            SrvGroup = (StarSrvGroupClass)Service._Get("_ServiceGroup");

            bool Result = SrvGroup._InitRaw("python34", Service);
            StarObjectClass python = Service._ImportRawContext("python", "", false, "");

            string CorePath = SrvGroup._GetCorePath();
            Service._DoFile("python", CorePath + "\\test.py", "");

            python._Set("CSClass", typeof(CallBackClass));
            Service._DoFile("python", CorePath + "\\test_callcs.py", "");  //should not use null

            python._Call("import", "sys");
            StarObjectClass pythonSys = python._GetObject("sys");
            StarObjectClass pythonPath = (StarObjectClass)pythonSys._Get("path");
            //pythonPath._Call("insert", 0, CorePath + "\\Django-1.10.2-py3.4.egg.zip");
        }
    }

    public class CallBackClass
    {
        StarObjectClass PythonClass;
        public CallBackClass(String Info)
        {
            Debug.WriteLine(Info);
        }
        public void callback(float val)
        {
            Debug.WriteLine("" + val);
        }
        public void callback(String val)
        {
            Debug.WriteLine("" + val);
        }
        public void SetPythonObject(Object rb)
        {
            PythonClass = (StarObjectClass)rb; // Ruby File
            String aa = "";
            StarParaPkgClass data1 = MainPage.Host.SrvGroup._NewParaPkg("b", 789, "c", 456, "a", 123)._AsDict(true);
            Object d1 = PythonClass._Call("dumps", data1, MainPage.Host.SrvGroup._NewParaPkg("sort_keys", true)._AsDict(true));
            Debug.WriteLine("" + d1);
            Object d2 = PythonClass._Call("dumps", data1, null);
            Debug.WriteLine("" + d2);
            Object d3 = PythonClass._Call("dumps", data1, MainPage.Host.SrvGroup._NewParaPkg("sort_keys", true, "indent", 4)._AsDict(true));
            Debug.WriteLine("" + d3);
        }
    }
}
