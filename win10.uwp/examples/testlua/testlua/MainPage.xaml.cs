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
using Windows.ApplicationModel;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace testlua
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

            Service._CheckPassword(false);

            /*----run lua code----*/
            SrvGroup._InitRaw("lua", Service);
            StarObjectClass lua = Service._ImportRawContext("lua", "", false, "");

            String CorePath = Package.Current.InstalledLocation.Path;
            //--load lua module ---*/
            SrvGroup._LoadRawModule("lua", "", CorePath + "\\testlua.lua", false);
            //--call lua function tt, the return contains two integer, which will be wrapped into StarObjectClass
            StarObjectClass retobj = (StarObjectClass)lua._Call("tt", "hello ", "world");
            Debug.WriteLine("ret from lua :  " + retobj._Get(1) + "   " + retobj._Get(2));
            //--get global int value g1--------*/
            Debug.WriteLine("lua value g1 :  " + lua._Get("g1"));
            //--get global table value c, which is a table with function, it will be mapped to cle object --------*/
            StarObjectClass c = lua._GetObject("c");
            //--get int value x from c--------*/
            Debug.WriteLine("c value x :  " + c._Get("x"));
            //--call c function yy, the return is a table, which will be mapped to cle object ---*/
            StarObjectClass yy = (StarObjectClass)c._Call("yy", c, "hello ", "world", "!");
            Debug.WriteLine("yy value [1] :  " + yy._Get(1));
            Debug.WriteLine("yy value [Type] :  " + yy._Get("Type"));

            /*-------*/
            lua._Set("CSClass", typeof(CallBackClass));
            Service._DoFile("lua", CorePath + "\\test_callcs.lua", "");  //should not use null
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
            public void SetLuaObject(Object[] rb)
            {
                foreach (Object Item in rb)
                {
                    Debug.WriteLine("" + Item);
                }
            }
        }
    }
}
