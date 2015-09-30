using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
//using System.Threading;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamPrice
{    
    public partial class Form1 : Form
    {
        static object locker = new object();
        int page = 1;
        int rowsPerPage = 20;
        double price80 = 2.0;        
        List<NewGame> games = new List<NewGame>();
        public Form1()
        {
            InitializeComponent();
            progressBar1.Minimum = 0;            
            progressBar1.Value = 1;
            progressBar1.Step = 1;
            
            refresh_80_price();

        }        


        private void button7_Click(object sender, EventArgs e)
        {
            
        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            using (var fStream = File.OpenRead("./NewGames.dat"))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                games = (List<NewGame>)formatter.Deserialize(fStream);
            }
            

            
            refreshGrid();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (var fStream = new FileStream("./NewGames.dat", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(fStream, games);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            page = (page == 1) ? 1 : page - 1;
            refreshGrid();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            page = (page == games.Count/rowsPerPage) ? page : page + 1;
            refreshGrid();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            games.Sort(delegate(NewGame x, NewGame y)
            {
                if (x.profit < y.profit)
                    return 1;
                return -1;
            });            
            refreshGrid();
            
            /*newGameBindingSource.DataSource = games.FindAll(x => x.cards.FindAll(c=>c.updated>DateTime.Today).Count>0);
            //newGameBindingSource.DataSource = games.FindAll(x => x.name.IndexOf(textBox1.Text, StringComparison.OrdinalIgnoreCase) != -1).Take(200);
            if (dataGridView1.Rows[0].Cells[0].Value != null)
                foreach (DataGridViewRow dgr in dataGridView1.Rows)
                {
                    double volume = 0;
                    int kol = 0;
                    foreach (GameCard gc in ((NewGame)dgr.DataBoundItem).cards.FindAll(x => !x.foil))
                    {
                        kol++;
                        if (gc != null)
                            volume += gc.volume == null ? 0 : Convert.ToDouble(gc.volume);
                    }
                    if (kol > 0)
                    {
                        int red = Convert.ToInt32(((volume / kol) * 50 > 255 ? 0 : 255 - (volume / kol) * 50));
                        int green = Convert.ToInt32(((volume / kol) * 50 > 255 ? 255 : (volume / kol) * 50));
                        dgr.DefaultCellStyle.BackColor = Color.FromArgb(red, green, 0);
                    }
                }
            dataGridView1.Refresh();*/
        }
        public double profit(NewGame game) {
            int kol = 0;
            double summ = 0;            
            string volume = "";
            foreach (GameCard gc in game.cards.FindAll(x=>!x.foil))
            {
                if (gc.game.id == game.id )
                {
                    kol++;
                    if (gc != null)
                    {
                        if (gc.LowestPrice != null)
                            summ += Convert.ToDouble(gc.LowestPrice);
                        volume += gc.volume + " ";
                    }
                }
            }
            double genCardsProfit = kol == 0 ? 0 : ((summ * 3 / kol) / 1.15);
            return kol == 0 ? 0 : ((genCardsProfit > Convert.ToDouble(game.gamePack.LowestPrice) ? genCardsProfit : Convert.ToDouble(game.gamePack.LowestPrice)) - 6000 / kol / 80 * price80);
        }

        public void refreshGrid(){
            //List<NewGame> sdf = games.FindAll(x => x.id!= null).FindAll(x=>x.id.IndexOf("320760")!=-1);
            newGameBindingSource.DataSource = games.FindAll(x => x.favorite == checkBox1.Checked).FindAll(x => x.name.IndexOf(textBox1.Text, StringComparison.OrdinalIgnoreCase) != -1).Skip((page - 1) * rowsPerPage).Take(rowsPerPage);
            //newGameBindingSource.DataSource = games.FindAll(x => x.name.IndexOf(textBox1.Text, StringComparison.OrdinalIgnoreCase) != -1).Take(200);
            if (dataGridView1.Rows[0].Cells[0].Value!=null)
                foreach (DataGridViewRow dgr in dataGridView1.Rows)
                {
                    double volume = 0;
                    int kol = 0;
                    foreach (GameCard gc in ((NewGame)dgr.DataBoundItem).cards.FindAll(x=>!x.foil))
                    {
                        kol++;
                        if (gc != null)
                            volume += gc.volume == null ? 0 : Convert.ToDouble(gc.volume);
                    }
                    if (kol > 0)
                    {
                        int red = Convert.ToInt32(((volume / kol) * 50 > 255 ? 0 : 255 - (volume / kol) * 50));
                        int green = Convert.ToInt32(((volume / kol) * 50 > 255 ? 255 : (volume / kol) * 50));
                        dgr.DefaultCellStyle.BackColor = Color.FromArgb(red, green, 0);
                    }
                }
            dataGridView1.Refresh();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://www.steamcardexchange.net/index.php?boosterprices");
            request.AllowAutoRedirect = false;
            //request.CookieContainer.Add(new Cookie("PHPSESSID", "tep5gkkt47qktdl8fsrh0rqeqqvbhs5j"));
            HttpWebResponse flixresponse = (HttpWebResponse)request.GetResponse();            
            request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://www.steamcardexchange.net/index.php?boosterprices");
            request.CookieContainer = new CookieContainer();
            string value= flixresponse.Headers["Set-Cookie"].Remove(0,10);
            value=value.Remove(value.IndexOf(";"),value.Length-value.IndexOf(";"));
            Cookie cookie = new Cookie("PHPSESSID", value, "/", "localhost");
            request.CookieContainer.Add(cookie);
            flixresponse = (HttpWebResponse)request.GetResponse();
            
            StreamReader response = new StreamReader(flixresponse.GetResponseStream(), Encoding.UTF8);
            string html = response.ReadToEnd();
            html = html.Remove(0,html.IndexOf("<tbody>") + 7);
            html = html.Remove(html.IndexOf("</tbody>"), html.Length - html.IndexOf("</tbody>"));

            MatchCollection matches = Regex.Matches(html, "(?<=<tr>)(.*?)(?<=</tr>)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
            if (matches.Count != 0)
            {
                foreach (Match match in matches)
                {
                    string currmatch = match.Groups[0].Value;
                    
                    string gameId = Regex.Match(currmatch, "(?<=appid-)(.*?)(?=\\\">)").ToString();
                    currmatch = currmatch.Remove(0, currmatch.IndexOf("appid"));
                    string gameName = Regex.Match(currmatch, "(?<=\">)(.*)(?=</a>)").ToString();

                    currmatch = match.Groups[0].Value;
                    string ItemGame = currmatch.Substring(currmatch.IndexOf(">")+1);
                    ItemGame = ItemGame.Remove(ItemGame.IndexOf("</a"));

                    if (games.Find(x => x.name == gameName) == null)
                    {
                        NewGame tmpNG = new NewGame();
                        tmpNG.name = gameName;
                        tmpNG.id = gameId;

                        //карты в руки
                        string responseHtml = Requests.GetHttpResponse("http://steamcommunity.com/market/search/render/?query=" + gameName + " Trading Card&start=0&count=200");
                        if (responseHtml == null)
                            break;
                        tmpNG.GetCards(responseHtml);

                        //обновили карты
                        refreshPrices(tmpNG);

                        //схоронили игоря
                        games.Add(tmpNG);
                    }
                }
            }

            /*
            HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://steamcommunity.com/market/search/render/?query=Trading Card&start=0&count=1");
            HttpWebResponse flixresponse = (HttpWebResponse)request.GetResponse();
            StreamReader response = new StreamReader(flixresponse.GetResponseStream(), Encoding.UTF8);
            string html = response.ReadToEnd();
            var searchJS = JsonConvert.DeserializeObject<SearchBody>(html);
            List<NewGame> tmpList = new List<NewGame>();              
            if (searchJS.Success) {
                int count = Convert.ToInt32(searchJS.TotalCount);                
                int countPerPage = 100;
                for (int page = 0; page < count / countPerPage; page++)
                {
                    GetGamesThreadable(page, tmpList);
                }
            }
             */
        }
        public int CompareGames(NewGame x, NewGame y) {
            if (profit(x) < profit(y))
                return -1;
            return 1;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            NewGame ng = dataGridView1.Rows[((System.Windows.Forms.DataGridView)sender).CurrentCellAddress.Y].DataBoundItem as NewGame;
            //refreshPrices(ng);
            //dataGridView1.Refresh();
            GameInfo gi = new GameInfo(ng);
            gi.Show();
        }

        public void refreshPrices(NewGame g)
        {
            progressBar1.Value = 0;
            progressBar1.Maximum = g.cards.Count == 0 ?1:g.cards.Count / 2;
            foreach (GameCard gc in g.cards.FindAll(x=>!x.foil))
            {                
                gc.RefreshThreadable();
            }
            g.gamePack.Refresh(progressBar1);
            g.profit = profit(g);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            refreshGrid();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            NewGame ng = new NewGame();
            games.Add(ng);
            GameInfo gi = new GameInfo(ng);
            gi.Show();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            refresh_80_price();
        }

        private void refresh_80_price() {
            string url = "http://steamcommunity.com/market/priceoverview/?country=US&currency=5&appid=753&market_hash_name=550-Cemetery";            
            try
            {
                HttpWebRequest requestCards = (HttpWebRequest)WebRequest.Create(url);
                requestCards.Method = "GET";
                requestCards.Accept = "application/json";
                requestCards.ContentType = "application/json";
                DataContractJsonSerializer jsonSerializerCards = new DataContractJsonSerializer(typeof(GameEntity));
                HttpWebResponse response = (HttpWebResponse)requestCards.GetResponse();
                Stream stream = response.GetResponseStream();
                GameEntity responseData = (GameEntity)jsonSerializerCards.ReadObject(stream);                
                price80 = Convert.ToDouble(responseData.LowestPrice);
                textBox2.Text = price80.ToString();
            }
            catch (WebException web)
            {

            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows[0].Cells[0].Value != null)
                foreach (DataGridViewRow dgr in dataGridView1.Rows)
                {
                    dgr.Selected = true;
                    dataGridView1.Refresh();
                    refreshPrices((NewGame)dgr.DataBoundItem);
                    dgr.Selected = false;
                    dataGridView1.Refresh();
                    
                }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            page = 1;
            refreshGrid();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            foreach (NewGame ng in games)
                refreshPrices(ng);
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            games.Sort(delegate(NewGame x, NewGame y)
            {
                return String.Compare(x.name, y.name);
            });
            refreshGrid();
        }

        public void GetGamesThreadable(int page,List<NewGame> ngList)
        {
            Thread thread;

            thread = new Thread(delegate() { GetGames(page, ngList); });
            
            thread.Name = "get games page " + page;
            thread.Start();
        }

        private void GetGames(int page,List<NewGame> ngList)
        {            
            int countPerPage = 100;
            lock (locker)
            {
                Thread.Sleep(5000);
            }


            HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://steamcommunity.com/market/search/render/?query=Trading Card&start=" + page * countPerPage + "&count=" + countPerPage);
            HttpWebResponse flixresponse = (HttpWebResponse)request.GetResponse();
            StreamReader response = new StreamReader(flixresponse.GetResponseStream(), Encoding.UTF8);
            string html = response.ReadToEnd();
            var searchJS = JsonConvert.DeserializeObject<SearchBody>(html);
                    int kol = 0;
                    if (searchJS.Success)
                    {
                        MatchCollection matches = Regex.Matches(searchJS.HtmlRes, "(?<=market_listing_row_link\" href)(.*?)(?<=</a>)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
                        if (matches.Count != 0)
                        {
                            foreach (Match match in matches)
                            {
                                string currmatch = match.Groups[1].Value;
                                string ItemGame = Regex.Match(html, "(?<=game_name\">)(.*)(?=</span>)").ToString();
                                ItemGame = ItemGame.Replace(" Foil Trading Card", "");
                                ItemGame = ItemGame.Replace(" Trading Card", "");
                                /*string url = Regex.Match(html, "(?<==\")(.*)(?=\" id)").ToString();
                                string volume = Regex.Match(html, "(?<=num_listings_qty\">)(.*)(?=</span>)").ToString();
                                string ItemName = Regex.Match(html, "(?<=listing_item_name\" style=\"color:)(.*)(?=</span>)").ToString();
                                ItemName = ItemName.Remove(0, ItemName.IndexOf(">") + 1);
                                string name = ItemName;
                                string img_url = Regex.Match(html, "(?<=net/economy/image/)(.*)(/62fx62f)", RegexOptions.Singleline).ToString();*/
                                NewGame tmpNG = new NewGame();
                                tmpNG.name = ItemGame;
                                lock (locker)
                                {
                                    ngList.Add(tmpNG);
                                }
                                //NewGame tmpNG = games.Find(x => x.name == name);
                            }
                        }
                    }
        }

        
    }
}
