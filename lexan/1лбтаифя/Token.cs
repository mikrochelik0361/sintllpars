using System.Collections.Generic;
using System.Linq;

namespace task
{
    public class Token
    {
        public char Type { get; set; }
        public string Value { get; set; }

        public Token(char type, string value)
        {
            Type = type;
            Value = value;
        }
    }

    public class Lexan
    {
        enum State { S, I, D, R }

        private static bool IsEnglishLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        public static List<Token> InterpString(string inputString)
        {
            List<Token> Tokens = new List<Token>();

            string[] TerminalsArr = { "fun", "in", "val", "var", "for", "int", "char", "step", "main" };
            string[] SeparatorsArr = { "(", ")", "{", "}", ",", ":", ";", "+", "-", "*", "/", "%", "<", ">", "=", "!", ".", "++", "--", "*=", "/=", "%=", "<=", ">=", "==", "!=", "->", ".." };
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
                            else if (SeparatorsArr.Contains(c.ToString())) { state = State.R; buffer += c; }
                            break;

                        case State.I:
                            if (char.IsLetterOrDigit(c)) { buffer += c; }
                            else
                            {
                                char type = TerminalsArr.Contains(buffer) ? 't' : 'i';
                                Tokens.Add(new Token(type, buffer));
                                buffer = "";
                                state = State.S;
                                reRecState = 1;
                            }
                            break;

                        case State.D:
                            if (char.IsDigit(c)) { buffer += c; }
                            else
                            {
                                Tokens.Add(new Token('l', buffer));
                                buffer = "";
                                state = State.S;
                                reRecState = 1;
                            }
                            break;

                        case State.R:
                            if (SeparatorsArr.Contains(buffer + c)) { buffer += c; }
                            else
                            {
                                Tokens.Add(new Token('s', buffer));
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
    }
}
