/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2017-11-21
 * Time: 21:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.WindowsMediaServices.Interop;
using System.IO;
using System.Xml;

namespace AutoSwitchChanel
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private WMSServer                            Server=null;
		private IWMSPublishingPoints                 PubPoints=null;
		private IWMSPublishingPoint                  PubPoint=null;
		private IWMSBroadcastPublishingPoint 		 CastPointDJ=null;
		private IWMSBroadcastPublishingPoint 		 CastPointFM=null;
		
		private string baseurl="http://127.0.0.1:8888/";
		
		private string djState="未知状态";
		private int djStateTime=0;
		
		private int AirOnSeconds=10;
		private int RecOnSeconds=30;
		
		private string AirPoint="yhc_dj";//直播点
		private string RecPoint="yhc_rd";//录播点
		private string OutPoint="yhc_fm";//对外点
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void Timer1Tick(object sender, EventArgs e)
		{
			checkState();
		}
		
		void LoadConfig(){
			string path=AppDomain.CurrentDomain.BaseDirectory;
			string configFile=path+"config.xml";
			if(File.Exists(configFile)){
				try{
					XmlDocument doc = new XmlDocument();
					doc.Load(configFile);
					XmlNode nodeOnSeconds = doc.SelectSingleNode("//OnSeconds");
					string air=nodeOnSeconds.Attributes["Air"].Value;
					string rec=nodeOnSeconds.Attributes["Rec"].Value;
					int.TryParse(air,out AirOnSeconds);
					int.TryParse(rec,out RecOnSeconds);
					XmlNode nodePoint = doc.SelectSingleNode("//Point");
					string airPoint=nodePoint.Attributes["Air"].Value;
					string recPoint=nodePoint.Attributes["Rec"].Value;
					string outPoint=nodePoint.Attributes["Out"].Value;
					if(airPoint.Length>0)
						AirPoint=airPoint;
					if(recPoint.Length>0)
						RecPoint=recPoint;
					if(outPoint.Length>0)
						OutPoint=outPoint;
				}
				catch{
					
				}
			}
			this.button4.Text=AirOnSeconds+"秒";
			this.button5.Text=RecOnSeconds+"秒";
			this.button6.Text=AirPoint;
			this.button7.Text=RecPoint;
			this.button8.Text=OutPoint;
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
			try{
				LoadConfig();				
				Server = new WMSServerClass();				
				timer1.Enabled=true;
				PubPoints = Server.PublishingPoints;
				for(int i=0;i<PubPoints.length;i++){
					PubPoint = PubPoints[i];
			        if(PubPoint.Name==AirPoint){
						CastPointDJ=(IWMSBroadcastPublishingPoint)PubPoint;
					}
					else if(PubPoint.Name==OutPoint){
						CastPointFM=(IWMSBroadcastPublishingPoint)PubPoint;
					}	
				}
				if(CastPointFM==null){
					MessageBox.Show("错误，找不到"+OutPoint+"发布点！");
				}
				checkState();
			}
			catch{
				MessageBox.Show("无法创建WMS对象！请确保安装了WMS。","错误", MessageBoxButtons.OK,
				               MessageBoxIcon.Error);
			}
			
		}
		
		void checkState(){
						
			if(CastPointDJ.BroadcastStatus== WMS_BROADCAST_PUBLISHING_POINT_STATUS.WMS_BROADCAST_PUBLISHING_POINT_STARTED){
				djState="正在直播";
			}
			else{
				djState="尚未启动";
			}
			if(button1.Text!=djState){
				djStateTime=0;
			}
			else{
				djStateTime+=timer1.Interval/1000;
			}
			button1.Text =djState;
			
			string truePath=CastPointFM.Path.Replace("\\","/");
			int loc=truePath.IndexOf("?t=");
			if(loc!=-1){
				truePath=truePath.Substring(0,loc);
			}
			
			if(truePath.EndsWith("/"+AirPoint)){
				button3.Text ="直播";
			}
			else if(truePath.EndsWith("/"+RecPoint)){
				button3.Text ="录播";
			}
			
			button2.Text=getTimeDesc(djStateTime);
			if(djStateTime>AirOnSeconds && djState=="正在直播" && button3.Text=="录播"){
				switchChanel(AirPoint);
			}
			if(djStateTime>RecOnSeconds && djState=="尚未启动" && button3.Text=="直播"){
				switchChanel(RecPoint);
			}
		}
		
		private string getTimeDesc(int time){
			TimeSpan ts = new TimeSpan(0, 0, time);
			string s=ts.Seconds+"秒";
			if(ts.Minutes>0){
				s=ts.Minutes +"分"+s;
			}
			if(ts.Hours>0){
				s=ts.Hours +"时"+s;
			}
			if(ts.Days>0){
				s=ts.Days +"天"+s;
			}
			return s;
		}
		
		void switchChanel(string chanel){
			try{
				CastPointFM.Stop();
				CastPointFM.Path=baseurl+chanel;
				CastPointFM.Start();
				CastPointFM.Stop();
				CastPointFM.Start();
			}
			catch{
				//
			}
		}
	}
}
