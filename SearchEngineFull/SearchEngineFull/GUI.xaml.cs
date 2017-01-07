﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;

namespace SearchEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GUI : Window
    {

        ///lera
        //////noa
        ///mainDictionary to show 
        Dictionary<string, int> dictionary;
        public Dictionary<string, DocumentData> docInfo;
        public Dictionary<string, TermData> mainDictionary;
        Dictionary<string, List<string>> AutoCompletion;
        internal string corpusFolder = "";
        internal string postingsFolder = "";
        bool stemmer = false;
        ReadFile rf;

        public GUI()
        {
            InitializeComponent();
            mainDictionary = new Dictionary<string, TermData>();
            AutoCompletion = new Dictionary<string, List<string>>();
            docInfo = new Dictionary<string, DocumentData>();
        }

        //delete all the folders and reset the class Read File
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            //delete all files from the posting's directory
            if (postingsFolder == "")
                System.Windows.MessageBox.Show("posting directory path required", "Error");
            else
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(postingsFolder);

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                rf = null;
                System.Windows.MessageBox.Show("Reset Completed", "Error");
            }

        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {

          
            corpusFolder = InputPathTxt.Text;
            postingsFolder = postingsTextBox.Text;
            if (corpusFolder == "" || postingsFolder == "")
                System.Windows.MessageBox.Show("Path Required!", "Error");

            else
            {
                labelClock.Visibility = Visibility.Visible;
                imageClock.Visibility = Visibility.Visible;
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                System.Windows.MessageBox.Show("The process started");
                rf = new ReadFile(corpusFolder, postingsFolder, stemmer);
                rf.MainProcess();
                foreach (string lang in rf.languages)
                {
                    comboBoxLang.Items.Add(lang);
                }

                string time = (stopWatch.ElapsedMilliseconds / 1000).ToString();
                labelClock.Visibility = Visibility.Hidden;
                imageClock.Visibility = Visibility.Hidden;
                Dictionary<string, TermData> theDictionary = rf.mainDictionary;
                List<string> termList = rf.termList;
                dictionary = new Dictionary<string, int>();
                System.Windows.MessageBox.Show("Finish indexing in " + time + " seconds\n"
                    + "number of Documents: " + rf.docsCounter + "\n"
                   + "number of terms: " + theDictionary.Count
                    , "Finish Inexing");

                // add the term to dictionary for show
                foreach (string term in termList)
                {
                    dictionary.Add(term, theDictionary[term].NumInCorpus);
                }
                stopWatch.Stop();
            }
        }

        //load the Dictionary from file
        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {

            corpusFolder = InputPathTxt.Text;
            postingsFolder = postingsTextBox.Text;
            dictionary = new Dictionary<string, int>();
            string path = postingsFolder;
            string[] splitedLine;
            if (postingsFolder == "")
                System.Windows.MessageBox.Show("Path Required!", "Error");
            else
            {
                try
                {
                    string[] lines;
                    if (stemmer)
                        lines = File.ReadAllLines(path + @"\with Stemmer" + @"\dictionary.txt");
                    else
                        lines = File.ReadAllLines(path + @"\withOut Stemmer" + @"\dictionary.txt");

                    /*
                    foreach (string line in lines)
                    {
                        splitedLine = line.Split('^');
                        string term = splitedLine[0]; 
                        mainDictionary.Add(term, new TermData());
                        mainDictionary[term].Df = int.Parse(splitedLine[1]);
                        mainDictionary[term].Path = splitedLine[2];
                        mainDictionary[term].LineNum= int.Parse(splitedLine[3]);
                        mainDictionary[term].NumInCorpus = int.Parse(splitedLine[4]);
                    }
                    */


                    foreach (string line in lines)
                    {
                        splitedLine = line.Split('^');
                        dictionary.Add(splitedLine[0], int.Parse(splitedLine[4]));
                    }
                    System.Windows.MessageBox.Show("loaded completed!");
                }
                catch (IOException)
                {
                    System.Windows.MessageBox.Show("No dictionary found!");
                }
            }


        }

        //add the path to corpus
        private void corpusBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            DialogResult result = ofd.ShowDialog();
            corpusFolder = ofd.SelectedPath;
            InputPathTxt.Text = corpusFolder;
        }

        //add the path to the posting 
        private void postingBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            DialogResult result = ofd.ShowDialog();
            postingsFolder = ofd.SelectedPath;
            postingsTextBox.Text = postingsFolder;
        }

        //show the dictionary
        private void showBtn_Click(object sender, RoutedEventArgs e)
        {

            if (dictionary == null)
                System.Windows.MessageBox.Show("You didn't load a dictionary ", "Error");
            else
            {
                List<string> termsList = new List<string>();
                termsList.Add(string.Format("term\t\tfrequency\n"));
                foreach (KeyValuePair<string, int> kvp in dictionary)
                {
                    termsList.Add(string.Format("{0}\t\t{1}\n", kvp.Key, kvp.Value));
                }
                DictionaryWin dw = new DictionaryWin(termsList);
                dw.ShowDialog();
                // listBox.ItemsSource = termsList;
                // listBox.Visibility = Visibility.Visible;
            }
        }

        //if user checked the checkbox stemming is true
        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            stemmer = true;
        }

        //if user unchecked the checkbox stemming is false
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            stemmer = false;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            loadDictionarys();
            string s = "Water Pollution";
            Searcher search = new Searcher(mainDictionary, corpusFolder, stemmer, postingsFolder, docInfo);
            search.search(s);



        }


        private void loadDictionarys()
        {


            string path = postingsFolder;
            string[] splitedLine;

            try
            {
                mainDictionary = new Dictionary<string, TermData>();
                docInfo = new Dictionary<string, DocumentData>(); 
                string[] lines;
                if (stemmer)
                    lines = File.ReadAllLines(path + @"\with Stemmer" + @"\dictionary.txt");
                else
                    lines = File.ReadAllLines(path + @"\withOut Stemmer" + @"\dictionary.txt");
                string term;
                foreach (string line in lines)
                {
                    splitedLine = line.Split('^', '~');
                    term = splitedLine[0];
                    mainDictionary.Add(term, new TermData());
                    mainDictionary[term].Df = int.Parse(splitedLine[1]);
                    mainDictionary[term].Path = splitedLine[2];
                    mainDictionary[term].LineNum = int.Parse(splitedLine[3]);
                    mainDictionary[term].NumInCorpus = int.Parse(splitedLine[4]);
                }

                string[] doclines;

                if (stemmer)
                    doclines = File.ReadAllLines(path + @"\with Stemmer" + @"\docsInfo.txt");
                else
                    doclines = File.ReadAllLines(path + @"\withOut Stemmer" + @"\docsInfo.txt");

                foreach (string line in doclines)
                {
                    splitedLine = line.Split('^', '~');
                    term = splitedLine[0];
                    docInfo.Add(term, new DocumentData());

                    docInfo[term].NumUniqueTerm = int.Parse(splitedLine[1]);
                    docInfo[term].max_tf = int.Parse(splitedLine[2]);
                    docInfo[term].Language = splitedLine[3];
                    docInfo[term].Date = splitedLine[4];
                    docInfo[term].Length = int.Parse(splitedLine[5]);
                }

                /*
                string lineCom;
                string[] splitedLineComp;
                List<string> wordList;
                StreamReader sr;
                if (stemmer)
                    sr = new StreamReader(path + @"\with Stemmer" + @"\after.txt");
                else
                    sr = new StreamReader(path + @"\withOut Stemmer" + @"\after.txt");

                lineCom = sr.ReadLine();
                while (lineCom != null)
                {
                    splitedLineComp = lineCom.Split('~');
                    wordList = new List<string>();
                    for (int i = 1; i < splitedLineComp.Length; i += 2)
                    {
                        wordList.Add(splitedLineComp[i]);
                    }

                    AutoCompletion.Add(splitedLineComp[0], wordList);
                    lineCom = sr.ReadLine();

                }


    */
            }
            catch (IOException)
            {
                System.Windows.MessageBox.Show("No dictionary found!");
            }
        }


    }
}
