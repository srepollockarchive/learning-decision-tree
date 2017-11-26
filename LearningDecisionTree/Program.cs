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
            // Part One
            Console.WriteLine("Learned Tree:\n-----------");
            Console.WriteLine(learnedTree.ToString());
            // Part Two
            ID3Data testingData = parser.ParseID3InformationFile(testingFileName);
            ArrayList treeClassification = learnedTree.GetTreeClassifications(testingData);
            Console.WriteLine("Classifications:\n-------------");
            Console.WriteLine(testingData.CompareClassifications(treeClassification));
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
        #region ID3
        public Node ID3(ArrayList examples, ArrayList attributes, ID3Data data)
        {
            /**
             *  A <- best attribute
             *  Assign A as decision attribute for node
             *  foreach value of A
             *      create a descendent of node
             *  sort training examples to leaves
             *  if examples perfectly classified STOP
             *  else iterate over leaves
             */
            //  if all example same category (pure)
            //      return leaf with that category
            //  if attributes.empty
            //      return a leaf with most common category in examples
            if (attributes.Count == 0)
            {
                // string mostCommon = GetMostCommonValue(examples);
                string mostCommon = GetMostCommonCategory(examples);
                return new Node(Label:mostCommon, Decision:null);
            }
            string bestAttribute = GetBestAttribute(examples, attributes, data);
            Node tree = new Node(Label:bestAttribute, Decision:null); // This nodes decision category
            foreach (string value in data.Attributes[bestAttribute])
            {
                ArrayList subset = SubSet(examples, value);
                Dictionary<string, int> dictionary = SummarizeExamplesValue(examples, value, data);
                foreach (KeyValuePair<string, int> kvp in dictionary) if (kvp.Value == examples.Count) return new Node(Label:kvp.Key, Decision:null);
                ArrayList newAttributes = attributes;
                newAttributes.Remove(bestAttribute);
                Node subtree = ID3(subset, newAttributes, data);
                subtree.Decision = value;
                tree.AddBranch(subtree);
            }
            return tree;
        }
        /// <summary>
        /// Returns a subset of examples with the targetValue as an attribute of theirs.
        /// </summary>
        /// <param name="examples"></param>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        ArrayList SubSet(ArrayList examples, string targetValue)
        {
            ArrayList output = new ArrayList();
            foreach (Data example in examples) if (example.Attributes.Contains(targetValue)) output.Add(example);
            return output;
        }
        /// <summary>
        /// Gets the most common attribute from the examples.
        /// </summary>
        /// <param name="examples">Example list</param>
        /// <returns>Most Common attribute</returns>
        string GetMostCommonValue(ArrayList examples)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            foreach (Data example in examples)
            {
                foreach (string value in example.Attributes)
                {
                    if (dictionary.ContainsKey(value))  dictionary[value] += 1;
                    else dictionary.Add(value, 1);
                }
            }
            KeyValuePair<string, int> max = new KeyValuePair<string, int>();
            foreach (KeyValuePair<string, int> kvp in dictionary) if (kvp.Value > max.Value) max = kvp;
            return max.Key;
        }
        string GetMostCommonCategory(ArrayList examples)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            foreach (Data example in examples)
            {
                if (dictionary.ContainsKey(example.Category))  dictionary[example.Category] += 1;
                else dictionary.Add(example.Category, 1);
            }
            KeyValuePair<string, int> max = new KeyValuePair<string, int>();
            foreach (KeyValuePair<string, int> kvp in dictionary) if (kvp.Value > max.Value) max = kvp;
            return max.Key;
        }
        string GetBestAttribute(ArrayList examples, ArrayList attributes, ID3Data data)
        {
            string output = "";
            double best = double.MinValue;
            foreach (string value in attributes)
            {
                double temp = InformationGain(examples, value, Entropy(examples, value, data), data);
                if (temp > best) // REVIEW: Depends on how you do this
                {
                    output = value;
                    best = temp;
                }
            }
            return output;
        }
        double InformationGain(ArrayList examples, string attribute, double entropyOfSet, ID3Data data)
        {
            double gain = entropyOfSet; // The current gain
            foreach (string value in data.Attributes[attribute]) // For each value the attribute can be
            {
                ArrayList subset = SubSet(examples, value);
                gain -= (float)subset.Count / (float)examples.Count * (float)Entropy(subset, attribute, data);
            }
            return gain;
        }
        double Entropy(ArrayList examples, string targetAttribute, ID3Data data)
        {
            double result = 0;
            Dictionary<string, int> dictionary = SummarizeExamplesAttribute(examples, targetAttribute, data);
            foreach (KeyValuePair<string, int> kvp in dictionary)
            {
                double proportion = (float)dictionary[kvp.Key] / (float)examples.Count;
                result -= proportion * Math.Log(proportion, 2);
            }
            return result;
        }
        /// <summary>
        /// Summarizes how many of each attribute are in the current examples.
        /// </summary>
        /// <param name="examples">Examples to check</param>
        /// <param name="targetAttribute">Target attribute to iterate over it's values</param>
        /// <param name="data">Data object</param>
        /// <returns>Dictionary of summarized examples</returns>
        Dictionary<string, int> SummarizeExamplesValue(ArrayList examples, string targetValue, ID3Data data)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            foreach (string value in data.GetSimilarAttributeValues(targetValue))
                foreach (Data example in examples)
                {
                    if (example.Attributes.Contains(value))
                    {
                        if (dictionary.ContainsKey(value))
                            dictionary[value] += 1;
                        else
                            dictionary.Add(value, 1);
                    }
                }
            return dictionary;
        }
        /// <summary>
        /// Summarizes how many of each attribute are in the current examples.
        /// </summary>
        /// <param name="examples">Examples to check</param>
        /// <param name="targetAttribute">Target attribute to iterate over it's values</param>
        /// <param name="data">Data object</param>
        /// <returns>Dictionary of summarized examples</returns>
        Dictionary<string, int> SummarizeExamplesAttribute(ArrayList examples, string targetAttribute, ID3Data data)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            foreach (string value in data.Attributes[targetAttribute])
                foreach (Data example in examples)
                {
                    if (example.Attributes.Contains(value))
                    {
                        if (dictionary.ContainsKey(value))
                            dictionary[value] += 1;
                        else
                            dictionary.Add(value, 1);
                    }
                }
            return dictionary;
        }
        #endregion
    }
    public class Node
    {
        public string Label { get; set; } 
        public string Decision { get; set; }
        public ArrayList Children { get; set; }
        public Node()
        {
            this.Label = null;
            this.Decision = null;
            this.Children = new ArrayList();
        }
        public Node(string Label, string Decision)
        {
            this.Label = Label;
            this.Decision = Decision;
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
            Console.WriteLine(Label + " || " + Decision);

            for (int i = 0; i < Children.Count; i++)
                ((Node)(Children[i])).PrintPretty(indent, i == Children.Count - 1);
        }
        #region Check Data
        string Traverse(Data example)
        {
            if (Children.Count == 0) // NOTE: Hit leaf
            {
                return this.Label;
            }
            else
            {
                foreach (Node child in Children)
                {
                    if (example.Attributes.Contains(child.Decision))
                        return child.Traverse(example);
                }
            }
            return null; // DEBUG: Should never hit
        }
        public ArrayList GetTreeClassifications(ID3Data testingData)
        {
            ArrayList output = new ArrayList();
            foreach (Data example in testingData.TestData)
            {
                string exmapleClassification = Traverse(example);
                output.Add(Tuple.Create<string, string>(example.Name, exmapleClassification));
            }
            return output;
        }
        #endregion
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
            return output;
        }
        public string CompareClassifications(ArrayList classifications)
        {
            string output = "";
            int count = 0, errorCount = 0;
            foreach (Tuple<string, string> classification in classifications) {
                if (((Data)(TestData[count])).Category != classification.Item2)
                {
                    output += classification.Item1 + ": CLASSIFICATION INCORRECT\n";
                    errorCount++;
                }
                count++;
            }
            output += "\nTotal Error: (" + errorCount + "/" + count + ")";
            return output;
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
