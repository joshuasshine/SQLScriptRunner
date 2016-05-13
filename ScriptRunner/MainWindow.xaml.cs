using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Documents;
using System.Collections.Generic;

using System.Threading;
using System.Text.RegularExpressions;
using System.Data.Sql;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using ScriptRunner.Node;
using ScriptRunner.Control;

namespace ScriptRunner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : Window
    {
        public delegate void OnWorkerMethodCompleteDelegate();
        public event OnWorkerMethodCompleteDelegate OnWorkerComplete;
        private ScriptExecution pbw;

        public string FileText { get; set; }
        private bool scriptavail = false;
        private string loadedta;
        List<string> filesE;

        bool ValidDB = false;
        List<string> DB = new List<string>();
        Hashtable MT = new Hashtable();
        bool upgradationDB = false;

        public string currentpath = "";
        public string currentfile = "";

        public MainWindow()
        {
            InitializeComponent();
            this.saveFile.IsEnabled = false;
            this.saveAsFile.IsEnabled = false;
            this.unCheckAll.IsEnabled = false;
            this.runScript.IsEnabled = false;
            this.updateScript.IsEnabled = false;
            LoadTreeView();
        }


        /// <summary>
        /// Load files and folder in Treeview 
        /// </summary>
        private void LoadTreeView()
        {
            ScriptNode root = this.tree.Items[0] as ScriptNode;
            base.CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, (sender, e) => // Execute
            {
                e.Handled = true;
                root.IsChecked = false;
                this.tree.Focus();
            },
                    (sender, e) => // CanExecute
                    {
                        e.Handled = true;
                        e.CanExecute = (root.IsChecked != false);
                    }));

            this.tree.Focus();
            scriptavail = root.getScriptAvail;
        }



        /// <summary>
        /// sql file node selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            System.Windows.Controls.TreeView tree = (System.Windows.Controls.TreeView)sender;
            ScriptNode temp = ((ScriptNode)tree.SelectedItem);
            //string path = "";
            if (temp == null)
                return;
            else if (!(temp.Name.EndsWith(".sql", StringComparison.Ordinal)))
            {
                ScriptPara.Inlines.Clear();
                this.saveFile.IsEnabled = false;
                this.saveAsFile.IsEnabled = false;
                return;
            }
            else
            {
                if (temp._parent != null)
                {
                    currentpath = temp.pathnode;
                    ScriptPara.Inlines.Clear();
                    FileText = File.ReadAllText(currentpath);
                    ScriptPara.Inlines.Add(FileText);
                    loadedta = StringFromRichTextBox(SQLScript);
                    this.saveFile.IsEnabled = true;
                    this.saveAsFile.IsEnabled = true;
                }
                else
                    return;
            }
        }


        #region MenuEvents
        private void Source_Path(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new FolderBrowserDialog();
                DialogResult result = dlg.ShowDialog();
                if (result.ToString() == "OK")
                    System.Windows.Forms.MessageBox.Show("Selected Path is " + dlg.SelectedPath.ToString(), "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    return;
                string s = dlg.SelectedPath.ToString();
                ScriptNode.rootpath = s;

                tree.Resources.Clear();
                ScriptPara.Inlines.Clear();

                FolderLocation.Refresh();
                LoadTreeView();
                if (!scriptavail)
                    System.Windows.Forms.MessageBox.Show("Selected Path has no SQL Script file, listed are just Folders", "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                else
                {
                    this.runScript.IsEnabled = true;
                    this.updateScript.IsEnabled = true;
                    this.unCheckAll.IsEnabled = true;
                }

            }
            catch (Exception ex) { ex.ToString(); tree.Resources.Clear(); FolderLocation.Refresh(); System.Windows.Forms.MessageBox.Show("Folder Access permission denied, Please check log for details", "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
        }

        private void Save_File(object sender, RoutedEventArgs e)
        {

            string script = StringFromRichTextBox(SQLScript);
            if ((script != "\r\n") || (script != "") && currentpath.EndsWith(".sql", StringComparison.Ordinal))
            {

                string comp = FileText;

                if (loadedta.Length != script.Length)
                {
                    File.WriteAllText(currentpath, string.Empty);
                    File.WriteAllText(currentpath, script);
                    System.Windows.Forms.MessageBox.Show("Change has been saved to the Script file", "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }


        private void UnCheckAll(object sender, RoutedEventArgs e)
        {
            ScriptNode root = this.tree.Items[0] as ScriptNode;
            root.SetUnChecked(false, false, false);
        }

        private void Run_Scripts(object sender, RoutedEventArgs e)
        {
            ScriptNode root = this.tree.Items[0] as ScriptNode;
            filesE = root.GetCheckStateList(root);
            if (scriptavail)
            {
                if (filesE.Count > 0)
                {
                    OnWorkerMethodStart(CheckCondition);
                    if (ValidDB)
                        OnWorkerMethodStart(scriptrun);
                }
                else
                    System.Windows.Forms.MessageBox.Show("Please select SQL Script file to Execute", "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
                System.Windows.Forms.MessageBox.Show("There is no SQL Script file to Execute", "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }





        private void Update_Scripts(object sender, RoutedEventArgs e)
        {

            ScriptNode root = this.tree.Items[0] as ScriptNode;
            filesE = root.GetCheckStateList(root);

            if (scriptavail)
            {
                if (filesE.Count > 0)
                {
                   OnWorkerMethodStart(CheckCondition);
                    if (ValidDB)
                    {
                        if (CollectMTID())
                        {
                            upgradationDB = true;
                            OnWorkerMethodStart(scriptrun);
                        }
                    }

                }
                else
                    System.Windows.Forms.MessageBox.Show("Please select SQL Script file to Execute", "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
                System.Windows.Forms.MessageBox.Show("There is no SQL Script file to Execute", "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            upgradationDB = false;
        }



        private void SaveAs_File(object sender, RoutedEventArgs e)
        {
            string script = StringFromRichTextBox(SQLScript);

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Script"; // Default file name
            dlg.DefaultExt = ".sql"; // Default file extension
            dlg.Filter = "SQL File (.sql)|*.sql"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                File.WriteAllText(filename, script);
                System.Windows.Forms.MessageBox.Show("SQL Script file been saved", "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion





        /// <summary>
        /// To convert richtextbox content to string
        /// </summary>
        /// <param name="rtb"></param>
        /// <returns></returns>
        private string StringFromRichTextBox(System.Windows.Controls.RichTextBox rtb)
        {
            System.Windows.Documents.TextRange textRange = new TextRange(
                // TextPointer to the start of content in the RichTextBox.
                rtb.Document.ContentStart,
                // TextPointer to the end of content in the RichTextBox.
                rtb.Document.ContentEnd
            );

            // The Text property on a TextRange object returns a string 
            // representing the plain text content of the TextRange. 
            return textRange.Text;
        }


        ///
        ///check DB server Avail
        ///
        private void CheckCondition()
        {
            SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
            System.Data.DataTable table = instance.GetDataSources();

            foreach (System.Data.DataRow row in table.Rows)
            {
                if ((row[0].ToString() == Environment.MachineName) && (row[1].ToString() == System.Configuration.ConfigurationManager.AppSettings["DBName"]))
                    ValidDB = true;
            }
            OnWorkerComplete();
            if (!ValidDB)
            {
                System.Windows.Forms.MessageBox.Show("There is no SQL Server or Instance to execute the script", "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// To run the script
        /// </summary>
        private void scriptrun()
        {
            string logsDirectory = Path.Combine(Environment.CurrentDirectory, "logs");

            Directory.CreateDirectory(logsDirectory);

            string saveNow = (((DateTime.Now.ToString()).Replace("/", "-")).Replace(" ", "_")).Replace(":", "_");
            string path = Path.Combine(logsDirectory, saveNow + "_log.txt");
            StreamWriter writr = new StreamWriter(path);

            string dname = System.Configuration.ConfigurationManager.AppSettings["DBName"];
            bool success = true;

            foreach (string file in filesE)
            {
                ////  Base tenant execution

                //success = ScriptRunner.DBProcess.DBProcess.executeProcess(writr, dname, success, file);
                if (!success)
                    break;


                //// check file is for Upgrade

                if (upgradationDB)
                {
                    string tempDirectory = Path.Combine(Environment.CurrentDirectory, "Temp");
                    Directory.CreateDirectory(tempDirectory);

                    foreach (DictionaryEntry entry in MT)
                    {
                        if ((bool)entry.Value)
                        {
                            string tempcreatingfilename = "";
                            string readText = File.ReadAllText(file);


                            if ((Path.GetFileNameWithoutExtension(file)).EndsWith("_MT"))
                                tempcreatingfilename = Path.GetFileNameWithoutExtension(file).Replace("MT", "_MT_" + entry.Key + ".sql");
                            else
                                tempcreatingfilename = Path.GetFileNameWithoutExtension(file) + ("_" + entry.Key + ".sql");


                            string newpath = tempDirectory + "\\" + tempcreatingfilename;


                            if (!File.Exists(newpath))
                            {
                                string replacedtxt = "";

                                for (int k = 0; k < DB.Count; k++)
                                {
                                    Regex replaceRegex = ScriptRunner.RegExp.RegExpression.GetRegExpression(DB[k]);
                                    String replacedString;
                                    if (k == 0)
                                        replacedString = replaceRegex.Replace(readText, "[" + DB[k] + "_" + entry.Key + "]");
                                    else
                                        replacedString = replaceRegex.Replace(replacedtxt, "[" + DB[k] + "_" + entry.Key + "]");
                                    replacedtxt = replacedString;
                                }
                                if (readText != replacedtxt)
                                    File.WriteAllText(newpath, replacedtxt);
                            }

                            //success = ScriptRunner.DBProcess.DBProcess.executeProcess(writr, dname, success, newpath);
                            if (!success)
                                goto EndExec;
                        }

                    }
                    if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["TempDelete"]))
                        Directory.Delete(tempDirectory, true);
                }

            }
            EndExec:
            writr.Close();
           OnWorkerComplete();
            FinalState(success);
            ValidDB = false;
        }


        /// <summary>
        /// Method call once all Script file got executed successfuly
        /// </summary>
        /// <param name="success"></param>
        private void FinalState(bool success)
        {
            if (success)
            {
                System.Windows.Forms.MessageBox.Show("Scripts run successfully", "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ScriptNode root = this.tree.Items[0] as ScriptNode;
                root.SetUnChecked(false, false, false);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Failed to execute, Check log for details", "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        /// <summary>
        /// Method for all background process meanwhile "On Process" window will be pop up 
        /// </summary>
        /// <param name="method"></param>       
        private void OnWorkerMethodStart(Action method)
        {
            this.OnWorkerComplete += new OnWorkerMethodCompleteDelegate(OnWorkerMethodComplete);
            ThreadStart tStart = new ThreadStart(method);
            Thread t = new Thread(tStart);
            t.Start();

            pbw = new ScriptExecution();
            pbw.Owner = this;
            pbw.ShowDialog();
        }

        /// <summary>
        /// Once background process complete, method been called to close the "On Process" pop up
        /// </summary>
        private void OnWorkerMethodComplete()
        {
            pbw.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate () { pbw.Close(); }));
        }




        /// <summary>
        /// To do collect all Multitenant from database
        /// </summary>
        /// <returns></returns>

        private bool CollectMTID()
        {
            Tenants tnt = null;
            //if (MT.Count < 1)
            //{
            //    MT.Add("20121", false);
            //    MT.Add("33333", false);
            //    MT.Add("44444", false);
            //}
            try
            {
                string ServerName = System.Environment.MachineName + "\\" + System.Configuration.ConfigurationManager.AppSettings["DBName"];
                SqlConnectionStringBuilder connection = new SqlConnectionStringBuilder();
                connection.DataSource = ServerName;
                connection.IntegratedSecurity = true;
                String strConn = connection.ToString();
                SqlConnection sqlConn = new SqlConnection(strConn);
                sqlConn.Open();
                DataTable databases = sqlConn.GetSchema("Databases");
                sqlConn.Close();

                if (databases != null)
                {
                    MT.Add("BASE DataBase",true);
                    foreach (DataRow row in databases.Rows)
                    {
                        string dbName = row[0].ToString();
                        if ((dbName.StartsWith("AP") || dbName.StartsWith("Ap")) && (dbName.Length > 30))
                        {
                            if (!DB.Contains(dbName.Split('_')[0]))
                                DB.Add(dbName.Split('_')[0]);
                            if (!MT.ContainsKey(dbName.Split('_')[1]))
                                MT.Add(dbName.Split('_')[1], false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string exception = ex.Message;
                System.Windows.Forms.MessageBox.Show(exception, "Script Runner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            tnt = new Tenants(MT);
            tnt.Owner = this;
            if (tnt.ShowDialog() == true)
            {
                MT = tnt.status;
                return true;
            }
            else
                return false;
        }
    }
}




