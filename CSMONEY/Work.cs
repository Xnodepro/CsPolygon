using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Windows.Forms;
using System.Threading;
using HtmlAgilityPack;
using System.Drawing;
using System.Data;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using WebSocketSharp;
using System.IO;
using System.Web;

namespace CSMONEY
{
    class Work
    {
        IWebDriver driver;
        int ID = 0;

        List<string> me = new List<string>();
    //    List<ForItemsUSer> them = new List<ForItemsUSer>();
        DatUser ItemsUser;
        Dat ItemsBot;
        string apiKey = Properties.Settings.Default.ApiKey;
        string TradeURL = "";

        CookieContainer cookies = new CookieContainer();
        HttpClientHandler handler = new HttpClientHandler();
        #region botInventoryStruc
        public struct Dat
        {
           
            public string error { get; set; }
            public string balance { get; set; }
            public List<ite> items { get; set; }
        }
        public struct ite
        {

            public long assetid { get; set; }
          //  public int bot { get; set; }
            public int botid { get; set; }
         //   public List<it> items { get; set; }
            public long price { get; set; }
            public string name { get; set; }
        }
       

        #endregion
        #region ForItems
        public struct AnswerQuyery
        {
            public string tid { get; set; }
           
        }
        #endregion
        #region UserInventoryStruc
        public struct DatUser
        {
            public List<iteUser> items { get; set; }
        }
        public struct iteUser
        {

            public string name { get; set; }
            public double price { get; set; }
            public List<itUser> items { get; set; }
            public string id { get; set; }
        }
        public struct itUser
        {
            public string id { get; set; }

        }
        #endregion

        public Work(int id)
        {
            ID = id;
        }

        public void INI()
        {
            try
            {
                var driverService = ChromeDriverService.CreateDefaultService();  //скрытие 
                driverService.HideCommandPromptWindow = true;                    //консоли
                driver = new ChromeDriver(driverService);
                driver.Navigate().GoToUrl("https://csgopolygon.com");
                MessageBox.Show("Введите все данные , после этого программа продолжит работу!");
                driver.Manage().Window.Position = new Point(5000, 5000);
                driver.Navigate().GoToUrl("https://steamcommunity.com/id/me/tradeoffers/privacy#trade_offer_access_url");
                IWebElement offer = driver.FindElement(By.Id("trade_offer_access_url"));
                if (offer.GetAttribute("value") != Properties.Settings.Default.csmoney)
                {
                    MessageBox.Show("Выберите аккаунт к которому привязана программа!");
                    driver.Quit();
                    return;
                }

                driver.Navigate().GoToUrl("https://csgopolygon.com/withdraw.php");
                driver.Manage().Window.Position = new Point(0, 0);
                TradeURL = driver.FindElement(By.Id("tradeurl")).GetAttribute("value").ToString();
                var _cookies = driver.Manage().Cookies.AllCookies;
                foreach (var item in _cookies)
                {
                    handler.CookieContainer.Add(new System.Net.Cookie(item.Name, item.Value) { Domain = item.Domain });
                }
                //Запуск запросов на отслеживания инвентаря ботов
                for (int i = 0; i < Program.threadCount; i++)
                {
                    new System.Threading.Thread(delegate ()
                    {
                        try
                        {
                            Get(handler);
                        }
                        catch (Exception ex) { }
                    }).Start();
                }
                     
                
                //new System.Threading.Thread(delegate () {
                //        while (true)
                //        {
                //            try
                //            {
                //                Thread.Sleep(2000);
                //                driver.Navigate().Refresh();
                //                Thread.Sleep(300000);
                //            }
                //            catch (Exception ex) { }

                //        }
                //    }).Start();
                
            }
            catch (Exception ex) { Program.Mess.Enqueue(ex.Message); }

        }

