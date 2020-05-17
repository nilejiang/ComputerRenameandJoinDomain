using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.IO;
using System.Net;
using System.Xml;
using System.Windows.Forms;

namespace ComputerandDomain
{
    public partial class About : Form
    {
        private const string updateUrl = "http://sourceforge.net/projects/renameandjoindomaintool/files/RenameAndJoinDomain/version-en.xml";//The url of check version xml file.
        public About()
        {
            InitializeComponent();
        }

        private void checkUpdate_Click(object sender, EventArgs e)
        {
            checkIfUpdate();
        }

        private void checkIfUpdate()
        {
            try
            {
                //bool isUpdate = false;
                string newVersion = "";
                string newSoftAddr = "";
                WebClient wc = new WebClient();
                Stream stream = wc.OpenRead(updateUrl);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(stream);
                XmlNode list = xmlDoc.SelectSingleNode("Update");
                foreach (XmlNode node in list)
                {
                    if (node.Name == "Software" && node.Attributes["Name"].Value == "RenameAndJoinDomainTool")
                    {
                        foreach (XmlNode xml in node)
                        {
                            if (xml.Name == "Version")
                            {
                                newVersion = xml.InnerText;
                            }
                            else
                            {
                                newSoftAddr = xml.InnerText;
                            }
                        }
                    }
                }
                Version verNew = new Version(newVersion);
                Version verCurrent = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                int comResult = verCurrent.CompareTo(verNew);
                string currentDirPath = System.Environment.CurrentDirectory;
                if (comResult >= 0)
                {
                    //isUpdate = false;
                    MessageBox.Show("This is the latest version!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    //isUpdate = true;
                    //MessageBox.Show("New " + currentDirPath + verNew.ToString());
                    if (MessageBox.Show("The new version v" + newVersion + " is available, do you want to download it to current directory?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        wc.DownloadFile(newSoftAddr, currentDirPath + "\\RenameAndJoinDomainTool-v" + newVersion + ".zip");
                        wc.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Check update error! Please make sure if you can connect to internet successfully or go to http://sourceforge.net/projects/renameandjoindomaintool to download!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
