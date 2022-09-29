using System.Threading.Tasks;
using Zero.Game.Local;

namespace Zero.Game.Mock
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await ZeroLocal.RunAsync<MockPlugin>(args);
        }
    }
}
