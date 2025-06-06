using System;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace task
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Поле не может быть пустым!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dataGridView1.Rows.Clear();

            string[] lines = textBox1.Lines;
            string[] TerminalsArr = { "fun", "in", "val", "var", "for", "int", "char", "step", "main" };
            string[] SeparatorsArr = { "(", ")", "{", "}", ",", ":", ";", "+", "-", "*", "/", "%", "<", ">", "=", "!", ".", "++", "--", "*=", "/=", "%=", "<=", ">=", "==", "!=", "->", ".." };
            string[] DigitsArr = { "a", "b","c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
                      "A", "B","C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            string[] LettersArr = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            foreach (string line in lines)
            {
                var tokens = Lexan.InterpString(line);
                for (int i = 0; i < tokens.Count; i++)
                {
                    var token = tokens[i];
                    int index = -1; // По умолчанию индекс не определен

                    // Определяем индекс в зависимости от типа токена
                    if (token.Type == 't')
                    {
                        index = Array.IndexOf(TerminalsArr, token.Value);
                    }
                    else if (token.Type == 's')
                    {
                        index = Array.IndexOf(SeparatorsArr, token.Value);
                    }
                    else if (token.Type == 'i')
                    {
                        index = Array.IndexOf(DigitsArr, token.Value);
                    }
                    else if (token.Type == 'l')
                    {
                        // For literals, we parse the value to an integer and use it as the index
                        if (int.TryParse(token.Value, out int literalValue))
                        {
                            index = literalValue;
                        }
                    }

                    dataGridView1.Rows.Add(token.Value, GetFullName(token.Type), index);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileContent = System.IO.File.ReadAllText(openFileDialog.FileName);
                    textBox1.Text = fileContent;
                }
            }
        }
        private string GetFullName(char type)
        {
            if (type == 's') return "Р";
            else if (type == 't') return "Т";
            else if (type == 'i') return "И";
            else if (type == 'l') return "Л";
            else return "";
        }
    }
}
