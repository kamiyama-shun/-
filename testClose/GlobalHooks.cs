using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace testClose
{
    public class GlobalHooks
    {
        // イベント定義
        public delegate void HookReplacedEventHandler();
        public delegate void WindowEventHandler(IntPtr Handle);
        public delegate void SysCommandEventHandler(int SysCommand, int lParam);
        public delegate void ActivateShellWindowEventHandler();
        public delegate void TaskmanEventHandler();
        public delegate void BasicHookEventHandler(IntPtr Handle1, IntPtr Handle2);
        public delegate void WndProcEventHandler(IntPtr Handle, IntPtr Message, IntPtr wParam, IntPtr lParam);

        [DllImport("trustDll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool InitializeCbtHook(int threadID, IntPtr DestWindow);
        [DllImport("trustDll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UninitializeCbtHook();

        [DllImport("trustDll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool InitializeShellHook(int threadID, IntPtr DestWindow);
        [DllImport("trustDll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UninitializeShellHook();

        [DllImport("trustDll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitializeCallWndProcHook(int threadID, IntPtr DestWindow);
        [DllImport("trustDll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UninitializeCallWndProcHook();

        [DllImport("trustDll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitializeGetMsgHook(int threadID, IntPtr DestWindow);
        [DllImport("trustDll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UninitializeGetMsgHook();

        // API
        [DllImport("user32.dll")]
        private static extern int RegisterWindowMessage(string lpString);
        [DllImport("user32.dll")]
        private static extern IntPtr GetProp(IntPtr hWnd, string lpString);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        private IntPtr _Handle;

        private CBTHook _CBT;
        private ShellHook _Shell;
        private CallWndProcHook _CallWndProc;
        private GetMsgHook _GetMsg;

        public GlobalHooks(IntPtr handle)
        {
            _Handle = handle;

            _CBT = new CBTHook(_Handle);
            _Shell = new ShellHook(_Handle);
            _CallWndProc = new CallWndProcHook(_Handle);
            _GetMsg = new GetMsgHook(_Handle);
        }

        ~GlobalHooks()
        {
            _CBT.Stop();
            _Shell.Stop();
            _CallWndProc.Stop();
            _GetMsg.Stop();
        }

        public void ProcessWindowMessage(ref System.Windows.Forms.Message m)
        {
            _CBT.ProcessWindowMessage(ref m);
            _Shell.ProcessWindowMessage(ref m);
            _CallWndProc.ProcessWindowMessage(ref m);
            _GetMsg.ProcessWindowMessage(ref m);
        }

        #region プロパティ

        public CBTHook CBT
        {
            get { return _CBT; }
        }

        public ShellHook Shell
        {
            get { return _Shell; }
        }

        public CallWndProcHook CallWndProc
        {
            get { return _CallWndProc; }
        }

        public GetMsgHook GetMsg
        {
            get { return _GetMsg; }
        }

        #endregion

        /// <summary>
        /// Hook抽象クラス
        /// </summary>
        public abstract class Hook
        {
            protected bool _isActive = false;
            protected IntPtr _handle;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="handle"></param>
            public Hook(IntPtr handle)
            {
                _handle = handle;
            }

            /// <summary>
            /// 開始
            /// </summary>
            public void Start()
            {
                if (!_isActive)
                {
                    _isActive = true;
                    OnStart();
                }
            }

            /// <summary>
            /// 終了
            /// </summary>
            public void Stop()
            {
                if (_isActive)
                {
                    OnStop();
                    _isActive = false;
                }
            }

            /// <summary>
            /// デクストラクタ
            /// </summary>
            ~Hook()
            {
                Stop();
            }

            /// <summary>
            /// 有効フラグ
            /// </summary>
            public bool IsActive
            {
                get { return _isActive; }
            }

            protected abstract void OnStart();
            protected abstract void OnStop();
            public abstract void ProcessWindowMessage(ref System.Windows.Forms.Message m);
        }

        /// <summary>
        /// CBTHook
        /// </summary>
        public class CBTHook : Hook
        {
            private int _MsgID_CBT_HookReplaced;
            private int _MsgID_CBT_Activate;
            private int _MsgID_CBT_CreateWnd;
            private int _MsgID_CBT_DestroyWnd;
            private int _MsgID_CBT_MinMax;
            private int _MsgID_CBT_MoveSize;
            private int _MsgID_CBT_SetFocus;
            private int _MsgID_CBT_SysCommand;

            public event HookReplacedEventHandler HookReplaced;
            public event WindowEventHandler Activate;
            public event WindowEventHandler CreateWindow;
            public event WindowEventHandler DestroyWindow;
            public event WindowEventHandler MinMax;
            public event WindowEventHandler MoveSize;
            public event WindowEventHandler SetFocus;
            public event SysCommandEventHandler SysCommand;
 
            public CBTHook(IntPtr handle) : base(handle)
            {
            }

            protected override void OnStart()
            {
                _MsgID_CBT_HookReplaced = RegisterWindowMessage("I_CLICK_HOOK_CBT_REPLACED");
                _MsgID_CBT_Activate = RegisterWindowMessage("I_CLICK_HOOK_HCBT_ACTIVATE");
                _MsgID_CBT_CreateWnd = RegisterWindowMessage("I_CLICK_HOOK_HCBT_CREATEWND");
                _MsgID_CBT_DestroyWnd = RegisterWindowMessage("I_CLICK_HOOK_HCBT_DESTROYWND");
                _MsgID_CBT_MinMax = RegisterWindowMessage("I_CLICK_HOOK_HCBT_MINMAX");
                _MsgID_CBT_MoveSize = RegisterWindowMessage("I_CLICK_HOOK_HCBT_MOVESIZE");
                _MsgID_CBT_SetFocus = RegisterWindowMessage("I_CLICK_HOOK_HCBT_SETFOCUS");
                _MsgID_CBT_SysCommand = RegisterWindowMessage("I_CLICK_HOOK_HCBT_SYSCOMMAND");

                InitializeCbtHook(0, _handle);
            }

            protected override void OnStop()
            {
                UninitializeCbtHook();
            }

            public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
            {
                if (m.Msg == _MsgID_CBT_HookReplaced)
                {
                    if (HookReplaced != null)
                        this.HookReplaced();
                }
                else if (m.Msg == _MsgID_CBT_Activate)
                {
                    if (Activate != null)
                        this.Activate(m.WParam);
                }
                else if (m.Msg == _MsgID_CBT_CreateWnd)
                {
                    if (CreateWindow != null)
                        this.CreateWindow(m.WParam);
                }
                else if (m.Msg == _MsgID_CBT_DestroyWnd)
                {
                    if (DestroyWindow != null)
                        this.DestroyWindow(m.WParam);
                }
                else if (m.Msg == _MsgID_CBT_MinMax)
                {
                    if (MinMax != null)
                        this.MinMax(m.WParam);
                }
                else if (m.Msg == _MsgID_CBT_MoveSize)
                {
                    if (MoveSize != null)
                        this.MoveSize(m.WParam);
                }
                else if (m.Msg == _MsgID_CBT_SetFocus)
                {
                    if (SetFocus != null)
                        this.SetFocus(m.WParam);
                }
                else if (m.Msg == _MsgID_CBT_SysCommand)
                {
                    if (SysCommand != null)
                        this.SysCommand(m.WParam.ToInt32(), m.LParam.ToInt32());
                }
            }
        }

        /// <summary>
        /// ShellHook
        /// </summary>
        public class ShellHook : Hook
        {
            private int _MsgID_Shell_ActivateShellWindow;
            private int _MsgID_Shell_GetMinRect;
            private int _MsgID_Shell_Language;
            private int _MsgID_Shell_Redraw;
            private int _MsgID_Shell_Taskman;
            private int _MsgID_Shell_HookReplaced;
            private int _MsgID_Shell_WindowActivated;
            private int _MsgID_Shell_WindowCreated;
            private int _MsgID_Shell_WindowDestroyed;

            public event HookReplacedEventHandler HookReplaced;
            public event ActivateShellWindowEventHandler ActivateShellWindow;
            public event WindowEventHandler GetMinRect;
            public event WindowEventHandler Language;
            public event WindowEventHandler Redraw;
            public event TaskmanEventHandler Taskman;
            public event WindowEventHandler WindowActivated;
            public event WindowEventHandler WindowCreated;
            public event WindowEventHandler WindowDestroyed;

            public ShellHook(IntPtr Handle) : base(Handle)
            {
            }

            protected override void OnStart()
            {
                _MsgID_Shell_HookReplaced = RegisterWindowMessage("I_CLICK_HOOK_SHELL_REPLACED");
                _MsgID_Shell_ActivateShellWindow = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_ACTIVATESHELLWINDOW");
                _MsgID_Shell_GetMinRect = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_GETMINRECT");
                _MsgID_Shell_Language = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_LANGUAGE");
                _MsgID_Shell_Redraw = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_REDRAW");
                _MsgID_Shell_Taskman = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_TASKMAN");
                _MsgID_Shell_WindowActivated = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_WINDOWACTIVATED");
                _MsgID_Shell_WindowCreated = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_WINDOWCREATED");
                _MsgID_Shell_WindowDestroyed = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_WINDOWDESTROYED");

                InitializeShellHook(0, _handle);
            }

            protected override void OnStop()
            {
                UninitializeShellHook();
            }

            public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
            {
                if (m.Msg == _MsgID_Shell_HookReplaced)
                {
                    if (HookReplaced != null)
                        this.HookReplaced();
                }
                else if (m.Msg == _MsgID_Shell_ActivateShellWindow)
                {
                    if (ActivateShellWindow != null)
                        this.ActivateShellWindow();
                }
                else if (m.Msg == _MsgID_Shell_GetMinRect)
                {
                    if (GetMinRect != null)
                        this.GetMinRect(m.WParam);
                }
                else if (m.Msg == _MsgID_Shell_Language)
                {
                    if (Language != null)
                        this.Language(m.WParam);
                }
                else if (m.Msg == _MsgID_Shell_Redraw)
                {
                    if (Redraw != null)
                        this.Redraw(m.WParam);
                }
                else if (m.Msg == _MsgID_Shell_Taskman)
                {
                    if (Taskman != null)
                        this.Taskman();
                }
                else if (m.Msg == _MsgID_Shell_WindowActivated)
                {
                    if (WindowActivated != null)
                        this.WindowActivated(m.WParam);
                }
                else if (m.Msg == _MsgID_Shell_WindowCreated)
                {
                    if (WindowCreated != null)
                        this.WindowCreated(m.WParam);
                }
                else if (m.Msg == _MsgID_Shell_WindowDestroyed)
                {
                    if (WindowDestroyed != null)
                        this.WindowDestroyed(m.WParam);
                }
            }
        }

        /// <summary>
        /// CallWndProcHook
        /// </summary>
        public class CallWndProcHook : Hook
        {
            private int _MsgID_CallWndProc;
            private int _MsgID_CallWndProc_Params;
            private int _MsgID_CallWndProc_HookReplaced;

            public event HookReplacedEventHandler HookReplaced;
            public event WndProcEventHandler CallWndProc;

            private IntPtr _CacheHandle;
            private IntPtr _CacheMessage;

            public CallWndProcHook(IntPtr Handle)
                : base(Handle)
            {
            }

            protected override void OnStart()
            {
                _MsgID_CallWndProc_HookReplaced = RegisterWindowMessage("I_CLICK_HOOK_CALLWNDPROC_REPLACED");
                _MsgID_CallWndProc = RegisterWindowMessage("I_CLICK_HOOK_CALLWNDPROC");
                _MsgID_CallWndProc_Params = RegisterWindowMessage("I_CLICK_HOOK_CALLWNDPROC_PARAMS");

                InitializeCallWndProcHook(0, _handle);
            }

            protected override void OnStop()
            {
                UninitializeCallWndProcHook();
            }

            public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
            {
                if (m.Msg == _MsgID_CallWndProc)
                {
                    _CacheHandle = m.WParam;
                    _CacheMessage = m.LParam;
                }
                else if (m.Msg == _MsgID_CallWndProc_Params)
                {
                    if (CallWndProc != null && _CacheHandle != IntPtr.Zero && _CacheMessage != IntPtr.Zero)
                        this.CallWndProc(_CacheHandle, _CacheMessage, m.WParam, m.LParam);
                    _CacheHandle = IntPtr.Zero;
                    _CacheMessage = IntPtr.Zero;
                }
                else if (m.Msg == _MsgID_CallWndProc_HookReplaced)
                {
                    if (HookReplaced != null)
                        this.HookReplaced();
                }
            }
        }

        /// <summary>
        /// GetMsgHook
        /// </summary>
        public class GetMsgHook : Hook
        {
            private int _MsgID_GetMsg;
            private int _MsgID_GetMsg_Params;
            private int _MsgID_GetMsg_HookReplaced;

            public event HookReplacedEventHandler HookReplaced;
            public event WndProcEventHandler GetMsg;

            private IntPtr _CacheHandle;
            private IntPtr _CacheMessage;

            public GetMsgHook(IntPtr Handle) : base(Handle)
            {
            }

            protected override void OnStart()
            {
                _MsgID_GetMsg_HookReplaced = RegisterWindowMessage("I_CLICK_HOOK_GETMSG_REPLACED");
                _MsgID_GetMsg = RegisterWindowMessage("I_CLICK_HOOK_GETMSG");
                _MsgID_GetMsg_Params = RegisterWindowMessage("I_CLICK_HOOK_GETMSG_PARAMS");

                InitializeGetMsgHook(0, _handle);
            }

            protected override void OnStop()
            {
                UninitializeGetMsgHook();
            }

            public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
            {
                if (m.Msg == _MsgID_GetMsg)
                {
                    _CacheHandle = m.WParam;
                    _CacheMessage = m.LParam;
                }
                else if (m.Msg == _MsgID_GetMsg_Params)
                {
                    if (GetMsg != null && _CacheHandle != IntPtr.Zero && _CacheMessage != IntPtr.Zero)
                        this.GetMsg(_CacheHandle, _CacheMessage, m.WParam, m.LParam);
                    _CacheHandle = IntPtr.Zero;
                    _CacheMessage = IntPtr.Zero;
                }
                else if (m.Msg == _MsgID_GetMsg_HookReplaced)
                {
                    if (HookReplaced != null)
                        this.HookReplaced();
                }
            }
        }
    }
}
