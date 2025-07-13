using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUI__Calculator_App
{
    public static class Calc
    {
        public static double Do(double val1, double val2, string operation)
        {
            try
            {
                return operation switch
                {
                    "+" => val1 + val2,
                    "−" or "-" => val1 - val2,
                    "×" or "*" or "x" => val1 * val2,
                    "÷" or "/" => val2 != 0 ? val1 / val2 : throw new DivideByZeroException("Cannot divide by zero"),
                    "%" => val1 % val2,
                    "^" => Math.Pow(val1, val2),
                    _ => throw new InvalidOperationException($"Unknown operation: {operation}")
                };
            }
            catch (DivideByZeroException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error in calculation: {ex.Message}");
            }
        }

        public static double Power(double baseValue, double exponent)
        {
            return Math.Pow(baseValue, exponent);
        }

        public static double SquareRoot(double value)
        {
            if (value < 0)
                throw new ArgumentException("Cannot calculate square root of negative number");
            return Math.Sqrt(value);
        }

        public static double Percentage(double value, double percentage)
        {
            return value * (percentage / 100);
        }

        public static double Factorial(double value)
        {
            if (value < 0 || value != Math.Floor(value))
                throw new ArgumentException("Factorial is only defined for non-negative integers");
            if (value == 0 || value == 1)
                return 1;

            double result = 1;
            for (int i = 2; i <= value; i++)
            {
                result *= i;
            }
            return result;
        }

        // Method untuk mengevaluasi ekspresi kompleks dengan prioritas operator
        public static double EvaluateExpression(string expression)
        {
            // Bersihkan ekspresi dari spasi
            expression = expression.Replace(" ", "");

            // Tokenize expression
            var tokens = TokenizeExpression(expression);

            // Evaluate dengan prioritas operator menggunakan Shunting Yard algorithm
            return EvaluateWithPrecedence(tokens);
        }

        private static List<string> TokenizeExpression(string expression)
        {
            var tokens = new List<string>();
            var currentToken = "";

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (char.IsDigit(c) || c == '.')
                {
                    currentToken += c;
                }
                else if (IsOperator(c.ToString()))
                {
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        tokens.Add(currentToken);
                        currentToken = "";
                    }
                    tokens.Add(c.ToString());
                }
            }

            if (!string.IsNullOrEmpty(currentToken))
            {
                tokens.Add(currentToken);
            }

            return tokens;
        }

        private static bool IsOperator(string op)
        {
            return op == "+" || op == "−" || op == "×" || op == "÷" ||
                   op == "*" || op == "/" || op == "-" || op == "^";
        }

        private static int GetOperatorPrecedence(string op)
        {
            switch (op)
            {
                case "+":
                case "−":
                case "-":
                    return 1;
                case "×":
                case "÷":
                case "*":
                case "/":
                    return 2;
                case "^":
                    return 3;
                default:
                    return 0;
            }
        }

        private static bool IsLeftAssociative(string op)
        {
            return op != "^"; // Only power operator is right associative
        }

        // Implementasi Shunting Yard algorithm untuk evaluasi ekspresi
        private static double EvaluateWithPrecedence(List<string> tokens)
        {
            var output = new Queue<string>();
            var operators = new Stack<string>();

            // Convert to postfix notation (Reverse Polish Notation)
            foreach (string token in tokens)
            {
                if (double.TryParse(token, out _))
                {
                    output.Enqueue(token);
                }
                else if (IsOperator(token))
                {
                    while (operators.Count > 0 &&
                           IsOperator(operators.Peek()) &&
                           ((IsLeftAssociative(token) && GetOperatorPrecedence(token) <= GetOperatorPrecedence(operators.Peek())) ||
                            (!IsLeftAssociative(token) && GetOperatorPrecedence(token) < GetOperatorPrecedence(operators.Peek()))))
                    {
                        output.Enqueue(operators.Pop());
                    }
                    operators.Push(token);
                }
            }

            while (operators.Count > 0)
            {
                output.Enqueue(operators.Pop());
            }

            // Evaluate postfix expression
            return EvaluatePostfix(output);
        }

        private static double EvaluatePostfix(Queue<string> postfix)
        {
            var stack = new Stack<double>();

            while (postfix.Count > 0)
            {
                string token = postfix.Dequeue();

                if (double.TryParse(token, out double number))
                {
                    stack.Push(number);
                }
                else if (IsOperator(token))
                {
                    if (stack.Count < 2)
                        throw new InvalidOperationException("Invalid expression");

                    double b = stack.Pop();
                    double a = stack.Pop();

                    double result = Do(a, b, token);
                    stack.Push(result);
                }
            }

            if (stack.Count != 1)
                throw new InvalidOperationException("Invalid expression");

            return stack.Pop();
        }

        // Method untuk menghitung ekspresi sederhana (backward compatibility)
        public static double Calculate(string expression)
        {
            return EvaluateExpression(expression);
        }
    }
}