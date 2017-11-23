using System;
using System.Collections;
using System.Collections.Generic;

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
            Parser parser = new Parser();
            string trainingFileName = args[1];
            string testingFileName = args[2];
            ID3Data id3Data = parser.ParseID3InformationFile(trainingFileName);
            id3Data.ToString();

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
    /// Parser for the Training and Testing files data.  
    /// *An ID3 parser*
    /// </summary>
    class Parser
    {
        public ID3Data ParseID3InformationFile(string filename)
        {
            /**
            * !File Format!
            * !Do not read lines starting with '//'!
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
            string rawFileContents = GetFileAsString(filename);
            ArrayList cleanFileContents = RemoveCommentsAndEmptyLines(rawFileContents);
            int i = 0;
            ID3Data id3Data = new ID3Data();
            id3Data.Labels = new ArrayList() { 
                cleanFileContents[i++], 
                cleanFileContents[i++] // NOTE: This gets line 0 and 1 (the class labels)
                };
            for (int f = 1; f < (int)(cleanFileContents[i++]); f++, i++) // NOTE: This line is the number of features
            {
                string[] line = cleanFileContents[i].ToString().Split(' ');
                id3Data.Attributes.Add(line[0], new ArrayList() {
                    line[1],
                    line[2]
                });
            }
            for (int e = 0; e < (int)(cleanFileContents[i++]); e++, i++)
            {
                string[] line = System.Text.RegularExpressions.Regex.Split(cleanFileContents[i].ToString(), @"\s+");
                id3Data.TestData.Add(
                    new Data(
                            line[0],
                            line[1],
                            new ArrayList() {
                                line[2],
                                line[3],
                                line[4],
                                line[5]
                            }
                        )
                    );
            }
            return id3Data;
        }
        ArrayList RemoveCommentsAndEmptyLines(string fileAsString)
        {
            ArrayList output = new ArrayList();
            string[] fileContentsByLine = fileAsString.Split('\n');
            foreach (string line in fileContentsByLine)
            {
                if (line.StartsWith("//") || line.StartsWith("\n")) break; // Don't add the line to the list
                    // NOTE: Handles comment lines and empty lines
                else output.Add(line);
            }
            return output;
        }
        public string GetFileAsString(string filename)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
            string output = "";
            while(reader.Peek() >= 0)
            {
                output += reader.ReadLine();
            }
            return output;
        }
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
        /// <summary>
        /// ID3 Learning Tree Algorithm.
        /// </summary>
        /// <param name="examples">Example data</param>
        /// <param name="attributes">Attribute data</param>
        /// <returns>Decision tree built through examples and attributes</returns>
        public static Tree BuildID3Tree(string[] examples, string[] attributes) // REVIEW: Should these be something else?
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
        public override string ToString() // TODO: Write an overrided ToString() funtion to print to files for saving data
        {
            return base.ToString();
        }
    }
    public abstract class Node
    {
        public ArrayList children { get; set; }
        public int label { get; set; } // NOTE: either [1||positive] || [2||negative]
        public Node(int label)
        {
            this.label = label;
        }
    }
    public class Action : Node
    {
        public Action(int label): base(label)
        {

        }
        public void PerformAction()
        {
            // TODO: Something here?
        }
    }
    class State : Node
    {
        public State(int label): base(label)
        {
            
        }
        public bool Decision()
        {
            return true; // TODO: This is where decisions go on?
        }
    }
    /// <summary>
    /// Attribute class for the Data class
    /// </summary>
    class Attribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public Attribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
        public override string ToString()
        {
            return "Attribute [ Name: " + this.Name + ", Value: " + this.Value + " ]";
        }
    }
    class ID3Data
    {
        public ArrayList Labels { get; set; }
        public Dictionary<string, ArrayList> Attributes { get; set; }
        public ArrayList TestData { get; set; }
        public ID3Data() 
        {
            this.TestData = new ArrayList();
        }
        public override string ToString()
        {
            string output = "ID3Data [ Labels: ";
            foreach (string label in Labels) output += "\t" + label + ", \n";
            output += "],\nAttributes: [\n";
            foreach (KeyValuePair<string, ArrayList> kvp in this.Attributes)
            {
                output += "\t" + kvp.Key + ": {";
                foreach (Attribute a in kvp.Value) output += "\t\t" + a.ToString() + ",\n";
                output += "\t}\n";
            }
            output += "],\nData: [";
            foreach (Data d in this.TestData)
            {
                output += "\t" + d.ToString() + "\n";
            }
            return output + "]";
        }
    }
    /// <summary>
    /// Data class
    /// </summary>
    class Data
    {
        /// <summary>
        /// Name of the data.
        /// </summary>
        /// <returns>Data name</returns>
        public string Name { get; set; }
        /// <summary>
        /// Category of the data.
        /// </summary>
        /// <returns>Data category</returns>
        public string Category { get; set; }
        /// <summary>
        /// Attributes of the data.
        /// </summary>
        /// <returns>Data attributes</returns>
        public ArrayList Attributes { get; set; }
        /// <summary>
        /// Data object to hold the learned data from the tests data and learning tree data.
        /// </summary>
        /// <param name="Name">Data name</param>
        /// <param name="Category">Data category</param>
        /// <param name="Attributes">Data attributes</param>
        public Data(string Name, string Category, ArrayList Attributes)
        {
            this.Name = Name;
            this.Category = Category;
            this.Attributes = Attributes;
        }
        /// <summary>
        /// Prints the class to a human readable format.
        /// </summary>
        /// <returns>A human readable class string</returns>
        public override string ToString()
        {
            string output = "Data [ Name: " + this.Name + ",\n Category: " + this.Category + ",\n Attributes: ";
            foreach (Attribute a in this.Attributes) output += "\t" + a.ToString() + ",\n";
            output += "]\n";
            return output;
        }
    }
}
