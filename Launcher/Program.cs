using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace IsekaiLauncher;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public class MainForm : Form
{
    private readonly Button _startButton;
    private readonly Label _statusLabel;

    public MainForm()
    {
        Text = "Isekai Desnecessário - Launcher";
        Width = 360;
        Height = 200;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        _startButton = new Button
        {
            Text = "Iniciar",
            Width = 160,
            Height = 50,
            Left = (ClientSize.Width - 160) / 2,
            Top = 40,
        };
        _startButton.Click += OnStartClicked;

        _statusLabel = new Label
        {
            Text = "Pronto para iniciar.",
            AutoSize = false,
            Width = ClientSize.Width - 40,
            Height = 40,
            Left = 20,
            Top = 110,
            TextAlign = ContentAlignment.MiddleCenter,
        };

        Controls.Add(_startButton);
        Controls.Add(_statusLabel);
    }

    private void OnStartClicked(object? sender, EventArgs e)
    {
        var root = FindProjectRoot();
        if (root is null)
        {
            _statusLabel.Text = "Não encontrei a raiz do projeto.";
            return;
        }

        var backendPath = Path.Combine(root, "backend", "IsekaiDesnecessario.API");
        var frontendPath = Path.Combine(root, "frontend", "isekai-desnecessario-app");

        try
        {
            StartInTerminal("Backend - Isekai Desnecessário", backendPath, "dotnet run");
            StartInTerminal("Frontend - Isekai Desnecessário", frontendPath, "npm start");
            _statusLabel.Text = "Backend e frontend iniciados!";
            _startButton.Enabled = false;
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Erro: {ex.Message}";
        }
    }

    private static void StartInTerminal(string title, string workingDirectory, string command)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/k title {title} && {command}",
            WorkingDirectory = workingDirectory,
            UseShellExecute = true,
        };
        Process.Start(psi);
    }

    private static string? FindProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "IsekaiDesnecessario.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
}
