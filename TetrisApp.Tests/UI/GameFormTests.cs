using System;
using System.Threading;
using System.Windows.Forms;
using TetrisApp;
using Xunit;

namespace TetrisApp.Tests.UI
{
    public class GameFormTests
    {
        [Fact]
        public void GameForm_InitializesSuccessfully()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    using var form = new GameForm();
                    Assert.NotNull(form);
                    Assert.Equal("Tetris Clásico", form.Text);
                    Assert.NotNull(form.Controls["picTablero"]);
                    Assert.NotNull(form.Controls["pnlLateral"]);

                    var panel = form.Controls["pnlLateral"] as Panel;
                    Assert.NotNull(panel);
                    Assert.NotNull(panel.Controls["lblScore"]);
                    Assert.NotNull(panel.Controls["lblLineas"]);
                    Assert.NotNull(panel.Controls["lblNivel"]);
                    Assert.NotNull(panel.Controls["btnIniciar"]);
                    Assert.NotNull(panel.Controls["btnPausa"]);
                    Assert.NotNull(panel.Controls["picSiguiente"]);
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.Null(threadException);
        }
    }
}
