using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Management;
using System.Xml;
using System.Xml.Schema;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Threading;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace ComputerandDomain
{
    public partial class Form1 : Form
    {
        string currentPCName = "";
        string currentDomain = "";
        string hostNameWithoutdomain = "";
        public Form1()
        {
            InitializeComponent();
            currentPCName = GetComputerName();
            currentDomain = GetDomainName();
            hostNameWithoutdomain = Environment.MachineName;
            textBoxPCName.Text = currentPCName;
            if (currentDomain != "")
            {
                textBoxDomainName.Text = currentDomain;
            }
            else
            {
                textBoxDomainName.Text = "Not in domain.";
            }
        }

        

        public string GetComputerName()
        {
            string pcName = "";
            ManagementClass objMC;
            ManagementObjectCollection objMOC;

            try
            {
                objMC = new ManagementClass("Win32_ComputerSystem");
                objMOC = objMC.GetInstances();
            }
            catch (Exception e)
            {
                textBoxOutput.AppendText(e.Message);
                return pcName;
            }

            foreach (ManagementObject objMO in objMOC)
            {
                if (null != objMO)
                {
                    pcName = objMO["Name"].ToString();
                    break;
                }
            }

            return pcName;
        }

        public string GetDomainName()
        {
            string domainName = "";
            ManagementClass objMC;
            ManagementObjectCollection objMOC;

            try
            {
                objMC = new ManagementClass("Win32_ComputerSystem");
                objMOC = objMC.GetInstances();
            }
            catch (Exception e)
            {
                textBoxOutput.AppendText(e.Message);
                return domainName;
            }

            foreach (ManagementObject objMO in objMOC)
            {
                if (null != objMO)
                {
                    if ((bool)objMO["partofdomain"])
                    {
                        domainName = objMO["domain"].ToString();
                        textBoxOutput.AppendText("Current computer is in domain: " + domainName + Environment.NewLine);
                    }
                    else
                    {
                        textBoxOutput.AppendText("Current computer is in work group, not in domain!" + Environment.NewLine);
                    }
                }
            }

            return domainName;
        }

        private void unjoinDomainAndRename()
        {
            if (MessageBox.Show("Are you sure to leave the domain and rename your computer?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {

                string currentHostname = currentPCName;
                ManagementObject objMO = new ManagementObject("Win32_ComputerSystem.Name='" + currentHostname + "'");
                ManagementBaseObject result;
                objMO.Scope.Options.EnablePrivileges = true;
                objMO.Scope.Options.Authentication = AuthenticationLevel.PacketPrivacy;
                objMO.Scope.Options.Impersonation = ImpersonationLevel.Impersonate;
                if (currentDomain != "")
                {
                    try
                    {
                        ManagementBaseObject query;
                        query = objMO.GetMethodParameters("UnjoinDomainOrWorkgroup");
                        query["UserName"] = "username";
                        query["Password"] = "password";
                        query["FUnjoinOptions"] = 0;

                        result = objMO.InvokeMethod("UnjoinDomainOrWorkgroup", query, null);
                        if ((uint)result["ReturnValue"] == 0)
                        {
                            textBoxOutput.AppendText("Leave the domain successfully! You will need to know the password of the local administrator account to log in to your computer. You must restart your computer to apply these changes!" + Environment.NewLine);
                        }
                        else
                        {
                            textBoxOutput.AppendText("Leave the domain failed! Please run this tool as administrator!" + Environment.NewLine);
                        }
                    }
                    catch (ManagementException e)
                    {
                        textBoxOutput.AppendText("Leave the domain failed, the error code is: " + (uint)e.ErrorCode + " , can not leave the domain: " + e.Message + Environment.NewLine);
                        return;
                    }
                }
                else
                {
                    textBoxOutput.AppendText("Leave the domain failed, current computer is not in the domain!" + Environment.NewLine);
                }

                if (textBoxNewName.Text.Trim() != "" && textBoxNewName.Text.Trim().ToLower() != hostNameWithoutdomain.ToLower())
                {
                    string newPCName = textBoxNewName.Text.Trim();

                    try
                    {
                        ManagementBaseObject rename;
                        rename = objMO.GetMethodParameters("Rename");
                        rename["Name"] = newPCName;
                        rename["Password"] = null;
                        rename["UserName"] = null;

                        result = objMO.InvokeMethod("Rename", rename, null);
                        if ((uint)result["ReturnValue"] == 0)
                        {
                            textBoxOutput.AppendText("Rename current computer successfully! You must restart your computer to apply these changes!" + Environment.NewLine);
                        }
                        else
                        {
                            textBoxOutput.AppendText("Rename current computer failed, Please run this tool as administrator!" + Environment.NewLine);
                        }
                    }
                    catch (ManagementException ex)
                    {
                        textBoxOutput.AppendText("Rename current computer failed, the error code is: " + (uint)ex.ErrorCode + ", can not rename: " + ex.Message + Environment.NewLine);
                        return;
                    }
                }
                else
                {
                    textBoxOutput.AppendText("Rename current computer failed, please input a new computer name!" + Environment.NewLine);
                }
            }
        }

        public void unJoinDomain()
        {
            if (MessageBox.Show("Are you sure to leave the domain?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (currentDomain != "")
                {
                    try
                    {
                        string currentHostname = currentPCName;
                        ManagementObject objMO = new ManagementObject("Win32_ComputerSystem.Name='" + currentHostname + "'");
                        ManagementBaseObject result;
                        objMO.Scope.Options.EnablePrivileges = true;
                        objMO.Scope.Options.Authentication = AuthenticationLevel.PacketPrivacy;
                        objMO.Scope.Options.Impersonation = ImpersonationLevel.Impersonate;

                        ManagementBaseObject query;
                        query = objMO.GetMethodParameters("UnjoinDomainOrWorkgroup");
                        query["UserName"] = "username";
                        query["Password"] = "password";
                        query["FUnjoinOptions"] = 0;

                        result = objMO.InvokeMethod("UnjoinDomainOrWorkgroup", query, null);
                        if ((uint)result["ReturnValue"] == 0)
                        {
                            textBoxOutput.AppendText("Leave the domain successfully! You will need to know the password of the local administrator account to log in to your computer. You must restart your computer to apply these changes!" + Environment.NewLine);
                        }
                        else
                        {
                            textBoxOutput.AppendText("Leave the domain failed! Please run this tool as administrator!" + Environment.NewLine);
                        }
                    }
                    catch (ManagementException e)
                    {
                        textBoxOutput.AppendText("Leave the domain failed, the error code is: " + (uint)e.ErrorCode + " , can not leave the domain: " + e.Message + Environment.NewLine);
                        return;
                    }
                }
                else
                {
                    textBoxOutput.AppendText("Leave the domain failed, current computer is not in the domain!" + Environment.NewLine);
                    MessageBox.Show("Leave the domain failed, current computer is not in the domain!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        public void renamePCName(string newPCName)
        {
            try
            {
                string currentHostname = currentPCName;
                ManagementObject objMO = new ManagementObject("Win32_ComputerSystem.Name='" + currentHostname + "'");
                ManagementBaseObject result;
                objMO.Scope.Options.EnablePrivileges = true;
                objMO.Scope.Options.Authentication = AuthenticationLevel.PacketPrivacy;
                objMO.Scope.Options.Impersonation = ImpersonationLevel.Impersonate;

                ManagementBaseObject rename;
                rename = objMO.GetMethodParameters("Rename");
                rename["Name"] = newPCName;
                rename["Password"] = null;
                rename["UserName"] = null;

                result = objMO.InvokeMethod("Rename", rename, null);
                if ((uint)result["ReturnValue"] == 0)
                {
                    textBoxOutput.AppendText("Rename current computer successfully! You must restart your computer to apply these changes!" + Environment.NewLine);
                }
                else
                {
                    textBoxOutput.AppendText("Rename current computer failed, Please run this tool as administrator!" + Environment.NewLine);
                }
            }
            catch (ManagementException e)
            {
                textBoxOutput.AppendText("Rename current computer failed, the error code is: " + (uint)e.ErrorCode + ", can not rename:  " + e.Message + Environment.NewLine);
                return;
            }
        }

        private void joinDomain()
        {
            if (currentDomain == "")
            {
                if (MessageBox.Show("Are you sure to join the new domain?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string newDomain = textBoxNewDomain.Text.Trim();
                    string username = textBoxUsername.Text.Trim();
                    string password = textBoxPwd.Text.Trim();
                    if (newDomain != "" && username != "" && password != "")
                    {
                        joinNewDomain(newDomain, username, password);
                    }
                    else
                    {
                        MessageBox.Show("Please input the new domain name, domain user name and password!", "Erron", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                textBoxOutput.AppendText("Current computer is in the domain, please leave the domain first then join the new domain!" + Environment.NewLine);
                MessageBox.Show("Current computer is in the domain, please leave the domain first then join the new domain!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        public void joinNewDomain(string newDomainName, string username, string password)
        {
            try
            {
                string currentHostname = currentPCName;
                ManagementObject objMO = new ManagementObject("Win32_ComputerSystem.Name='" + currentHostname + "'");
                ManagementBaseObject result;
                objMO.Scope.Options.EnablePrivileges = true;
                objMO.Scope.Options.Authentication = AuthenticationLevel.PacketPrivacy;
                objMO.Scope.Options.Impersonation = ImpersonationLevel.Impersonate;

                ManagementBaseObject query;
                query = objMO.GetMethodParameters("JoinDomainOrWorkgroup");
                query["Name"] = newDomainName;
                query["Password"] = password;
                query["UserName"] = username;
                query["FJoinOptions"] = 3;

                result = objMO.InvokeMethod("JoinDomainOrWorkgroup", query, null);
                if ((uint)result["ReturnValue"] == 0)
                {
                    textBoxOutput.AppendText("Current computer joins the new domain successfully! You must restart your computer to apply these changes!" + Environment.NewLine);
                }
                else
                {
                    textBoxOutput.AppendText("Current computer joins the new domain failed! Please run this tool as administrator and make sure there is no same name in the domain!" + Environment.NewLine);
                }

                if (checkBoxAddtoLocalAdmins.Checked == true)
                {
                    AddDomainUsertoLocalGroup(username);
                }
            }
            catch (ManagementException e)
            {
                textBoxOutput.AppendText("Join new domain failed, the error code is: " + (uint)e.ErrorCode + " can not join domain: " + e.Message + Environment.NewLine);
                return;
            }
        }
        public void HandleJoinandRenameDomain(string newDomainName, string newHostName, string username, string password)
        {
            ManagementObject objMO = new ManagementObject("Win32_ComputerSystem.Name='" + currentPCName + "'");
            if (null != objMO)
            {
                ManagementBaseObject result;

                objMO.Scope.Options.EnablePrivileges = true;
                objMO.Scope.Options.Authentication = AuthenticationLevel.PacketPrivacy;
                objMO.Scope.Options.Impersonation = ImpersonationLevel.Impersonate;

                if ("" != currentDomain)
                {
                    if (currentDomain.ToUpper() == newDomainName.ToUpper())
                    {
                        textBoxOutput.AppendText("Current computer is already in the new domain: " + currentDomain + Environment.NewLine);
                        return;

                    }
                    else
                    {
                        textBoxOutput.AppendText("Current computer is already in other domain: " + currentDomain + Environment.NewLine);
                        return;
                    }
                }
                else
                {
                    ManagementBaseObject query;
                    try
                    {
                        query = objMO.GetMethodParameters("JoinDomainOrWorkgroup");
                    }
                    catch (ManagementException)
                    {
                        textBoxOutput.AppendText("Join new domain failed! Can not find the computer name!" + Environment.NewLine);
                        return;
                    }
                    query["Name"] = newDomainName;
                    query["Password"] = password;
                    query["UserName"] = username;
                    query["FJoinOptions"] = 3;

                    try
                    {
                        result = objMO.InvokeMethod("JoinDomainOrWorkgroup", query, null);
                    }
                    catch (ManagementException e)
                    {
                        textBoxOutput.AppendText("Join new domain failed, the error code is: " + (uint)e.ErrorCode + ", can not join domain: " + e.Message + Environment.NewLine);
                        return;
                    }

                    if (0 != (uint)result["ReturnValue"])
                    {
                        textBoxOutput.AppendText("Join new domain failed: " + (uint)result["ReturnValue"] + ", can not join domain." + Environment.NewLine);
                        return;
                    }
                    else
                    {
                        textBoxOutput.AppendText("Join new domain " + newDomainName + " successfully! You must restart your computer to apply these changes!" + Environment.NewLine);
                    }
                }

                //Thread.Sleep(45000);

                if ((currentPCName.ToUpper() != newHostName.ToUpper()) && ("" != newHostName))
                {
                    try
                    {
                        textBoxOutput.AppendText("New computer name: " + newHostName + Environment.NewLine);
                        ManagementBaseObject query2;
                        query2 = objMO.GetMethodParameters("Rename");
                        query2["Name"] = newHostName;
                        query2["Password"] = password;
                        query2["UserName"] = username;

                        result = objMO.InvokeMethod("Rename", query2, null);

                        if (0 != (uint)result["ReturnValue"])
                        {
                            textBoxOutput.AppendText("Rename computer failed, the error code is: " + result["ReturnValue"].ToString() + Environment.NewLine);
                        }
                        else
                        {
                            textBoxOutput.AppendText("Rename computer successfully! You must restart your computer to apply these changes!" + Environment.NewLine);
                        }

                    }
                    catch (InvalidOperationException e)
                    {
                        textBoxOutput.AppendText("Rename computer failed: " + e.Message + Environment.NewLine);
                        return;
                    }
                    catch (ManagementException e)
                    {
                        textBoxOutput.AppendText("Rename computer failed, can not rename computer: " + e.Message + Environment.NewLine);
                        return;
                    }
                }

                if (checkBoxAddtoLocalAdmins.Checked == true)
                {
                    AddDomainUsertoLocalGroup(username);
                }
            }
            else
            {
                textBoxOutput.AppendText("Join new domain failed: can not open the object!" + Environment.NewLine);
                return;
            }
        }

        #region Add domain user to local administrators group.
        public void AddDomainUsertoLocalGroup(string username)
        {
            try
            {
                DirectoryEntry adRoot = new DirectoryEntry(string.Format("WinNT://{0}", textBoxNewDomain.Text.Trim()), textBoxUsername.Text.Trim(), textBoxPwd.Text.Trim());
                DirectoryEntry user = adRoot.Children.Find(username, "User");

                DirectoryEntry localGroupRoot = new DirectoryEntry("WinNT://" + Environment.MachineName + ",Computer");
                DirectoryEntry localGroup = localGroupRoot.Children.Find("Administrators", "group");

                localGroup.Invoke("Add", new Object[] { user.Path.ToString() });
                textBoxOutput.AppendText("Add user " + username + " to local Administrators group successfully!" + Environment.NewLine);
            }
            catch (Exception e)
            {
                textBoxOutput.AppendText(e.Message + Environment.NewLine);
            }
        }
        #endregion

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (treeViewRenameAndJoinDomain.SelectedNode.Name == "unjoinDomainAndRename")
                unjoinDomainAndRename();
            else if (treeViewRenameAndJoinDomain.SelectedNode.Name == "unjoinDomain")
                unJoinDomain();
            else if (treeViewRenameAndJoinDomain.SelectedNode.Name == "rename")
            {
                if (MessageBox.Show("Are you sure to rename your computer?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string newPCName = textBoxNewName.Text.Trim();
                    if (newPCName != "" && newPCName.ToLower() != hostNameWithoutdomain.ToLower())
                    {
                        renamePCName(newPCName);
                    }
                    else
                    {
                        textBoxOutput.AppendText("Rename computer failed! Please input a different new computer name!" + Environment.NewLine);
                        MessageBox.Show("Rename computer failed! Please input a different new computer name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            else if (treeViewRenameAndJoinDomain.SelectedNode.Name == "joinDomain")
                joinDomain();
            else if (treeViewRenameAndJoinDomain.SelectedNode.Name == "renameAndJoinDomain")
            {
                if (currentDomain == "")
                {
                    if (MessageBox.Show("Are you sure to rename your computer and join new domain?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        string newDomain = textBoxNewDomain.Text.Trim();
                        string newHostName = textBoxNewName.Text.Trim();
                        string username = textBoxUsername.Text.Trim();
                        string password = textBoxPwd.Text.Trim();
                        if (newHostName != "" && newHostName.ToLower() != hostNameWithoutdomain.ToLower() && newDomain != "" && username != "" && password != "")
                        {
                            HandleJoinandRenameDomain(newDomain, newHostName, username, password);
                        }
                        else
                        {
                            textBoxOutput.AppendText("Rename and join new domain failed, please input a different new computer name, domain user name and password!" + Environment.NewLine);
                            MessageBox.Show("Rename and join new domain failed, please input a different new computer name, domain user name and password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    textBoxOutput.AppendText("Rename and join new domain failed, your computer is already in domain, please leave the current domain then join new domain!" + Environment.NewLine);
                    MessageBox.Show("Rename and join new domain failed, your computer is already in domain, please leave the current domain then join new domain!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an option on the left tree!","Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void treeViewRenameAndJoinDomain_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeViewRenameAndJoinDomain.SelectedNode.Name == "unjoinDomainAndRename")
            {
                textBoxNewName.ReadOnly = false;
                textBoxNewDomain.ReadOnly = true;
                textBoxUsername.ReadOnly = true;
                textBoxPwd.ReadOnly = true;
                checkBoxAddtoLocalAdmins.Enabled = false;
            }
            else if (treeViewRenameAndJoinDomain.SelectedNode.Name == "unjoinDomain")
            {
                textBoxNewName.ReadOnly = true;
                textBoxNewDomain.ReadOnly = true;
                textBoxUsername.ReadOnly = true;
                textBoxPwd.ReadOnly = true;
                checkBoxAddtoLocalAdmins.Enabled = false;
            }
            else if (treeViewRenameAndJoinDomain.SelectedNode.Name == "rename")
            {
                textBoxNewName.ReadOnly = false;
                textBoxNewDomain.ReadOnly = true;
                textBoxUsername.ReadOnly = true;
                textBoxPwd.ReadOnly = true;
                checkBoxAddtoLocalAdmins.Enabled = false;
            }
            else if (treeViewRenameAndJoinDomain.SelectedNode.Name == "joinDomain")
            {
                textBoxNewName.ReadOnly = true;
                textBoxNewDomain.ReadOnly = false;
                textBoxUsername.ReadOnly = false;
                textBoxPwd.ReadOnly = false;
                checkBoxAddtoLocalAdmins.Enabled = true;
            }
            else if (treeViewRenameAndJoinDomain.SelectedNode.Name == "renameAndJoinDomain")
            {
                textBoxNewName.ReadOnly = false;
                textBoxNewDomain.ReadOnly = false;
                textBoxUsername.ReadOnly = false;
                textBoxPwd.ReadOnly = false;
                checkBoxAddtoLocalAdmins.Enabled = true;
            }
        }
    }
}
