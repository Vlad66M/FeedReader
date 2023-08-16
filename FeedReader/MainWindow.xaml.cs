using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using CodeHollow.FeedReader;
using static System.Net.Mime.MediaTypeNames;

namespace FeedReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDarkTheme = false;

        private List<FeedSource> currentSources = new List<FeedSource>();
        private List<Feed> currentFeeds = new List<Feed>();
        private FeedSource currentSource = new FeedSource();
        int currentTitleIndex = -1;

        int sourcesFontSize = 14;
        int titlesFontSize = 14;
        int contentSize = 16;

        private DispatcherTimer rotationTimer = new DispatcherTimer();
        private bool isUpdating = true;
        private int rotationState = 0;

        private int sizeModification = 0;

        private void SetFontSize()
        {
            listboxFeedsTitle.FontSize = titlesFontSize;
            dgFeeds.FontSize = sourcesFontSize;

        }

        private void HideRotationButtons()
        {
            buttonImg1.Visibility = Visibility.Collapsed;
            buttonImg2.Visibility = Visibility.Collapsed;
            buttonImg3.Visibility = Visibility.Collapsed;
        }
        private async Task Rotate()
        {
            if (!isUpdating)
            {
                return;
            }
            if (rotationState == 0)
            {
                buttonImg1.Visibility = Visibility.Visible;
                buttonImg2.Visibility = Visibility.Collapsed;
                buttonImg3.Visibility = Visibility.Collapsed;
                rotationState = 1;
                return;
            }
            if (rotationState == 1)
            {
                buttonImg1.Visibility = Visibility.Collapsed;
                buttonImg2.Visibility = Visibility.Visible;
                buttonImg3.Visibility = Visibility.Collapsed;
                rotationState = 2;
                return;
            }
            if (rotationState == 2)
            {
                buttonImg1.Visibility = Visibility.Collapsed;
                buttonImg2.Visibility = Visibility.Collapsed;
                buttonImg3.Visibility = Visibility.Visible;
                rotationState = 0;
                return;
            }
        }
        private void SetRotationTimer()
        {
            rotationTimer.Tick += new EventHandler(RotationTimerTick);
            rotationTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            //rotationTimer.Start();
        }
        private void CreateUpdateTimer()
        {
            var timer = new DispatcherTimer();
            timer.Tick += new EventHandler(TimerTick);
            timer.Interval = new TimeSpan(0, 0, 60);
            timer.Start();
        }

        private void CreateGarbageTimer()
        {
            var timer = new DispatcherTimer();
            timer.Tick += new EventHandler(GarbageTimerTick);
            timer.Interval = new TimeSpan(1, 0, 60);
            timer.Start();
        }

        private void GarbageTimerTick(object sender, EventArgs e)
        {
            CleanGarbage();
        }

        private async Task CleanGarbage()
        {
            FeedsDatabaseManager.CleanGarbage();
        }
        private void RotationTimerTick(object sender, EventArgs e)
        {
            Rotate();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            UpdateData();
        }
        public MainWindow()
        {
            InitializeComponent();

            FeedsDatabaseManager.CreateDB();
            try
            {
                FeedsDatabaseManager.InsertFeedSource("https://66.ru/news/rss/");
                FeedsDatabaseManager.InsertFeedSource("https://www.e1.ru/news/rdf/full.xml");
                FeedsDatabaseManager.InsertFeedSource("http://veved.ru/rss.xml");
                FeedsDatabaseManager.InsertFeedSource("https://www.justmedia.ru/rss");
                FeedsDatabaseManager.InsertFeedSource("https://www.uralweb.ru/feed/");
                FeedsDatabaseManager.InsertFeedSource("https://pravdaurfo.ru/rss/all/rss.xml/");
                FeedsDatabaseManager.InsertFeedSource("https://ural-meridian.ru/yandex/news/");
                FeedsDatabaseManager.InsertFeedSource("https://midural.ru/news/100046/newsRSS/");
                FeedsDatabaseManager.InsertFeedSource("https://fedpress.ru/feed/rss?region=66");
                FeedsDatabaseManager.InsertFeedSource("https://urbc.ru/rss.xml");
                FeedsDatabaseManager.InsertFeedSource("https://momenty.org/rss");
                FeedsDatabaseManager.InsertFeedSource("https://newdaynews.ru/ekb/rss/");
                FeedsDatabaseManager.InsertFeedSource("https://eanews.ru/rss");
                FeedsDatabaseManager.InsertFeedSource("https://politsovet.ru/rss.xml");
                FeedsDatabaseManager.InsertFeedSource("https://pravdaurfo.ru/rss/all/rss.xml/");


            }
            catch { }

            UpdateFeedsList();
            SetFontSize();
            CreateUpdateTimer();
            CreateGarbageTimer();
            SetRotationTimer();

            SetImagesSources();
        }

        private void SetImagesSources()
        {
            string currentPath = Environment.CurrentDirectory + @"/Images/";
            imgUpdate.Source = new BitmapImage(new Uri(currentPath + "update.png", UriKind.Absolute));
            imgBtnDelSource.Source = new BitmapImage(new Uri(currentPath + "delete.png", UriKind.Absolute));
            imgAdd.Source = new BitmapImage(new Uri(currentPath + "add.png", UriKind.Absolute));
            imgRead.Source = new BitmapImage(new Uri(currentPath + "read.png", UriKind.Absolute));
            imgDelete.Source = new BitmapImage(new Uri(currentPath + "delete.png", UriKind.Absolute));
            imgTheme.Source = new BitmapImage(new Uri(currentPath + "theme.png", UriKind.Absolute));
            imgClose.Source = new BitmapImage(new Uri(currentPath + "close.png", UriKind.Absolute));
            imgMaximize.Source = new BitmapImage(new Uri(currentPath + "maximize.png", UriKind.Absolute));
            imgHide.Source = new BitmapImage(new Uri(currentPath + "hide.png", UriKind.Absolute));
            img1.Source = new BitmapImage(new Uri(currentPath + "1.png", UriKind.Absolute));
            img2.Source = new BitmapImage(new Uri(currentPath + "2.png", UriKind.Absolute));
            img3.Source = new BitmapImage(new Uri(currentPath + "3.png", UriKind.Absolute));
        }

        private async Task UpdateData()
        {
            try
            {
                int currentSourceIndex = dgFeeds.SelectedIndex;
                int currentTitlesIndex = listboxFeedsTitle.SelectedIndex;

                rotationTimer.Start();
                await FeedsDatabaseManager.UpdateFeeds();
                UpdateFeedsList();
                UpdateTitles();
                //LoadContentText();
                listboxFeedsTitle.SelectedIndex = 0;
                LoadC();
                rotationTimer.Stop();
                HideRotationButtons();

                dgFeeds.SelectedIndex = currentSourceIndex;
                listboxFeedsTitle.SelectedIndex = currentTitlesIndex;
            }
            catch { }
        }

        private void UpdateTheme()
        {
            try
            {
                int currentSourceIndex = dgFeeds.SelectedIndex;
                int currentTitlesIndex = listboxFeedsTitle.SelectedIndex;

                UpdateFeedsList();
                UpdateTitles();
                listboxFeedsTitle.SelectedIndex = 0;
                LoadC();
                rotationTimer.Stop();
                HideRotationButtons();

                dgFeeds.SelectedIndex = currentSourceIndex;
                listboxFeedsTitle.SelectedIndex = currentTitlesIndex;
            }
            catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateData();
            CleanGarbage();
        }
        private void UpdateUnreadNumbers()
        {
            int currentSourceIndex = dgFeeds.SelectedIndex;
            int currentTitleIndex = listboxFeedsTitle.SelectedIndex;

            if (currentSourceIndex != -1 && currentTitleIndex != -1)
            {
                UpdateFeedsList();
                dgFeeds.SelectedIndex = currentSourceIndex;
                listboxFeedsTitle.SelectedIndex = currentTitleIndex;
            }
        }
        private void UpdateFeedsList()
        {

            dgFeeds.Items.Clear();
            string allName = "Все";
            int unread = FeedsDatabaseManager.GetUnreadNumber();
            string allUnreadCount;
            if (unread== 0)
            {
                allUnreadCount = "";
            }
            else
            {
                allUnreadCount =" " + unread.ToString();
            }
            
            FS fsAll = new FS(allName, allUnreadCount);
            dgFeeds.Items.Add(fsAll);
            currentSources = FeedsDatabaseManager.GetFeedsSources();
            foreach(FeedSource source in currentSources)
            {
                int unreadCount = FeedsDatabaseManager.GetUnreadNumber(source.SourceName);
                string unreadStr;
                if (unreadCount == 0)
                {
                    unreadStr = "";
                }
                else
                {
                    unreadStr =" " + unreadCount.ToString();
                }
                
                string sourceName = source.SourceName;
                if (!String.IsNullOrEmpty(sourceName))
                {
                    string firstChar = sourceName[0].ToString().ToUpper();
                    sourceName = firstChar + sourceName.Remove(0, 1);
                }

                var fs = new FS(sourceName, unreadStr);
                dgFeeds.Items.Add(fs);
            }
            FS fsTrash = new FS("Корзина", "");
            dgFeeds.Items.Add(fsTrash);

            //Style CellStyle = new Style();
            Style _CellStyle = new Style();
            if (isDarkTheme)
            {
                _CellStyle = this.FindResource("MyDarkCellStyle") as Style;
            }
            else
            {
                _CellStyle = this.FindResource("MyWhiteCellStyle") as Style;
            }

            var CellSetter = new Setter(DataGridCell.ToolTipProperty, new Binding() { RelativeSource = new RelativeSource(RelativeSourceMode.Self), Path = new PropertyPath("Content.Text") });
            var CellSetter2 = new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0,0,0,0));
            try
            {
                _CellStyle.Setters.Add(CellSetter);
                _CellStyle.Setters.Add(CellSetter2);
            }
            catch { }
            if (isDarkTheme)
            {
                //dgFeeds.Background = Brushes.Black;
                /*var CellSetterBackground = new Setter(DataGridCell.BackgroundProperty, Brushes.Black);
                CellStyle.Setters.Add(CellSetterBackground);*/
                try
                {
                    var CellSetterForeground = new Setter(DataGridCell.ForegroundProperty, Brushes.LightGray);
                    _CellStyle.Setters.Add(CellSetterForeground);
                }
                catch { }
                

                /*var CellSetterBackground2 = new Setter(DataGridCell.IsHitTestVisibleProperty, true) ;
                CellStyle_ToolTip.Setters.Add(CellSetterBackground2);*/

                /*var dt = new Trigger();

                dt.Property = */

                /*dt.Setters.Add(new Setter()
                {
                    Property = Control.BackgroundProperty,
                    Value = ColorConverter.ConvertFromString("Red")
                });
                CellStyle_ToolTip.Triggers.Add(dt);*/
            }
            else
            {
                /*var CellSetterBackground = new Setter(DataGridCell.BackgroundProperty, Brushes.White);
                _CellStyle.Setters.Add(CellSetterBackground);*/
                try
                {
                    var CellSetterForeground = new Setter(DataGridCell.ForegroundProperty, Brushes.Black);
                    _CellStyle.Setters.Add(CellSetterForeground);
                }
                catch { }
            }

            

            dgFeeds.CellStyle = _CellStyle;

            dgFeeds.SelectedIndex = 0;

            if (isDarkTheme)
            {
                dgFeeds.Background = Brushes.Black;
                dgFeeds.Foreground = Brushes.White;
            }
            else
            {
                dgFeeds.Background = Brushes.White;
                dgFeeds.Foreground = Brushes.Black;
            }
        }
        private void UpdateTitles(string source="")
        {
            currentFeeds = FeedsDatabaseManager.GetFeeds(source);
            listboxFeedsTitle.Items.Clear();
            foreach (var item in currentFeeds)
            {
                var lbi = new ListBoxItem();
                var toolTip = new ToolTip();
                toolTip.Content = item.Title;
                lbi.ToolTip = toolTip;

                var template = this.FindResource("myTaskTemplate2") as DataTemplate;
                lbi.ContentTemplate = template;

                lbi.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
                lbi.BorderBrush = Brushes.Gray;
                lbi.Padding = new Thickness(0, 0, 0, 0);
                lbi.Margin = new Thickness(1, 0, 1, 0);

                if (isDarkTheme)
                {
                    lbi.Foreground = Brushes.LightGray;
                    lbi.Background = Brushes.Black;
                }
                else
                {
                    lbi.Foreground = Brushes.Black;
                    lbi.Background = Brushes.White;
                }
                lbi.Content = item.Title + Environment.NewLine + item.Date.ToString() + " - " + item.SourceName.Trim();
                if (item.IsRead == 0)
                {
                    lbi.FontWeight = FontWeights.Bold;
                    if (isDarkTheme)
                    {
                        lbi.Foreground = Brushes.LightBlue;
                    }
                    else
                    {
                        lbi.Foreground = Brushes.MidnightBlue;
                    }
                    
                }
                else
                {
                    lbi.FontWeight = FontWeights.Regular;
                }

                listboxFeedsTitle.Items.Add(lbi);
            }
            try
            {
                var sv = GetScrollViewer(listboxFeedsTitle) as ScrollViewer;
                if(sv!= null)
                {
                    sv.ScrollToLeftEnd();
                }
            }
            catch { }
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }

        public static DependencyObject GetScrollBar(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollBar)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollBar(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }
        private void LoadContent()
        {
            int currentSourceIndex;
            if (dgFeeds.SelectedIndex == -1)
            {
                dgFeeds.SelectedIndex = 0;
            }
            currentSourceIndex = dgFeeds.SelectedIndex;
            if(currentSourceIndex == -1)
            {
                currentSourceIndex = 0;
            }
            if (currentSourceIndex == 0)
            {
                UpdateTitles();
            }
            else
            {
                if (currentSourceIndex > currentSources.Count)
                {
                    UpdateTitles("trash");
                }
                else
                {
                    string source = currentSources[currentSourceIndex - 1].SourceName;
                    UpdateTitles(source);
                }
            }
            //LoadContentText();
            listboxFeedsTitle.SelectedIndex = 0;
            LoadC();
        }

        /*private void LoadContentText()
        {
            listboxFeedsTitle.SelectedIndex = 0;
            LoadC();
        }*/
        

        private void buttonUpdate(object sender, RoutedEventArgs e)
        {
            UpdateData();
            /*UpdateFeedsList();
            UpdateTitles();
            LoadContentText();*/
        }

        private void LoadC()
        {
            try
            {
                if (isDarkTheme)
                {
                    listBoxContentFirst.Background = Brushes.Black;
                }
                else
                {
                    listBoxContentFirst.Background = Brushes.White;
                }
                //listBoxContent.Items.Clear();
                /*int index = listboxFeedsTitle.SelectedIndex;
                string content = currentFeeds[index].Content;
                string link = currentFeeds[index].Link;
                string title = currentFeeds[index].Title;
                string source = currentFeeds[index].SourceName;
                DateTime? date = currentFeeds[index].Date;*/
                int index = 0;
                string content = "";
                string link = "";
                string title = "";
                string source = "";
                DateTime? date = null;

                try
                {
                    index = listboxFeedsTitle.SelectedIndex;
                    content = currentFeeds[index].Content;
                    link = currentFeeds[index].Link;
                    title = currentFeeds[index].Title;
                    source = currentFeeds[index].SourceName;
                    date = currentFeeds[index].Date;
                }
                catch { }

                ListBoxItem lbi1 = new ListBoxItem();
                lbi1.Content = title;
                lbi1.FontSize = contentSize + 10;

                ListBoxItem lbi2 = new ListBoxItem();
                lbi2.Content = source + Environment.NewLine + date?.ToString("yyyy.MM.dd HH:mm");
                lbi2.FontSize = contentSize + 5;

                ListBoxItem lbi3 = new ListBoxItem();
                lbi3.Content = content + Environment.NewLine + link.Trim();
                lbi3.FontSize = contentSize;

                var template = this.FindResource("myTaskTemplate") as DataTemplate;
                var style = this.FindResource("myLbiNoSelection") as Style;
                lbi1.Style = style;
                lbi2.Style = style;
                lbi3.Style = style;
                lbi1.ContentTemplate = template;
                lbi2.ContentTemplate = template;
                lbi3.ContentTemplate = template;
                listBoxContentFirst.Items.Clear();

                if (isDarkTheme)
                {
                    lbi1.Background = Brushes.Black;
                    lbi2.Background = Brushes.Black;
                    lbi3.Background = Brushes.Black;
                    lbi1.Foreground = Brushes.LightGray;
                    lbi2.Foreground = Brushes.LightGray;
                    lbi3.Foreground = Brushes.LightGray;
                }
                else
                {
                    lbi1.Background = Brushes.White;
                    lbi2.Background = Brushes.White;
                    lbi3.Background = Brushes.White;
                    lbi1.Foreground = Brushes.Black;
                    lbi2.Foreground = Brushes.Black;
                    lbi3.Foreground = Brushes.Black;
                }

                listBoxContentFirst.Items.Add(lbi1);
                listBoxContentFirst.Items.Add(lbi2);
                listBoxContentFirst.Items.Add(lbi3);

            }
            catch { }
        }

        private void OnTitleChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadC();
        }

        private async Task DelFeedFromDB()
        {
            try
            {
                if (dgFeeds.SelectedIndex > currentSources.Count)
                {
                    foreach (var item in listboxFeedsTitle.SelectedItems)
                    {
                        int index = listboxFeedsTitle.Items.IndexOf(item);
                        string link = currentFeeds[index].Link;
                        FeedsDatabaseManager.TrullyDelFeed(link);
                        //FeedsDatabaseManager.DelFeed(link, trash:true);
                    }
                }
                else
                {
                    foreach (var item in listboxFeedsTitle.SelectedItems)
                    {
                        int index = listboxFeedsTitle.Items.IndexOf(item);
                        string link = currentFeeds[index].Link;
                        FeedsDatabaseManager.DelFeed(link);
                    }
                }

                UpdateUnreadNumbers();
            }
            catch { }
        }

        private async Task DelFeed()
        {
            try
            {
                int currentTitleIndex = listboxFeedsTitle.SelectedIndex;
                await DelFeedFromDB();
                /*try
                {
                    if(dgFeeds.SelectedIndex > currentSources.Count)
                    {
                        foreach (var item in listboxFeedsTitle.SelectedItems)
                        {
                            int index = listboxFeedsTitle.Items.IndexOf(item);
                            string link = currentFeeds[index].Link;
                            FeedsDatabaseManager.TrullyDelFeed(link);
                            //FeedsDatabaseManager.DelFeed(link, trash:true);
                        }
                    }
                    else
                    {
                        foreach (var item in listboxFeedsTitle.SelectedItems)
                        {
                            int index = listboxFeedsTitle.Items.IndexOf(item);
                            string link = currentFeeds[index].Link;
                            FeedsDatabaseManager.DelFeed(link);
                        }
                    }

                    UpdateUnreadNumbers();


                    LoadContent();
                    listboxFeedsTitle.SelectedIndex=currentTitleIndex;
                }
                catch { }*/

                LoadContent();
                listboxFeedsTitle.SelectedIndex = currentTitleIndex;
            }
            catch { }
        }

        private async Task MarkAsReadINDB()
        {
            try
            {
                foreach (var item in listboxFeedsTitle.SelectedItems)
                {
                    int index = listboxFeedsTitle.Items.IndexOf(item);
                    string link = currentFeeds[index].Link;
                    FeedsDatabaseManager.MarkAsRead(link);
                }
                UpdateUnreadNumbers();
            }
            catch { }
        }
        private async Task MarkAsRead()
        {
            int selectedtitleIndex = listboxFeedsTitle.SelectedIndex;
            try
            {
                await MarkAsReadINDB();
                LoadContent();
                listboxFeedsTitle.SelectedIndex = selectedtitleIndex;
            }
            catch { }
            /*try
            {
                foreach (var item in listboxFeedsTitle.SelectedItems)
                {
                    int index = listboxFeedsTitle.Items.IndexOf(item);
                    string link = currentFeeds[index].Link;
                    FeedsDatabaseManager.MarkAsRead(link);
                }
            }
            catch { }
            finally
            {
                UpdateUnreadNumbers();
                LoadContent();
                listboxFeedsTitle.SelectedIndex = selectedtitleIndex;
            }*/
        }
        private void listboxFeedsTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete)
            {
                DelFeed();
            }
            if (e.Key == Key.K)
            {
                MarkAsRead();
            }
        }        

        private void IncreaseFontSize()
        {
            if (titlesFontSize > 50 || sourcesFontSize > 50 || contentSize > 50)
            {
                return;
            }
            else
            {
                titlesFontSize++;
                sourcesFontSize++;
                contentSize++;
                SetFontSize();
                try
                {
                    foreach (ListBoxItem lbi in listBoxContentFirst.Items)
                    {
                        lbi.FontSize++;
                    }
                }
                catch { }
            }
        }

        private void DecreaseFontSize()
        {
            if (titlesFontSize < 5 || sourcesFontSize < 5 || contentSize < 5)
            {
                return;
            }
            else
            {
                titlesFontSize--;
                sourcesFontSize--;
                contentSize--;
                SetFontSize();
                try
                {
                    foreach (ListBoxItem lbi in listBoxContentFirst.Items)
                    {
                        lbi.FontSize--;
                    }
                }
                catch { }
            }
        }

        private void IncreaseControlsSize()
        {
            /*if (buttonUpd.Width > 30)
            {
                return;
            }*/
            buttonUpd.Width += 1;
            buttonUpd.Height += 1;

            buttonDelSource.Width += 1;
            buttonDelSource.Height += 1;

            buttonAddSrce.Width += 1;
            buttonAddSrce.Height += 1;

            buttonImg1.Width += 1;
            buttonImg1.Height += 1;

            buttonImg2.Width += 1;
            buttonImg2.Height += 1;

            buttonImg3.Width += 1;
            buttonImg3.Height += 1;

            buttonMarkFeedsRead.Width += 1;
            buttonMarkFeedsRead.Height += 1;

            buttonDeleteFeed.Width += 1;
            buttonDeleteFeed.Height += 1;

            buttonTheme.Width += 1;
            buttonTheme.Height += 1;

            buttonDelSource.Margin = new Thickness(buttonDelSource.Margin.Left + 1, buttonDelSource.Margin.Top, buttonDelSource.Margin.Right, buttonDelSource.Margin.Bottom);
            buttonAddSrce.Margin = new Thickness(buttonAddSrce.Margin.Left + 2, buttonAddSrce.Margin.Top, buttonAddSrce.Margin.Right, buttonAddSrce.Margin.Bottom);
            buttonImg1.Margin = new Thickness(buttonImg1.Margin.Left + 3, buttonImg1.Margin.Top, buttonImg1.Margin.Right, buttonImg1.Margin.Bottom);
            buttonImg2.Margin = new Thickness(buttonImg2.Margin.Left + 3, buttonImg2.Margin.Top, buttonImg2.Margin.Right, buttonImg2.Margin.Bottom);
            buttonImg3.Margin = new Thickness(buttonImg3.Margin.Left + 3, buttonImg3.Margin.Top, buttonImg3.Margin.Right, buttonImg3.Margin.Bottom);
            buttonDeleteFeed.Margin = new Thickness(buttonDeleteFeed.Margin.Left + 1, buttonDeleteFeed.Margin.Top, buttonDeleteFeed.Margin.Right, buttonMarkFeedsRead.Margin.Bottom);

            gridrow1.Height = new GridLength(gridrow1.ActualHeight + 1);
        }

        private void IncreaseSize()
        {
            if( sizeModification < 10)
            {
                IncreaseFontSize();
                IncreaseControlsSize();
                sizeModification++;
            }
        }

        private void DecreaseSize()
        {
            if (sizeModification > -10)
            {
                DecreaseFontSize();
                DecreaseControlsSize();
                sizeModification--;
            }
        }
        private void DecreaseControlsSize()
        {
            /*if (buttonUpd.Width < 3)
            {
                return;
            }*/
            buttonUpd.Width -= 1;
            buttonUpd.Height -= 1;

            buttonDelSource.Width -= 1;
            buttonDelSource.Height -= 1;

            buttonAddSrce.Width -= 1;
            buttonAddSrce.Height -= 1;

            buttonImg1.Width -= 1;
            buttonImg1.Height -= 1;

            buttonImg2.Width -= 1;
            buttonImg2.Height -= 1;

            buttonImg3.Width -= 1;
            buttonImg3.Height -= 1;

            buttonMarkFeedsRead.Width -= 1;
            buttonMarkFeedsRead.Height -= 1;

            buttonDeleteFeed.Width -= 1;
            buttonDeleteFeed.Height -= 1;

            buttonTheme.Width -= 1;
            buttonTheme.Height -= 1;

            buttonDelSource.Margin = new Thickness(buttonDelSource.Margin.Left - 1, buttonDelSource.Margin.Top, buttonDelSource.Margin.Right, buttonDelSource.Margin.Bottom);
            buttonAddSrce.Margin = new Thickness(buttonAddSrce.Margin.Left - 2, buttonAddSrce.Margin.Top, buttonAddSrce.Margin.Right, buttonAddSrce.Margin.Bottom);
            buttonImg1.Margin = new Thickness(buttonImg1.Margin.Left - 3, buttonImg1.Margin.Top, buttonImg1.Margin.Right, buttonImg1.Margin.Bottom);
            buttonImg2.Margin = new Thickness(buttonImg2.Margin.Left - 3, buttonImg2.Margin.Top, buttonImg2.Margin.Right, buttonImg2.Margin.Bottom);
            buttonImg3.Margin = new Thickness(buttonImg3.Margin.Left - 3, buttonImg3.Margin.Top, buttonImg3.Margin.Right, buttonImg3.Margin.Bottom);
            buttonDeleteFeed.Margin = new Thickness(buttonDeleteFeed.Margin.Left - 1, buttonDeleteFeed.Margin.Top, buttonDeleteFeed.Margin.Right, buttonMarkFeedsRead.Margin.Bottom);

            gridrow1.Height = new GridLength(gridrow1.ActualHeight - 1);
        }
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Delta > 0)
                {
                    IncreaseSize();
                }
                else
                {
                    DecreaseSize();
                }
            }
        }

        private void OpenLink()
        {
            try
            {
                int index = listboxFeedsTitle.SelectedIndex;
                string linkStr = currentFeeds[index].Link;
                var link = new Uri(linkStr);
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = $"/c start {link.AbsoluteUri}"
                };
                Process.Start(psi);
            }
            catch { }
        }

        private void listboxFeedsTitle_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenLink();
        }

        private void listBoxContentFirst_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenLink();
        }

        private async Task DeleteSource()
        {
            int index = dgFeeds.SelectedIndex;
            if(index == 0 || index==-1 || index> currentSources.Count)
            {
                return;
            }
            if (MessageBox.Show("Удалить источник?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                int currentSourceIndex = dgFeeds.SelectedIndex;
                if (currentSourceIndex != 0 && currentSourceIndex <= currentSources.Count)
                {
                    string source = currentSources[currentSourceIndex - 1].Source;
                    string sourceName = currentSources[currentSourceIndex - 1].SourceName;
                    FeedsDatabaseManager.DelFeedSource(source, sourceName);
                    UpdateFeedsList();
                }
            }
        }


        private void buttonDeleteSource(object sender, RoutedEventArgs e)
        {
            DeleteSource();
        }

        private void buttonDeleteSourceKeyDown(object sender, KeyEventArgs e)
        {
           
        }

        private void buttonAddSource(object sender, RoutedEventArgs e)
        {
            var addWindow = new WindowAddSource(this);
            addWindow.Show();
        }

        public async Task AddFeedSource(string source)
        {
            FeedsDatabaseManager.InsertFeedSource(source);
            UpdateFeedsList();
            await FeedsDatabaseManager.UpdateFeeds();
            UpdateFeedsList();
            //UpdateTitles();
            //UpdateUnreadNumbers();
        }

        private async void buttonMarkRead(object sender, RoutedEventArgs e)
        {
            await MarkAsRead();
        }

        private async void buttonDelFeed(object sender, RoutedEventArgs e)
        {
            await DelFeed();
        }

        private void dgFeeds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadContent();
            //currentSourceIndex = dgFeeds.SelectedIndex;
            
        }

        //private int currentSourceIndex = 0;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.OemPlus)
                {
                    IncreaseSize();
                }
                if(e.Key == Key.OemMinus)
                {
                    DecreaseSize();
                }
            }
        }

        private void buttonThemeClick(object sender, RoutedEventArgs e)
        {
            if (isDarkTheme)
            {
                isDarkTheme = false;
            }
            else
            {
                isDarkTheme = true;
            }

            if (isDarkTheme)
            {
                this.Background = Brushes.Black;
                this.BorderBrush = Brushes.Black;
                listboxFeedsTitle.Background = Brushes.Black;
                gridsplitter1.Background = Brushes.Black;
                gridsplitter2.Background = Brushes.Black;
                gridsplitter3.Background = Brushes.Black;
                gridsplitter4.Background = Brushes.Black;
            }
            else
            {
                this.Background = Brushes.White;
                this.BorderBrush = Brushes.White;
                listboxFeedsTitle.Background = Brushes.White;
                gridsplitter1.Background = Brushes.White;
                gridsplitter2.Background = Brushes.White;
                gridsplitter3.Background = Brushes.White;
                gridsplitter4.Background = Brushes.White;
            }

            try
            {
                var sbTitles = GetScrollBar(listboxFeedsTitle) as ScrollBar;
                if (sbTitles != null)
                {
                    if (isDarkTheme)
                    {
                        sbTitles.Background = Brushes.Black;
                        sbTitles.BorderBrush = Brushes.Black;
                    }
                    else
                    {
                        sbTitles.Background = Brushes.White;
                        sbTitles.BorderBrush = Brushes.White;
                    }
                }

                var sbSources = GetScrollBar(dgFeeds) as ScrollBar;
                if (sbSources != null)
                {
                    if (isDarkTheme)
                    {
                        sbSources.Background = Brushes.Black;
                        sbSources.BorderBrush = Brushes.Black;
                    }
                    else
                    {
                        sbSources.Background = Brushes.White;
                        sbSources.BorderBrush = Brushes.White;
                    }
                }

                var sbContent = GetScrollBar(listBoxContentFirst) as ScrollBar;
                if (sbContent != null)
                {
                    if (isDarkTheme)
                    {
                        sbContent.Background = Brushes.Black;
                        sbContent.BorderBrush = Brushes.Black;
                    }
                    else
                    {
                        sbContent.Background = Brushes.White;
                        sbContent.BorderBrush = Brushes.White;
                    }
                }
            }
            catch { }

            UpdateTheme();

        }

        private void OnMouseDownMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void buttonCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonMaximizeClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void buttonHideClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        
    }

    public class FS
    {
        public FS(string sourceName, string count)
        {
            SourceName = sourceName;
            Count = count;
        }

        public string SourceName { get; set; }
        public string Count { get; set; }
    }
}
