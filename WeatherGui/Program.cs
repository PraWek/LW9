using System;
using System.Windows.Forms;

namespace WeatherGui
{
    /// <summary>
    /// точка входа приложения
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// главный метод запуска приложения
        /// </summary>
        /// <param name="args">аргументы командной строки</param>
        /// <returns>void</returns>
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
