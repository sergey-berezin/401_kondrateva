using System;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using NuGetBert;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        Bert bertModel;
        CancellationTokenSource cts;
        ObservableCollection<Message> messages;
        bool textIsLoaded = false;

        public MainWindow()
        {
            InitializeComponent();
            messages = new ObservableCollection<Message>();
            MessagesList.ItemsSource = messages;
            cts = new CancellationTokenSource();
            bertModel = new Bert();
            ModelLoad(bertModel);

        }

        private async void ModelLoad(Bert bertModel)
        {
            try
            {
                await bertModel.ModelLoader(cts.Token);

                Message message = new Message("Модель загружена.", false);
                messages.Add(message);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                string textPath = openFileDialog.FileName;

                bertModel.TextInitializer(textPath);
                textIsLoaded = true;

                Message message = new Message("Текст для анализа:\n\n" + bertModel.text, false);
                messages.Add(message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }

        private async void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            cancelButton.IsEnabled = true;
            answerButton.IsEnabled = false;

            if (!textIsLoaded)
            {
                messages.Add(new Message($"Сначала загрузите файл.\n", false));

                cancelButton.IsEnabled = false;
                answerButton.IsEnabled = true;
                return;
            }

            string Question = questionTextBox.Text;

            Message message = new Message(Question, true);
            messages.Add(message);

            var token = cts.Token;
            try
            {
                var answer = await bertModel.GetAnswer(Question, token);

                messages.Add(new Message(answer, false));
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                {
                    cancelButton.IsEnabled = false;
                    answerButton.IsEnabled = true;
                    messages.Add(new Message(ex.Message, false));
                    return;
                }
                MessageBox.Show($"{ex.Message}");
            }
            cancelButton.IsEnabled = false;
            answerButton.IsEnabled = true;
            questionTextBox.Text = "";
        }
    }
}
