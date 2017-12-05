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

namespace testruby
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            StarCoreFactoryInit.Init(this);

            StarCoreFactory starcore = StarCoreFactory.GetFactory();
            StarServiceClass Service = (StarServiceClass)starcore._InitSimple("test", "123", 0, 0, null);
            StarSrvGroupClass SrvGroup = (StarSrvGroupClass)Service._Get("_ServiceGroup");
            starcore._RegMsgCallBack_P(new StarMsgCallBackInterface(delegate (int ServiceGroupID, int uMes, object wParam, object lParam) {
                if (uMes == starcore._Getint("MSG_VSDISPMSG") || uMes == starcore._Getint("MSG_VSDISPLUAMSG") || uMes == starcore._Getint("MSG_DISPMSG") || uMes == starcore._Getint("MSG_DISPLUAMSG"))
                {
                    Debug.WriteLine((string)wParam);
                }
                return null;
            }));

            bool InitRawFlag = SrvGroup._InitRaw("ruby", Service);
            //---set module path
            StarObjectClass ruby = Service._ImportRawContext("ruby", "", false, "");
            StarObjectClass RbPath = (StarObjectClass)ruby._Get("$LOAD_PATH");

            string CorePath = SrvGroup._GetCorePath();
            RbPath._Call("unshift", CorePath);

            ruby._Call("require", "cmath");
            Debug.WriteLine(ruby._Get("CMath"));

            //--load ruby module ---*/
            SrvGroup._LoadRawModule("ruby", "", Package.Current.InstalledLocation.Path + "\\testrb.rb", false);
            //--attach object to global ruby space ---*/
            StarObjectClass Obj = Service._ImportRawContext("ruby", "", false, "");
            //--call ruby function tt, the return contains two integer, which will be packed into parapkg ---*/
            StarObjectClass RetObj = (StarObjectClass)Obj._Call("tt", "hello ", "world");
            Debug.WriteLine("ret from ruby : " + RetObj._Get(0) + "  " + RetObj._Get(1));
            //--get global int value g1--------*/
            Debug.WriteLine("ruby value g1 :  " + Obj._Get("g1"));
            //--get global class Multiply
            StarObjectClass Multiply = Service._ImportRawContext("ruby", "Multiply", true, "");
            StarObjectClass multiply = Multiply._New("", "", 33, 44);
            //--call instance method multiply
            Debug.WriteLine("instance multiply = " + multiply._Call("multiply", 11, 22));

            ruby._Set("$CSClass", typeof(CallBackClass));
            Service._DoFile("ruby", CorePath + "\\test_callcs.rb", "");  //should not use null

        }
    }

    public class CallBackClass
    {
        StarObjectClass RBClass;
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
        public void SetRubyObject(Object rb)
        {
            RBClass = (StarObjectClass)rb; // Ruby File
            StarObjectClass f = RBClass._New("", "", ApplicationData.Current.LocalFolder.Path + "\\test.txt", "w+");
            f._Call("puts", "I am Jack");
            f._Call("close");
        }
    }
}
