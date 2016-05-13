using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace ScriptRunner.Node
{
     public class ScriptNode : INotifyPropertyChanged
     {       

        #region Data

        bool? _isChecked = false;
        public ScriptNode _parent;

        public static string rootpath = "";
        private string nodepath;

        #endregion // Data

        #region CreateNode

        public static List<ScriptNode> CreateNode()
        {           
            ScriptNode root = new ScriptNode("ROOT FOLDER");     
            root.IsInitiallySelected = true;
            if (rootpath != "")
            {               
                root = treeConstruction(root, rootpath);
                root.Name = rootpath.Substring(rootpath.LastIndexOf("\\") + 1);
                root.Initialize();
                return new List<ScriptNode> { root };    
            }
            else
            {
                root.Name = "EMPTY";
                root.Initialize();
                return new List<ScriptNode> { root };
            }
        }
             
        private static ScriptNode treeConstruction(ScriptNode rt, string path)
        {
            ScriptAvail = false;
            ScriptNode branch = rt;
            for(int i=0; i < Directory.GetDirectories(path).Length; i++)
            {
                string s = Directory.GetDirectories(path)[i];                
                branch.Children.Add(new ScriptNode(s.Substring(s.LastIndexOf("\\") + 1)));
                if (Directory.GetDirectories(s).Length > 0 || Directory.GetFiles(s).Length > 0)
                  branch.Children[i] = newbranch(s);

                for (int j = 0; j < Directory.GetFiles(path).Length; j++)
                {
                    string w = Directory.GetFiles(path)[j];
                    string filename = w.Substring(w.LastIndexOf("\\") + 1);
                    if (filename.EndsWith(".sql", StringComparison.Ordinal))
                    { branch.Children.Add(new ScriptNode(filename)); ScriptAvail = true; }
                }   
            }

            for (int j = 0; j < Directory.GetFiles(path).Length; j++)
            {
                string w = Directory.GetFiles(path)[j];
                string filename = w.Substring(w.LastIndexOf("\\") + 1);
                if (filename.EndsWith(".sql", StringComparison.Ordinal))
                { branch.Children.Add(new ScriptNode(filename,w)); ScriptAvail = true; }
            }   
            return branch;
        }

        private static ScriptNode newbranch(string s)
        {
            ScriptNode subbranch = new ScriptNode(s.Substring(s.LastIndexOf("\\") + 1));            
            int m = Directory.GetDirectories(s).Length;
            for (int j = 0; j < Directory.GetDirectories(s).Length; j++)
            {
                string p = Directory.GetDirectories(s)[j];
                subbranch.Children.Add(new ScriptNode(p.Substring(p.LastIndexOf("\\") + 1)));
                if (Directory.GetDirectories(p).Length > 0 || Directory.GetFiles(p).Length > 0)
                    subbranch.Children[j] = newbranch(p);
            }


            for (int i = 0; i < Directory.GetFiles(s).Length; i++)
            {
                string k = Directory.GetFiles(s)[i];
                string filename = k.Substring(k.LastIndexOf("\\") + 1);
                if (filename.EndsWith(".sql", StringComparison.Ordinal))
                { subbranch.Children.Add(new ScriptNode(filename,k)); ScriptAvail = true; }
            }

            return subbranch;
        }


        public ScriptNode(string name)
        {
            this.Name = name;
            this.Children = new List<ScriptNode>();
        }

        ScriptNode(string rname, string pth)
        {
            this.Name = rname;
            this.nodepath = pth;
            this.Children = new List<ScriptNode>();
        } 

        void Initialize()
        {
            foreach (ScriptNode child in this.Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        #endregion // CreateNode

        #region Properties

        public List<ScriptNode> Children { get; private set; }

        public bool IsInitiallySelected { get; private set; }

        private static bool ScriptAvail = false;
        public bool getScriptAvail
        {
            get { return ScriptAvail; }
        }

        public string Name { get; private set; }

        public string pathnode
        {
            get
            { return nodepath; }
            set
            { nodepath = value; }
        }

        List<string> checklist = new List<string>();


        #region CheckList
       public List<string> GetCheckStateList(ScriptNode cn)
        {
            for (int i = 0; i < cn.Children.Count; ++i)
            {
                bool nd_s = false;
                if (cn.Children[i].IsChecked == null)
                    nd_s = true;
                else
                    nd_s = (bool)cn.Children[i].IsChecked;

                if (nd_s && ((cn.Children[i].Name).EndsWith(".sql", StringComparison.Ordinal)))
                {
                    checklist.Add(cn.Children[i].pathnode);                  
                }

                if (cn.Children[i].Children.Count > 0)
                    GetCheckStateList(cn.Children[i]);
            }
            return checklist;
        }
        #endregion CheckList


        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child ScriptNodes.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }

         //added for unchecking all nodes starts

        public void SetUnChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;
            _isChecked = value;            
                int k = this.Children.Count;
                this.Children.ForEach( c => c.SetIsChecked(_isChecked, false, false));
                childUnchecked(this.Children);

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }

         void childUnchecked(List<ScriptNode> temp)
        {
            List<ScriptNode> ck = temp;
            //ck.ForEach(c => c.SetIsChecked(_isChecked, false, false));
            if(ck.Count > 0)
            {
             for(int i = 0; i < ck.Count; i++)
             {
                 ScriptNode tempnode = ck[i];
                 tempnode.SetIsChecked(_isChecked, false, false);
                 ck[i].childUnchecked(ck[i].Children);
             }
            }
        }

        //added for unchecking all nodes ends

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion // Properties

        #region INotifyPropertyChanged Members

        public void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

       

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
