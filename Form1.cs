using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocket4Net;
using System.IO;


namespace kimonoちゃんコメビュ
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.IsUpgrade == false)
            {
                // Upgradeを実行する
                Properties.Settings.Default.Upgrade();

                // 「Upgradeを実行した」という情報を設定する
                Properties.Settings.Default.IsUpgrade = true;

                // 現行バージョンの設定を保存する
                Properties.Settings.Default.Save();
            }


            this.Width = Properties.Settings.Default.windowwidth;
            this.Height = Properties.Settings.Default.windowhe;


            this.DoubleBuffered = true;

            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnCount = 2;

            dataGridView1.Columns[1].Visible = false;

            dataGridView1.Columns[0].Width = Properties.Settings.Default.width;
            dataGridView1.Columns[0].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.Font = Properties.Settings.Default.font;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            
            System.Type dgvtype = typeof(DataGridView);

            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            // プロパティ設定の取得
            System.Reflection.PropertyInfo dgvPropertyInfo =
            dgvtype.GetProperty(
            "DoubleBuffered", System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);


            // 対象のDataGridViewにtrueをセットする
            dgvPropertyInfo.SetValue(dataGridView1, true, null);

            dataGridView1.ContextMenuStrip = this.contextMenuStrip1;

        }
        private string userre()
        {
            /*if(System.Text.RegularExpressions.Regex.IsMatch(user_id.Text , @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?"))
            {

            }*/
            if (user_id.Text.Contains("twitcasting.tv"))
            {
                if (user_id.Text.Contains("https://"))
                {
                    string rep = user_id.Text.Replace("https://", "");
                    string rep2 = rep.Replace("twitcasting.tv/", "");
                    //Comment(rep2);
                    //Console.WriteLine(rep2);
                    return rep2;
                }
                else
                {
                    string rep = user_id.Text.Replace("twitcasting.tv/", "");
                    //Console.WriteLine(rep);
                    return rep;
                }
            }
            else
            {
                string streaming = user_id.Text;
                return streaming;

            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "接続")
            {
                var ttp = new HttpClient();
                var metajson = await ttp.GetAsync("https://frontendapi.twitcasting.tv/users/" + userre() + "/latest-movie");
                var jsp = await metajson.Content.ReadAsStringAsync();
                var stjs = DynamicJson.Parse(jsp);
                if (stjs.IsDefined("error"))
                {
                    Console.WriteLine("errrrrr");
                    MessageBox.Show("エラー：" + stjs.error.message + " (" + stjs.error.code + ")", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (stjs.movie.is_on_live)
                    {
                        button1.Enabled = true;
                        user_id.Enabled = false;
                        button1.Text = "切断";
                        radioButton1.Enabled = false;
                        radioButton2.Enabled = false;
                        radioButton3.Enabled = false;
                        radioButton4.Enabled = false;
                        Comment();
                    }
                    else
                    {
                        Console.WriteLine("falseeeeeeeeee");
                        MessageBox.Show("現在配信していません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                button1.Text = "接続";
                radioButton1.Enabled = true;
                radioButton2.Enabled = true;
                radioButton3.Enabled = true;
                radioButton4.Enabled = true;
                user_id.Enabled = true;
            }
        }
        private async Task<string> token_s()
        {
            var bo = new Dictionary<string, string>()
            {
                {"movie_id",await movieid() }
            };
            var bo2 = new FormUrlEncodedContent(bo);
            var hc = new HttpClient();
            var tokenjs = await hc.PostAsync("https://twitcasting.tv/happytoken.php", bo2);
            var token = DynamicJson.Parse(await tokenjs.Content.ReadAsStringAsync()).token;
            return token;
        }
        private async Task<string> movieid()
        {
            var cl = new HttpClient();
            var movjs = await cl.GetAsync("https://frontendapi.twitcasting.tv/users/" + userre() + "/latest-movie");
            var movid = DynamicJson.Parse(await movjs.Content.ReadAsStringAsync());
            var movie_id = movid.movie.id;
            string movv = movie_id.ToString();
            return movv;
        }
        private async void Comment()
        {
            var nnn = new HttpClient();
            var movie_id = await movieid();
            var np = new Dictionary<string, string>
            {
                {"movie_id", movie_id }
            };
            var was = await nnn.PostAsync("https://twitcasting.tv/eventpubsuburl.php", new FormUrlEncodedContent(np));
            var wsurl = DynamicJson.Parse(await was.Content.ReadAsStringAsync()).url;

            Task t = Task.Run(() =>
            {
                GetWebtest(wsurl);
            });

            Task wt = Task.Run(() =>
            {
                stream_meta();
            });
        }
        private void GetWebtest(string wsurl)
        {
            var ws = new WebSocket(wsurl);
            ws.MessageReceived += Ws_MessageReceived;
            ws.DataReceived += Ws_DataReceived;
            
            ws.Opened += Ws_Opened;
            ws.Closed += Ws_Closed;
            ws.Open();

            while (true)
            {
                var str = Console.ReadLine();
                if (str == "exit")
                {
                    break;
                }
                if (ws.State == WebSocketState.Open)
                {
                    //ws.Send(str);
                }
                else
                {
                    //Console.WriteLine("{0}:wait...", DateTime.Now.ToString());
                }
                if (button1.Text == "接続")
                {
                    break;
                }
            }
            ws.Close();
        }

        private static void Ws_Closed(object sender, EventArgs e)
        {

        }

        private static void Ws_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Opended");
        }

        private static void Ws_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"{e.Data.Length} bytes recived.");
        }

        private void Ws_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var jsdt = e.Message;
            if (!(jsdt == "[]"))
            {

                Task tsk = Task.Run(() => {
                    ad(jsdt);
                });
            }
        }

        private void ad(string jsdt)
        {
            var js = DynamicJson.Parse(jsdt);

            Task tsk = Task.Run(() =>
            {
                for(int i = 0; js.IsDefined(i); i++)
                {
                    add(js[i].message, js[i].id);
                }
            });
        }

        private void add(string nakami,double id)
        {
            Task aaaaaaaaa = Task.Run(() =>
            {
                var us = Regex.Unescape(nakami);
                this.Invoke((MethodInvoker)(() => dataGridView1.Rows.Add(us,id.ToString())));
                if (checkBox1.Checked == false)
                {
                    this.Invoke((MethodInvoker)(() => dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1));
                    this.Invoke((MethodInvoker)(() => checkBox1.Checked = false));
                    /*int hi = dataGridView1.Height;
                    int ch = dataGridView1.ColumnHeadersHeight;
                    int h = dataGridView1.RowTemplate.Height;
                    int n = (hi - ch) / h;
                    hi = ch + h * n ;
                    this.Invoke((MethodInvoker)(() => dataGridView1.RowTemplate.Height = hi));*/
                    
                }
            });
            
        }
        private async void stream_meta()
        {
            while (true)
            {
                if (button1.Text == "接続")
                {
                    //this.Invoke((MethodInvoker)(() => title.Text = ""));
                    //this.Invoke((MethodInvoker)(() => telop.Text = ""));
                    this.Invoke((MethodInvoker)(() => viewer.Text = "閲覧数：- / -"));
                    this.Invoke((MethodInvoker)(() => max.Text = "0"));
                    //this.Invoke((MethodInvoker)(() => cat.Text = ""));
                    //this.Invoke((MethodInvoker)(() => status.ImageLocation = ""));
                    break;
                }
                else
                {

                    var ttp = new HttpClient();
                    var metajson = await ttp.GetAsync("https://frontendapi.twitcasting.tv/movies/" + await movieid() + "/status/viewer?token=" + await token_s());
                    var jsp = DynamicJson.Parse(await metajson.Content.ReadAsStringAsync());

                    this.Invoke((MethodInvoker)(() => viewer.Text = "閲覧数：" + jsp.movie.viewers.current + " / " + jsp.movie.viewers.total));
                    double now = jsp.movie.viewers.current;
                    double te = double.Parse(max.Text);
                    if(te < now)
                    {
                        this.Invoke((MethodInvoker)(() => max.Text = now.ToString()));
                    }

                    if (jsp.movie.IsDefined("title"))
                    {
                        //this.Invoke((MethodInvoker)(() => title.Text = jsp.movie.title));
                    }
                    else
                    {
                        double mom = jsp.movie.id;
                        //this.Invoke((MethodInvoker)(() => title.Text = "ライブ #" + mom.ToString()));
                    }

                    if (jsp.movie.IsDefined("category"))
                    {
                        //this.Invoke((MethodInvoker)(() => cat.Text = jsp.movie.category.name));
                    }
                    else
                    {
                        //this.Invoke((MethodInvoker)(() => cat.Text = "(カテゴリーなし)"));
                    }


                    if (jsp.movie.IsDefined("telop"))
                    {
                        //this.Invoke((MethodInvoker)(() => telop.Text = jsp.movie.telop));
                    }
                    else
                    {
                        //this.Invoke((MethodInvoker)(() => telop.Text = "(テロップなし)"));
                    }

                    await Task.Delay(5000);
                }
            }

        }
        private void 終了XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void 設定IToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingForm2 n = new SettingForm2();
            n.TopMost = true;
            n.ShowDialog();
            n.Dispose();
        }
        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            //checkBox1.Checked = true;
        }

        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            ///checkBox1.Checked = true;
        }

        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            checkBox1.Checked = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            user_id.Enabled = false;
            user_id.Text = "c:gomikuzunokiwami";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            user_id.Enabled = false;
            user_id.Text = "c:kzhodowameku";
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            user_id.Enabled = false;
            user_id.Text = "c:yurusunakimono";
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            user_id.Enabled = true;
            user_id.Text = null;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.width = dataGridView1.Columns[0].Width;
            Properties.Settings.Default.windowwidth = this.Width;
            Properties.Settings.Default.windowhe = this.Height;
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
            checkBox1.Checked = false;
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                /*if(e.ColumnIndex >= 0 && e.RowIndex >= 0)
                {*/

                dataGridView1.ClearSelection();
                dataGridView1.Rows[e.RowIndex].Selected = true;
                    //Clipboard.SetText(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());

                /*}*/
            }
        }

        private void コピーCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                /*if(e.ColumnIndex >= 0 && e.RowIndex >= 0)
                {*/

                dataGridView1.ClearSelection();
                dataGridView1.Rows[e.RowIndex].Selected = true;
                //Clipboard.SetText(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());

                /*}*/
            }
        }

        private async void コメントリンクを開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://twitcasting.tv/" + userre() + "/comment/" + await movieid() + "-" + dataGridView1.SelectedRows[0].Cells[1].Value.ToString()) ;
        }
    }
}
