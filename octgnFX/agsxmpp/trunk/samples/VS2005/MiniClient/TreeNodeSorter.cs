using System;
using System.Windows.Forms;
using System.Collections;
using System.Text;

namespace MiniClient
{
    public class TreeNodeSorter : IComparer
    {
        // Compare the length of the strings, or the strings
        // themselves, if they are the same length.
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;

            // If they are the same length, call Compare.
            return string.Compare(tx.Text, ty.Text);
        }
    }
}