        public HttpClient Prox(HttpClient client1, HttpClientHandler handler, string paroxyu)
        {

            HttpClient client = client1;
            try
            {
                string
                httpUserName = "webminidead",
                httpPassword = "159357Qq";
                string proxyUri = paroxyu;
                NetworkCredential proxyCreds = new NetworkCredential(
                    httpUserName,
                    httpPassword
                );
                WebProxy proxy = new WebProxy(proxyUri, false)
                {
                    UseDefaultCredentials = false,
                    Credentials = proxyCreds,
                };
                try
                {
                    handler.Proxy = null;
                    handler.Proxy = proxy;
                    handler.PreAuthenticate = true;
                    handler.UseDefaultCredentials = false;
                    handler.Credentials = new NetworkCredential(httpUserName, httpPassword);
                    handler.AllowAutoRedirect = true;
                }
                catch (Exception ex) { }
                client = new HttpClient(handler);
            }
            catch (Exception ex) { }
            return client;
        }
        private void Get(HttpClientHandler handler)
        {
            HttpClientHandler handler1 = handler;

            while (true)
            {
                try
                {
                    HttpClientHandler handler2 = new HttpClientHandler();
                    var _cookies = driver.Manage().Cookies.AllCookies;
                    foreach (var item in _cookies)
                    {
                        handler2.CookieContainer.Add(new System.Net.Cookie(item.Name, item.Value) { Domain = item.Domain });
                    }

                    HttpClient client = null;
                    if (Program.ProxyList.Count <= 0)
                    {
                        client = new HttpClient(handler2);
                    }
                    else
                    { 
                        string newProxy = Program.ProxyList.Dequeue();
                        handler2.Proxy = null;
                        client = Prox(client, handler2, newProxy);
                    }

                    client.Timeout = TimeSpan.FromSeconds(40);
                    
                    client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36");
                    client.DefaultRequestHeaders.Add("accept", "*/*");
                    client.DefaultRequestHeaders.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                    client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
                    //     client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");

                    var response = client.GetAsync("https://csgopolygon.com/scripts/_get_bank.php?captcha="+Program.CapchaString).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;
                        string responseString = responseContent.ReadAsStringAsync().Result;
                  //      Program.Mess.Enqueue("" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "|" + "Завершил загрузку предметов:" );
                        var ITEMS = JsonConvert.DeserializeObject<Dat>(responseString);
                 //       ItemsBot = ITEMS;
                        //Program.DataJar = ITEMS;
                        ClickItem(ITEMS);
                        Program.Mess.Enqueue("" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "|" + "Завершил загрузку предметов:" + ITEMS.items.Count);
                    }

                    Thread.Sleep(1000);
                }
                catch (Exception ex) { Program.Mess.Enqueue("" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "|"  + ex.Message); }
            }
            // return new Data();
        }
       
      
     
        private bool ClickItem(Dat json)
        {

            try
            {
                var da = json;
                foreach (var item in da.items)
                {

                    foreach (var name in Program.Data)
                    {
                       // string _name = HttpUtility.UrlDecode(item.itemURLName);
                        if (item.name.Replace(" ", "") == (name.Name).Replace(" ", ""))
                        {
                            Program.Mess.Enqueue("БОТ[" + ID + "] " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "| Нашел предмет :" + item.name + "|Цена_Сайта:" + item.price + "|Цена_Наша:" + name.Price);
                            if (item.price <= name.Price)
                            {
                                Program.Mess.Enqueue("БОТ[" + ID + "] " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "| Оправил Запрос |");
                                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                string ss1 = "function test() {var xhr = new XMLHttpRequest();" +
                                    " xhr.open(\"GET\", 'https://csgopolygon.com/scripts/_withdraw.php?assetids="+item.assetid.ToString()+"%2C&tradeurl="+HttpUtility.UrlEncode(TradeURL)+"&checksum="+item.price.ToString()+"&remember=on&botid="+item.botid.ToString()+"', false); " +
                                    " xhr.setRequestHeader('Accept', '*/*');" +
                                    " xhr.send();return xhr.responseText; } return test();";
                                var title = js.ExecuteScript(ss1);
                                Program.Mess.Enqueue("БОТ[" + ID + "] " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "|Ответ от сервера на запрос|"+ title);
                                var ITEMS = JsonConvert.DeserializeObject<AnswerQuyery>(title.ToString());
                                if (ITEMS.tid != null)
                                {
                                    Program.Mess.Enqueue("БОТ[" + ID + "] " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "| Ждем 40 сек для подтверждения |");
                                    Thread.Sleep(40000);
                                    string ss2 = "function test() {var xhr = new XMLHttpRequest();" +
                                    " xhr.open(\"GET\", 'https://csgopolygon.com/scripts/_confirm.php?tid=" + ITEMS.tid.ToString()  + "', false); " +
                                    " xhr.setRequestHeader('Accept', '*/*');" +
                                    " xhr.send();return xhr.responseText; } return test();";
                                    var title2 = js.ExecuteScript(ss2);
                                    Program.Mess.Enqueue("БОТ[" + ID + "] " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "|Ответ от сервера на подтверждение|" + title2);
                                }
                                Program.Mess.Enqueue(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "Завершил все запросы!");
                                Thread.Sleep(3000);
                                return false;

                            }
                            else
                            {
                                SetListBadPrice(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), "cs.money", item.name.ToString(), name.Price.ToString(), item.price.ToString());
                            }

                        }
                    }

                }
            }
            catch (Exception ex) { Program.Mess.Enqueue("БОТ[" + ID + "] " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "|Ошибка2 :" + ex.Message); }
            return false;
        }
       
     
        private void SetListBadPrice(string _Data, string _Site, string _Name, string _OldPrice, string _NewPrice)
        {
            Program.DataViewBadPrice item = new Program.DataViewBadPrice()
            {
                Date = _Data,
                Site = _Site,
                Name =_Name,
                OldPrice = _OldPrice,
                NewPrice =_NewPrice
            };
            Program.BadPrice.Add(item);
        }
    }
}
