using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace SteamPrice
{

    public class SearchBody
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("results_html")]
        public string HtmlRes { get; set; }
        [JsonProperty("total_count")]
        public string TotalCount { get; set; }
    }

    [DataContractAttribute]
    class Response
    {
        [DataMemberAttribute]
        public string catkey { get; set; }
        [DataMemberAttribute]
        public string itemname { get; set; }
        [DataMemberAttribute]
        public string itemtype { get; set; }
        [DataMemberAttribute]
        public string game { get; set; }
        [DataMemberAttribute]
        public bool trading_card { get; set; }
        [DataMemberAttribute]
        public double price { get; set; }
        [DataMemberAttribute]
        public string updated { get; set; }
    }
    [DataContractAttribute]
    class GameResponse
    {
        [DataMemberAttribute]
        public string appid { get; set; }
        [DataMemberAttribute]
        public string name { get; set; }
        [DataMemberAttribute]
        public Response[] cards { get; set; }
    }    
    [DataContractAttribute, Serializable]
    public class GameCardData
    {
        [DataMemberAttribute]
        public bool success { get; set; }
        [DataMemberAttribute]
        public string lowest_price { get; set; }
        [DataMemberAttribute]
        public string volume { get; set; }
        [DataMemberAttribute]
        public string median_price { get; set; }
        
    }

    [DataContractAttribute, Serializable]
    public class GameEntity
    {
        public NewGame game { get; set; }
        [DataMemberAttribute]
        public bool success { get; set; }
        [DataMemberAttribute]
        private string lowest_price;
        [DataMemberAttribute]
        public string LowestPrice
        {
            get
            {
                return lowest_price == null ?
                        "0" :
                        lowest_price.IndexOf(" ") == -1 ?
                            lowest_price :
                            lowest_price.Remove(lowest_price.IndexOf(" "));
            }
            set
            {
                lowest_price = value == null ?
                    "0" :
                    value.IndexOf(" ") == -1 ?
                        value :
                        value.Remove(value.IndexOf(" "));
            }
        }
        [DataMemberAttribute]
        public string volume { get; set; }
        [DataMemberAttribute]
        public string median_price { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string img_url { get; set; }
        public DateTime updated { get; set; }

        public void Refresh(System.Windows.Forms.ProgressBar pg)
        {
            
            url = "http://steamcommunity.com/market/priceoverview/?country=US&currency=5&appid=753&market_hash_name=" + game.id + "-" + name;
            string request_string = url;
            try
            {
                HttpWebRequest requestCards = (HttpWebRequest)WebRequest.Create(request_string);
                requestCards.Method = "GET";
                requestCards.Accept = "application/json";
                requestCards.ContentType = "application/json";
                DataContractJsonSerializer jsonSerializerCards = new DataContractJsonSerializer(typeof(GameEntity));
                HttpWebResponse response = (HttpWebResponse)requestCards.GetResponse();
                Stream stream = response.GetResponseStream();
                GameEntity responseData = (GameEntity)jsonSerializerCards.ReadObject(stream);

                success = responseData.success;
                LowestPrice = responseData.LowestPrice;
                volume = responseData.volume;
                median_price = responseData.median_price == null ? " " : responseData.median_price;
                median_price = median_price.Remove(median_price.IndexOf(" "));
                updated = DateTime.Now;
                pg.PerformStep();
            }
            catch (WebException web)
            {

            }
        }
        public void RefreshThreadable()
        {
            Thread thread = new Thread(this.Refresh);
            thread.Name = "update " + name;
            thread.Start();
        }
        private void Refresh()
        {
            url = "http://steamcommunity.com/market/priceoverview/?country=US&currency=5&appid=753&market_hash_name=" + game.id + "-" + name;
            string request_string = url;
            try
            {
                HttpWebRequest requestCards = (HttpWebRequest)WebRequest.Create(request_string);
                requestCards.Method = "GET";
                requestCards.Accept = "application/json";
                requestCards.ContentType = "application/json";
                DataContractJsonSerializer jsonSerializerCards = new DataContractJsonSerializer(typeof(GameEntity));
                HttpWebResponse response = (HttpWebResponse)requestCards.GetResponse();
                Stream stream = response.GetResponseStream();
                GameEntity responseData = (GameEntity)jsonSerializerCards.ReadObject(stream);

                success = responseData.success;
                LowestPrice = responseData.LowestPrice;
                volume = responseData.volume;
                median_price = responseData.median_price == null ? " " : responseData.median_price;
                median_price = median_price.Remove(median_price.IndexOf(" "));
                updated = DateTime.Now;
            }
            catch (WebException web)
            {

            }
        }
    }

    [Serializable]
    public class GameCard:GameEntity
    {
        private string currmatch;

        public GameCard() { }
        public GameCard(string html,NewGame ng) {            
            string ItemGame = Regex.Match(html, "(?<=game_name\">)(.*)(?=</span>)").ToString();
            ItemGame = ItemGame.Replace(" Foil Trading Card", "");
            ItemGame = ItemGame.Replace(" Trading Card", "");
            if (ItemGame.Equals(ng.name))
            {
                this.game = ng;
                //Fix for Steam update 5/01/14 4:00 PM PST
                this.url = Regex.Match(html, "(?<==\")(.*)(?=\" id)").ToString();
                this.volume = Regex.Match(html, "(?<=num_listings_qty\">)(.*)(?=</span>)").ToString();
                string ItemName = Regex.Match(html, "(?<=listing_item_name\" style=\"color:)(.*)(?=</span>)").ToString();
                ItemName = ItemName.Remove(0, ItemName.IndexOf(">") + 1);
                this.name = ItemName;
                this.img_url = Regex.Match(html, "(?<=net/economy/image/)(.*)(/62fx62f)", RegexOptions.Singleline).ToString();

            }
        }

        public bool foil { get; set; }
        public void Refresh(System.Windows.Forms.ProgressBar pg)
        {
            if (!foil)
                base.Refresh(pg);
        }
        public void FoilRefresh(System.Windows.Forms.ProgressBar pg)
        {
            if (foil)
                base.Refresh(pg);
        }

    }

    [Serializable]
    public class GamePack : GameCard
    {
        public GamePack(NewGame ng) { foil = false; game = ng; name = ng.name + "%20Booster%20Pack"; }
    }

    [Serializable]
    public class NewGame
    {
        public string name { get; set; }
        public string id { get; set; }
        public double profit { get; set; }
        public double emoticonsGems { get; set; }
        public List<GameCard> cards;
        public List<Emoticon> emoticons;
        public List<Background> backgrounds;
        public GamePack gamePack { get; set; }

        public bool favorite { get; set; }
        public void SetID() { 
            string s= getResponse("http://api.steamcardsheet.com/data/Games/?name="+name);
            if (s.IndexOf(": ") != -1)
                    s = s.Remove(0, s.IndexOf(": ") + 2);
                if (s.IndexOf(",") != -1)
                    s = s.Remove(s.IndexOf(","));
            id=s;
        }
        public string getResponse(string uri)
        {
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            int count = 0;
            do
            {
                count = resStream.Read(buf, 0, buf.Length);
                if (count != 0)
                {
                    sb.Append(Encoding.Default.GetString(buf, 0, count));
                }
            }
            while (count > 0);
            return sb.ToString();
        }

        public string profitString
        {
            get
            {
                return profit.ToString("C");
            }
        }

        public string referenceWithPack
        {
            get
            {
                int kol = 0;
                double sum = 0;
                foreach (GameCard gc in cards.FindAll(x => !x.foil))
                {
                    kol++;
                    sum += Convert.ToDouble(gc.LowestPrice);

                }
                return (gamePack.LowestPrice + " / " + (sum * 3 / kol).ToString("C"));
            }
        }
        public void GetCards(string response) {
            var searchJS = JsonConvert.DeserializeObject<SearchBody>(response);

            if (searchJS.Success)
            {
                this.cards.Clear();

                MatchCollection matches = Regex.Matches(searchJS.HtmlRes, "(?<=market_listing_row_link\" href)(.*?)(?<=</a>)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
                if (matches.Count != 0)
                {
                    foreach (Match match in matches)
                    {
                        string currmatch = match.Groups[1].Value;
                        GameCard gc = new GameCard(currmatch,this);
                        gc.foil = gc.name.Contains("Foil");
                        this.cards.Add(gc);
                    }

                }
            }
            this.gamePack = new GamePack(this);
        }
    }
    [Serializable]
    public class Emoticon:GameEntity
    {        
        public string type { get; set; }     
    }
    [Serializable]
    public class Background:GameEntity
    {        
        public string type { get; set; }        
    }
}
