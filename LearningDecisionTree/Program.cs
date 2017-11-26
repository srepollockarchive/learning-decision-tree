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
            string trainingFileName = args[0];
            string testingFileName = args[1];
            ID3Data id3Data = parser.ParseID3InformationFile(trainingFileName);
            Console.Write(id3Data.ToString());

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
        /// <summary>
        /// Translate the file data to ID3 data.
        /// </summary>
        /// <param name="filename">Filename to read from</param>
        /// <returns>ID3Data object</returns>
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
            id3Data.Categories = new ArrayList() { 
                cleanFileContents[i++], 
                cleanFileContents[i++] // NOTE: This gets line 0 and 1 (the class labels)
                };
            int attributeCount = Int32.Parse(cleanFileContents[i++].ToString());
            for (int f = 0; f < attributeCount; f++, i++) // NOTE: This line is the number of features
            {
                string[] line = cleanFileContents[i].ToString().Split(' ');
                id3Data.Attributes.Add(line[0], new ArrayList() {
                    line[1],
                    line[2]
                });
            }
            int dataItemCount = Int32.Parse(cleanFileContents[i++].ToString());
            for (int e = 0; e < dataItemCount; e++, i++)
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
        /// <summary>
        /// Removes all comment and empty lines.
        /// </summary>
        /// <param name="fileAsString">File as a string</param>
        /// <returns>File as an arraylist of lines</returns>
        ArrayList RemoveCommentsAndEmptyLines(string fileAsString)
        {
            ArrayList output = new ArrayList();
            string[] fileContentsByLine = fileAsString.Split('\n');
            foreach (string line in fileContentsByLine)
            {
                if (line.StartsWith("//") || line.StartsWith("\n") || line == "") continue; // Don't add the line to the list
                    // NOTE: Handles comment lines and empty lines
                else output.Add(line);
            }
            return output;
        }
        /// <summary>
        /// Gets the file as a single string.
        /// </summary>
        /// <param name="filename">Filename to read from</param>
        /// <returns>File as a string</returns>
        public string GetFileAsString(string filename)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
            string output = "";
            while(reader.Peek() >= 0)
            {
                output += reader.ReadLine() + "\n";
            }
            return output;
        }
    }
    public class Tree
    {
        public Tree()
        {

        }
        public void ID3(ArrayList examples, ArrayList attributes)
        {
            /**
                if all instances in examples are class P:
                    create node P and stop
                else:
                    select an attribute and create a decision node
                    partition C into subsets according to values of V
                    apply recursively to each of the subsets
             */
             
        }
        double Entropy(ArrayList examples)
        {
            double output = 0;
            Dictionary<string, int> dictionary = SummarizeExamples(examples, targetAttribute);
            foreach (KeyValuePair<string, int> kvp in dictionary)
            {
                float proportion = dictionary[kvp.Key] / examples.Count;
                output -= proportion * Math.Log(proportion, 2);
            }
            return output;
        }
        Dictionary<string, int> SummarizeExamples(ArrayList examples, string targetAttribute)
        {
            Dictionary<string, int> output = new Dictionary<string, int>();
            foreach (Data example in examples)
            {
                foreach (string value in example.Attributes)
                {
                    output[value] += 1;
                }
            }
            return output;
        }
    }
    public abstract class Node
    {
        /// <summary>
        /// Label for the node
        /// </summary>
        public string Category { get; set; } 
        public ArrayList Children { get; set; }
        public Node(string category)
        {
            this.Category = category;
        }
        public void AddBranch(Tree root)
        {
            this.Children.Add(root);
        }
    }
    /// <summary>
    /// Represents a choice between a number of alternatives.
    /// </summary>
    public class DecisionNode: Node
    {
        public DecisionNode(string category): base(category) {}
        public bool Decision()
        {
            return true; // TODO: Some decision happens here
        }
    }
    /// <summary>
    /// Represents a classification or decision.
    /// </summary>
    public class LeafNode: Node
    {
        public LeafNode(string category): base(category) {}
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
        public ArrayList Categories { get; set; }
        public Dictionary<string, ArrayList> Attributes { get; set; }
        public ArrayList TestData { get; set; }
        public ID3Data() 
        {
            this.Attributes = new Dictionary<string, ArrayList>();
            this.TestData = new ArrayList();
        }
        public override string ToString()
        {
            string output = "ID3Data [ \nLabels: \n";
            foreach (string label in this.Categories) output += "\t" + label + ", \n";
            output += "],\nAttributes: [\n";
            foreach (KeyValuePair<string, ArrayList> kvp in this.Attributes)
            {
                output += "\t" + kvp.Key + ": {\n";
                foreach (string a in kvp.Value) output += "\t\t" + a.ToString() + ",\n";
                output += "\t}\n";
            }
            output += "],\nData: [\n";
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
            string output = "Data [ \n\tName: " + this.Name + ",\n\tCategory: " + this.Category + ",\n\tAttributes: {\n";
            foreach (string a in this.Attributes) output += "\t\t" + a.ToString() + ",\n";
            output += "\t\t}\n\t]\n";
            return output;
        }
    }
}
