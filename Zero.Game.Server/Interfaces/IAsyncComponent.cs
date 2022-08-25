using System;
using System.Threading.Tasks;

namespace Zero.Game.Server
{
    public interface IAsyncComponent
    {
        Task OnDestroyAsync();

        Task<bool> OnInitAsync();

        internal async Task DestroyAsync()
        {
            try
            {
                await OnDestroyAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(OnDestroyAsync));
            }
        }

        internal async Task<bool> InitAsync()
        {
            try
            {
                return await OnInitAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(OnInitAsync));
                return false;
            }
        }
    }
}
