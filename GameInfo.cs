using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace SteamPrice
{
    public partial class GameInfo : Form
    {
        NewGame game;
        List<GameCard> gcList;
        int sortWay = 1;
        public GameInfo(NewGame g)
        {
            this.game = g;
            InitializeComponent();
            game.cards = game.cards == null ? new List<GameCard>() : game.cards;
            game.gamePack = game.gamePack == null ? new GamePack(game) : game.gamePack;

            int kol = 0;
            double sum = 0;
            foreach(GameCard gc in game.cards.FindAll(x=>!x.foil))
            {
                kol++;
                sum+=Convert.ToDouble(gc.LowestPrice);

            }
            label5.Text = game.gamePack.LowestPrice + "/" + (sum * 3 / kol).ToString();

            progressBar1.Minimum = 0;
            progressBar1.Maximum = g.cards.Count / 2;
            progressBar1.Value = 0;
            progressBar1.Step = 1;            
            gcList = new List<GameCard>(game.cards);
            gcList.Add(game.gamePack);
            checkBox2.Checked = game.favorite;



            gameCardBindingSource.DataSource = gcList.FindAll(x => x.foil == checkBox1.Checked | !x.foil);
            dataGridView1.DataSource = gameCardBindingSource;
            textBox1.Text = game.name;
            textBox2.Text = game.id;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            game.emoticonsGems = Convert.ToDouble(numericUpDown1.Value);
            game.name = textBox1.Text;
            game.id = textBox2.Text;
            game.favorite = checkBox2.Checked;
            this.Close();
        }


        public string removeWith(string source, string template)
        {
            return source.Remove(0, source.IndexOf(template) + template.Length);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://steamcommunity.com/market/search/render/?query="+game.name+" Trading Card&start=0&count=200");
            HttpWebResponse flixresponse = (HttpWebResponse)request.GetResponse();
            StreamReader response = new StreamReader(flixresponse.GetResponseStream(), Encoding.UTF8);
            game.GetCards(response.ReadToEnd());
            gcList = new List<GameCard>(game.cards);
            gcList.Add(game.gamePack);
            gameCardBindingSource.DataSource = gcList.FindAll(x => x.foil == checkBox1.Checked | !x.foil);
            dataGridView1.Refresh();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            gameCardBindingSource.DataSource = gcList.FindAll(x => x.foil == checkBox1.Checked | !x.foil);
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            sortWay *= -1;
            gcList.Sort(delegate(GameCard x, GameCard y)
            {
                if (Convert.ToDouble(x.LowestPrice) < Convert.ToDouble(y.LowestPrice))
                    return 1*sortWay;
                return -1*sortWay;
            });
            gameCardBindingSource.DataSource = gcList.FindAll(x => x.foil == checkBox1.Checked | !x.foil);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(dataGridView1.SelectedRows.Count!=0)
            {
                GameCard gc = dataGridView1.SelectedRows[0].DataBoundItem as GameCard;
                game.cards.Remove(gc);
                gcList = new List<GameCard>(game.cards);
                gcList.Add(game.gamePack);
                gameCardBindingSource.DataSource = gcList.FindAll(x => x.foil == checkBox1.Checked | !x.foil);
            }
        }

    }
}
