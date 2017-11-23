using System;
using System.Collections;

namespace LearningDecisionTree
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2) // NOTE: Check argument length
            {
                PrintHelp();
                return;
            }

        }
        /// <summary>
        /// ID3 Learning Tree Algorithm.
        /// </summary>
        /// <param name="examples">Example data</param>
        /// <param name="attributes">Attribute data</param>
        /// <returns>Decision tree built through examples and attributes</returns>
        public static Tree ID3(string[] examples, string[] attributes) // REVIEW: Should these be something else?
        {
            Tree t = null;
            /**
             *  if all examples in same catgorty then:
             *      return a leaf node with that category
             *  if attributes is empty then:
             *      return a leaf node with the most common category in examples
             *  best = Choose-Attribute(examples, attributes)
             *  tree = new Treee with Best as root attribute test
             *  foreach value v of best:
             *      examples = subset of examples with best == v
             *      subtree = ID3(examples, attributes - best)
             *      add a branch to tree with best == v && subtree beneath
             *  return tree
             */
            return t;
        }
        /// <summary>
        /// Prints the help command to the console for the user.
        /// </summary>
        public static void PrintHelp()
        {
            Console.Write("\n\t------------\n\tUsage: LearningDecisionTree.exe <Training set filename> <Test set filename>\n");
            Console.Write("\n\tThe command will ignore all extrenuous commands.\n");
        }
    }
    /// <summary>
    /// Parser for the Training and Testing files data
    /// </summary>
    class Parser
    {
        public Parser()
        {

        }
        /**
         * !File Format!
         * !Do not read lines starting with '\\\\' (\\; have to esacpe them)!
         * \SOF
         * class labels (could be any number; assume 2 for testing purposes)
         * # of features (nF) (how many to read in)
         * feature1 (list starts)
         * ... -> nF (list ends)
         * # of examles (nE) (how many to read in)
         * example1 (list starts)
         * ... -> nE (list ends)
         * \EOF
         */
    }
    class Tree
    {
        Node Root { get; set; }
        public Tree()
        {

        }
        public Tree(Node root)
        {
            this.Root = root;
        }
        public override string ToString() // TODO: Write an overrided ToString() funtion to print to files for saving data
        {
            return base.ToString();
        }
    }
    class Node
    {
        public Node()
        {
            
        }
    }
    class Attribute
    {
        string Name { get; set; }
        ArrayList Values { get; set; }
        public Attribute(string NameType, params string[] values)
        {
            this.Name = NameType;
            foreach (string value in values)
            {
                this.Values.Add(value);
            }
        }
    }
}
