using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace task
{
    public partial class Form1 : Form
    {
        List<Token> Tokens;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            textBox2.Clear();

            string[] lines = textBox1.Lines;
            Tokens = new List<Token>();

            for (int i = 0; i < lines.Length; ++i)
            {
                Tokens.AddRange(LexAn.InterpString(lines[i]));
            }

            int j = 0;
            foreach (Token token in Tokens)
            {
                string tokenType = GetReadableTokenType(token);
                listBox1.Items.Add($"{j}. {token.Value} - {tokenType}");
                j++;
            }

            SyntLLParser syntLLParser = new SyntLLParser(Tokens);
            ErrorCode result = syntLLParser.Parse();

            if (result != ErrorCode.ExpectedCloseBrace)
            {
                foreach (string error in syntLLParser.GetErrors())
                {
                    textBox2.AppendText(error + Environment.NewLine);
                }
            }
            else
            {
                textBox2.Text = "Парсинг завершился успешно.";
            }
        }

        private string GetReadableTokenType(Token token)
        {
            switch (token.Type)
            {


                case "T":
                    return $"Ключевое слово";
                case "ID":
                    return "ID";
                case "L":
                    return "Литерал";
                case "S":
                    if (token.Value == "+" || token.Value == "-" || token.Value == "*" ||
                        token.Value == "/" || token.Value == "%" || token.Value == "<" ||
                        token.Value == ">" || token.Value == "=" || token.Value == "!" ||
                        token.Value == "." || token.Value == "+=" || token.Value == "-=" ||
                        token.Value == "*=" || token.Value == "/=" || token.Value == "%=" ||
                        token.Value == "<=" || token.Value == ">=" || token.Value == "==" ||
                        token.Value == "!=" || token.Value == "->" || token.Value == "..")
                    {
                        return $"Оператор";
                    }
                    else
                    {
                        return $"Разделитель";
                    }
                default:
                    return token.Type;
            }
        }
    }
}
