using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace MAUI__Calculator_App;

public class HistoryItem : INotifyPropertyChanged
{
    public string Expression { get; set; }
    public string Result { get; set; }
    public DateTime Timestamp { get; set; }
    public double ResultValue { get; set; }
    public string FullExpression { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
}

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private string currentInput = "";
    private string fullExpression = ""; // Untuk menyimpan seluruh ekspresi
    private double result = 0;
    private bool isOperatorPressed = false;
    private bool isEqualsPressed = false;
    private bool isDecimalPressed = false;
    private List<string> history = new List<string>();

    private ObservableCollection<HistoryItem> _historyItems;
    public ObservableCollection<HistoryItem> HistoryItems
    {
        get => _historyItems;
        set
        {
            _historyItems = value;
            OnPropertyChanged(nameof(HistoryItems));
        }
    }

    public MainPage()
    {
        InitializeComponent();
        HistoryItems = new ObservableCollection<HistoryItem>();
        resultView.Text = "0";

        // Set binding context for history
        this.BindingContext = this;

        // Make display area clickable for copy
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnResultTapped;
        resultView.GestureRecognizers.Add(tapGesture);

        // Add keyboard input support
        this.Loaded += MainPage_Loaded;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void MainPage_Loaded(object sender, EventArgs e)
    {
        // Focus on page to receive keyboard input
        this.Focus();
    }

    private void OnHistoryItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is HistoryItem selectedItem)
        {
            // Restore state from history item
            RestoreFromHistory(selectedItem);

            // Clear selection
            historyCollectionView.SelectedItem = null;
        }
    }

    private void RestoreFromHistory(HistoryItem historyItem)
    {
        try
        {
            // Set result from history
            result = historyItem.ResultValue;
            currentInput = historyItem.ResultValue.ToString();
            resultView.Text = FormatNumber(currentInput);
            historyView.Text = historyItem.Expression;
            fullExpression = "";

            // Reset state
            isOperatorPressed = false;
            isEqualsPressed = true;

            DisplayToast($"Restored: {historyItem.Expression} = {historyItem.Result}");
        }
        catch (Exception ex)
        {
            DisplayToast("Error restoring from history");
        }
    }

    private void AddToHistory(string expression, string result, double resultValue)
    {
        var historyItem = new HistoryItem
        {
            Expression = expression,
            Result = result,
            ResultValue = resultValue,
            Timestamp = DateTime.Now,
            FullExpression = $"{expression} = {result}"
        };

        // Add to beginning of list (most recent first)
        HistoryItems.Insert(0, historyItem);

        // Also add to simple history list
        history.Insert(0, $"{expression} = {result}");

        // Keep only last 20 items
        while (HistoryItems.Count > 20)
        {
            HistoryItems.RemoveAt(HistoryItems.Count - 1);
        }

        while (history.Count > 20)
        {
            history.RemoveAt(history.Count - 1);
        }
    }

    private void ClearHistoryItems()
    {
        HistoryItems.Clear();
        history.Clear();
        DisplayToast("History cleared");
    }

    private void HandleNumberInput(string number)
    {
        if (isEqualsPressed)
        {
            Clear(null, null);
        }

        if (isOperatorPressed)
        {
            currentInput = "";
            isOperatorPressed = false;
        }

        if (currentInput == "0" && number != "0")
        {
            currentInput = number;
        }
        else
        {
            currentInput += number;
        }

        resultView.Text = FormatNumber(currentInput);
        isEqualsPressed = false;
    }

    private void HandleDecimalInput()
    {
        if (isEqualsPressed)
        {
            Clear(null, null);
        }

        if (isOperatorPressed)
        {
            currentInput = "0";
            isOperatorPressed = false;
        }

        if (!currentInput.Contains("."))
        {
            if (string.IsNullOrEmpty(currentInput))
            {
                currentInput = "0";
            }
            currentInput += ".";
            resultView.Text = currentInput;
        }

        isEqualsPressed = false;
    }

    private void HandleOperatorInput(string op)
    {
        if (!string.IsNullOrEmpty(currentInput))
        {
            // Add current number to full expression
            if (string.IsNullOrEmpty(fullExpression))
            {
                fullExpression = currentInput;
            }
            else if (!isOperatorPressed)
            {
                fullExpression += currentInput;
            }
            else
            {
                // Replace last operator if user pressed operator twice
                int lastSpaceIndex = fullExpression.TrimEnd().LastIndexOf(' ');
                if (lastSpaceIndex > 0)
                {
                    fullExpression = fullExpression.Substring(0, lastSpaceIndex);
                }
            }

            // Add operator to full expression
            fullExpression += " " + op + " ";

            // Update history view to show current expression
            historyView.Text = fullExpression.Trim();

            isOperatorPressed = true;
            isEqualsPressed = false;
            isDecimalPressed = false;
        }
    }

    void Clear(object sender, EventArgs e)
    {
        currentInput = "";
        fullExpression = "";
        result = 0;
        isOperatorPressed = false;
        isEqualsPressed = false;
        isDecimalPressed = false;
        historyView.Text = "";
        resultView.Text = "0";
    }

    void NumberSelect(object sender, EventArgs e)
    {
        Button button = sender as Button;
        string number = button.Text;

        if (number == ".")
        {
            HandleDecimalInput();
        }
        else
        {
            HandleNumberInput(number);
        }
    }

    void OperatorSelect(object sender, EventArgs e)
    {
        Button button = sender as Button;
        string operatorText = button.Text;

        // Handle special operators
        switch (operatorText)
        {
            case "( )":
                HandleParentheses();
                return;
            case "%":
                HandlePercentage();
                return;
            default:
                HandleOperatorInput(operatorText);
                break;
        }
    }

    private void HandleParentheses()
    {
        // Simple implementation for parentheses - can be developed further
        DisplayToast("Parentheses feature coming soon");
    }

    private void HandlePercentage()
    {
        if (!string.IsNullOrEmpty(currentInput))
        {
            double value = double.Parse(currentInput);
            value = value / 100;
            currentInput = value.ToString();
            resultView.Text = FormatNumber(currentInput);
        }
    }

    void Calculate(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(currentInput))
        {
            try
            {
                // Add final number to expression
                string completeExpression = fullExpression + currentInput;

                // Calculate the result using expression evaluator
                double calculationResult = EvaluateExpression(completeExpression);

                string formattedResult = FormatNumber(calculationResult.ToString());

                // HANYA MASUKKAN KE HISTORY SAAT TOMBOL EQUALS (=) DITEKAN
                AddToHistory(completeExpression, formattedResult, calculationResult);

                // Update display
                historyView.Text = completeExpression;
                resultView.Text = formattedResult;

                // Reset for next calculation
                result = calculationResult;
                currentInput = calculationResult.ToString();
                fullExpression = "";
                isOperatorPressed = false;
                isEqualsPressed = true;
            }
            catch (DivideByZeroException)
            {
                DisplayToast("Cannot divide by zero");
                Clear(null, null);
            }
            catch (Exception ex)
            {
                DisplayToast("Error in calculation: " + ex.Message);
                Clear(null, null);
            }
        }
    }

    // Method untuk mengevaluasi ekspresi dengan prioritas operator yang benar
    private double EvaluateExpression(string expression)
    {
        // Bersihkan ekspresi dari spasi berlebih
        expression = expression.Replace(" ", "");

        // Tokenize expression
        var tokens = TokenizeExpression(expression);

        // Evaluate dengan prioritas operator
        return EvaluateTokens(tokens);
    }

    private List<string> TokenizeExpression(string expression)
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

    private bool IsOperator(string op)
    {
        return op == "+" || op == "−" || op == "×" || op == "÷" || op == "*" || op == "/" || op == "-";
    }

    private int GetOperatorPrecedence(string op)
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
            default:
                return 0;
        }
    }

    private double EvaluateTokens(List<string> tokens)
    {
        var numbers = new Stack<double>();
        var operators = new Stack<string>();

        for (int i = 0; i < tokens.Count; i++)
        {
            string token = tokens[i];

            if (double.TryParse(token, out double number))
            {
                numbers.Push(number);
            }
            else if (IsOperator(token))
            {
                while (operators.Count > 0 &&
                       GetOperatorPrecedence(operators.Peek()) >= GetOperatorPrecedence(token))
                {
                    ProcessOperator(numbers, operators);
                }
                operators.Push(token);
            }
        }

        while (operators.Count > 0)
        {
            ProcessOperator(numbers, operators);
        }

        return numbers.Pop();
    }

    private void ProcessOperator(Stack<double> numbers, Stack<string> operators)
    {
        if (numbers.Count < 2) return;

        double b = numbers.Pop();
        double a = numbers.Pop();
        string op = operators.Pop();

        double result = Calc.Do(a, b, op);
        numbers.Push(result);
    }

    private async void OnResultTapped(object sender, EventArgs e)
    {
        try
        {
            string textToCopy = resultView.Text;
            await Clipboard.SetTextAsync(textToCopy);
            DisplayToast($"Result '{textToCopy}' copied to clipboard");
        }
        catch (Exception ex)
        {
            DisplayToast("Failed to copy to clipboard");
        }
    }

    private string FormatNumber(string number)
    {
        if (double.TryParse(number, out double value))
        {
            // Format with thousand separators and max 10 decimal places
            if (value == Math.Floor(value))
            {
                return value.ToString("N0", CultureInfo.CurrentCulture);
            }
            else
            {
                return value.ToString("N10", CultureInfo.CurrentCulture).TrimEnd('0').TrimEnd('.');
            }
        }
        return number;
    }

    private void DisplayToast(string message)
    {
        // Simple toast notification implementation
        DisplayAlert("Info", message, "OK");
    }

    // Method to show history (can be called from separate button)
    public void ShowHistory()
    {
        if (history.Count > 0)
        {
            string historyText = string.Join("\n", history.Take(10));
            DisplayAlert("History", historyText, "OK");
        }
        else
        {
            DisplayToast("History is empty");
        }
    }

    // Method to clear history
    public void ClearHistory()
    {
        ClearHistoryItems();
    }

    // Method untuk menangani tombol delete
    void Delete(object sender, EventArgs e)
    {
        try
        {
            if (isEqualsPressed)
            {
                // Jika baru saja selesai perhitungan, mulai dari awal
                Clear(null, null);
                return;
            }

            if (!string.IsNullOrEmpty(currentInput))
            {
                // Hapus karakter terakhir dari currentInput
                currentInput = currentInput.Substring(0, currentInput.Length - 1);

                if (string.IsNullOrEmpty(currentInput))
                {
                    currentInput = "0";
                }

                resultView.Text = FormatNumber(currentInput);
            }
            else if (!string.IsNullOrEmpty(fullExpression))
            {
                // Hapus karakter terakhir dari fullExpression
                string trimmed = fullExpression.TrimEnd();

                if (trimmed.Length > 0)
                {
                    // Cari posisi spasi terakhir untuk menghapus operator
                    int lastSpaceIndex = trimmed.LastIndexOf(' ');

                    if (lastSpaceIndex > 0 && lastSpaceIndex == trimmed.Length - 1)
                    {
                        // Hapus operator (termasuk spasi)
                        fullExpression = trimmed.Substring(0, lastSpaceIndex - 1);
                        if (!string.IsNullOrEmpty(fullExpression))
                        {
                            fullExpression += " ";
                        }
                        isOperatorPressed = false;
                    }
                    else
                    {
                        // Hapus karakter terakhir
                        fullExpression = trimmed.Substring(0, trimmed.Length - 1);
                        if (!string.IsNullOrEmpty(fullExpression) && !fullExpression.EndsWith(" "))
                        {
                            fullExpression += " ";
                        }
                    }

                    historyView.Text = fullExpression.Trim();

                    if (string.IsNullOrEmpty(fullExpression.Trim()))
                    {
                        historyView.Text = "";
                        currentInput = "0";
                        resultView.Text = "0";
                    }
                }
            }
            else
            {
                // Jika tidak ada input, reset ke 0
                currentInput = "0";
                resultView.Text = "0";
            }
        }
        catch (Exception ex)
        {
            // Jika terjadi error, reset
            Clear(null, null);
        }
    }
}