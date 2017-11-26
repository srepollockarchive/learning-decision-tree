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
            DecisionTree dt = new DecisionTree();
            string trainingFileName = args[0];
            string testingFileName = args[1];
            ID3Data id3Data = parser.ParseID3InformationFile(trainingFileName);
            Node learnedTree = dt.ID3(id3Data.TestData, id3Data.GetKeyAttributes(), id3Data);
            Console.WriteLine(learnedTree.ToString());
            Console.WriteLine("\n\nPress any key to exit...");
            Console.ReadKey();
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
    public class DecisionTree
    {
        public DecisionTree()
        {

        }
        /// <summary>
        /// Creates a decision tree with the ID3 algorithm.
        /// </summary>
        /// <param name="examples">Training examples</param>
        /// <param name="attributes">Key attributes (not their values)</param>
        /// <param name="data">Data object</param>
        /// <returns>Root node of the tree</returns>
        public Node ID3(ArrayList examples, ArrayList attributes, ID3Data data)
        {
            /**
                if (CheckSameCategory(examples))
                    return new LeafNode(examples.first.category)
                if (attributes.count == 0)
                    return new LeafNode(GetMostCommonCategory(examples))
                string bestAttribute = ChooseAttribute(examples, attributes) // best classifies examples
                Node tree = new Node(bestAttribute) // new node with category of best attribute
                foreach (string value in bestAttribute)
                    ArrayList subset = Subset(examples, value) // examples where example.Attribute[bestAttribute] == value
                    ArrayList removedAttributes = attributes.remove(bestAttribute)
                    Node subtree = ID3(subset, removedAttributes)
                    node.AddBranch(new Node());
                return tree;
             */
            if (CheckSameCategory(examples)) return new Node(((Data)(examples[0])).Category);
            if (attributes.Count == 0) return new Node(GetMostCommonCategory(examples));
            string bestAttribute = ChooseAttribute(examples, attributes, data);
            Node tree = new Node(bestAttribute);
            foreach (string value in data.Attributes[bestAttribute])
            {
                ArrayList subset = SubSet(examples, value);
                ArrayList removedAttributes = (ArrayList)(attributes.Clone());
                removedAttributes.Remove(bestAttribute);
                Node subTree = ID3(examples, removedAttributes, data);
                tree.AddBranch(subTree);
            }
            return tree;
        }
        bool CheckSameCategory(ArrayList examples)
        {
            Data dataObject = (Data)(examples[0]);
            string firstCategory = dataObject.Category;
            foreach (Data example in examples)
            {
                if (example.Category != firstCategory) 
                    return false;
            }
            return true;
        }
        string GetMostCommonCategory(ArrayList examples)
        {
            string output = "";
            int best = int.MinValue;
            Dictionary<string, int> dictionary = SummarizeExamples(examples);
            foreach (KeyValuePair<string, int> kvp in dictionary)
            {
                if (dictionary[kvp.Key] > best)
                {
                    output = kvp.Key;
                    best = dictionary[kvp.Key];
                }
            }
            return output;
        }
        Dictionary<string, int> SummarizeExamples(ArrayList examples)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            foreach (Data example in examples)
            {
                int value;
                if (dictionary.TryGetValue(example.Category, out value))
                    dictionary[example.Category] += 1;
                else
                    dictionary.Add(example.Category, 1);
            }
            return dictionary;
        }
        string ChooseAttribute(ArrayList examples, ArrayList attributes, ID3Data data)
        {
            string output = "";
            double best = double.MinValue;
            foreach(string attribute in attributes)
            {
                double temp = InformationGain(examples, attribute, Entropy(examples), data);
                if (best < temp)
                {
                    output = attribute;
                    best = temp;
                }
            }
            return output;
        }
        ArrayList SubSet(ArrayList examples, string attributeValue)
        {
            ArrayList output = new ArrayList();
            foreach (Data example in examples) if (example.Attributes.Contains(attributeValue)) output.Add(example);
            return output;
        }
        double InformationGain(ArrayList examples, string attribute, double entropyOfSet, ID3Data data)
        {
            double gain = entropyOfSet;
            foreach (string value in data.Attributes[attribute])
            {
                ArrayList subset = SubSet(examples, value);
                gain -= subset.Count / examples.Count * Entropy(subset);
            }
            return gain;
        }
        double Entropy(ArrayList examples)
        {
            double output = 0;
            float proportion = 0;
            Dictionary<string, int> dictionary = SummarizeExamples(examples);
            foreach (KeyValuePair<string, int> kvp in dictionary)
            {
                proportion = (float)kvp.Value / (float)examples.Count;
                output -= proportion * Math.Log(proportion, 2);
            }
            return output;
        }
    }
    public class Node
    {
        /// <summary>
        /// Label for the node
        /// </summary>
        public string Category { get; set; } 
        public ArrayList Children { get; set; }
        public Node(string category)
        {
            this.Category = category;
            this.Children = new ArrayList();
        }
        public void AddBranch(Node root)
        {
            this.Children.Add(root);
        }
        public void PrintPretty(string indent, bool last)
        {
            Console.Write(indent);
            if (last)
            {
                Console.Write("\\-");
                indent += "  ";
            }
            else
            {
                Console.Write("|-");
                indent += "| ";
            }
            Console.WriteLine(Category);

            for (int i = 0; i < Children.Count; i++)
                ((Node)(Children[i])).PrintPretty(indent, i == Children.Count - 1);
        }
        public override string ToString()
        {   
            PrintPretty("", false);
            return "";
        }
    }
    public class ID3Data
    {
        public ArrayList Categories { get; set; }
        public Dictionary<string, ArrayList> Attributes { get; set; }
        public ArrayList TestData { get; set; }
        public ID3Data() 
        {
            this.Attributes = new Dictionary<string, ArrayList>();
            this.TestData = new ArrayList();
        }
        public ArrayList GetAttributeValues()
        {
            ArrayList output = new ArrayList();
            foreach (KeyValuePair<string, ArrayList> kvp in Attributes) foreach (string value in Attributes[kvp.Key]) output.Add(value);
            return output;
        }
        public ArrayList GetKeyAttributes()
        {
            ArrayList output = new ArrayList();
            foreach (KeyValuePair<string, ArrayList> kvp in Attributes) output.Add(kvp.Key);
            return output;
        }
        public ArrayList GetSimilarAttributeValues(string attribute)
        {
            ArrayList output = new ArrayList();
            foreach (KeyValuePair<string, ArrayList> kvp in Attributes)
            {
                if (Attributes[kvp.Key].Contains(attribute))
                {
                    foreach (string value in Attributes[kvp.Key]) output.Add(value);
                    return output;
                }
            }
            return null;
        }
        public override string ToString()
        {
            string output = "ID3Data [ \nCategories: \n";
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
