using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task
{
    public enum ErrorCode
    {
        // Ошибки ожидаемых символов
        ExpectedOpenParenthesis = 100,
        ExpectedCloseParenthesis = 101,
        ExpectedOpenBrace = 102,
        ExpectedCloseBrace = 103,
        ExpectedComma = 104,
        ExpectedColon = 105,
        ExpectedSemicolon = 106,
        ExpectedArrow = 107,
        ExpectedDoubleDot = 108,
        ExpectedEquals = 109,

        // Ошибки ключевых слов
        ExpectedFun = 200,
        ExpectedMain = 201,
        ExpectedFor = 202,
        ExpectedVar = 203,
        ExpectedVal = 204,
        ExpectedIntOrChar = 205,
        ExpectedStep = 206,
        ExpectedLt = 207,
        Expectedinvcikl = 208,

        // Ошибки выражений
        ExpectedIdentifier = 300,
        ExpectedLiteral = 301,
        InvalidExpression = 302,
        InvalidStatement = 303,
        InvalidDeclaration = 304,
        InvalidOperator = 305,
        UnexpectedToken = 306
    }

    public class SyntLLParser
    {
        List<Token> Tokens;
        int index;
        private List<string> errorMessages;

        public SyntLLParser(List<Token> tokens)
        {
            Tokens = tokens;
            index = 0;
            errorMessages = new List<string>();
        }

        public List<string> GetErrors()
        {
            return errorMessages;
        }

        private void Error(ErrorCode code, int index)
        {
            string tokenInfo = index < Tokens.Count ?
                $"'{GetToken(index).Value}'" : "end of input";

            errorMessages.Add($"Error #{(int)code} at token {index}: {code.ToString()} - Unexpected {tokenInfo}");
        }

        private Token GetToken(int idx)
        {
            if (idx < Tokens.Count)
                return Tokens[idx];

            throw new IndexOutOfRangeException("End of token stream reached");
        }

        public ErrorCode Parse()
        {
            try
            {
                return prog();
            }
            catch (IndexOutOfRangeException ex)
            {
                var stackTrace = new StackTrace(ex, true);
                string methodName = stackTrace.GetFrame(1).GetMethod().Name;
                errorMessages.Add($"Error: Unexpected end of input in '{methodName}' block");
                return ErrorCode.UnexpectedToken;
            }
        }

        private ErrorCode prog()
        {
            if (!(GetToken(index).Type == "T" && GetToken(index).Value == "fun"))
            {
                Error(ErrorCode.ExpectedFun, index);
                return ErrorCode.ExpectedFun;
            }
            index++;

            if (!(GetToken(index).Type == "T" && GetToken(index).Value == "main"))
            {
                Error(ErrorCode.ExpectedMain, index);
                return ErrorCode.ExpectedMain;
            }
            index++;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == "("))
            {
                Error(ErrorCode.ExpectedOpenParenthesis, index);
                return ErrorCode.ExpectedOpenParenthesis;
            }
            index++;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == ")"))
            {
                Error(ErrorCode.ExpectedCloseParenthesis, index);
                return ErrorCode.ExpectedCloseParenthesis;
            }
            index++;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == "{"))
            {
                Error(ErrorCode.ExpectedOpenBrace, index);
                return ErrorCode.ExpectedOpenBrace;
            }
            index++;

            ErrorCode code = spis_oper();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == "}"))
            {
                Error(ErrorCode.ExpectedCloseBrace, index);
                return ErrorCode.ExpectedCloseBrace;
            }
            index++;

            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode spis_oper()
        {
            if (GetToken(index).Type == "S" && GetToken(index).Value == "}")
            {
                return ErrorCode.ExpectedCloseBrace;
            }

            ErrorCode code = oper();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            return S();
        }

        private ErrorCode S()
        {
            if (GetToken(index).Type == "S" && GetToken(index).Value == "}")
            {
                return ErrorCode.ExpectedCloseBrace;
            }

            ErrorCode code = oper();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            return S();
        }

        private ErrorCode oper()
        {
            if (GetToken(index).Type == "T" && GetToken(index).Value == "for")
            {
                return cikl();
            }
            else if (GetToken(index).Type == "ID")
            {
                return arifm();
            }
            else if (GetToken(index).Type == "T" && GetToken(index).Value == "var")
            {
                return opis_per();
            }
            else if (GetToken(index).Type == "T" && GetToken(index).Value == "val")
            {
                return opis_konst();
            }
            else
            {
                Error(ErrorCode.InvalidStatement, index);
                return ErrorCode.InvalidStatement;
            }
        }

        private ErrorCode opis_konst()
        {
            if (!(GetToken(index).Type == "T" && GetToken(index).Value == "val"))
            {
                Error(ErrorCode.ExpectedVal, index);
                return ErrorCode.ExpectedVal;
            }
            index++;

            if (!(GetToken(index).Type == "ID"))
            {
                Error(ErrorCode.ExpectedIdentifier, index);
                return ErrorCode.ExpectedIdentifier;
            }
            index++;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == ":"))
            {
                Error(ErrorCode.ExpectedColon, index);
                return ErrorCode.ExpectedColon;
            }
            index++;

            ErrorCode code = tip();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            code = K();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == ";"))
            {
                Error(ErrorCode.ExpectedSemicolon, index);
                return ErrorCode.ExpectedSemicolon;
            }
            index++;

            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode K()
        {
            if (GetToken(index).Type == "S" && GetToken(index).Value == "=")
            {
                index++;
                if (!(GetToken(index).Type == "L"))
                {
                    Error(ErrorCode.ExpectedLiteral, index);
                    return ErrorCode.ExpectedLiteral;
                }
                index++;
            }
            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode opis_per()
        {
            if (!(GetToken(index).Type == "T" && GetToken(index).Value == "var"))
            {
                Error(ErrorCode.ExpectedVar, index);
                return ErrorCode.ExpectedVar;
            }
            index++;

            if (!(GetToken(index).Type == "ID"))
            {
                Error(ErrorCode.ExpectedIdentifier, index);
                return ErrorCode.ExpectedIdentifier;
            }
            index++;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == ":"))
            {
                Error(ErrorCode.ExpectedColon, index);
                return ErrorCode.ExpectedColon;
            }
            index++;

            ErrorCode code = tip();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            code = P();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == ";"))
            {
                Error(ErrorCode.ExpectedSemicolon, index);
                return ErrorCode.ExpectedSemicolon;
            }
            index++;

            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode P()
        {
            if (GetToken(index).Type == "S" && GetToken(index).Value == "=")
            {
                index++;
                return fakt();
            }
            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode tip()
        {
            if (GetToken(index).Type == "T" && (GetToken(index).Value == "Int" || GetToken(index).Value == "Char"))
            {
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else
            {
                Error(ErrorCode.ExpectedIntOrChar, index);
                return ErrorCode.ExpectedIntOrChar;
            }
        }

        private ErrorCode arifm()
        {
            if (!(GetToken(index).Type == "ID"))
            {
                Error(ErrorCode.ExpectedIdentifier, index);
                return ErrorCode.ExpectedIdentifier;
            }
            index++;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == "="))
            {
                Error(ErrorCode.ExpectedEquals, index);
                return ErrorCode.ExpectedEquals;
            }
            index++;

            ErrorCode code = vyr();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == ";"))
            {
                Error(ErrorCode.ExpectedSemicolon, index);
                return ErrorCode.ExpectedSemicolon;
            }
            index++;

            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode vyr()
        {
            ErrorCode code = slag();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            return E();
        }

        private ErrorCode E()
        {
            if (GetToken(index).Type == "S" && (GetToken(index).Value == "+" || GetToken(index).Value == "-"))
            {
                index++;
                ErrorCode code = slag();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                return E();
            }
            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode slag()
        {
            ErrorCode code = fakt();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            return M();
        }

        private ErrorCode M()
        {
            if (GetToken(index).Type == "S" &&
                (GetToken(index).Value == "*" || GetToken(index).Value == "/" || GetToken(index).Value == "%"))
            {
                index++;
                ErrorCode code = fakt();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                return M();
            }
            return ErrorCode.ExpectedCloseBrace;
        }

        private ErrorCode fakt()
        {
            if (GetToken(index).Type == "ID" || GetToken(index).Type == "L")
            {
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else if (GetToken(index).Type == "S" && GetToken(index).Value == "(")
            {
                index++;
                ErrorCode code = vyr();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                if (!(GetToken(index).Type == "S" && GetToken(index).Value == ")"))
                {
                    Error(ErrorCode.ExpectedCloseParenthesis, index);
                    return ErrorCode.ExpectedCloseParenthesis;
                }
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else
            {
                Error(ErrorCode.InvalidExpression, index);
                return ErrorCode.InvalidExpression;
            }
        }

        private ErrorCode cikl()
        {
            if (!(GetToken(index).Type == "T" && GetToken(index).Value == "for"))
            {
                Error(ErrorCode.ExpectedFor, index);
                return ErrorCode.ExpectedFor;
            }
            index++;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == "("))
            {
                Error(ErrorCode.ExpectedOpenParenthesis, index);
                return ErrorCode.ExpectedOpenParenthesis;
            }
            index++;

            if (!(GetToken(index).Type == "ID"))
            {
                Error(ErrorCode.ExpectedIdentifier, index);
                return ErrorCode.ExpectedIdentifier;
            }
            index++;

            if (!(GetToken(index).Type == "T" && GetToken(index).Value == "in"))
            {
                Error(ErrorCode.Expectedinvcikl, index);
                return ErrorCode.Expectedinvcikl;
            }
            index++;

            ErrorCode code = fakt();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == ".."))
            {
                Error(ErrorCode.ExpectedDoubleDot, index);
                return ErrorCode.ExpectedDoubleDot;
            }
            index++;

            code = fakt();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            code = C();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == ")"))
            {
                Error(ErrorCode.ExpectedCloseParenthesis, index);
                return ErrorCode.ExpectedCloseParenthesis;
            }
            index++;

            return blok();
        }

        private ErrorCode C()
        {
            // Проверка необязательного шага 'step'
            if (GetToken(index).Type == "T" && GetToken(index).Value == "step")
            {
                index++;

                // Теперь ожидаем литерал после step
                if (!(GetToken(index).Type == "L")) // L - тип для литералов
                {
                    Error(ErrorCode.ExpectedLiteral, index);
                    return ErrorCode.ExpectedLiteral;
                }
                index++;
            }
            return ErrorCode.ExpectedCloseBrace; // ε-правило - шаг может отсутствовать
        }

        private ErrorCode blok()
        {
            if (GetToken(index).Type == "S" && GetToken(index).Value == "{")
            {
                index++;
                ErrorCode code = spis_oper();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                if (!(GetToken(index).Type == "S" && GetToken(index).Value == "}"))
                {
                    Error(ErrorCode.ExpectedCloseBrace, index);
                    return ErrorCode.ExpectedCloseBrace;
                }
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else
            {
                return oper();
            }
        }
    }
}