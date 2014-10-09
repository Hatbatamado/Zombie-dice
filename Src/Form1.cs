using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace game
{
    enum Micsoda { HOST,CLIENT};
    public partial class Form1 : Form
    {
        #region Kezdo form
        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(100, 100);
            Button multi = new Button();
            Controls.Add(multi);
            multi.Text = "Többjátékos";
            multi.Location = new Point(100, 50);
            multi.Click += multi_Click;
        }
        
        void multi_Click(object sender, EventArgs e)
        {
            Controls.Clear();
            Multi();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sock != null)
                sock.Close();
            else if (serv != null)
                serv.Close();
            if (thread_running == true)
                thread_running = false;
            Application.Exit();
        }
        #endregion
        #region 1) Multi valaszto
        static TextBox nickk;
        private void Multi()
        {
            Label cim = new Label();
            Controls.Add(cim);
            cim.Text = "Többjátékos mód";
            cim.Size = new System.Drawing.Size(150, 30);
            cim.Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
            cim.Location = new Point(65, 25);

            Label nick = new Label();
            Controls.Add(nick);
            nick.Text = "Nick: ";
            nick.Location = new Point(60, 75);
            nick.Font = new System.Drawing.Font("Arial", 10);
            nick.Size = new Size(38, 22);

            nickk = new TextBox();
            Controls.Add(nickk);
            nickk.Location = new Point(100, 74);
            nickk.Size = new Size(80, 22);
            nickk.Text = Betolt(1);

            Button host = new Button();
            Controls.Add(host);
            host.Text = "Host";
            host.Location = new Point(40, 110);
            host.Click += host_Click;

            Button connect = new Button();
            Controls.Add(connect);
            connect.Text = "Csatlakozás";
            connect.Location = new Point(140, 110);
            connect.Click += connect_Click;
        }

        public string Betolt(int melyik)
        {
            if (File.Exists("game.conf"))
            {
                string vissza = "";
                StreamReader sr = new StreamReader("game.conf");
                for (int i = 0; i < melyik; i++)
                {
                    vissza = sr.ReadLine();
                }
                sr.Close();
                return vissza;
            }
            return "";
        }

        public void Kiir(int melyik, string mit)
        {
            string[] seged;
            StreamWriter sw;
            if (File.Exists("game.conf"))
            {
                int db = 0;
                StreamReader sr = new StreamReader("game.conf");
                while (!sr.EndOfStream)
                {
                    sr.ReadLine();
                    db++;
                }
                sr.Close();
                seged = new string[db+1];

                sr = new StreamReader("game.conf");
                for (int i = 0; i < db; i++)
                    seged[i] = sr.ReadLine();
                sr.Close();
            }
            else
            {
                if (melyik > 1)
                    seged = new string[2];
                else
                    seged = new string[1];
            }
            if (seged.Length > 1)
                seged[melyik - 1] = mit;
            else
                seged[0] = mit;
            sw = new StreamWriter("game.conf", false);
            for (int i = 0; i < seged.Length && seged[i] != ""; i++)
                sw.WriteLine(seged[i]);
            sw.Close();
        }
        #endregion
        #region 2) Host/Client form
        static string host_neve;
        void host_Click(object sender, EventArgs e)
        {
            host_neve = nickk.Text;
            Kiir(1, nickk.Text);
            Controls.Clear();
            Host();
        }

        void connect_Click(object sender, EventArgs e)
        {
            Kiir(1, nickk.Text);
            Controls.Clear();
            Client();
        }

        TextBox ipp;
        IPAddress hostip;
        Button go;
        private void Host()
        {
            Label cim = new Label();
            Controls.Add(cim);
            cim.Text = "Többjátékos mód";
            cim.Size = new System.Drawing.Size(150, 30);
            cim.Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
            cim.Location = new Point(65, 25);

            Label ip = new Label();
            Controls.Add(ip);
            ip.Text = "IP címed:";
            ip.Font = new System.Drawing.Font("Arial", 10);
            ip.Size = new Size(72, 22);
            ip.Location = new Point(50, 75);

            ipp = new TextBox();
            Controls.Add(ipp);
            ipp.Location = new Point(124, 74);
            ipp.Size = new Size(100, 22);
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                    hostip = addr;
            }
            //ipp.Text = hostip.ToString();
            ipp.Text = "127.0.0.1";
            ipp.KeyDown += ipp_KeyDown;

            go = new Button();
            Controls.Add(go);
            go.Text = "Host";
            go.Location = new Point(100, 125);
            go.Click += go_Click;
        }

        void ipp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                go.PerformClick();
            }
        }

        private void Client()
        {
            Label cim = new Label();
            Controls.Add(cim);
            cim.Text = "Többjátékos mód";
            cim.Size = new System.Drawing.Size(150, 30);
            cim.Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
            cim.Location = new Point(65, 25);

            Label ip = new Label();
            Controls.Add(ip);
            ip.Text = "Host IP-címe:";
            ip.Font = new System.Drawing.Font("Arial", 10);
            ip.Size = new Size(100, 22);
            ip.Location = new Point(30, 75);

            ipp = new TextBox();
            Controls.Add(ipp);
            ipp.Location = new Point(130, 74);
            ipp.Size = new Size(100, 22);
            ipp.Text = Betolt(2);

            Button go = new Button();
            Controls.Add(go);
            go.Text = "Csatlakozás";
            go.Location = new Point(100, 125);
            go.Click += go_Click_c;
        }
        
        void go_Click(object sender, EventArgs e)
        {
            Kiir(2, ipp.Text);
            hostip = IPAddress.Parse(ipp.Text);
            Controls.Clear();
            Run(Micsoda.HOST);
        }

        void go_Click_c(object sender, EventArgs e)
        {
            Kiir(2, ipp.Text);
            Controls.Clear();
            Run(Micsoda.CLIENT);
        }
        #endregion
        #region 3) Host/Client fut
        static Socket sock;
        static bool thread_running = true;
        Socket serv;
        static string ID = "";
        private void Run(Micsoda a)
        {
            Thread thr = new Thread(new ThreadStart(Read_s));
            IPAddress ip;
            if (a == Micsoda.HOST)
                ip = hostip;
            else
                ip = IPAddress.Parse(Betolt(2));
            IPEndPoint ep = new IPEndPoint(ip, 12345);
            Random R = new Random();
            for (int i = 0; i < 6; i++)
                if (R.Next(0, 10) >= 5)
                    ID += (char)R.Next(65, 91);
                else
                    ID += R.Next(0, 10);
            switch (a)
            {
                case Micsoda.HOST:
                    serv = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);
                    serv.Bind(ep);
                    serv.Listen(5);
                    Label var = new Label();
                    Controls.Add(var);
                    var.Text = "Játékosra várunk";
                    var.Location = new Point(5, 25);
                    var.Font = new System.Drawing.Font("Arial", 10);
                    var.Size = new Size(130, 22);
                    this.Refresh();

                    sock = serv.Accept();
                    thr.Start();
                    break;
                case Micsoda.CLIENT:
                    sock = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                    sock.Connect(ep);
                    sock.Send(Encoding.Unicode.GetBytes(ID + '^' + Betolt(1) + "§"));
                    thr.Start();
                    break;
                default:
                    break;
            }
        }
        List<Felhasznalok> felhasznalok = new List<Felhasznalok>();
        #endregion
        #region 4) Beerkezo adatok olvasas
        bool udv = true;
        private void Read_s()
        {
            byte[] buffer = new byte[255];
            int db = 0;
            string bejovo = "";
            Inditas_elotti_chat();
            string nev;
            while (thread_running)
            {
                try
                {
                    db = sock.Receive(buffer);
                }
                catch (SocketException)
                {
                    if (sock != null)
                        sock.Close();
                    else if (serv != null)
                        serv.Close();
                    if (thread_running == true)
                        thread_running = false;
                    Application.Exit();                    
                }
                if (thread_running)
                {
                    bejovo = Encoding.Unicode.GetString(buffer, 0, db);
                    string[] bejovok;
                    bejovok = bejovo.Split('§');
                    string[] seged = bejovok[0].Split('ˇ');
                    nev = Kicsoda(bejovok[0]);
                    if (udv)
                    {
                        sock.Send(Encoding.Unicode.GetBytes(ID + '^' + Betolt(1) + "§"));
                        udv = false;
                    }
                    if (bejovok[1] != "" && nev != "nincs")
                    {
                        if (rt.InvokeRequired)
                        {
                            this.Invoke(new MethodInvoker(delegate
                                {
                                    rt.Text += nev + " (" + DateTime.Now.ToShortTimeString() + "): " + bejovok[1] + "\n";
                                    rt.SelectionStart = rt.Text.Length;
                                    rt.ScrollToCaret();
                                }));
                        }
                    }
                    else if (seged.Length > 2)
                        Read_it(bejovok[0]);
                }
            }
        }
        
        private string Kicsoda(string bejovo)
        {
            string[] seged = bejovo.Split('^');
            if (felhasznalok.Count == 0)
            {
                felhasznalok.Add(new Felhasznalok(seged[0], seged[1]));
                return seged[1];
            }
            else
            {
                for (int i = 0; i < felhasznalok.Count; i++)
                    if (felhasznalok[i].ID == seged[0])
                        return felhasznalok[i].Nev;
            }
            return "nincs";
        }
        #endregion
        #region 6) Chat
        static RichTextBox rt;
        TextBox chat;
        private void Inditas_elotti_chat()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                    {
                        this.WindowState = FormWindowState.Maximized;
                        Controls.Clear();
                        rt = new RichTextBox();
                        Controls.Add(rt);
                        int hossz = this.ClientRectangle.Width;
                        int magas = this.ClientRectangle.Height;
                        rt.Size = new Size(hossz / 4, magas / 4);
                        rt.Location = new Point(hossz - (hossz / 4) - 10, 10);
                        rt.ReadOnly = true;

                        chat = new TextBox();
                        Controls.Add(chat);
                        chat.Size = new Size(hossz / 4, magas / 4);
                        chat.Location = new Point(hossz - (hossz / 4) - 10, magas / 4 + 10);
                        chat.KeyDown += chat_KeyDown;

                        Game();

                        Refresh();
                    }));                
            }          
        }

        void chat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                sock.Send(Encoding.Unicode.GetBytes(ID + "§" + chat.Text));
                rt.Text+= nickk.Text + " (" + DateTime.Now.ToShortTimeString() + "): "+chat.Text+"\n";
                rt.SelectionStart = rt.Text.Length;
                rt.ScrollToCaret();
                chat.Text = "";
            }
        }
        #endregion
        #region Game
        private void Game()
        {
            Button roll = new Button();
            Controls.Add(roll);
            roll.Location = new Point(100, 100);
            roll.Text = "Roll";
            roll.Size = new Size(40, 22);
            roll.Click += roll_Click;

            Button passz = new Button();
            Controls.Add(passz);
            passz.Location = new Point(150, 100);
            passz.Text = "Passz";
            passz.Size = new Size(50, 22);
            passz.Click += passz_Click;
        }

        void passz_Click(object sender, EventArgs e)
        {
            
        }

        Random R = new Random();
        int green = 6;
        int yellow = 4;
        int red = 3;
        List<Control> lista = new List<Control>(); //dobas
        List<string> lista_string = new List<string>(); //dobas stringként
        List<Control> lista2 = new List<Control>(); //agyak
        List<int> lista3 = new List<int>(); //1-zöld 2-sárga 3-piros
        int kocka = 0;
        int agy_db = 0;
        int loves_db = 0;
        int szin;
        void roll_Click(object sender, EventArgs e)
        {
            lista_string = new List<string>();
            int szum;
            megint_piros = 0;
            megint_sarga = 0;
            megint_zold = 0;
            for (int i = 0; i < lista.Count; i++)
                Controls.Remove(lista[i]);
            lista = new List<Control>();
            Bitmap kep = null;
            Cipo();
            while (kocka < 330)
            {
                if (green == 0 && yellow == 0 && red == 0)
                    Elfogyott();
                PictureBox hol = new PictureBox();
                Controls.Add(hol);
                lista.Add(hol);
                szum = green + yellow + red;
                szin = R.Next(1, szum + 1);
                if (szin <= green)
                {
                    #region zold
                    szin = R.Next(1, 7);
                    if (szin <= 3)
                    {
                        kep = Kepek("G/A");
                        Agy_talalt(kep);
                        green--;
                    }
                    else if (szin > 3 && szin <= 5)
                    {
                        kep = Kepek("G/R");
                        Cipo("zold");
                    }
                    else if (szin == 6)
                    {
                        kep = Kepek("G/L");
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"Sound/shot.wav");
                        player.Play();
                        Thread.Sleep(100);
                        Loves_talalt(kep);
                        loves_db++;
                        green--;
                        lista3.Add(1);
                    }
                    #endregion
                }
                else if(szin > green && szin <= green+yellow)
                {
                    #region sarga
                    szin = R.Next(1, 7);
                    if (szin <= 2)
                    {
                        kep = Kepek("Y/A");
                        Agy_talalt(kep);
                        yellow--;
                    }
                    else if (szin > 2 && szin <= 4)
                    {
                        kep = Kepek("Y/R");
                        Cipo("sarga");
                    }
                    else if (szin > 4)
                    {
                        kep = Kepek("Y/L");
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"Sound/shot.wav");
                        player.Play();
                        Thread.Sleep(100);
                        Loves_talalt(kep);
                        loves_db++;
                        yellow--;
                        lista3.Add(2);
                    }
                    #endregion
                }
                else if (szin > green+yellow && szin <= green+yellow+red)
                {
                    #region piros
                    szin = R.Next(1, 7);
                    if (szin == 1)
                    {
                        kep = Kepek("R/A");
                        Agy_talalt(kep);
                        red--;
                    }
                    if (szin > 1 && szin <= 3)
                    {
                        kep = Kepek("R/R");
                        Cipo("piros");
                    }
                    if (szin > 3)
                    {
                        kep = Kepek("R/L");
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"Sound/shot.wav");
                        player.Play();
                        Thread.Sleep(100);
                        Loves_talalt(kep);
                        loves_db++;
                        red--;
                        lista3.Add(3);
                    }
                    #endregion
                }
                hol.Location = new Point(75 + kocka, 150);
                hol.Size = new System.Drawing.Size(100, 100);
                hol.SizeMode = PictureBoxSizeMode.StretchImage;
                hol.Image = new Bitmap(kep);
                
                kocka += 110;
            }
            kocka = 0;
            Loves_halal();
            Dobas_Kuldes();
        }

        private Bitmap Kepek(string dobas)
        {
            Bitmap kep = null;
            lista_string.Add(dobas);
            switch (dobas)
            {
                case "R/A": // red - agy
                    kep = new Bitmap("Pics/red_brain.bmp");
                    break;
                case "R/R": // red - run
                    kep = new Bitmap("Pics/red_run.bmp");
                    break;
                case "R/L": // red - loves
                    kep = new Bitmap("Pics/red_shot.bmp");
                    break;
                case "Y/L": // yellow - loves
                    kep = new Bitmap("Pics/yellow_shot.bmp");
                    break;
                case "Y/R": // yellow - run
                    kep = new Bitmap("Pics/yellow_run.bmp");
                    break;
                case "Y/A": // yellow - agy
                    kep = new Bitmap("Pics/yellow_brain.bmp");
                    break;
                case "G/A": // green - agy
                    kep = new Bitmap("Pics/green_brain.bmp");
                    break;
                case "G/R": // green - run
                    kep = new Bitmap("Pics/green_run.bmp");
                    break;
                case "G/L": // green - loves
                    kep = new Bitmap("Pics/green_shot.bmp");
                    break;
            }
            return kep;
        }

        private void Agy_talalt(Bitmap kep)
        {
            PictureBox agy = new PictureBox();
            Controls.Add(agy);
            lista2.Add(agy);
            agy.Location = new Point(10 + agy_db, ClientRectangle.Height - 60);
            agy.Size = new Size(50, 50);
            agy.SizeMode = PictureBoxSizeMode.StretchImage;
            agy.Image = (Image)kep;
            agy_db += 60;
        }

        private void Loves_halal()
        {
            if (loves_db >= 3)
            {
                for (int i = 0; i < lista2.Count; i++)
                    Controls.Remove(lista2[i]);
                lista2 = new List<Control>();
                loves_db = 0;
                agy_db = 0;
                green = 6;
                yellow = 4;
                red = 3;
                megint_piros = 0;
                megint_sarga = 0;
                megint_zold = 0;
            }
        }

        private void Loves_talalt(Bitmap kep)
        {
            int szokoz = 0;
            if (loves_db == 0)
                szokoz = 0;
            else
                szokoz = 10;
            PictureBox loves = new PictureBox();
            Controls.Add(loves);
            lista2.Add(loves);
            loves.Location = new Point(10 + loves_db * 50 + szokoz, ClientRectangle.Height - 120);
            loves.Size = new Size(50, 50);
            loves.SizeMode = PictureBoxSizeMode.StretchImage;
            loves.Image = (Image)kep;
        }

        int piros = 0, sarga = 0, zold = 0;
        int megint_piros = 0;
        int megint_sarga = 0;
        int megint_zold = 0;
        private void Cipo(string szin)
        {            
            if (szin == "piros")
                piros++;
            else if (szin == "sarga")
                sarga++;
            else if (szin == "zold")
                zold++;
        }
        private void Cipo()
        {
            Bitmap kep;
            if (piros != 0 || sarga != 0 || zold != 0)
            {
                if (piros != 0)
                {
                    #region piros
                    int i = 0;
                    while (i != piros)
                    {
                        if (green == 0 && yellow == 0 && red == 0)
                            Elfogyott();
                        PictureBox hol = new PictureBox();
                        Controls.Add(hol);
                        lista.Add(hol);
                        szin = R.Next(1, 7);
                        if (szin == 1)
                        {
                            kep = Kepek("R/A");
                            hol.Image = new Bitmap(kep);
                            Agy_talalt(kep);
                            red--;
                        }
                        if (szin > 1 && szin <= 3)
                        {
                            kep = Kepek("R/R");                          
                            hol.Image = new Bitmap(kep);
                            megint_piros++;
                        }
                        if (szin > 3)
                        {
                            kep = Kepek("R/L");
                            hol.Image = new Bitmap(kep);
                            System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"Sound/shot.wav");
                            player.Play();
                            Thread.Sleep(100);
                            Loves_talalt(kep);
                            loves_db++;
                            red--;
                            lista3.Add(3);
                        }
                        hol.Location = new Point(75 + kocka, 150);
                        hol.Size = new System.Drawing.Size(100, 100);
                        hol.SizeMode = PictureBoxSizeMode.StretchImage;

                        kocka += 110;
                        i++;
                    } 
                    #endregion
                }
                if (sarga != 0)
                {
                    #region sarga
                    int i = 0;
                    while (i != sarga) 
                    {
                        if (green == 0 && yellow == 0 && red == 0)
                            Elfogyott();
                        PictureBox hol = new PictureBox();
                        Controls.Add(hol);
                        lista.Add(hol);
                        szin = R.Next(1, 7);
                        if (szin <= 2)
                        {
                            kep = Kepek("Y/A");
                            hol.Image = new Bitmap(kep);
                            Agy_talalt(kep);
                            yellow--;
                        }
                        else if (szin > 2 && szin <= 4)
                        {
                            kep = Kepek("Y/R");
                            hol.Image = new Bitmap(kep);
                            megint_sarga++;
                        }
                        else if (szin > 4)
                        {
                            kep = Kepek("Y/L");
                            hol.Image = new Bitmap(kep);
                            System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"Sound/shot.wav");
                            player.Play();
                            Thread.Sleep(100);
                            Loves_talalt(kep);
                            loves_db++;
                            yellow--;
                            lista3.Add(2);
                        }
                        hol.Location = new Point(75 + kocka, 150);
                        hol.Size = new System.Drawing.Size(100, 100);
                        hol.SizeMode = PictureBoxSizeMode.StretchImage;

                        kocka += 110;
                        i++;
                    }
                    #endregion
                }
                if (zold != 0)
                {
                    #region zold
                    int i = 0;
                    while (i != zold) 
                    {
                        if (green == 0 && yellow == 0 && red == 0)
                            Elfogyott();
                        PictureBox hol = new PictureBox();
                        Controls.Add(hol);
                        lista.Add(hol);
                        szin = R.Next(1, 7);
                        if (szin <= 3)
                        {
                            kep = Kepek("G/A");
                            hol.Image = new Bitmap(kep);
                            Agy_talalt(kep);
                            green--;
                        }
                        else if (szin > 3 && szin <= 5)
                        {
                            kep = Kepek("G/R");
                            hol.Image = new Bitmap(kep);
                            megint_zold++;
                        }
                        else if (szin == 6)
                        {
                            kep = Kepek("G/L");
                            hol.Image = new Bitmap(kep);
                            System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"Sound/shot.wav");
                            player.Play();
                            Thread.Sleep(100);
                            Loves_talalt(kep);
                            loves_db++;
                            green--;
                            lista3.Add(1);
                        }
                        hol.Location = new Point(75 + kocka, 150);
                        hol.Size = new System.Drawing.Size(100, 100);
                        hol.SizeMode = PictureBoxSizeMode.StretchImage;

                        kocka += 110;
                        i++;
                    }
                    #endregion
                }
            }
            piros = megint_piros;
            sarga = megint_sarga;
            zold = megint_zold;
        }

        private void Elfogyott()
        {
            green = 6;
            yellow = 4;
            red = 3;
            for (int i = 0; i < lista3.Count; i++)
                if (lista3[i] == 1)
                    green--;
                else if (lista3[i] == 2)
                    yellow--;
                else if (lista3[i] == 3)
                    red--;
        }

        private void Dobas_Kuldes()
        {
            string kuldes = "";
            for (int i = 0; i < 3; i++)
                kuldes += 'ˇ' + lista_string[i];
            sock.Send(Encoding.Unicode.GetBytes(ID + kuldes + "§"));
        }

        private void Read_it(string fuckme)
        {
            string[] seged = fuckme.Split('ˇ');
            string nev = Kicsoda(seged[0]);
            Bitmap kep = null;
            int x = ClientRectangle.Width - ((ClientRectangle.Width / 4) / 2 -80);
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                    {
                        for (int i = 0; i < lista.Count; i++)
                            Controls.Remove(lista[i]);

                        Label fuck = new Label();
                        Controls.Add(fuck);
                        lista.Add(fuck);
                        fuck.Text = nev + " dobása:";
                        fuck.Size = new System.Drawing.Size(70, 20);
                        fuck.Location = new Point(x+20, ClientRectangle.Height / 2 + (ClientRectangle.Height / 2) / 4);
                        for (int i = 0; i < 3; i++)
                        {
                            PictureBox hol = new PictureBox();
                            Controls.Add(hol);
                            lista.Add(hol);
                            kep = Kepek(seged[i + 1]);
                            hol.Image = (Image)kep;
                            hol.Size = new Size(30, 30);
                            hol.Location = new Point(x, ClientRectangle.Height / 2 + (ClientRectangle.Height / 2) / 4 + 20);
                            x += 35;
                            hol.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }));
            }

        }
        #endregion
    }
}
