using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace testClose
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int size);
        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder cassName, int maxCnt);

        private GlobalHooks _GlobalHooks;

        [STAThread]
        static void Main()
        {
            // フォーム起動
            Application.Run(new Form1());
        }

        public Form1()
        {
            InitializeComponent();

            // フック機能初期化
            _GlobalHooks = new GlobalHooks(this.Handle);

            // フック設定
            _GlobalHooks.CBT.Activate += new GlobalHooks.WindowEventHandler(_GlobalHooks_CbtActivate);

            // フック開始
            this.StartHook();
        }

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.EndHook(false);

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// フック開始
        /// </summary>
        private void StartHook()
        {
            _GlobalHooks.CBT.Start();
        }

        /// <summary>
        /// フック終了
        /// </summary>
        /// <param name="isClose"></param>
        private void EndHook(bool isClose)
        {
            _GlobalHooks.CBT.Stop();

            if (isClose)
            {
                this.Close();
            }
        }

        /// <summary>
        /// アクティブウィンドウのフック
        /// </summary>
        /// <param name="handle"></param>
        private void _GlobalHooks_CbtActivate(IntPtr handle)
        {
            // フック対象外のイベントか判定
            if (IsCheckWindow(handle) == false)
            {
                return;
            }
        }

        /// <summary>
        /// 処理対象ウィンドウか判定
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        private bool IsCheckWindow(IntPtr handle)
        {
            bool isTraget = false;

            // ウィンドウ名称の文字数取得
            int len = GetWindowTextLength(handle);

            if (0 < len)
            {
                // ウィンドウ名称取得
                StringBuilder sbTitle = new StringBuilder(len + 1);
                GetWindowText(handle, sbTitle, sbTitle.Capacity);

                // ウィンドウクラス名取得
                StringBuilder sbClass = new StringBuilder(256);
                GetClassName(handle, sbClass, sbClass.Capacity);

                // TODO ■ テスト
                this.txtBox.Text += "クラス名:" + sbTitle.ToString() + Environment.NewLine;
                this.txtBox.Text += "タイトル:" + sbClass.ToString() + Environment.NewLine;
                this.txtBox.Text += Environment.NewLine;

                this.txtBox.AppendText

                isTraget = true;
            }

            return isTraget;
        }

        /// <summary>
        /// フックしたメッセージの編集
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (_GlobalHooks != null)
                _GlobalHooks.ProcessWindowMessage(ref m);

            base.WndProc(ref m);
        }
    }
}
