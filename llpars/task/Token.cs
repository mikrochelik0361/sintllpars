using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task
{
    public class Token
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public Token(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }

    public class LexAn
    {
        enum State { S, I, D, R }

        private static bool IsEnglishLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        public static List<Token> LexTok(string inputString)
        {
            List<Token> Tokens = new List<Token>();
            string[] TerminalsArr = { "fun", "in", "val", "var", "for", "Int", "Char", "step", "main" };
            string[] SingleSeparatorsArr = { "(", ")", "{", "}", ",", ":", ";" };
            string[] MultipleSeparatorsArr = { "+", "-", "*", "/", "%", "<", ">", "=", "!", "." };
            string[] TwoSeparatorsArr = { "++", "--", "*=", "/=", "%=", "<=", ">=", "==", "!=", "->", "..", "+=", "-=" };



            State state = State.S;
            string buffer = "";
            int reRecState = 1;

            foreach (char c in inputString + " ")
            {
                if (reRecState == 1) reRecState = 0;

                for (int i = -1; i < reRecState; ++i)
                {
                    switch (state)
                    {
                        case State.S:
                            if (IsEnglishLetter(c)) { state = State.I; buffer += c; }
                            else if (char.IsDigit(c)) { state = State.D; buffer += c; }
                            else if (char.IsWhiteSpace(c)) { }
                            else if (SingleSeparatorsArr.Contains(c.ToString()))
                            {
                                Tokens.Add(new Token("S", c.ToString()));
                            }
                            else if (MultipleSeparatorsArr.Contains(c.ToString())) { state = State.R; buffer += c; }
                            break;

                        case State.I:
                            if (char.IsLetterOrDigit(c)) { buffer += c; }
                            else
                            {
                                Tokens.Add(new Token(TerminalsArr.Contains(buffer) ? "T" : "ID", buffer));
                                buffer = "";
                                state = State.S;
                                reRecState = 1;
                            }
                            break;

                        case State.D:
                            if (char.IsDigit(c)) { buffer += c; }
                            else
                            {
                                Tokens.Add(new Token("L", buffer));
                                buffer = "";
                                state = State.S;
                                reRecState = 1;
                            }
                            break;

                        case State.R:
                            if (TwoSeparatorsArr.Contains(buffer + c))
                            {
                                buffer += c;
                                Tokens.Add(new Token("S", buffer));
                                buffer = "";
                                state = State.S;
                            }
                            else
                            {
                                Tokens.Add(new Token("S", buffer));
                                buffer = "";
                                state = State.S;
                                reRecState = 1;
                            }
                            break;
                    }
                }
            }

            return Tokens;
        }

        public static List<Token> InterpString(string inputString)
        {
            List<Token> Tokens2 = LexAn.LexTok(inputString);
            return Tokens2;
        }
    }
}