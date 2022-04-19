using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;

namespace Draw_Prompts
{
    public partial class MainWindow : Window
    {
        DateTime curDate, nexDate;
        string promptString;
        int curListMonth;
        bool curTheme, updatePrompt, listUpdating;
        List<string> promptList;

        public MainWindow()
        {
            InitializeComponent();
            UpdateDate();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            UpdateDate();
            nexDate = curDate;
            UpdateDateText();
            LoadStuff();
        }

        private async void LoadStuff()
        {
            promptStack.Opacity = 0;
            updatePrompt = true;
            await GetMonthPrompts(); 
            curListMonth = nexDate.Month;
            promptString = promptList[nexDate.Day - 1];
            SetPrompt();
            (promptStack.Resources["toCent"] as Storyboard).Begin();
        }

        private void UpdateDateText()
        {
            dateText.Text = nexDate.ToString("dddd d") + GetDaySuffix(nexDate.Day) + " " + nexDate.ToString("Y");
        }

        private async void UpdatePromptListMonth()
        {
            if (curListMonth != nexDate.Month || promptList.Count < 2) { await GetMonthPrompts(); curListMonth = nexDate.Month; }
            promptString = promptList[nexDate.Day - 1];
            if (updatePrompt) SetPrompt();
        }

        private void SetPrompt()
        {
            var promptMain = promptString;
            if (promptString.Contains('(') && promptString.Contains(')'))
            {
                string noteString = promptString.Substring(promptString.IndexOf("(") + 1, promptString.IndexOf(")") - promptString.IndexOf("(") - 1);
                promptMain = promptString.Substring(0, promptString.IndexOf("(")).Trim();
                noteText.Text = noteString;
                noteText.Visibility = Visibility.Visible;
            }
            else
            {
                noteText.Text = string.Empty;
                noteText.Visibility = Visibility.Collapsed;
            }
            curDate = nexDate;
            updatePrompt = false;
            promptText.Opacity = 1;
            promptText.Text = promptMain;
        }

        private async Task GetMonthPrompts()
        {
            listUpdating = true;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "C# console program");

            var content = await client.GetStringAsync("https://www.simpledailydrawing.com/drawing-prompts/" + nexDate.ToString("MMMM").ToLower() + "-" + nexDate.Year);

            int cutoffIndex = content.IndexOf(" a list of prompts, one for every day!</p>");
            if (cutoffIndex >= 0)
            {
                cutoffIndex += " a list of prompts, one for every day!</p>".Length;
                content = content.Substring(cutoffIndex, 1800);
            }

            List<string> prompts = new List<string>();

            for (int i = 1; i < 35; i++)
            {
                int startIndex = content.IndexOf(">" + (i.ToString().Length < 2 ? "0" : "") + i + " • ") + 6;
                if(startIndex > 5)
                {
                    content = content[startIndex..];
                    int endIndex = content.IndexOf("<");
                    while (endIndex < 1)
                    {
                        Console.WriteLine("we got a cutoff bois!!");
                        content = content[(content.IndexOf(">") + 1)..];
                        endIndex = content.IndexOf("<");
                    }
                    string result = content[..endIndex];
                    if (result.Contains("***")) result = result[..result.IndexOf("***")];
                    prompts.Add(result.Trim());
                }
                else { break; }
            }
            listUpdating = false;
            promptList = prompts;
        }

        private void UpdateDate()
        {
            curDate = DateTime.Now;
        }

        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private string GetDaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";

                case 2:
                case 22:
                    return "nd";

                case 3:
                case 23:
                    return "rd";

                default:
                    return "th";
            }
        }

        private void OnExit(object sender, EventArgs e)
        {

        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SwitchTheme(object sender, RoutedEventArgs e)
        {
            ResourceDictionary newRes = Application.Current.Resources.MergedDictionaries[1];
            newRes.MergedDictionaries.Clear();
            newRes.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("res/dic/Themes/Theme" + (curTheme? 1 : 2) + ".xaml", UriKind.Relative) });
            curTheme = !curTheme;
        }

        private void SearchPrompt(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://www.google.com/images?q=" + promptText.Text.Replace(' ', '+')) { UseShellExecute = true});
        }

        private void PrevClick(object sender, RoutedEventArgs e)
        {
            (promptStack.Resources["exDn"] as Storyboard).Begin();
            nexDate = nexDate.AddDays(-1);
            updatePrompt = false;
            UpdatePromptListMonth();
            UpdateDateText();
        }

        private void PrevArrowMouseEnter(object sender, MouseEventArgs e)
        {
            (prevArrow.Resources["upAnim"] as Storyboard).Begin();
        }
        private void PrevArrowMouseLeave(object sender, MouseEventArgs e)
        {
            (prevArrow.Resources["dnAnim"] as Storyboard).Begin();
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            (promptStack.Resources["exUp"] as Storyboard).Begin();
            nexDate = nexDate.AddDays(1);
            updatePrompt = false;
            UpdatePromptListMonth();
            UpdateDateText();
        }

        private void NextArrowMouseEnter(object sender, MouseEventArgs e)
        {
            (nextArrow.Resources["upAnimat"] as Storyboard).Begin();
        }
        private void NextArrowMouseLeave(object sender, MouseEventArgs e)
        {
            (nextArrow.Resources["dnAnimat"] as Storyboard).Begin();
        }

        private void PromptChangeEnter(object sender, EventArgs e)
        {
            if (listUpdating)
            {
                updatePrompt = true;
                promptText.Text = "Loading...";
                promptText.Opacity = 0.4;
                //Show loading sign
            }
            else
            {
                SetPrompt();
            }
            (promptStack.Resources["toCent"] as Storyboard).Begin();
        }
    }
}
