
using System.Windows.Forms;

namespace SistemaVittis;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        bool continuarEjecutando = true;

        while (continuarEjecutando)
        {
            using var login = new FormLogin();
            if (login.ShowDialog() != DialogResult.OK)
                break;

            using var principal = new FormPrincipal(login.UsuarioActual);
            Application.Run(principal);

            continuarEjecutando = principal.DeseaCerrarSesion;
        }
    }
}
