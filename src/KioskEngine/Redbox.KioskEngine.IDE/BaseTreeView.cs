using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE;

public class BaseTreeView : TreeView
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
	internal struct TVITEM
	{
		public uint mask;

		public IntPtr hItem;

		public uint state;

		public uint stateMask;

		public IntPtr pszText;

		public int cchTextMax;

		public int iImage;

		public int iSelectedImage;

		public int cChildren;

		public IntPtr lParam;
	}

	internal const int TV_FIRST = 4352;

	internal const int TVIF_CHILDREN = 64;

	internal const int TVM_GETITEM = 4364;

	internal const int TVM_SETITEM = 4365;

	public void SetNodeButton(TreeNode treeNode, bool flag)
	{
		TVITEM lParam = new TVITEM
		{
			mask = 64u,
			hItem = treeNode.Handle,
			cChildren = (flag ? 1 : 0)
		};
		SendMessage(base.Handle, 4365u, 0u, ref lParam);
	}

	public bool GetNodeButton(TreeNode treeNode)
	{
		TVITEM lParam = new TVITEM
		{
			mask = 64u,
			hItem = treeNode.Handle
		};
		SendMessage(base.Handle, 4364u, 0u, ref lParam);
		return lParam.cChildren > 0;
	}

	public TreeNode FindTreeNode(TreeNodeCollection nodes, string label)
	{
		TreeNode result = null;
		foreach (TreeNode node in nodes)
		{
			if (!(node.Text != label))
			{
				result = node;
				break;
			}
		}
		return result;
	}

	public void SelectTreePath(string path)
	{
		if (path == null)
		{
			return;
		}
		TreeNode treeNode = null;
		TreeNode treeNode2 = null;
		string[] array = path.Split(base.PathSeparator.ToCharArray());
		foreach (string label in array)
		{
			treeNode = FindTreeNode((treeNode == null) ? base.Nodes : treeNode.Nodes, label);
			if (treeNode == null)
			{
				break;
			}
			treeNode2 = treeNode;
			treeNode.Expand();
		}
		base.SelectedNode = treeNode2;
	}

	[DllImport("user32.dll")]
	internal static extern uint SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, uint wParam, ref TVITEM lParam);
}
