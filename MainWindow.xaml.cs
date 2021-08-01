﻿using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FEVSF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            documentation.Text = File.ReadAllText(@"documentation.txt");
        }

        /*private void FEVS_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                MessageBox.Show("CTRL + S");
            }
        }*/

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".fevs";
            dlg.Filter = "FEVS Files (*.fevs)|*.fevs";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                Title = "FEVS - " + filename;
                string text = File.ReadAllText(filename);
                SourceCode.Text = text;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string[] filename = Title.Split(new[] { " - " }, StringSplitOptions.None);
            if (filename.Length == 2)
            {
                string content = SourceCode.Text;
                string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                using (StreamWriter sw = new StreamWriter(filename[1]))
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (i > 0)
                            sw.WriteLine();
                        sw.Write(lines[i]);
                    }
                }
            }
            else
            {
                MessageBox.Show("You don't have any loaded file to save.");
            }
        }

        private void Transform_Click(object sender, RoutedEventArgs e)
        {
            string[] filename = Title.Split(new[] { " - " }, StringSplitOptions.None);
            if (filename.Length != 2)
            {
                MessageBox.Show("You don't have any loaded file to save.");
            }
            else
            {
                string[] text = File.ReadAllLines(filename[1]);
                int[][] finishedCommands = new int[32][];

                if (text.Length > 32)
                    MessageBox.Show("Too many lines to encode.");
                else
                {
                    for (int q = 0; q < finishedCommands.Length; q++)
                    {
                        finishedCommands[q] = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    }
                    for (int i = 0; i < text.Length; i++)
                    {
                        string[] parsed = text[i].Split('(');
                        string cmd = parsed[0];
                        string argument = parsed[1].Remove(parsed[1].Length - 1);
                        string[] arguments = argument.Split(',');
                        for (int t = 0; t < arguments.Length; t++)
                        {
                            arguments[t] = arguments[t].Trim();
                        }

                        // Création de l'instance de la classe avec System.Reflection notre Dieu.
                        // C'est pour invoquer la commande donnée ça.
                        Type type = Type.GetType("FEVSF.Commands");
                        ConstructorInfo magicConstructor = type.GetConstructor(Type.EmptyTypes);
                        object CommandsObject = magicConstructor.Invoke(new object[] { });

                        // Là, c'est pour invoke la commande avec les arguments directement
                        MethodInfo magicMethod = type.GetMethod(cmd);

                        // Enculée de méthode
                        LinkedList<string> parametersArray = new LinkedList<string>();
                        for (int j = 0; j < arguments.Length; j++)
                        {
                            parametersArray.AddLast(arguments[j]);
                        }
                        object[] args = parametersArray.ToArray();

                        try
                        {
                            // Si les args et la commande sont bons, alors on va l'appeler et modifier le finishedCommands[i]
                            int[] commandResult = (int[])magicMethod.Invoke(CommandsObject, args);
                            finishedCommands[i] = commandResult;
                        }
                        catch (NullReferenceException)
                        {
                            MessageBox.Show("Something went wrong : Check your commands names.\nSome bytes were set to 0 because of\nthis error to avoid crashing.");
                        }
                        catch (TargetInvocationException)
                        {
                            MessageBox.Show("Something went wrong : One or multiple arguments are not valid.\nSome bytes were set to 0 because of\nthis error to avoid crashing.");
                        }
                    }
                    string[] filename1 = Title.Split(new[] { " - " }, StringSplitOptions.None);
                    string[] file = filename1[1].Split('.');
                    using (StreamWriter sw = new StreamWriter(file[0] + ".sevs"))
                    {
                        for (int i = 0; i < finishedCommands.Length; i++)
                        {
                            if (i > 0)
                                sw.WriteLine();
                            for (int j = 0; j < finishedCommands[i].Length; j++)
                            {
                                if (j > 0)
                                    sw.Write(", ");
                                sw.Write(Convert.ToString(finishedCommands[i][j], 16).ToUpper());
                            }
                        }
                    }
                    MessageBox.Show("Done.");
                }
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog1 = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog1.Filter = "FEVS Files|*.fevs";
            saveFileDialog1.Title = "Create a new .fevs file";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                FileStream fs = (FileStream)saveFileDialog1.OpenFile();
                fs.Write(new byte[] { }, 0, 0);
                fs.Close();

                // Load le fichier après avoir été créé (équivalent de Open_Click sans le dialogue)
                string filename = saveFileDialog1.FileName;
                Title = "FEVS - " + filename;
                string text = File.ReadAllText(filename);
                SourceCode.Text = text;
            }
        }
    }
}