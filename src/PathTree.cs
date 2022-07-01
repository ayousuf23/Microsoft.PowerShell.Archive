using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    internal class PathTree
    {

        private Node sentinel;

        public PathTree()
        {
            sentinel = new Node("Sentinel")
            {
                IsDirectory = true,
            };
        }

        public void AddPath(string path)
        {
            string[] pathComponents = path.Split(System.IO.Path.DirectorySeparatorChar);
            Node root = sentinel;

            for (int i = 0; i < pathComponents.Length; i++) 
            {       
                var component = pathComponents[i];
                if (root.Children.TryGetValue(component, out var child))
                {
                    root = child;
                }
                else
                {
                    Node newNode = new Node(component)
                    {
                        IsDirectory = (i == pathComponents.Length - 1) ? path.EndsWith(System.IO.Path.DirectorySeparatorChar) : true,
                    };
                    root.Children.Add(component, newNode);
                    root = newNode;
                }
            }

            //Make last component available
            root.IsAvailable = true;
        }

        public string GetAvailablePath(string path)
        {
            string resultPath = "";
            string[] pathComponents = path.Split(System.IO.Path.DirectorySeparatorChar);
            Node root = sentinel;

            for (int i = 0; i < pathComponents.Length; i++)
            {
                var component = pathComponents[i];
                if (root.Children.TryGetValue(component, out var child))
                {
                    root = child;

                    if (root.IsAvailable)
                    {
                        resultPath += root.Name;
                        if (root.IsDirectory) resultPath += System.IO.Path.AltDirectorySeparatorChar;

                        //If root is available, add it to path
                        resultPath = System.IO.Path.Combine(resultPath, component);
                    }
                    
                    
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            return resultPath;
        }
    }

    internal class Node
    {
        public string Name { get; set; }

        public Dictionary<string, Node> Children { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsDirectory { get; set; }

        public Node(string name)
        {
            Name = name;
            Children = new Dictionary<string, Node>();
        }
    }
}
