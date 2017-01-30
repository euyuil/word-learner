using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace WordLearner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Regex _englishWordRegex = new Regex(
            @"\b(?<word>[a-z]+([\'\`\-][a-z]+)?)\b",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog {Multiselect = true};

            var result = dialog.ShowDialog(this);

            if (result != true) return;

            var wordsExist = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var word in Learnt.Items.Cast<string>()) wordsExist.Add(word);
            foreach (var word in NotLearnt.Items.Cast<string>()) wordsExist.Add(word);
            foreach (var word in NotConfirmed.Items.Cast<string>()) wordsExist.Add(word);

            var words = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var fileStream in dialog.OpenFiles())
            {
                string content;
                using (fileStream)
                using (var fileReader = new StreamReader(fileStream))
                {
                    content = fileReader.ReadToEnd();
                }

                var matches = _englishWordRegex.Matches(content);
                foreach (Match match in matches)
                {
                    if (!match.Success) continue;
                    var word = match.Groups["word"].Value;
                    words.Add(word);
                }
            }

            foreach (var word in words)
            {
                if (!wordsExist.Contains(word))
                {
                    NotConfirmed.Items.Add(word);
                }
            }
        }

        private void LoadWords(object sender, EventArgs e)
        {
            try
            {
                LoadWords(Learnt, "Learnt.txt");
                LoadWords(NotLearnt, "NotLearnt.txt");
                LoadWords(NotConfirmed, "NotConfirmed.txt");
                DoCleanUp();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void SaveWords(object sender, EventArgs e)
        {
            try
            {
                DoCleanUp();
                SaveWords(Learnt, "Learnt.txt");
                SaveWords(NotLearnt, "NotLearnt.txt");
                SaveWords(NotConfirmed, "NotConfirmed.txt");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private static void LoadWords(ItemsControl itemsControl, string fileName)
        {
            itemsControl.Items.Clear();

            var words = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            using (var fileStream = File.OpenRead(fileName))
            using (var fileReader = new StreamReader(fileStream))
            {
                string line;
                while (null != (line = fileReader.ReadLine()))
                {
                    var tw = TrimWord(line);
                    if (string.IsNullOrWhiteSpace(tw)) continue;
                    words.Add(tw);
                }
            }

            foreach (var word in words)
            {
                itemsControl.Items.Add(word);
            }
        }

        private static void SaveWords(ItemsControl itemsControl, string fileName)
        {
            var words = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (string item in itemsControl.Items)
            {
                words.Add(item);
            }

            using (var fileStream = File.OpenWrite(fileName))
            using (var fileWriter = new StreamWriter(fileStream))
            {
                foreach (var word in words)
                {
                    var tw = TrimWord(word);
                    if (string.IsNullOrWhiteSpace(tw)) continue;
                    fileWriter.WriteLine(tw);
                }
            }
        }

        private void DoCleanUp()
        {
            // TODO If a word exists in learnt and not-learnt, remove it from both list and add it to unconfirmed.
            // TODO If a word exists in either learnt or not-learnt, remove it from unconfirmed.
            // TODO Remove empty.
        }

        private static string TrimWord(string word)
        {
            if (word == null) return null;
            word = word.Trim();
            if (word.EndsWith("'s")) word = word.Substring(0, word.Length - 2);
            if (word.EndsWith("'ve")) word = word.Substring(0, word.Length - 3);
            if (word.EndsWith("'ll")) word = word.Substring(0, word.Length - 3);
            if (word.EndsWith("'d")) word = word.Substring(0, word.Length - 2);
            if (word.EndsWith("'re")) word = word.Substring(0, word.Length - 3);
            if (word.EndsWith("n't")) word = word.Substring(0, word.Length - 3);
            return word;
        }

        private void ConfirmWordsNotLearnt(object sender, RoutedEventArgs e)
        {
            foreach (var selectedItem in NotConfirmed.SelectedItems)
            {
                NotLearnt.Items.Add(selectedItem);
            }

            foreach (var selectedItem in NotConfirmed.SelectedItems.Cast<object>().ToArray())
            {
                NotConfirmed.Items.Remove(selectedItem);
            }

            NotConfirmed.SelectedItems.Clear();
        }

        private void ConfirmWordsLearnt(object sender, RoutedEventArgs e)
        {
            foreach (var selectedItem in NotConfirmed.SelectedItems)
            {
                Learnt.Items.Add(selectedItem);
            }

            foreach (var selectedItem in NotConfirmed.SelectedItems.Cast<object>().ToArray())
            {
                NotConfirmed.Items.Remove(selectedItem);
            }

            NotConfirmed.SelectedItems.Clear();
        }

        private void UndoConfirmWordsLearnt(object sender, RoutedEventArgs e)
        {
            foreach (var selectedItem in Learnt.SelectedItems)
            {
                NotConfirmed.Items.Add(selectedItem);
            }

            foreach (var selectedItem in Learnt.SelectedItems.Cast<object>().ToArray())
            {
                Learnt.Items.Remove(selectedItem);
            }

            Learnt.SelectedItems.Clear();
        }

        private void UndoConfirmWordsNotLearnt(object sender, RoutedEventArgs e)
        {
            foreach (var selectedItem in NotLearnt.SelectedItems)
            {
                NotConfirmed.Items.Add(selectedItem);
            }

            foreach (var selectedItem in NotLearnt.SelectedItems.Cast<object>().ToArray())
            {
                NotLearnt.Items.Remove(selectedItem);
            }

            NotLearnt.SelectedItems.Clear();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadWords(sender, e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWords(sender, e);
        }
    }
}
