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
        ExpectedIn = 201,
        ExpectedMain = 209,
        ExpectedFor = 210,
        ExpectedElse = 211,
        ExpectedVarOrVal = 212,
        ExpectedIntOrChar = 213,

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

        private Token GetToken(int idx)
        {
            if (idx < Tokens.Count)
                return Tokens[idx];

            throw new IndexOutOfRangeException("End of token stream reached");
        }


        public ErrorCode prog()
        {
            index = 0;

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
            // Если следующая лексема — '}', значит, тело пустое — это допустимо
            if (GetToken(index).Type == "S" && GetToken(index).Value == "}")
            {
                return ErrorCode.ExpectedCloseBrace; // Разрешаем пустое тело
            }

            // Иначе проверяем список операций
            ErrorCode code = oper();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            return bolshe_oper();
        }

        private ErrorCode oper()
        {
            if (index >= Tokens.Count)
            {
                Error(ErrorCode.UnexpectedToken, index);
                return ErrorCode.UnexpectedToken;
            }

            // <тип_пам> id <опис_id>
            if (GetToken(index).Type == "T" && (GetToken(index).Value == "var" || GetToken(index).Value == "val"))
            {
                index++;
                if (GetToken(index).Type == "ID")
                {
                    index++;
                    return opis_id();
                }
                else
                {
                    Error(ErrorCode.ExpectedIdentifier, index);
                    return ErrorCode.ExpectedIdentifier;
                }
            }
            // id <действ_id>
            else if (GetToken(index).Type == "ID")
            {
                index++;
                return deistv_id();
            }
            // for (x in 10..1 step -1) { <spis_oper> }
            else if (GetToken(index).Type == "T" && GetToken(index).Value == "for")
            {
                index++;
                if (!(GetToken(index).Type == "S" && GetToken(index).Value == "("))
                {
                    Error(ErrorCode.ExpectedOpenParenthesis, index);
                    return ErrorCode.ExpectedOpenParenthesis;
                }
                index++;

                // Проверка переменной цикла
                if (GetToken(index).Type != "ID")
                {
                    Error(ErrorCode.ExpectedIdentifier, index);
                    return ErrorCode.ExpectedIdentifier;
                }
                index++;

                // Проверка ключевого слова 'in'
                if (!(GetToken(index).Type == "T" && GetToken(index).Value == "in"))
                {
                    Error(ErrorCode.ExpectedIn, index); // Используем существующий ExpectedIn
                    return ErrorCode.ExpectedIn;
                }
                index++;

                // Обработка диапазона (начальное значение)
                ErrorCode code = vyr();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                // Проверка оператора '..'
                if (!(GetToken(index).Type == "S" && GetToken(index).Value == ".."))
                {
                    Error(ErrorCode.ExpectedDoubleDot, index); // Используем существующий ExpectedDoubleDot
                    return ErrorCode.ExpectedDoubleDot;
                }
                index++;

                // Обработка конечного значения диапазона
                code = vyr();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                // Обработка необязательного 'step'
                if (GetToken(index).Type == "T" && GetToken(index).Value == "step")
                {
                    index++;
                    code = vyr();
                    if (code != ErrorCode.ExpectedCloseBrace) return code;
                }

                // Проверка закрывающей скобки
                if (!(GetToken(index).Type == "S" && GetToken(index).Value == ")"))
                {
                    Error(ErrorCode.ExpectedCloseParenthesis, index);
                    return ErrorCode.ExpectedCloseParenthesis;
                }
                index++;

                // Проверка открывающей фигурной скобки
                if (!(GetToken(index).Type == "S" && GetToken(index).Value == "{"))
                {
                    Error(ErrorCode.ExpectedOpenBrace, index);
                    return ErrorCode.ExpectedOpenBrace;
                }
                index++;

                // Обработка тела цикла
                code = spis_oper();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                // Проверка закрывающей фигурной скобки
                if (!(GetToken(index).Type == "S" && GetToken(index).Value == "}"))
                {
                    Error(ErrorCode.ExpectedCloseBrace, index);
                    return ErrorCode.ExpectedCloseBrace;
                }
                index++;

                return ErrorCode.ExpectedCloseBrace; // Успешное завершение
            }
            else
            {
                Error(ErrorCode.InvalidStatement, index);
                return ErrorCode.InvalidStatement;
            }
        }

        private ErrorCode bolshe_oper()
        {
            if (index >= Tokens.Count ||
                (GetToken(index).Type == "S" && GetToken(index).Value == "}"))
            {
                return ErrorCode.ExpectedCloseBrace; // Разрешаем завершение
            }

            ErrorCode code = oper();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            return bolshe_oper();
        }

        private ErrorCode opis_id()
        {
            if (GetToken(index).Type == "S" && GetToken(index).Value == ":")
            {
                index++;
                ErrorCode code = tip();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                code = prisv();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                if (!(GetToken(index).Type == "S" && GetToken(index).Value == ";"))
                {
                    Error(ErrorCode.ExpectedSemicolon, index);
                    return ErrorCode.ExpectedSemicolon;
                }
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else if (GetToken(index).Type == "S" && GetToken(index).Value == "=")
            {
                index++;
                ErrorCode code = vyr();
                if (code != ErrorCode.ExpectedSemicolon) return code;

                if (!(GetToken(index).Type == "S" && GetToken(index).Value == ";"))
                {
                    Error(ErrorCode.ExpectedSemicolon, index);
                    return ErrorCode.ExpectedSemicolon;
                }
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else
            {
                Error(ErrorCode.InvalidDeclaration, index);
                return ErrorCode.InvalidDeclaration;
            }
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

        private ErrorCode prisv()
        {
            if (GetToken(index).Type == "S" && GetToken(index).Value == "=")
            {
                index++;
                return vyr();
            }
            return ErrorCode.ExpectedCloseBrace; // ε case
        }

        private ErrorCode vyr()
        {
            ErrorCode code = slag();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            return bolshe_slag();
        }

        private ErrorCode slag()
        {
            ErrorCode code = fakt();
            if (code != ErrorCode.ExpectedCloseBrace) return code;

            return bolshe_fakt();
        }

        private ErrorCode fakt()
        {
            if (GetToken(index).Type == "ID") // id
            {
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else if (GetToken(index).Type == "L") // lit
            {
                index++;
                return ErrorCode.ExpectedCloseBrace;
            }
            else if (GetToken(index).Type == "S" && GetToken(index).Value == "(") // (
            {
                index++;

                ErrorCode code = vyr();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                if (!(GetToken(index).Type == "S" && GetToken(index).Value == ")")) // )
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

        private ErrorCode bolshe_fakt()
        {
            if (index >= Tokens.Count)
                return ErrorCode.ExpectedCloseBrace;

            if (GetToken(index).Type == "S" && GetToken(index).Value == "*") // *
            {
                index++;
                ErrorCode code = fakt();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                return bolshe_fakt();
            }
            else if (GetToken(index).Type == "S" && GetToken(index).Value == "/") // /
            {
                index++;
                ErrorCode code = fakt();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                return bolshe_fakt();
            }
            else if (GetToken(index).Type == "S" && GetToken(index).Value == "%") // %
            {
                index++;
                ErrorCode code = fakt();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                return bolshe_fakt();
            }
            return ErrorCode.ExpectedCloseBrace; // ε case
        }

        private ErrorCode bolshe_slag()
        {
            if (index >= Tokens.Count)
                return ErrorCode.ExpectedCloseBrace;

            if (GetToken(index).Type == "S" && GetToken(index).Value == "+") // +
            {
                index++;
                ErrorCode code = slag();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                return bolshe_slag();
            }
            else if (GetToken(index).Type == "S" && GetToken(index).Value == "-") // -
            {
                index++;
                ErrorCode code = slag();
                if (code != ErrorCode.ExpectedCloseBrace) return code;

                return bolshe_slag();
            }
            return ErrorCode.ExpectedCloseBrace; // ε case
        }

        private ErrorCode deistv_id()
        {
            if (index >= Tokens.Count)
            {
                Error(ErrorCode.UnexpectedToken, index);
                return ErrorCode.UnexpectedToken;
            }

            // Проверяем, является ли текущий токен оператором присваивания
            if (GetToken(index).Type == "S" && GetToken(index).Value == "=") // =
            {
                index++;
                ErrorCode code = vyr();
                if (code != ErrorCode.ExpectedCloseBrace) return code;
            }
            else if (GetToken(index).Type == "S" &&
                    (GetToken(index).Value == "+=" ||
                     GetToken(index).Value == "-=" ||
                     GetToken(index).Value == "*=" ||
                     GetToken(index).Value == "/=" ||
                     GetToken(index).Value == "%=")) // сл_ар_опер
            {
                index++;
                ErrorCode code = vyr();
                if (code != ErrorCode.ExpectedCloseBrace) return code;
            }
            else
            {
                // Если токен не является оператором присваивания, проверяем, является ли он частью другого допустимого контекста
                if (GetToken(index).Type == "S" && GetToken(index).Value == "{")
                {
                    // Если это открывающая скобка, возвращаем ошибку, но более точную
                    Error(ErrorCode.InvalidStatement, index);
                    return ErrorCode.InvalidStatement;
                }
                else
                {
                    Error(ErrorCode.InvalidOperator, index);
                    return ErrorCode.InvalidOperator;
                }
            }

            if (!(GetToken(index).Type == "S" && GetToken(index).Value == ";")) // ;
            {
                Error(ErrorCode.ExpectedSemicolon, index);
                return ErrorCode.ExpectedSemicolon;
            }
            index++;
            return ErrorCode.ExpectedCloseBrace;
        }
    }
}