using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CyPhyComponentFidelitySelector
{

    public partial class SpiceSelector : Form
    {
        private XElement root;
        Timer checkSelectedSpiceModelsDebounceTimer;

        public SpiceSelector(XElement root, FidelitySelectionRules selectors)
        {
            this.root = root;
            InitializeComponent();
            this.selectedTreeView.CheckBoxes = true;
            this.selectedTreeView.Nodes.Clear();
            TreeNode rootNode = null;
            Queue<Tuple<TreeNode, XElement>> elements = new Queue<Tuple<TreeNode, XElement>>();
            elements.Enqueue(new Tuple<TreeNode, XElement>(null, root));
            while (elements.Count > 0)
            {
                var tuple = elements.Dequeue();
                var parentTreeNode = tuple.Item1;
                var element = tuple.Item2;
                TreeNode node;
                if (parentTreeNode == null)
                {
                    node = rootNode = new TreeNode((string)element.Attribute("Name"));
                    this.selectedTreeView.Nodes.Add(rootNode);
                }
                else
                {
                    node = new TreeNode((string)element.Attribute("Name"));
                    parentTreeNode.Nodes.Add(node);
                }
                node.Tag = element;
                foreach (var child in element.Elements())
                {
                    elements.Enqueue(new Tuple<TreeNode, XElement>(node, child));
                }
                node.Expand();
            }
            this.CancelButton = cancelButton;
            this.AcceptButton = okButton;
            this.selectors = selectors;

            this.Load += SpiceSelector_Load;

            foreach (var selector in selectors.rules)
            {
                addXPath(this.xpathsPanel, selector);
            }
            selectors.rules.Add(new FidelitySelectionRules.SelectionRule()
            {
            });
            addXPath(this.xpathsPanel, selectors.rules.Last());

            EnableXPathsPanelButtons();

            checkSelectedSpiceModelsDebounceTimer = new Timer();
            checkSelectedSpiceModelsDebounceTimer.Interval = 100;
            checkSelectedSpiceModelsDebounceTimer.Tick += (obj, args) =>
            {
                CheckSelectedSpiceModels();
                checkSelectedSpiceModelsDebounceTimer.Stop();
            };
            this.FormClosing += (sender, e) =>
            {
                checkSelectedSpiceModelsDebounceTimer.Dispose();
            };

            // need to bold everything, then switch to regular, or the nodes are too short to display the bold string
            regularFont = new Font(selectedTreeView.Font, FontStyle.Regular);
            this.Disposed += (sender, e) =>
            {
                regularFont.Dispose();
            };
            selectedTreeView.CheckBoxes = false;
        }
        Font regularFont;

        private void CheckSelectedSpiceModels()
        {
            bool hasErrors = false;
            int i = 0;
            foreach (var xpath in selectors.rules.Where(xp => string.IsNullOrWhiteSpace(xp.xpath) == false))
            {
                xpathsPanel.Controls[i].BackColor = xpathsPanel.BackColor;
                try
                {
                    root.XPathSelectElements(xpath.xpath).OrderBy(el => (xpath.lowest ? -1 : 1) * el.Ancestors().Count());
                }
                catch
                {
                    hasErrors = true;
                    xpathsPanel.Controls[i].BackColor = Color.Red;
                }
                i++;
            }
            if (hasErrors)
            {
                okButton.Enabled = false;
                return;
            }
            okButton.Enabled = true;

            var rootNode = this.selectedTreeView.Nodes[0];
            var selected = FidelitySelectionRules.SelectElements(root, selectors);
            foreach (var node in getNodeAndDescendents(rootNode))
            {
                node.NodeFont = selected.Contains((XElement)node.Tag) ? selectedTreeView.Font : regularFont;
            }
        }

        IEnumerable<TreeNode> getNodeAndDescendents(TreeNode node)
        {
            Queue<TreeNode> nodes = new Queue<TreeNode>();
            nodes.Enqueue(node);
            while (nodes.Count > 0)
            {
                node = nodes.Dequeue();
                foreach (TreeNode childNode in node.Nodes)
                {
                    nodes.Enqueue(childNode);
                }
                yield return node;
            }
        }

        IEnumerable<TreeNode> getNodeAndAncestors(TreeNode node)
        {
            while (node != null)
            {
                yield return node;
                node = node.Parent;
            }
        }

        private void SpiceSelector_Load(object sender, EventArgs e)
        {
            foreach (var node in getNodeAndDescendents(selectedTreeView.Nodes[0]))
            {
                node.NodeFont = regularFont;
            }
            CheckSelectedSpiceModels();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (selectors.rules.Last().xpath == "")
            {
                selectors.rules.RemoveAt(selectors.rules.Count - 1);
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        const string downButtonName = "_downButton";
        const string upButtonName = "_upButton";
        const string delButtonName = "_delButton";
        private FidelitySelectionRules selectors;

        private void addXPath(Panel parent, FidelitySelectionRules.SelectionRule selector)
        {
            var xpathPanel = new System.Windows.Forms.Panel();
            var delButton = new Button();
            var upButton = new Button();
            var downButton = new Button();
            var xpathTextBox = new TextBox();
            xpathPanel.Controls.Add(delButton);
            xpathPanel.Controls.Add(upButton);
            xpathPanel.Controls.Add(downButton);
            xpathPanel.Controls.Add(xpathTextBox);

            xpathPanel.Location = new System.Drawing.Point(0, 10 + 25 * parent.Controls.Count - parent.VerticalScroll.Value);
            xpathPanel.Size = new System.Drawing.Size(parent.Size.Width - 17 /*leave room for scrollbar*/, 25);
            xpathPanel.Name = "xpathPanel" + parent.Controls.Count;
            xpathPanel.Tag = selector;

            delButton.Image = global::CyPhyComponentFidelitySelector.Properties.Resources.ic_close_black_18dp;
            delButton.Location = new System.Drawing.Point(1, 1);
            delButton.Name = xpathPanel.Name + delButtonName;
            delButton.UseVisualStyleBackColor = true;
            delButton.Size = new System.Drawing.Size(22, 23);
            delButton.UseVisualStyleBackColor = true;
            delButton.TabStop = false;
            delButton.Click += DelButton_Click;

            upButton.Image = global::CyPhyComponentFidelitySelector.Properties.Resources.ic_expand_less_black_18dp;
            upButton.Location = new System.Drawing.Point(delButton.Location.X + delButton.Width + 2, 1);
            upButton.Name = xpathPanel.Name + upButtonName;
            upButton.UseVisualStyleBackColor = true;
            upButton.Size = new System.Drawing.Size(22, 23);
            upButton.UseVisualStyleBackColor = true;
            upButton.TabStop = false;
            upButton.Click += UpButton_Click;

            downButton.Image = global::CyPhyComponentFidelitySelector.Properties.Resources.ic_expand_more_black_18dp;
            downButton.Location = new System.Drawing.Point(upButton.Location.X + upButton.Width + 2, 1);
            downButton.Name = xpathPanel.Name + downButtonName;
            downButton.UseVisualStyleBackColor = true;
            downButton.Size = new System.Drawing.Size(22, 23);
            downButton.UseVisualStyleBackColor = true;
            downButton.TabStop = false;
            downButton.Click += DownButton_Click;

            xpathTextBox.Location = new System.Drawing.Point(downButton.Location.X + downButton.Width + 2, 2);
            xpathTextBox.Name = xpathPanel.Name + "_xpathBox";
            xpathTextBox.Size = new System.Drawing.Size(433, 22);
            xpathTextBox.Text = selector.xpath;
            xpathTextBox.TextChanged += XpathTextBox_TextChanged;
            var source = new AutoCompleteStringCollection();
            source.AddRange(new string[]
            {
                // TODO: generate this list based on a suggested set of queries and the current state of the text box.
                "//*/Component/SpiceModel",
                // "//*/SpiceModel[@Fidelity>1]",
                "//*/SpiceModel[@Name='SPICEModel']",
                "/*/*/*/SpiceModel",
                "//SpiceModel[count(ancestor::*) < 2]",
                "//*/Component[contains(@Classifications, 'resistor')]/SpiceModel",
                "//*[@Name]='GMEName'/SpiceModel",
                "//ComponentAssembly[./*[@Name='ChildGMEName']]/SpiceModel",
            });
            xpathTextBox.AutoCompleteCustomSource = source;
            xpathTextBox.AutoCompleteMode = AutoCompleteMode.Suggest;
            xpathTextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;

            var depthSelector = new Button();
            xpathPanel.Controls.Add(depthSelector);
            SetDepthSelectorState(selector, depthSelector);
            depthSelector.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            depthSelector.Location = new System.Drawing.Point(xpathTextBox.Location.X + xpathTextBox.Width + 2, 1);
            xpathTextBox.Name = xpathPanel.Name + "_depthSelector";
            depthSelector.Size = new System.Drawing.Size(34, 23);
            depthSelector.UseVisualStyleBackColor = true;
            depthSelector.TabStop = false;
            depthSelector.Click += DepthSelector_Click;

            xpathPanel.TabIndex = parent.Controls.Count;
            parent.Controls.Add(xpathPanel);
        }

        private static void SetDepthSelectorState(FidelitySelectionRules.SelectionRule selector, Button depthSelector)
        {
            depthSelector.BackgroundImage = selector.lowest ?
                global::CyPhyComponentFidelitySelector.Properties.Resources.tiny_tree_leaf :
                global::CyPhyComponentFidelitySelector.Properties.Resources.tiny_tree_root;
        }

        private void DepthSelector_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Panel panel = (Panel)button.Parent;
            var index = xpathsPanel.Controls.IndexOf(panel);
            var selector = (FidelitySelectionRules.SelectionRule)panel.Tag;

            selector.lowest = !selector.lowest;

            SetDepthSelectorState(selector, button);
            SignalCheckSelectedSpiceModels();
        }

        private void XpathTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;
            Panel panel = (Panel)box.Parent;
            var index = xpathsPanel.Controls.IndexOf(panel);
            var selector = (FidelitySelectionRules.SelectionRule)panel.Tag;
            if (selector.xpath == "")
            {
                selectors.rules.Add(new FidelitySelectionRules.SelectionRule()
                {
                });
                addXPath(this.xpathsPanel, selectors.rules.Last());
                EnableXPathsPanelButtons();
            }
            selector.xpath = box.Text;
            SignalCheckSelectedSpiceModels();
        }

        private void SignalCheckSelectedSpiceModels()
        {
            checkSelectedSpiceModelsDebounceTimer.Stop();
            checkSelectedSpiceModelsDebounceTimer.Start();
        }


        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        private void DelButton_Click(object sender, EventArgs e)
        {
            Panel panel = (Panel)((Button)sender).Parent;
            var index = xpathsPanel.Controls.IndexOf(panel);
            var oldLocation = panel.Location;
            xpathsPanel.Controls.Remove(panel);

            foreach (var panelBelow in xpathsPanel.Controls.Cast<Control>().Skip(index))
            {
                var tmp = panelBelow.Location;
                panelBelow.Location = oldLocation;
                oldLocation = tmp;
            }
            selectors.rules.RemoveAt(index);

            EnableXPathsPanelButtons();
            SignalCheckSelectedSpiceModels();
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            Panel panel = (Panel)((Button)sender).Parent;
            var index = xpathsPanel.Controls.IndexOf(panel);
            Down((Panel)xpathsPanel.Controls[index - 1]);
        }

        private void DownButton_Click(object sender, EventArgs e)
        {
            Panel panel = (Panel)((Button)sender).Parent;
            Down(panel);
        }

        private void Down(Panel panel)
        {
            var index = xpathsPanel.Controls.IndexOf(panel);
            var oldLocation = panel.Location;
            panel.Location = xpathsPanel.Controls[index + 1].Location;
            xpathsPanel.Controls[index + 1].Location = oldLocation;
            xpathsPanel.Controls[index + 1].TabIndex = index;
            panel.TabIndex = index + 1;
            xpathsPanel.Controls.SetChildIndex(panel, index + 1);

            selectors.rules[index] = selectors.rules[index + 1];
            selectors.rules[index + 1] = (FidelitySelectionRules.SelectionRule)panel.Tag;

            EnableXPathsPanelButtons();
            SignalCheckSelectedSpiceModels();
        }

        private void EnableXPathsPanelButtons()
        {
            xpathsPanel.Controls[xpathsPanel.Controls.Count - 1].Controls.Cast<Control>().Where(c => c.Name.EndsWith(downButtonName)).First().Enabled = false;
            xpathsPanel.Controls[xpathsPanel.Controls.Count - 1].Controls.Cast<Control>().Where(c => c.Name.EndsWith(upButtonName)).First().Enabled = false;
            xpathsPanel.Controls[xpathsPanel.Controls.Count - 1].Controls.Cast<Control>().Where(c => c.Name.EndsWith(delButtonName)).First().Enabled = false;
            if (xpathsPanel.Controls.Count > 1)
            {
                xpathsPanel.Controls[0].Controls.Cast<Control>().Where(c => c.Name.EndsWith(upButtonName)).First().Enabled = false;
                xpathsPanel.Controls[xpathsPanel.Controls.Count - 2].Controls.Cast<Control>().Where(c => c.Name.EndsWith(delButtonName)).First().Enabled = true;
                xpathsPanel.Controls[xpathsPanel.Controls.Count - 2].Controls.Cast<Control>().Where(c => c.Name.EndsWith(upButtonName)).First().Enabled = true;
                xpathsPanel.Controls[xpathsPanel.Controls.Count - 2].Controls.Cast<Control>().Where(c => c.Name.EndsWith(downButtonName)).First().Enabled = false;
                if (xpathsPanel.Controls.Count > 2)
                {
                    xpathsPanel.Controls[1].Controls.Cast<Control>().Where(c => c.Name.EndsWith(upButtonName)).First().Enabled = true;
                    xpathsPanel.Controls[xpathsPanel.Controls.Count - 3].Controls.Cast<Control>().Where(c => c.Name.EndsWith(downButtonName)).First().Enabled = true;
                }
            }
        }
    }

}
