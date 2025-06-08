using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using кофейня;

internal static class Program
{    
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();                

        // Запуск приложения
        Application.Run(new vhod());
    }
}

