﻿using System;
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
using CefSharp;
using CSharpCrawler.Model;
using ZT.Enhance;

namespace CSharpCrawler.Views
{
    /// <summary>
    /// FetchDynamicResource.xaml 的交互逻辑
    /// </summary>
    public partial class FetchDynamicResource : Page
    {
        //动态网页加载次数
        int count = 0;
        //记录当前网址动态加载获取到的网页内容
        List<HtmlStruct> recordList = new List<HtmlStruct>();

        int countIE = 0;
        List<HtmlStruct> recordListIE = new List<HtmlStruct>();

        public FetchDynamicResource()
        {
            InitializeComponent();
        }

        #region Chromium 事件
        private void cbox_ShowChrome_Checked(object sender, RoutedEventArgs e)
        {
            ShowOrHideChromiumBrowser(true);
        }

        private void cbox_ShowChrome_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowOrHideChromiumBrowser(false);
        }

       

        private void btn_Fetch_Click(object sender, RoutedEventArgs e)
        {
            string url = this.tbox_Url.Text.Trim();
            if(string.IsNullOrEmpty(url))
            {
                EMessageBox.Show("请输入网址");
                return;
            }

            OpenUrlWithChromium(url);
        }

        /// <summary>
        /// Chromium加载网页完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void chromiumBrowser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            count++;
            string url = "";
            string title = "";
            ShowCountMessage(count);
            string html =await chromiumBrowser.GetSourceAsync();
            this.Dispatcher.Invoke(()=> {
                url = chromiumBrowser.Address;
                title= chromiumBrowser.Title;
            });
            ShowHtml(html,url,title);
        }


        private void combox_Record_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = this.combox_Record.SelectedIndex;
            if (index > -1)
                ShowRecord(index);
        }

        #endregion

        #region Chromium 方法

        /// <summary>
        /// 显示或隐藏浏览器
        /// </summary>
        /// <param name="showFlag"></param>
        private void ShowOrHideChromiumBrowser(bool showFlag)
        {
            if (showFlag == true)
            {
                this.chromiumBrowser.Width = this.grid_Content.ActualWidth / 2;
            }
            else
            {
                this.chromiumBrowser.Width = 0;
            }
        }

        /// <summary>
        /// 使用Chromium浏览器打开网址
        /// </summary>
        /// <param name="url"></param>
        private void OpenUrlWithChromium(string url)
        {
            ResetCounter();
            ClearRecordList();
            this.chromiumBrowser.Address = url;
        }

        /// <summary>
        /// 重置计数器
        /// </summary>
        private void ResetCounter()
        {
            count = 0;
            ShowCountMessage(count);
        }

        /// <summary>
        /// 显示计数信息
        /// </summary>
        /// <param name="count"></param>
        private void ShowCountMessage(int count)
        {
            this.Dispatcher.Invoke(()=> {
                this.lbl_StatusText.Content = "当前网址抓取次数: " + count.ToString();
            });
        }

        /// <summary>
        /// 显示Html到文本框中
        /// </summary>
        /// <param name="html"></param>
        private void ShowHtml(string html,string url,string title)
        {
            this.Dispatcher.Invoke(()=> {
                this.rtbox_Resource.Document = new FlowDocument(new Paragraph(new Run(html)));
                HtmlStruct htmlStruct = new HtmlStruct() { Content = html,Url = url,Title = title };
                RecordHtml(htmlStruct);
            }); 
            
        }

        /// <summary>
        /// 记录动态加载的网页内容
        /// </summary>
        /// <param name="html"></param>
        private void RecordHtml(HtmlStruct htmlStruct)
        {
            recordList.Add(htmlStruct);
            this.combox_Record.ItemsSource = null;
            this.combox_Record.ItemsSource = recordList.Select(x=>x.Url); ;
        }

        /// <summary>
        /// 清空网页记录列表
        /// </summary>
        private void ClearRecordList()
        {
            recordList.Clear();
            this.combox_Record.ItemsSource = null;
        }

        /// <summary>
        /// 获取历史记录
        /// </summary>
        /// <param name="index"></param>
        private void ShowRecord(int index)
        {
            if (index >= 0 && index < recordList.Count)
            {
                this.Dispatcher.Invoke(()=> {
                    string html = recordList[index].Content;
                    this.rtbox_Resource.Document = new FlowDocument(new Paragraph(new Run(html)));
                });
            }
        }
        #endregion

        #region IE 事件
        private void cbox_ShowChrome_IE_Checked(object sender, RoutedEventArgs e)
        {
            ShowOrHideIEBrowser(true);
        }

        private void cbox_ShowChrome_IE_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowOrHideIEBrowser(false);
        }

        private void btn_Fetch_IE_Click(object sender, RoutedEventArgs e)
        {
            string url = this.tbox_Url_IE.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                EMessageBox.Show("请输入网址");
                return;
            }

            OpenUrlWithIE(this.tbox_Url_IE.Text);
        }

        private void combox_Record_IE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = this.combox_Record_IE.SelectedIndex;
            if (index > -1)
                ShowRecordIE(index);
        }


        private void ieBrowser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            countIE++;
            string url = "";
            string title = "";
            ShowCountMessageIE(countIE);
            string html = ieBrowser.Document.GetElementsByTagName("html")[0].OuterHtml;
            this.Dispatcher.Invoke(() => {
                url = ieBrowser.Url.ToString();
                title = ieBrowser.Document.Title;
            });
            ShowHtmlIE(html, url, title);
        }
        #endregion

        #region IE 方法
        /// <summary>
        /// 显示或隐藏浏览器
        /// </summary>
        /// <param name="showFlag"></param>
        private void ShowOrHideIEBrowser(bool showFlag)
        {
            if (showFlag == true)
            {
                this.formHost.Width = (int)this.grid_Content_IE.ActualWidth / 2;
            }
            else
            {
                this.formHost.Width = 0;
            }
        }

        public void ShowCountMessageIE(int count)
        {
            this.Dispatcher.Invoke(() => {
                this.lbl_StatusText_IE.Content = "当前网址抓取次数: " + count.ToString();
            });
        }

        private void ShowHtmlIE(string html,string url,string title)
        {
            this.Dispatcher.Invoke(() => {
                this.rtbox_Resource_IE.Document = new FlowDocument(new Paragraph(new Run(html)));
                HtmlStruct htmlStruct = new HtmlStruct() { Content = html, Url = url, Title = title };
                RecordHtmlIE(htmlStruct);
            });
        }

        private void RecordHtmlIE(HtmlStruct htmlStruct)
        {
            recordListIE.Add(htmlStruct);
            this.combox_Record_IE.ItemsSource = null;
            this.combox_Record_IE.ItemsSource = recordListIE.Select(x => x.Url); ;
        }

        public void ResetCounterIE()
        {
            countIE = 0;
            ShowCountMessageIE(countIE);
        }

        private void ClearRecordListIE()
        {
            recordListIE.Clear();
            this.combox_Record_IE.ItemsSource = null;
        }

        private void OpenUrlWithIE(string url)
        {
            ResetCounterIE();
            ClearRecordListIE();
            this.ieBrowser.Navigate(url);
        }

        private void ShowRecordIE(int index)
        {
            if (index >= 0 && index < recordListIE.Count)
            {
                this.Dispatcher.Invoke(() => {
                    string html = recordListIE[index].Content;
                    this.rtbox_Resource_IE.Document = new FlowDocument(new Paragraph(new Run(html)));
                });
            }
        }
        #endregion

        #region Puppeteer 事件

        private async void btn_Fetch_Puppeteer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var url = this.tbox_Url_Puppeteer.Text.Trim();

                if(string.IsNullOrEmpty(url))
                {
                    ShowStatusTextPuppeteer("请输入网址");
                    return;
                }

                var html = await GetHtmlSourceByPuppeteer(url);
                this.rtbox_Resource_Puppeteer.Document = new FlowDocument(new Paragraph(new Run(html)));
            }
            catch(Exception ex)
            {
                ShowStatusTextPuppeteer(ex.Message);
            }
        }

        #endregion

        #region Puppeteer 方法

        private async Task<string> GetHtmlSourceByPuppeteer(string url)
        {
            //增加一些状态消息 
            ShowStatusTextPuppeteer("正在下载Chromium浏览器");

            //下载Chromium浏览器
            await new PuppeteerSharp.BrowserFetcher().DownloadAsync(PuppeteerSharp.BrowserFetcher.DefaultRevision);

            ShowStatusTextPuppeteer("初始化浏览器");
            var options = new PuppeteerSharp.LaunchOptions();

            var showBrowerFlag = this.cbox_ShowChrome_Puppeteer.IsChecked.Value;
            //如果显示浏览器，需要设置为false
            options.Headless = showBrowerFlag == true ? false : true;
            //启动浏览器
            var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(options);

            //打开一个标签
            var page = await browser.NewPageAsync();
            ShowStatusTextPuppeteer($"打开网址{url}");
            await page.GoToAsync(url);

            var html =  await page.GetContentAsync();

            //关闭浏览器
            browser.Disconnect();
            browser.Dispose();
            ShowStatusTextPuppeteer("就绪");
            return html;
        }

        private void ShowStatusTextPuppeteer(string text)
        {
            this.Dispatcher.Invoke(() => {
                this.lbl_StatusText_Puppeteer.Content = text;
            });
        }
        #endregion


    }
}
