using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace ACM专用IDE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*GCHandle dCHandle = GCHandle.Alloc(Properties.Resources.YaHei_Consolas_1_11b, GCHandleType.Pinned);
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddMemoryFont(dCHandle.AddrOfPinnedObject(), Properties.Resources.YaHei_Consolas_1_11b.Length);
            richTextBox1.Font = new Font(pfc.Families[0], 20,FontStyle.Regular, GraphicsUnit.Pixel);*/
            richTextBox1.BackColor = Color.FromArgb(255, 250, 232);
            panel1.BackColor = richTextBox1.BackColor;
            panel2.BackColor = richTextBox1.BackColor;
            pictureBox1.BackColor = Color.FromArgb(241, 229, 195);
            menuStrip1.BackColor = Color.FromArgb(255, 254, 250);

            richTextBox1.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
            richTextBox2.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
            richTextBox1.Width = panel1.Width - 40;
            richTextBox1.MouseWheel += RichTextBox1_MouseWheel;
            setColorText(false);
            suitHeightByContent();
        }

        private void RichTextBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                suiTopByCursor(0, false, true);
            }
            else
            {
                suiTopByCursor(0, true, false);
            }
            suitHeightByContent();
        }

        private void 运行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isWriteSucceed = false;
            try
            {
                FileStream file = File.Open("temp.cpp", FileMode.Create);
                byte[] b = Encoding.Default.GetBytes(richTextBox1.Text);
                file.Write(b, 0, b.Length);
                file.Close();
                Console.WriteLine("写入文件成功");
                isWriteSucceed = true;
            }
            catch (Exception)
            {
                MessageBox.Show("发生异常错误!");
            }
            if (isWriteSucceed)
            {
                Process p = new Process();
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = "MinGW/bin/g++.exe";
                p.StartInfo.Arguments = "temp.cpp -o temp.exe";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                string path = System.Environment.CurrentDirectory + "\\MinGW\\bin\\";
                //Console.WriteLine(path);
                p.StartInfo.Environment.Add("path", path);
                p.Start();
                //string compileInfo = p.StandardOutput.ReadToEnd();
                string errorInfo = p.StandardError.ReadToEnd();
                Console.WriteLine("以下是编译信息:");
                if (errorInfo.Length == 0)
                {
                    Process program = new Process();
                    program.StartInfo.CreateNoWindow = false;
                    program.StartInfo.UseShellExecute = false;
                    program.StartInfo.Environment.Add("path", path);
                    program.StartInfo.FileName = "temp.exe";
                    program.Start();
                }
                else
                {
                    MessageBox.Show(errorInfo,"编译发生错误");
                }
            }
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {


            if (e.KeyCode == Keys.Tab)
            {
                int pos = richTextBox1.SelectionStart;
                richTextBox1.Text = richTextBox1.Text.Insert(pos, "    ");
                e.SuppressKeyPress = true;
                richTextBox1.SelectionStart = pos + 4;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                int pos = richTextBox1.SelectionStart;
                string text = richTextBox1.Text;
                string emptyLength = "";
                int curLineStart = richTextBox1.GetFirstCharIndexOfCurrentLine();
                for (int i = curLineStart; true; i++)
                {
                    if (i < text.Length && text[i] == ' ')
                    {
                        emptyLength += " ";
                    }
                    else
                    {
                        break;
                    }
                }
                int posOffset = emptyLength.Length + 1;
                if (pos > 0 && text[pos - 1] == '{')
                {
                    emptyLength += "    \r\n" + emptyLength;
                    posOffset += 4;
                }
                richTextBox1.Text = text.Insert(pos, "\r\n" + emptyLength);
                e.SuppressKeyPress = true;
                richTextBox1.SelectionStart = pos + posOffset;
                suiTopByCursor(richTextBox1.SelectionStart);
            }
            else if (e.KeyCode == Keys.Down)
            {
                suiTopByCursor(richTextBox1.SelectionStart + richTextBox1.SelectionLength); // down
            }
            else
            {
                suiTopByCursor(richTextBox1.SelectionStart);
            }
            
            suitHeightByContent();
        }
        
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void 文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
            p.StartInfo.UseShellExecute = false;
            p.Start();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Form2().ShowDialog();
        }

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            //setColorText();
        }
        private void setColorText(bool isWriteFile = true)
        {
            List<patternClip> patternClips = new List<patternClip>();
            patternClips.Add(new patternClip("(//.*)|(/\\*[\\s\\S]*?\\*/)", Color.Green));
            patternClips.Add(new patternClip("(\".*\"|\'.*\')", Color.Red));
            patternClips.Add(new patternClip("(true|false)", Color.Red));
            patternClips.Add(new patternClip("[0-9]+", Color.FromArgb(42, 161, 152)));
            patternClips.Add(new patternClip("#.*", Color.Gray));
            patternClips.Add(new patternClip("[^a-zA-z0-9](goto|this|private|protected|public|static|return|for|auto|bool|break|case|catch|char|class|const|continue|default|while|double|delete|if|else|enum|struct|int|float|string|using|namespace|long long)", Color.FromArgb(176, 153, 0), 1));
            //Console.WriteLine(patternClips.Count.ToString() + "pattern");
            setColorByPattern(patternClips, isWriteFile);
            patternClips.Clear();
        }
        private void setColorByPattern(List<patternClip> patternClips, bool isWriteFile)
        {
            int pos = richTextBox1.SelectionStart;
            Point scrollPos = richTextBox1.AutoScrollOffset;
            string s = richTextBox1.Text;
            List<clipString> clipStrings = new List<clipString>();


            foreach (patternClip patt in patternClips)
            {
                MatchCollection matchCollection = Regex.Matches(s, patt.pattern, RegexOptions.Multiline);
                foreach (Match match in matchCollection)
                {

                    GroupCollection groupCollection = match.Groups;
                    clipString clip = null;
                    int start = groupCollection[patt.posInGroups].Index;
                    int end = groupCollection[patt.posInGroups].Index + groupCollection[patt.posInGroups].Length;
                    clip = (new clipString(groupCollection[patt.posInGroups].Value, start, end, patt.color));
                    bool handled = false;
                    for (int i = 0; i < clipStrings.Count; i++)
                    {
                        clipString clipTemp = clipStrings[i];
                        bool cross1 = clip.startPos >= clipTemp.startPos && clip.startPos < clipTemp.endPos && clip.endPos > clipTemp.endPos;
                        bool cross2 = clip.startPos < clipTemp.startPos && clip.endPos >  clipTemp.startPos && clip.endPos <= clipTemp.endPos;
                        bool beIn = clip.startPos > clipTemp.startPos && clip.endPos < clipTemp.endPos;
                        bool beout = clip.startPos < clipTemp.startPos && clip.endPos > clipTemp.endPos;
                        if (cross1 || cross2 || beIn)
                        {
                            //self is small
                            handled = true;
                            break;
                        } else if (beout)
                        {
                            handled = true;
                            clipStrings[i] = clip;
                        }
                    }
                    if (!handled)
                    {
                        clipStrings.Add(clip);
                    }
                    /*richTextBox1.SelectionStart = match.Index;
                    richTextBox1.SelectionLength = match.Length;
                    richTextBox1.SelectionColor = color;*/
                }
            }
            clipStrings.Sort((a, b) =>a.startPos.CompareTo(b.startPos));
            //recover

            int curPos = 0;
            clipStrings.Add(new clipString("", s.Length, s.Length, Color.Black));
            int len = clipStrings.Count;
            for (int i = 0; i < len; i++)
            {
                int start = curPos;
                int end = clipStrings[i].startPos;
                curPos = clipStrings[i].endPos;
                if (start <= end)
                {
                    string content = s.Substring(start, end - start);
                    //Console.WriteLine(start.ToString() + " " + end.ToString());
                    //Console.WriteLine(content);
                    clipStrings.Add(new clipString(content, start, end, Color.Black));
                }
            }

            for (int i = 0; i < clipStrings.Count; i++)
            {
                //Console.WriteLine(clipStrings[i].content);
            }

            RichTextBox writeDestination = isWriteFile ? richTextBox2 : richTextBox1;

            writeDestination.Text = "";
            writeDestination.SelectionStart = 0;
            clipStrings.Sort((a, b) => a.startPos.CompareTo(b.startPos));
            foreach (clipString clip in clipStrings)
            {

                writeDestination.SelectionColor = clip.color;
                writeDestination.AppendText(clip.content);
            }

            int id = Process.GetCurrentProcess().Id;
            string filename = "format" + id.ToString() + ".rtf";

            try
            {
                richTextBox2.SaveFile(filename);
                if (File.Exists(filename) && isWriteFile)
                {
                    richTextBox1.LoadFile(filename);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("异常");
                Console.WriteLine(e.Message);
            }
            
            richTextBox1.SelectionStart = pos;
            richTextBox1.AutoScrollOffset = scrollPos;
            richTextBox1.SelectionLength = 0;
            clipStrings.Clear();
            
        }

        private void richTextBox1_TextChanged_1(object sender, EventArgs e)
        {
            setColorText();
        }

        private void richTextBox1_VScroll(object sender, EventArgs e)
        {
            //richTextBox1.Height += 23;
            //richTextBox1.AutoScrollOffset = new Point(0, 0);
        }

        private void suiTopByCursor(int cursorPos, bool forceDown = false, bool forceUp = false)
        {
            Point poss = new Point(0, richTextBox1.GetLineFromCharIndex(cursorPos) * 23);
            poss.Y += richTextBox1.Top - 20;
            if (poss.Y > panel1.Height - 40 || forceDown)
            {
                int preTop = richTextBox1.Top - 23;
                int bound = -(richTextBox1.Height - panel1.Height + 40 - 20);
                if (preTop < bound) preTop = bound;
                richTextBox1.Top = preTop;
            }
            else if (poss.Y < 23 || forceUp)
            {
                int preTop = richTextBox1.Top + 23;
                if (preTop > 20)
                {
                    preTop = 20;
                }
                richTextBox1.Top = preTop; ;
            }
        }

        private void suitHeightByContent()
        {

            //Console.WriteLine("line:" + richTextBox1.Lines.Length.ToString());



            int characterAmountOfALine = richTextBox1.Width / 11;
            int extraline = 0;
            int lines = richTextBox2.Lines.Length;

            if (characterAmountOfALine == 0)
            {
                return;
            }

            foreach (string line in richTextBox2.Lines)
            {
                int temp = 1;
                try
                {
                    temp = line.Length / characterAmountOfALine;
                }
                catch (Exception)
                {
                    Console.WriteLine("zero");
                }
                if (temp != 0 && line.Length % characterAmountOfALine == 0)
                {
                    extraline += temp - 1;
                }
                else
                {
                    extraline += temp;
                }
            }

            //Console.WriteLine("g:" + extraline);
            //Console.WriteLine(lines);
            int height = 23 * (lines + extraline + 1) + panel1.Height - 40;
            //Console.WriteLine("he:" + height);
            richTextBox1.Height = height;
            //Console.WriteLine("h:" + richTextBox1.Height);
            

            //set scroll bar
            int ex = (height - panel1.Height + 40); //scrollHeight
            int co = ex / 23;
            double coefficient = (0.5 - 0.001 * co);
            int scrollBarHight =  (int)(coefficient * (double)panel1.Height);
            if (scrollBarHight < 5) scrollBarHight = 5;
            pictureBox1.Height = scrollBarHight;
            // scroll pos
            int scrollTop = - richTextBox1.Top + 20;
            double percent = 0;
            try
            {
                percent = scrollTop / (double)ex;
            }
            catch (Exception)
            {
                Console.WriteLine("ex is zero");
            }
            pictureBox1.Top = (int)((panel2.Height - pictureBox1.Height) * percent);
        }
        

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            richTextBox1.Width = panel1.Width - 40;
            suitHeightByContent();
        }


        private int yOffset = 0;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            yOffset = e.Y;
            timer1.Enabled = true;
        }

        
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            //int mouseY = e.Y - yOffset;
            //pictureBox1.Location = new Point(pictureBox1.Location.X, mouseY);
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {

            timer1.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int y = (this.PointToClient(MousePosition).Y - menuStrip1.Height - yOffset);
            int bound = panel2.Height - pictureBox1.Height;
            if (y < 0)
            {
                y = 0;
            }
            else if (y > bound)
            {
                y = bound;
            }
            pictureBox1.Location = new Point(pictureBox1.Location.X, y);
            double percent = (double)y / bound;
            int scrollHeight = richTextBox1.Height - panel1.Height + 40;
            richTextBox1.Top = - (int)(scrollHeight * percent) + 20;
        }

        private void richTextBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.BackColor = Color.FromArgb(209, 183, 110);
        }

        private void richTextBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.BackColor = Color.FromArgb(241, 229, 195);
        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
        }
    }

    class patternClip
    {
        public string pattern;
        public Color color;
        public int posInGroups;
        public patternClip(string pattern, Color color, int posInGroups = 0)
        {
            this.pattern = pattern;
            this.color = color;
            this.posInGroups = posInGroups;
        }
    }

    class clipString
    {
        public string content;
        public int startPos;
        public int endPos;
        public Color color;
        public clipString(string content, int startPos,int endPos, Color color)
        {
            this.content = content;
            this.startPos = startPos;
            this.endPos = endPos;
            this.color = color;
        }
    }
}
