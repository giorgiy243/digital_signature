using System;
using System.IO;
using System.Numerics;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

namespace protect_inf_LR1
{
    public partial class Form1 : Form
    {
        char[] characters = new char[] { '#', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-' };

        public Form1()
        {
            InitializeComponent();
        }

        //зашифровать
        private void buttonEncrypt_Click(object sender, EventArgs e)
        {
            if ((textBox_p.Text.Length > 0) && (textBox_q.Text.Length > 0) && (sourceFilePathTextBox.Text.Length > 0) && (signFilePathTextBox.Text.Length > 0))
            {
                BigInteger p = BigInteger.Parse(textBox_p.Text/*"162259276829213363391578010288127"*/);;
                BigInteger q = BigInteger.Parse(textBox_q.Text/*"618970019642690137449562111"*/);

               // if (IsTheNumberSimple(p) && IsTheNumberSimple(q))
                {
                    string hash = File.ReadAllText(sourceFilePathTextBox.Text).GetHashCode().ToString();

                    BigInteger n = p * q;
                    BigInteger m = (p - 1) * (q - 1);
                    BigInteger d = BigInteger.Parse(Calculate_d(m).ToString()); // Calculate_d(m);  // Взаимнопростое с m
                    BigInteger e_ = Calculate_e(d, m);

                    List<string> result = RSA_Endoce(hash, e_, n);

                    StreamWriter sw = new StreamWriter(signFilePathTextBox.Text);
                    foreach (string item in result)
                        sw.WriteLine(item);
                    sw.Close();

                    textBox_d.Text = d.ToString();
                    textBox_n.Text = n.ToString();

                    Process.Start(signFilePathTextBox.Text);
                }
                //else
                //    MessageBox.Show("p или q - не простые числа!");
            }
            else
                MessageBox.Show("Введите p и q и пути к файлам!");
        }

        //расшифровать
        private void buttonDecipher_Click(object sender, EventArgs e)
        {
            if ((textBox_d.Text.Length > 0) && (textBox_n.Text.Length > 0) && (sourceFilePathTextBox.Text.Length > 0) && (signFilePathTextBox.Text.Length > 0))
            {
                BigInteger d = BigInteger.Parse(textBox_d.Text);
                BigInteger n = BigInteger.Parse(textBox_n.Text);

                List<string> input = new List<string>();

                StreamReader sr = new StreamReader(signFilePathTextBox.Text);
                while (!sr.EndOfStream)
                {
                    input.Add(sr.ReadLine());
                }
                sr.Close();

                string result = RSA_Dedoce(input, d, n);

                string hash = File.ReadAllText(sourceFilePathTextBox.Text).GetHashCode().ToString();

                if (result.Equals(hash))
                    MessageBox.Show("Файл подлинный. Подпись верна.");
                else
                    MessageBox.Show("Внимание! Файл НЕ подлинный!!!");
            }
            else
                MessageBox.Show("Введите секретный ключ и пути к файлам!");
        }

        //проверка: простое ли число?
        private bool IsTheNumberSimple(BigInteger n)
        {
            if (n < 2)
                return false;
            if (n == 2)
                return true;
            for (long i = 2; i < n; i++)
                if (n % i == 0)
                    return false;
            return true;
        }

        //зашифровать
        private List<string> RSA_Endoce(string s, BigInteger e, BigInteger n)
        {
            List<string> result = new List<string>();

            BigInteger bi;

            for (int i = 0; i < s.Length; i++)
            {
                int index = Array.IndexOf(characters, s[i]);

                bi = new BigInteger(index);

                bi = BigInteger.ModPow(bi, e, n);

                result.Add(bi.ToString());
            }

            return result;
        }

        //расшифровать
        private string RSA_Dedoce(List<string> input, BigInteger d, BigInteger n)
        {
            string result = "";
            BigInteger bi;

            foreach (string item in input)
            {
                bi = BigInteger.Parse(item);

                bi = BigInteger.ModPow(bi, d, n);

                BigInteger index = BigInteger.Parse(bi.ToString());

                try { result += characters[(UInt64)index].ToString(); }
                catch { }
            }

            return result;
        }

        //вычисление параметра d. d должно быть взаимно простым с m
        private BigInteger Calculate_d(BigInteger m)
        {
            BigInteger d = m - 1;

            while (BigInteger.GreatestCommonDivisor(m, d) > 1)
            {
                d--;
            }

            return d;
        }

        //вычисление параметра e
        private BigInteger Calculate_e(BigInteger d, BigInteger m)
        {
            BigInteger e = d;
            BigInteger rez; BigInteger.DivRem(BigInteger.Multiply(e, d), m, out rez);

            while (rez != 1)
            {
                e--;
                BigInteger.DivRem(BigInteger.Multiply(e, d), m, out rez);
            }
                
            return e;
        }

        private void sourceFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                sourceFilePathTextBox.Text = ofd.FileName;
            }
        }

        private void signFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                signFilePathTextBox.Text = ofd.FileName;
            }
        }
    }
}
