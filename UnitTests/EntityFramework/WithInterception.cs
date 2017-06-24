using System;
using System.Data.Entity.Infrastructure.Interception;

namespace UnitTests.EntityFramework
{
    public class WithInterception : IDisposable
    {
        private IDbCommandInterceptor interceptor;

        public WithInterception(IDbCommandInterceptor interceptor)
        {
            this.interceptor = interceptor;
            DbInterception.Add(this.interceptor);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DbInterception.Remove(this.interceptor);
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
