/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2017-11-21
 * Time: 21:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Windows.Forms;

namespace AutoSwitchChanel
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			CheckRun();
		}
		
		// <summary>
        /// 单进程检测
        /// </summary>
        private static void CheckRun()
        {

            bool isapprunning = false;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, System.Diagnostics.Process.GetCurrentProcess().ProcessName,
            out isapprunning);
            if (!isapprunning)
            {
                MessageBox.Show("本程序已经在运行了，请不要重复运行！","提示");
                Environment.Exit(1);
            }
            else
            {              
                Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm());
            }
        }
		
	}
}
